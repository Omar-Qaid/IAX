using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.Settlement;

/// <summary>
/// Ledger settlement: matches posted ledger account entries against each other and records the
/// match in LedgerTransSettlement. Settlement is matching data, not accounting — no voucher is
/// posted and no balance changes.
///
/// Scope is the equal, same-currency, same-main-account case the core cycle defines. A selection
/// that does not net to zero is refused, because an out-of-balance or cross-currency settlement
/// produces exchange adjustment or write-off accounting that this path does not perform: settle
/// the matched portion, or post the approved write-off first and settle afterwards.
/// </summary>
public sealed class LedgerSettlementService(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    ISysNumberSequenceService numbers) : ILedgerSettlementService
{
    private DbContext Db => unitOfWork.Context;

    public Task<(LedgerSettlementResultDto? Result, string? Error)> SettleAsync(LedgerSettlementRequestDto input, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => SettleCoreAsync(input, ct));

    private async Task<(LedgerSettlementResultDto? Result, string? Error)> SettleCoreAsync(LedgerSettlementRequestDto input, CancellationToken ct)
    {
        var area = currentUser.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(area)) return (null, "A company must be selected.");
        var ids = (input.AccountEntryRecIds ?? []).Distinct().ToList();
        if (ids.Count < 2) return (null, "Select at least two posted ledger entries to settle.");
        await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var support = new SubledgerPostingSupport(Db);
            await support.LockAsync($"LedgerSettlement:{area}", ct);

            // Ledger settlement is a parameter-gated feature. A company that has no parameter
            // record yet is not restricted; one that has turned the feature off is.
            var parameters = await LedgerParameters.LedgerParametersService.PeekAsync(Db, area, ct);
            if (parameters != null && parameters.EnableLedgerSettlement == NoYes.No)
                throw new SubledgerPostingException("Ledger settlement is not enabled in the general ledger parameters.");

            var entries = await Db.Set<GeneralJournalAccountEntry>().AsNoTracking()
                .Where(x => x.DataAreaId == area && ids.Contains(x.RecId)).ToListAsync(ct);
            if (entries.Count != ids.Count)
                throw new SubledgerPostingException("A selected entry does not exist in the selected company.");
            if (entries.Select(x => x.MainAccount).Distinct().Count() != 1)
                throw new SubledgerPostingException("Ledger settlement matches entries on one main account.");
            if (entries.Select(x => x.TransactionCurrencyCode).Distinct(StringComparer.Ordinal).Count() != 1)
                throw new SubledgerPostingException("Ledger settlement matches entries in one transaction currency.");

            var settled = await Db.Set<LedgerTransSettlement>().AsNoTracking()
                .Where(x => x.DataAreaId == area && ids.Contains(x.TransRecId) && x.IsReversed != NoYes.Yes)
                .Select(x => x.TransRecId).ToListAsync(ct);
            if (settled.Count > 0)
                throw new SubledgerPostingException($"Entry {settled[0]} is already settled; reverse that settlement first.");

            var balance = entries.Sum(x => x.AccountingCurrencyAmount);
            if (balance != 0m)
                throw new SubledgerPostingException(
                    $"The selection is out of balance by {balance}; settle the matched portion or post the approved write-off first." +
                    " Partial settlement is a separate parameter-gated feature that this path does not perform.");

            var settleDate = (input.SettleDate ?? DateTime.UtcNow).Date;
            var settleId = (await numbers.NextAsync("LedgerTransSettlement", area, ct)).Code;
            var processed = DateTime.UtcNow;
            var user = currentUser.GetCurrentUserId();
            foreach (var entry in entries)
                Db.Add(new LedgerTransSettlement
                {
                    // SettleId is the group. SettlementGroup, SettlementTableId and RefTransRecId stay 0,
                    // as they do in D365 for a ledger-entry settlement; no value is invented for them.
                    DataAreaId = area, SettleId = settleId,
                    TransRecId = entry.RecId, SettleDate = settleDate, DateProcessed = processed,
                    UserId = user,
                    SettleTransactionCurrencyAmount = entry.TransactionCurrencyAmount,
                    SettleAccountingCurrencyAmount = entry.AccountingCurrencyAmount,
                    SettleReportingCurrencyAmount = entry.ReportingCurrencyAmount
                });

            await unitOfWork.CompleteAsync(ct);
            await unitOfWork.CommitTransactionAsync(ct);
            // The settled amount is the matched side, not the net, which is zero by definition here.
            var amount = entries.Where(x => x.AccountingCurrencyAmount > 0).Sum(x => x.AccountingCurrencyAmount);
            return (new(settleId, entries.Count, amount, settleDate), null);
        }
        catch (SubledgerPostingException ex)
        {
            await unitOfWork.RollbackTransactionAsync(ct);
            Db.ChangeTracker.Clear();
            return (null, ex.Message);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(ct);
            Db.ChangeTracker.Clear();
            throw;
        }
    }

    public Task<(LedgerSettlementResultDto? Result, string? Error)> ReverseAsync(string settleId, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => ReverseCoreAsync(settleId, ct));

    private async Task<(LedgerSettlementResultDto? Result, string? Error)> ReverseCoreAsync(string settleId, CancellationToken ct)
    {
        var area = currentUser.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(area)) return (null, "A company must be selected.");
        var id = (settleId ?? string.Empty).Trim();
        if (id.Length == 0) return (null, "A settlement ID is required.");
        await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            await new SubledgerPostingSupport(Db).LockAsync($"LedgerSettlement:{area}", ct);
            var rows = await Db.Set<LedgerTransSettlement>()
                .Where(x => x.DataAreaId == area && x.SettleId == id).ToListAsync(ct);
            if (rows.Count == 0)
            {
                await unitOfWork.RollbackTransactionAsync(ct);
                return (null, null);
            }
            if (rows.All(x => x.IsReversed == NoYes.Yes))
                throw new SubledgerPostingException($"Settlement '{id}' is already reversed.");
            // Reversal marks the standing match as no longer in force; the rows remain as history.
            foreach (var row in rows)
            {
                row.IsReversed = NoYes.Yes;
                row.OriginalSettleId = id;
                row.DateProcessed = DateTime.UtcNow;
                row.UserId = currentUser.GetCurrentUserId();
            }
            await unitOfWork.CompleteAsync(ct);
            await unitOfWork.CommitTransactionAsync(ct);
            return (new(id, rows.Count,
                rows.Where(x => x.SettleAccountingCurrencyAmount > 0).Sum(x => x.SettleAccountingCurrencyAmount),
                rows[0].SettleDate), null);
        }
        catch (SubledgerPostingException ex)
        {
            await unitOfWork.RollbackTransactionAsync(ct);
            Db.ChangeTracker.Clear();
            return (null, ex.Message);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(ct);
            Db.ChangeTracker.Clear();
            throw;
        }
    }

    public async Task<(IReadOnlyList<LedgerOpenItemDto>? Items, string? Error)> OpenItemsAsync(string mainAccountId, CancellationToken ct)
    {
        var area = currentUser.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(area)) return (null, "A company must be selected.");
        var account = (mainAccountId ?? string.Empty).Trim();
        if (account.Length == 0) return (null, "A main account is required.");
        var accountRecIds = await Db.Set<MainAccount>().AsNoTracking()
            .Where(x => x.DataAreaId == area && x.MainAccountId == account).Select(x => x.RecId).ToListAsync(ct);
        if (accountRecIds.Count == 0) return (null, $"Main account '{account}' was not found in the selected company.");

        var standing = Db.Set<LedgerTransSettlement>().AsNoTracking()
            .Where(x => x.DataAreaId == area && x.IsReversed != NoYes.Yes).Select(x => x.TransRecId);
        var items = await (from line in Db.Set<GeneralJournalAccountEntry>().AsNoTracking()
                           join entry in Db.Set<GeneralJournalEntry>().AsNoTracking() on line.GeneralJournalEntry equals entry.RecId
                           where line.DataAreaId == area && accountRecIds.Contains(line.MainAccount)
                               && !standing.Contains(line.RecId)
                           orderby entry.AccountingDate, line.RecId
                           select new LedgerOpenItemDto(line.RecId, line.LedgerAccount, entry.AccountingDate,
                               entry.SubledgerVoucher, line.TransactionCurrencyCode, line.AccountingCurrencyAmount, line.Text))
            .ToListAsync(ct);
        return (items, null);
    }

}
