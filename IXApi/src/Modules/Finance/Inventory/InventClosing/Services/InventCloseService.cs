using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;
using ItemModelGroup = IAX.IXApi.Modules.Finance.Entities.InventModelGroup;
using PostingValidationException = IAX.IXApi.Modules.Finance.Common.SubledgerPostingException;

namespace IAX.IXApi.Modules.Finance.Inventory;

/// <summary>
/// Executes a basic FIFO inventory close: deterministic FIFO settlement of financially posted
/// issues against financially posted receipts within an item/financial-site pool, cost-adjustment
/// of each issue to its FIFO cost, settlement history, a ledger correction, and a close boundary.
/// Supports journal, purchase and sales sources in financial-site pools. Verified same-site
/// transfers conserve the pool and do not consume FIFO layers. Close reversal is not supported.
/// </summary>
public sealed class InventCloseService : IInventCloseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICompanyExecutionContext _company;
    private readonly ICurrentUserService _user;
    private readonly ISysNumberSequenceService _numbers;
    private DbContext Db => _unitOfWork.Context;
    private SubledgerPostingSupport Support => new(Db);

    public InventCloseService(IUnitOfWork unitOfWork, ICompanyExecutionContext company,
        ICurrentUserService user, ISysNumberSequenceService numbers)
    {
        _unitOfWork = unitOfWork;
        _company = company;
        _user = user;
        _numbers = numbers;
    }

    public Task<(InventCloseResultDto? Result, string? Error)> CloseAsync(DateTime cutoff, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => CloseCoreAsync(cutoff, string.Empty, ct));

    public Task<(InventCloseResultDto? Result, string? Error)> CloseItemAsync(string itemId, DateTime cutoff, CancellationToken ct = default) =>
        string.IsNullOrWhiteSpace(itemId)
            ? Task.FromResult<(InventCloseResultDto?, string?)>((null, "An item is required for scoped close."))
            : Db.Database.CreateExecutionStrategy().ExecuteAsync(() => CloseCoreAsync(cutoff, itemId.Trim(), ct));

    private async Task<(InventCloseResultDto? Result, string? Error)> CloseCoreAsync(DateTime cutoff, string itemId, CancellationToken ct)
    {
        var companyId = _company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(companyId)) return (null, "A company must be selected.");
        var cutoffDate = cutoff.Date;
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            await Support.LockAsync($"InventClose:{companyId}", ct);

            // Idempotency and close-boundary enforcement.
            var closings = await Db.Set<InventClosing>()
                .Where(x => x.DataAreaId == companyId && x.AdjustmentType == InventAdjustmentType.Closing)
                .ToListAsync(ct);
            var runNum = closings.Count == 0 ? 1 : closings.Max(x => x.RunNum) + 1;
            closings = closings.Where(x => itemId.Length == 0 || x.ScopeItemId == itemId || x.ScopeItemId.Length == 0).ToList();
            var existing = closings
                .Where(x => x.ScopeItemId == itemId && x.TransDate.Date == cutoffDate && x.InventCostStatus == InventCostStatus.Finished)
                .OrderByDescending(x => x.RunNum).FirstOrDefault();
            if (existing != null)
            {
                await _unitOfWork.CommitTransactionAsync(ct);
                return (new InventCloseResultDto(existing.RecId, existing.Voucher, cutoffDate,
                    existing.RunNum, 0, 0, 0, 0m), null);
            }
            var lastRun = closings.Where(x => x.InventCostStatus == InventCostStatus.Finished).ToList();
            if (lastRun.Any(x => x.TransDate.Date > cutoffDate || (x.TransDate.Date == cutoffDate && x.ScopeItemId.Length == 0)))
                throw new PostingValidationException(
                    "A later or equal inventory close already exists; backdated close is not supported in the basic scope.");

            var context = await LoadLedgerContextAsync(companyId, cutoffDate, ct);
            var pools = await LoadFifoPoolsAsync(companyId, cutoffDate, itemId, ct);
            if (itemId.Length > 0 && pools.Count == 0)
                throw new PostingValidationException("The selected item has no supported financial inventory to close.");
            var salesReturns = await Db.Set<SalesLine>().AsNoTracking().Where(x => x.DataAreaId == companyId && (itemId == "" || x.ItemId == itemId) && x.InventTransIdReturn != "")
                .ToDictionaryAsync(x => x.InventTransId, x => x.InventTransIdReturn, ct);
            var vendorReturns = await Db.Set<PurchLine>().AsNoTracking().Where(x => x.DataAreaId == companyId && (itemId == "" || x.ItemId == itemId) && x.InventTransIdReturn != "")
                .ToDictionaryAsync(x => x.InventTransId, x => x.InventTransIdReturn, ct);

            var voucher = (await _numbers.NextAsync("InventClosing", companyId, ct)).Code;
            var settlementsCreated = 0;
            var issuesSettled = 0;
            var ledgerEntriesCreated = 0;
            var totalAdjustment = 0m;

            foreach (var pool in pools)
            {
                // FIFO layers from financially posted receipts, oldest first.
                var layers = new LinkedList<Layer>();
                foreach (var row in pool.Rows)
                {
                    var trans = row.Transaction;
                    var inventTransId = row.InventTransId;
                    if (trans.Qty > 0 && trans.StatusReceipt == StatusReceipt.Purchased)
                    {
                        var receiptValue = trans.CostAmountPosted + trans.CostAmountAdjustment;
                        if (salesReturns.TryGetValue(inventTransId, out var soldId))
                        {
                            var sold = pool.Rows.Where(x => x.InventTransId == soldId && x.Transaction.Qty < 0).Select(x => x.Transaction).ToList();
                            if (sold.Count == 0 || sold.Any(x => x.ValueOpen != InventTransOpen.No))
                                throw new PostingValidationException("A customer return requires its original financially settled sales issue in the same pool.");
                            var targetReceipt = trans.Qty * sold.Sum(x => x.CostAmountPosted + x.CostAmountAdjustment) / sold.Sum(x => x.Qty);
                            var returnAdjustment = targetReceipt - receiptValue;
                            if (returnAdjustment != 0)
                            {
                                trans.CostAmountAdjustment += returnAdjustment;
                                await AdjustInventorySummaryAsync(companyId, trans.ItemId, trans.InventDimId, returnAdjustment, ct);
                                await CreateLedgerCorrectionAsync(companyId, voucher, trans, inventTransId, row.Category, pool.ItemGroupId, returnAdjustment, context, ct);
                                ledgerEntriesCreated++;
                                totalAdjustment += returnAdjustment;
                                receiptValue = targetReceipt;
                            }
                        }
                        if (receiptValue <= 0)
                            throw new PostingValidationException(
                                $"Receipt transaction {trans.RecId} has no positive financial cost.");
                        if (trans.Qty > trans.QtySettled)
                            layers.AddLast(new Layer { Receipt = trans, Remaining = trans.Qty - trans.QtySettled, UnitCost = receiptValue / trans.Qty });
                        continue;
                    }
                    if (trans.Qty >= 0 || trans.StatusIssue != StatusIssue.Sold)
                        throw new PostingValidationException(
                            $"Transaction {trans.RecId} has an unsupported financial quantity/status combination for close.");

                    if (trans.QtySettled == trans.Qty) continue;
                    if (trans.QtySettled != 0) throw new PostingValidationException("Partially settled legacy issues require reconciliation before close.");
                    var needed = -trans.Qty;
                    var fifoCost = 0m;
                    while (needed > 0 && layers.Count > 0)
                    {
                        var node = layers.First!;
                        if (vendorReturns.TryGetValue(inventTransId, out var purchaseId))
                        {
                            var receiptIds = pool.Rows.Where(x => x.InventTransId == purchaseId && x.Transaction.Qty > 0).Select(x => x.Transaction.RecId).ToHashSet();
                            while (node != null && !receiptIds.Contains(node.Value.Receipt.RecId)) node = node.Next;
                            if (node == null) throw new PostingValidationException("Vendor return exceeds the open quantity of its original purchase receipt.");
                        }
                        var layer = node.Value;
                        var quantity = Math.Min(needed, layer.Remaining);
                        var layerCost = quantity * layer.UnitCost;
                        fifoCost += layerCost;
                        needed -= quantity;
                        layer.Receipt.QtySettled += quantity;
                        layer.Remaining -= quantity;
                        if (layer.Remaining <= 0)
                        {
                            if (layer.Receipt.Qty - layer.Receipt.QtySettled <= 0)
                                layer.Receipt.ValueOpen = InventTransOpen.No;
                            layers.Remove(node);
                        }
                        settlementsCreated++;
                        Db.Add(new InventSettlement
                        {
                            DataAreaId = companyId, SettleTransId = (await _numbers.NextAsync("SettleTransId", companyId, ct)).Code,
                            InventTransId = inventTransId, ItemId = trans.ItemId, ItemGroupId = pool.ItemGroupId,
                            Voucher = voucher, TransDate = cutoffDate, TransRecId = trans.RecId,
                            ReceiptTransRecId = layer.Receipt.RecId,
                            QtySettled = -quantity, CostAmountSettled = -layerCost, CostAmountAdjustment = 0m,
                            SettleType = InventSettleType.FinancialClose, SettleModel = InventModel.FIFO,
                            Posted = NoYes.Yes, TransBeginTime = DateTime.UtcNow
                        });
                    }
                    if (needed > 0)
                        throw new PostingValidationException(
                            $"Issue transaction {trans.RecId} lacks {needed} financially received units in its FIFO pool.");

                    // target is the negative FIFO cost of the issued quantity.
                    var target = -fifoCost;
                    var recognized = trans.CostAmountPosted + trans.CostAmountAdjustment;
                    var adjustment = target - recognized;
                    trans.QtySettled = trans.Qty;
                    trans.CostAmountAdjustment += adjustment;
                    trans.CostAmountSettled = target;
                    trans.ValueOpen = InventTransOpen.No;
                    trans.DateClosed = cutoffDate;
                    issuesSettled++;
                    totalAdjustment += adjustment;

                    if (adjustment != 0m)
                    {
                        // FIFO close moves value between COGS and remaining inventory.
                        // adjustment d>0 => provisional COGS overstated: credit COGS, debit inventory.
                        await AdjustInventorySummaryAsync(companyId, trans.ItemId, trans.InventDimId, adjustment, ct);
                        await CreateLedgerCorrectionAsync(companyId, voucher, trans, inventTransId, row.Category, pool.ItemGroupId, adjustment, context, ct);
                        ledgerEntriesCreated++;
                    }
                }
            }

            var closing = new InventClosing
            {
                DataAreaId = companyId, Voucher = voucher, TransDate = cutoffDate,
                ScopeItemId = itemId,
                PeriodCode = Truncate(context.Period.Name, FieldLengths.PeriodCode),
                RunNum = runNum, NextRunNum = runNum + 1,
                AdjustmentType = InventAdjustmentType.Closing, AdjustmentSpec = InventAdjustmentSpec.Total,
                InventCostStatus = InventCostStatus.Finished, Executed = DateTime.UtcNow,
                Active = NoYes.Yes, Ledger = NoYes.Yes, Start_ = NoYes.Yes, End_ = NoYes.Yes,
                Notes = $"FIFO close run {runNum} by {_user.GetCurrentUserId()}"
            };
            Db.Add(closing);
            await _unitOfWork.CompleteAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
            return (new InventCloseResultDto(closing.RecId, voucher, cutoffDate, runNum,
                issuesSettled, settlementsCreated, ledgerEntriesCreated, totalAdjustment), null);
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

    private async Task<List<Pool>> LoadFifoPoolsAsync(string companyId, DateTime cutoffDate, string itemId, CancellationToken ct)
    {
        var end = cutoffDate.AddDays(1);
        var transactions = (await Db.Set<InventTrans>()
            .Where(x => x.DataAreaId == companyId && (itemId == "" || x.ItemId == itemId) && x.DateFinancial < end &&
                (x.StatusReceipt == StatusReceipt.Purchased || x.StatusIssue == StatusIssue.Sold))
            .ToListAsync(ct))
            .OrderBy(x => x.DateFinancial).ThenBy(x => x.RecId).ToList();
        if (transactions.Count == 0) return [];

        var dimensionIds = transactions.Select(x => x.InventDimId).Distinct().ToList();
        var dimensions = await Db.Set<InventDim>().AsNoTracking()
            .Where(x => x.DataAreaId == companyId && dimensionIds.Contains(x.InventDimId))
            .ToDictionaryAsync(x => x.InventDimId, ct);
        var originIds = transactions.Select(x => x.InventTransOrigin).Distinct().ToList();
        var origins = await Db.Set<InventTransOrigin>().AsNoTracking()
            .Where(x => x.DataAreaId == companyId && originIds.Contains(x.RecId))
            .ToDictionaryAsync(x => x.RecId, ct);
        var transferLines = await Db.Set<InventJournalTrans>().AsNoTracking()
            .Where(x => x.DataAreaId == companyId && x.JournalType == InventJournalType.Transfer)
            .ToListAsync(ct);
        var transferSet = transferLines.SelectMany(x => new[] { x.InventTransId, x.ToInventTransId }).Where(x => x.Length > 0).ToHashSet(StringComparer.Ordinal);
        foreach (var transfer in transferLines)
        {
            var pair = transactions.Where(x => origins.TryGetValue(x.InventTransOrigin, out var o)
                && (o.InventTransId == transfer.InventTransId || o.InventTransId == transfer.ToInventTransId)).ToList();
            if (pair.Count == 0) continue;
            if (pair.Count != 2 || pair.Sum(x => x.Qty) != 0 || pair.Sum(x => x.CostAmountPosted + x.CostAmountAdjustment) != 0
                || pair.Select(x => x.ItemId).Distinct().Count() != 1 || pair.Any(x => !dimensions.ContainsKey(x.InventDimId))
                || pair.Select(x => dimensions[x.InventDimId].InventSiteId).Distinct().Count() != 1
                || pair.Count(x => x.Qty < 0 && x.StatusIssue == StatusIssue.Sold) != 1
                || pair.Count(x => x.Qty > 0 && x.StatusReceipt == StatusReceipt.Purchased) != 1)
                throw new PostingValidationException("A transfer must have balanced financial issue/receipt legs in the same item/site pool before close.");
        }
        var assignments = await Db.Set<InventModelGroupItem>().AsNoTracking()
            .Where(x => x.ItemDataAreaId == companyId).ToListAsync(ct);
        var models = await Db.Set<ItemModelGroup>().AsNoTracking()
            .Where(x => x.DataAreaId == companyId).ToDictionaryAsync(x => x.ModelGroupId, ct);
        var itemGroups = (await Db.Set<InventItemGroupItem>().AsNoTracking()
            .Where(x => x.ItemDataAreaId == companyId).ToListAsync(ct))
            .GroupBy(x => x.ItemId).ToDictionary(x => x.Key, x => x.First().ItemGroupId);

        var rows = new List<(InventTrans Transaction, string Site, string ItemGroupId, string InventTransId, InventRefType Category)>();
        foreach (var transaction in transactions)
        {
            if (!dimensions.TryGetValue(transaction.InventDimId, out var dim) || string.IsNullOrWhiteSpace(dim.InventSiteId))
                throw new PostingValidationException($"Transaction {transaction.RecId} has no valid financial site dimension.");
            if (origins.TryGetValue(transaction.InventTransOrigin, out var transferOrigin) && transferSet.Contains(transferOrigin.InventTransId))
                continue; // Verified internal movement does not consume or create a site-level FIFO layer.
            var sourceSupported = origins.TryGetValue(transaction.InventTransOrigin, out var origin)
                && (origin.ReferenceCategory == InventRefType.InventoryJournal
                    || origin.ReferenceCategory == InventRefType.PhysicalinventoryCount
                    || origin.ReferenceCategory == InventRefType.SalesTable
                    || origin.ReferenceCategory == InventRefType.PurchaseOrder)
                && !transferSet.Contains(origin.InventTransId);
            if (!sourceSupported)
                throw new PostingValidationException(
                    $"Transaction {transaction.RecId} needs a verified journal or sales source before close.");
            var assignment = assignments.FirstOrDefault(x => x.ItemId == transaction.ItemId);
            if (assignment is null || !models.TryGetValue(assignment.ModelGroupId, out var model) || model.InventModel != InventModel.FIFO)
                throw new PostingValidationException($"Item {transaction.ItemId} does not have a verified FIFO model group.");
            rows.Add((transaction, dim.InventSiteId,
                itemGroups.GetValueOrDefault(transaction.ItemId, string.Empty), origin!.InventTransId, origin.ReferenceCategory));
        }

        return rows.GroupBy(x => (x.Transaction.ItemId, x.Site))
            .Select(g => new Pool(g.Key.ItemId, g.Key.Site, g.First().ItemGroupId,
                g.OrderBy(x => x.Transaction.DateFinancial).ThenBy(x => x.Transaction.RecId)
                    .Select(x => (x.Transaction, x.InventTransId, x.Category)).ToList()))
            .ToList();
    }

    private async Task AdjustInventorySummaryAsync(string companyId, string itemId, string dimId, decimal value, CancellationToken ct)
    {
        var sum = await Db.Set<InventSum>().FirstOrDefaultAsync(
            x => x.DataAreaId == companyId && x.ItemId == itemId && x.InventDimId == dimId, ct);
        if (sum == null) return;
        sum.PostedValue += value;
        sum.LastUpdDatePhysical = DateTime.UtcNow;
    }

    private async Task CreateLedgerCorrectionAsync(string companyId, string voucher, InventTrans issue,
        string inventTransId, InventRefType category, string itemGroupId, decimal adjustment, LedgerContext context, CancellationToken ct)
    {
        // The adjustment follows the issue's original posting: sales issues use the sales inventory-issue
        // and cost-of-units-invoiced accounts; journal issues keep the inventory journal accounts.
        var isSale = category == InventRefType.SalesTable;
        var inventoryType = isSale ? InventAccountType.SalesIssue : InventAccountType.InventReceipt;
        var costType = isSale ? InventAccountType.SalesConsump : InventAccountType.InventIssue;
        var inventory = await Support.ResolveInventPostingAsync(companyId, issue.ItemId, inventoryType, ct, itemGroupId);
        var cogs = await Support.ResolveInventPostingAsync(companyId, issue.ItemId, costType, ct, itemGroupId);
        var journalLine = category is InventRefType.InventoryJournal or InventRefType.PhysicalinventoryCount
            ? await Db.Set<InventJournalTrans>().AsNoTracking().SingleOrDefaultAsync(x => x.DataAreaId == companyId && x.InventTransId == inventTransId, ct)
            : null;
        if (journalLine != null)
        {
            inventory = await Support.ResolveInventPostingAsync(companyId, issue.ItemId, InventAccountType.InventIssue, ct, itemGroupId);
            cogs = journalLine.JournalType == InventJournalType.Movement
                ? await Db.Set<DimensionAttributeValueCombination>().SingleAsync(x => x.DataAreaId == companyId && x.RecId == journalLine.LedgerDimension, ct)
                : await Support.ResolveInventPostingAsync(companyId, issue.ItemId, InventAccountType.InventLoss, ct, itemGroupId);
        }
        var entry = Support.NewEntry(companyId, voucher, voucher, context.AccountingDate,
            new SubledgerLedgerContext(context.Ledger, context.Period));
        Db.Add(entry);
        await _unitOfWork.CompleteAsync(ct);
        var text = $"FIFO close adjustment for issue {inventTransId}";
        var currency = context.Ledger.AccountingCurrency;
        Db.AddRange(
            SubledgerPostingSupport.AccountEntry(companyId, entry.RecId, inventory, isSale ? LedgerPostingType.SalesIssue : journalLine != null ? LedgerPostingType.InventIssue : LedgerPostingType.InventReceipt, adjustment, text, issue.Qty, currency),
            SubledgerPostingSupport.AccountEntry(companyId, entry.RecId, cogs, isSale ? LedgerPostingType.SalesConsump : journalLine != null ? (journalLine.JournalType == InventJournalType.Movement ? LedgerPostingType.LedgerJournal : LedgerPostingType.InventLoss) : LedgerPostingType.InventIssue, -adjustment, text, issue.Qty, currency));
        Db.Add(new SubledgerVoucherGeneralJournalEntry
        {
            DataAreaId = companyId, GeneralJournalEntry = entry.RecId, Voucher = voucher,
            VoucherDataAreaId = companyId, AccountingDate = context.AccountingDate
        });
    }

    private async Task<LedgerContext> LoadLedgerContextAsync(string companyId, DateTime date, CancellationToken ct)
    {
        var context = await Support.LoadLedgerContextAsync(companyId, date, ct);
        if (!string.IsNullOrWhiteSpace(context.Ledger.ReportingCurrency) &&
            !string.Equals(context.Ledger.ReportingCurrency, context.Ledger.AccountingCurrency, StringComparison.OrdinalIgnoreCase))
            throw new PostingValidationException("Inventory close with a different reporting currency requires exchange-rate processing, which is not enabled in the basic inventory scope.");
        return new LedgerContext(context.Ledger, context.Period, date);
    }

    private static string Truncate(string value, int max) =>
        string.IsNullOrEmpty(value) || value.Length <= max ? value : value[..max];

    private sealed class Layer
    {
        public required InventTrans Receipt { get; init; }
        public decimal Remaining { get; set; }
        public decimal UnitCost { get; init; }
    }

    private sealed record Pool(string ItemId, string Site, string ItemGroupId,
        List<(InventTrans Transaction, string InventTransId, InventRefType Category)> Rows);

    private sealed record LedgerContext(Ledger Ledger, FiscalCalendarPeriod Period, DateTime AccountingDate);
}
