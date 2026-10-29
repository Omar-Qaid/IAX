using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

using PostingValidationException = IAX.IXApi.Modules.Finance.Common.SubledgerPostingException;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed partial class InventJournalPostingService
{
    public Task<string?> FreezeCountAsync(string journalId, CancellationToken ct) => CountActionAsync(journalId, null, false, ct);
    public Task<string?> ConfirmCountAsync(string journalId, IReadOnlyDictionary<long, decimal> quantities, CancellationToken ct) => CountActionAsync(journalId, quantities, false, ct);
    public Task<string?> CancelCountAsync(string journalId, CancellationToken ct) => CountActionAsync(journalId, null, true, ct);

    private Task<string?> CountActionAsync(string journalId, IReadOnlyDictionary<long, decimal>? quantities, bool cancel, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            var area = _company.GetDataAreaId();
            if (string.IsNullOrWhiteSpace(area)) return "A company must be selected.";
            await _unitOfWork.BeginTransactionAsync(ct);
            try
            {
                await Support.LockAsync($"InventJournal:{area}:{Normalize(journalId)}", ct);
                var header = await Db.Set<InventJournalTable>().SingleOrDefaultAsync(x => x.DataAreaId == area && x.JournalId == Normalize(journalId), ct);
                if (header == null || header.JournalType != InventJournalType.Counting || header.Posted == NoYes.Yes)
                    throw new PostingValidationException("An unposted counting journal is required.");
                var lines = await Db.Set<InventJournalTrans>().Where(x => x.DataAreaId == area && x.JournalId == header.JournalId)
                    .OrderBy(x => x.ItemId).ThenBy(x => x.InventDimId).ToListAsync(ct);
                if (lines.Count == 0 || lines.GroupBy(x => new { x.ItemId, x.InventDimId }).Any(x => x.Count() != 1))
                    throw new PostingValidationException("Counting requires unique item/dimension lines.");
                foreach (var line in lines)
                {
                    var error = await InventoryCountProtection.CheckAsync(Db, area, line.ItemId, line.InventDimId, ct, header.JournalId);
                    if (error != null) throw new PostingValidationException(error);
                }
                if (cancel)
                {
                    header.CountingFrozen = false;
                    header.CountingConfirmed = false;
                }
                else if (quantities == null)
                {
                    if (!header.CountingFrozen)
                    {
                        foreach (var line in lines)
                            line.InventOnHand = await Db.Set<InventSum>().Where(x => x.DataAreaId == area && x.ItemId == line.ItemId && x.InventDimId == line.InventDimId)
                                .Select(x => x.PhysicalInvent).SingleOrDefaultAsync(ct);
                        header.CountingFrozen = true;
                        header.CountingConfirmed = false;
                    }
                }
                else
                {
                    if (!header.CountingFrozen || quantities.Count != lines.Count || lines.Any(x => !quantities.ContainsKey(x.RecId))
                        || quantities.Values.Any(x => x < 0 || decimal.Round(x, 4) != x))
                        throw new PostingValidationException("Freeze the count, then provide a nonnegative counted inventory quantity for every line (maximum four decimals).");
                    if (header.CountingConfirmed && lines.Any(x => x.Counted != quantities[x.RecId]))
                        throw new PostingValidationException("A confirmed count cannot be changed; cancel and recount.");
                    foreach (var line in lines) line.Counted = quantities[line.RecId];
                    header.CountingConfirmed = true;
                }
                await _unitOfWork.CompleteAsync(ct);
                await _unitOfWork.CommitTransactionAsync(ct);
                return null;
            }
            catch (PostingValidationException ex)
            {
                await _unitOfWork.RollbackTransactionAsync(ct);
                Db.ChangeTracker.Clear();
                return ex.Message;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(ct);
                Db.ChangeTracker.Clear();
                throw;
            }
        });
}
