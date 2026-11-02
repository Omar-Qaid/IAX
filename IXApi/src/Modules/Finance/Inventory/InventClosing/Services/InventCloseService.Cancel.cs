using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using PostingValidationException = IAX.IXApi.Modules.Finance.Common.SubledgerPostingException;

namespace IAX.IXApi.Modules.Finance.Inventory;

/// <summary>
/// Cancellation of the most recent cost run. It reverses exactly what that run wrote - settlement
/// rows, settled quantities, cost adjustments, inventory value and the ledger correction - and
/// reopens the financial boundary, so a close made on wrong data is recoverable.
///
/// Only the latest live run can be cancelled, because every later run settled against the layers
/// this one consumed.
/// </summary>
public sealed partial class InventCloseService
{
    public Task<(InventCloseCancelResultDto? Result, string? Error)> CancelAsync(long closingRecId, CancellationToken ct = default) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => CancelCoreAsync(closingRecId, ct));

    private async Task<(InventCloseCancelResultDto? Result, string? Error)> CancelCoreAsync(long closingRecId, CancellationToken ct)
    {
        var companyId = _company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(companyId)) return (null, "A company must be selected.");
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            await Support.LockAsync($"InventClose:{companyId}", ct);
            var result = await CancelWithinTransactionAsync(companyId, closingRecId, ct);
            await _unitOfWork.CommitTransactionAsync(ct);
            return (result, null);
        }
        catch (PostingValidationException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            Db.ChangeTracker.Clear();
            return (null, ex.Message);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            Db.ChangeTracker.Clear();
            throw;
        }
    }

    private async Task<InventCloseCancelResultDto> CancelWithinTransactionAsync(string companyId, long closingRecId, CancellationToken ct)
    {
            var closing = await Db.Set<InventClosing>()
                .FirstOrDefaultAsync(x => x.DataAreaId == companyId && x.RecId == closingRecId, ct)
                ?? throw new PostingValidationException("The cost run was not found in the selected company.");
            if (closing.CancelClosing == NoYes.Yes)
                throw new PostingValidationException("A cancellation record cannot itself be cancelled.");
            if (closing.Cancellation == NoYes.Yes)
                throw new PostingValidationException("This cost run is already cancelled.");
            if (closing.InventCostStatus != InventCostStatus.Finished)
                throw new PostingValidationException("Only a finished cost run can be cancelled.");
            if (await Db.Set<InventClosing>().AnyAsync(x => x.DataAreaId == companyId && x.CancelClosing == NoYes.No
                    && x.Cancellation == NoYes.No && x.InventCostStatus == InventCostStatus.Finished
                    && x.RunNum > closing.RunNum, ct))
                throw new PostingValidationException(
                    "A later cost run exists. Cancel the runs in reverse order, starting with the most recent one.");

            var settlements = await Db.Set<InventSettlement>()
                .Where(x => x.DataAreaId == companyId && x.Voucher == closing.Voucher && x.Cancelled == NoYes.No)
                .ToListAsync(ct);

            var context = await LoadLedgerContextAsync(companyId, closing.TransDate.Date, ct);
            var voucher = (await _numbers.NextAsync("InventClosing", companyId, ct)).Code;
            var transactionRecIds = settlements.Select(x => x.TransRecId)
                .Concat(settlements.Where(x => x.ReceiptTransRecId != null).Select(x => x.ReceiptTransRecId!.Value))
                .Distinct().ToList();
            var transactions = await Db.Set<InventTrans>()
                .Where(x => x.DataAreaId == companyId && transactionRecIds.Contains(x.RecId))
                .ToDictionaryAsync(x => x.RecId, ct);
            var origins = await Db.Set<InventTransOrigin>().AsNoTracking()
                .Where(x => x.DataAreaId == companyId).Select(x => new { x.RecId, x.InventTransId, x.ReferenceCategory })
                .ToDictionaryAsync(x => x.RecId, ct);

            var ledgerEntriesCreated = 0;
            var totalReversed = 0m;

            foreach (var group in settlements.GroupBy(x => x.TransRecId))
            {
                if (!transactions.TryGetValue(group.Key, out var trans))
                    throw new PostingValidationException(
                        $"Settled inventory transaction {group.Key} is missing, so this run cannot be reversed.");
                var settledQty = group.Sum(x => x.QtySettled);
                var settledCost = group.Sum(x => x.CostAmountSettled);
                var adjustment = group.Sum(x => x.CostAmountAdjustment);

                trans.QtySettled -= settledQty;
                trans.CostAmountSettled -= settledCost;
                trans.CostAmountAdjustment -= adjustment;
                trans.ValueOpen = InventTransOpen.Yes;
                trans.DateClosed = default;

                if (adjustment != 0m)
                {
                    await AdjustInventorySummaryAsync(companyId, trans.ItemId, trans.InventDimId, -adjustment, ct);
                    var origin = origins.GetValueOrDefault(trans.InventTransOrigin);
                    await CreateLedgerCorrectionAsync(companyId, voucher, trans,
                        origin?.InventTransId ?? group.First().InventTransId,
                        origin?.ReferenceCategory ?? InventRefType.InventoryJournal,
                        group.First().ItemGroupId, -adjustment, context, ct);
                    ledgerEntriesCreated++;
                    totalReversed += -adjustment;
                }
            }

            // The receipt side of each consumption gives back the quantity the issue took from it.
            foreach (var group in settlements.Where(x => x.ReceiptTransRecId != null).GroupBy(x => x.ReceiptTransRecId!.Value))
            {
                if (!transactions.TryGetValue(group.Key, out var receipt))
                    throw new PostingValidationException(
                        $"Settled receipt transaction {group.Key} is missing, so this run cannot be reversed.");
                receipt.QtySettled += group.Sum(x => x.QtySettled);
                receipt.ValueOpen = InventTransOpen.Yes;
                receipt.DateClosed = default;
            }

            foreach (var settlement in settlements) settlement.Cancelled = NoYes.Yes;
            closing.Cancellation = NoYes.Yes;

            var history = await Db.Set<InventClosing>().AsNoTracking()
                .Where(x => x.DataAreaId == companyId).Select(x => x.RunNum).ToListAsync(ct);
            var runNum = history.Count == 0 ? 1 : history.Max() + 1;
            var reversal = new InventClosing
            {
                DataAreaId = companyId, Voucher = voucher, TransDate = closing.TransDate,
                ScopeItemId = closing.ScopeItemId, PeriodCode = closing.PeriodCode,
                RunNum = runNum, NextRunNum = runNum + 1,
                AdjustmentType = closing.AdjustmentType, AdjustmentSpec = closing.AdjustmentSpec,
                InventCostStatus = InventCostStatus.Finished, Executed = DateTime.UtcNow,
                Cancellation = NoYes.No, CancelClosing = NoYes.Yes, CancelClosingRefRecId = closing.RecId,
                RunRecalculation = closing.RunRecalculation,
                Active = NoYes.Yes, Ledger = NoYes.Yes, LedgerCorrection = NoYes.Yes,
                Start_ = NoYes.Yes, End_ = NoYes.Yes,
                Notes = $"Cancellation of run {closing.RunNum} ({closing.Voucher}) by {_user.GetCurrentUserId()}"
            };
            Db.Add(reversal);
            await _unitOfWork.CompleteAsync(ct);
            return new InventCloseCancelResultDto(reversal.RecId, voucher, closing.RecId, closing.RunNum,
                runNum, settlements.Count, ledgerEntriesCreated, totalReversed);
    }
}
