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
/// Basic inventory cost settlement: deterministic settlement of financially posted issues against
/// financially posted receipts within an item/financial-site pool, cost adjustment of each issue to
/// its settled cost, settlement history, a ledger correction, and - for a close - a financial
/// boundary. Supports journal, purchase and sales sources. Verified same-site transfers conserve
/// the pool and do not consume layers.
///
/// Two runs share this engine, as in D365:
/// <list type="bullet">
/// <item>Close (<see cref="InventAdjustmentType.Closing"/>) finishes the settled quantity: it marks
/// transactions closed and establishes a financial boundary that blocks earlier postings.</item>
/// <item>Recalculation (<see cref="InventAdjustmentType.Adjustment"/>) performs the same cost
/// adjustment but leaves transactions open and sets no boundary, so it can be repeated and a later
/// close still finishes the period.</item>
/// </list>
/// Supported inventory models are FIFO, LIFO and Weighted average. Either run can be cancelled -
/// see InventCloseService.Cancel.cs.
/// </summary>
public sealed partial class InventCloseService : IInventCloseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICompanyExecutionContext _company;
    private readonly ICurrentUserService _user;
    private readonly ISysNumberSequenceService _numbers;
    private DbContext Db => _unitOfWork.Context;
    private SubledgerPostingSupport Support => new(Db);

    /// <summary>The inventory models this engine settles. Other models are rejected, not approximated.</summary>
    private static readonly InventModel[] SupportedModels =
        [InventModel.FIFO, InventModel.LIFO, InventModel.WeightedAvg];

    public InventCloseService(IUnitOfWork unitOfWork, ICompanyExecutionContext company,
        ICurrentUserService user, ISysNumberSequenceService numbers)
    {
        _unitOfWork = unitOfWork;
        _company = company;
        _user = user;
        _numbers = numbers;
    }

    public Task<(InventCloseResultDto? Result, string? Error)> CloseAsync(DateTime cutoff, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() =>
            RunCoreAsync(cutoff, string.Empty, InventAdjustmentType.Closing, ct));

    public Task<(InventCloseResultDto? Result, string? Error)> CloseItemAsync(string itemId, DateTime cutoff, CancellationToken ct = default) =>
        string.IsNullOrWhiteSpace(itemId)
            ? Task.FromResult<(InventCloseResultDto?, string?)>((null, "An item is required for scoped close."))
            : Db.Database.CreateExecutionStrategy().ExecuteAsync(() =>
                RunCoreAsync(cutoff, itemId.Trim(), InventAdjustmentType.Closing, ct));

    public Task<(InventCloseResultDto? Result, string? Error)> RecalculateAsync(DateTime cutoff, CancellationToken ct = default) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() =>
            RunCoreAsync(cutoff, string.Empty, InventAdjustmentType.Adjustment, ct));

    public Task<(InventCloseResultDto? Result, string? Error)> RecalculateItemAsync(string itemId, DateTime cutoff, CancellationToken ct = default) =>
        string.IsNullOrWhiteSpace(itemId)
            ? Task.FromResult<(InventCloseResultDto?, string?)>((null, "An item is required for scoped recalculation."))
            : Db.Database.CreateExecutionStrategy().ExecuteAsync(() =>
                RunCoreAsync(cutoff, itemId.Trim(), InventAdjustmentType.Adjustment, ct));

    private async Task<(InventCloseResultDto? Result, string? Error)> RunCoreAsync(DateTime cutoff, string itemId,
        InventAdjustmentType runType, CancellationToken ct)
    {
        var companyId = _company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(companyId)) return (null, "A company must be selected.");
        var isClose = runType == InventAdjustmentType.Closing;
        var cutoffDate = cutoff.Date;
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            await Support.LockAsync($"InventClose:{companyId}", ct);

            // Run numbering spans both run types, so history reads as one ordered ledger of cost runs.
            var history = await Db.Set<InventClosing>()
                .Where(x => x.DataAreaId == companyId && x.CancelClosing == NoYes.No)
                .ToListAsync(ct);
            var runNum = history.Count == 0 ? 1 : history.Max(x => x.RunNum) + 1;
            var live = history
                .Where(x => x.Cancellation == NoYes.No && x.InventCostStatus == InventCostStatus.Finished)
                .Where(x => itemId.Length == 0 || x.ScopeItemId == itemId || x.ScopeItemId.Length == 0)
                .ToList();

            if (isClose)
            {
                // A repeated close for the same scope and cutoff is a no-operation, never a second settlement.
                var existing = live
                    .Where(x => x.AdjustmentType == InventAdjustmentType.Closing &&
                        x.ScopeItemId == itemId && x.TransDate.Date == cutoffDate)
                    .OrderByDescending(x => x.RunNum).FirstOrDefault();
                if (existing != null)
                {
                    await _unitOfWork.CommitTransactionAsync(ct);
                    return (new InventCloseResultDto(existing.RecId, existing.Voucher, cutoffDate,
                        existing.RunNum, InventAdjustmentType.Closing, 0, 0, 0, 0m), null);
                }
                if (live.Any(x => x.AdjustmentType == InventAdjustmentType.Closing &&
                        (x.TransDate.Date > cutoffDate || (x.TransDate.Date == cutoffDate && x.ScopeItemId.Length == 0))))
                    throw new PostingValidationException(
                        "A later or equal inventory close already exists; backdated close is not supported in the basic scope.");
            }
            else if (live.Any(x => x.AdjustmentType == InventAdjustmentType.Closing && x.TransDate.Date >= cutoffDate))
                throw new PostingValidationException(
                    "The period is already closed through this date; recalculation can only cover an open period.");

            var context = await LoadLedgerContextAsync(companyId, cutoffDate, ct);
            var pools = await LoadPoolsAsync(companyId, cutoffDate, itemId, ct);
            if (itemId.Length > 0 && pools.Count == 0)
                throw new PostingValidationException("The selected item has no supported financial inventory to settle.");
            var salesReturns = await Db.Set<SalesLine>().AsNoTracking().Where(x => x.DataAreaId == companyId && (itemId == "" || x.ItemId == itemId) && x.InventTransIdReturn != "")
                .ToDictionaryAsync(x => x.InventTransId, x => x.InventTransIdReturn, ct);
            var vendorReturns = await Db.Set<PurchLine>().AsNoTracking().Where(x => x.DataAreaId == companyId && (itemId == "" || x.ItemId == itemId) && x.InventTransIdReturn != "")
                .ToDictionaryAsync(x => x.InventTransId, x => x.InventTransIdReturn, ct);

            var voucher = (await _numbers.NextAsync("InventClosing", companyId, ct)).Code;
            var run = new RunTotals();

            foreach (var pool in pools)
                await SettlePoolAsync(companyId, pool, isClose, voucher, cutoffDate, context,
                    salesReturns, vendorReturns, run, ct);

            var closing = new InventClosing
            {
                DataAreaId = companyId, Voucher = voucher, TransDate = cutoffDate,
                ScopeItemId = itemId,
                PeriodCode = Truncate(context.Period.Name, FieldLengths.PeriodCode),
                RunNum = runNum, NextRunNum = runNum + 1,
                AdjustmentType = runType,
                AdjustmentSpec = itemId.Length == 0 ? InventAdjustmentSpec.Total : InventAdjustmentSpec.Item,
                RunRecalculation = isClose ? NoYes.No : NoYes.Yes,
                InventCostStatus = InventCostStatus.Finished, Executed = DateTime.UtcNow,
                Active = NoYes.Yes, Ledger = NoYes.Yes, Start_ = NoYes.Yes, End_ = NoYes.Yes,
                Notes = $"{(isClose ? "Close" : "Recalculation")} run {runNum} by {_user.GetCurrentUserId()}"
            };
            Db.Add(closing);
            await _unitOfWork.CompleteAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
            return (new InventCloseResultDto(closing.RecId, voucher, cutoffDate, runNum, runType,
                run.IssuesSettled, run.SettlementsCreated, run.LedgerEntriesCreated, run.TotalAdjustment), null);
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

    /// <summary>
    /// Settles one item/financial-site pool. Receipts become layers first, so an issue settles
    /// against every financially posted receipt of the period and not only against those that
    /// happen to precede it; this is what makes a periodic close different from posting-time cost.
    /// </summary>
    private async Task SettlePoolAsync(string companyId, Pool pool, bool isClose, string voucher,
        DateTime cutoffDate, LedgerContext context, IReadOnlyDictionary<string, string> salesReturns,
        IReadOnlyDictionary<string, string> vendorReturns, RunTotals run, CancellationToken ct)
    {
        var layers = new LinkedList<Layer>();
        foreach (var row in pool.Rows.Where(x => x.Transaction.Qty > 0))
        {
            var trans = row.Transaction;
            if (trans.StatusReceipt != StatusReceipt.Purchased)
                throw new PostingValidationException(
                    $"Transaction {trans.RecId} has an unsupported financial quantity/status combination for settlement.");
            var receiptValue = trans.CostAmountPosted + trans.CostAmountAdjustment;

            // A customer return re-enters stock at the cost its original sales issue carried.
            if (salesReturns.TryGetValue(row.InventTransId, out var soldId))
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
                    await CreateLedgerCorrectionAsync(companyId, voucher, trans, row.InventTransId, row.Category,
                        pool.ItemGroupId, returnAdjustment, context, ct);
                    run.LedgerEntriesCreated++;
                    run.TotalAdjustment += returnAdjustment;
                    // The adjustment is recorded so cancelling this run can reverse it exactly.
                    await AddSettlementAsync(companyId, voucher, cutoffDate, pool, trans, row.InventTransId,
                        null, 0m, 0m, returnAdjustment, isClose, ct);
                    run.SettlementsCreated++;
                    receiptValue = targetReceipt;
                }
            }
            if (receiptValue <= 0)
                throw new PostingValidationException($"Receipt transaction {trans.RecId} has no positive financial cost.");
            var openReceipt = trans.Qty - trans.QtySettled;
            if (openReceipt > 0)
                layers.AddLast(new Layer { Receipt = trans, Remaining = openReceipt, UnitCost = receiptValue / trans.Qty });
        }

        // Weighted average costs every issue of the period at one pool average, so it is taken
        // before any consumption changes the open layers.
        var poolQty = layers.Sum(x => x.Remaining);
        var averageCost = pool.Model == InventModel.WeightedAvg && poolQty > 0
            ? layers.Sum(x => x.Remaining * x.UnitCost) / poolQty
            : 0m;

        foreach (var row in pool.Rows.Where(x => x.Transaction.Qty < 0))
        {
            var trans = row.Transaction;
            if (trans.StatusIssue != StatusIssue.Sold)
                throw new PostingValidationException(
                    $"Transaction {trans.RecId} has an unsupported financial quantity/status combination for settlement.");

            // Settle whatever quantity is still open, so a partially settled issue is completed
            // rather than rejected.
            var needed = -(trans.Qty - trans.QtySettled);
            if (needed <= 0)
            {
                if (isClose && trans.ValueOpen != InventTransOpen.No) FinishIssue(trans, cutoffDate);
                continue;
            }
            var settledQty = needed;
            var consumed = new List<(Layer Layer, decimal Qty)>();
            while (needed > 0 && layers.Count > 0)
            {
                var node = pool.Model == InventModel.LIFO ? layers.Last! : layers.First!;
                if (trans.MarkingRefInventTransOrigin != 0)
                {
                    // A marked issue takes its cost from the receipt it is marked to, ahead of the
                    // inventory model. Any quantity left over after that receipt is exhausted
                    // follows the model order on the next pass.
                    var marked = layers.First;
                    while (marked != null && marked.Value.Receipt.InventTransOrigin != trans.MarkingRefInventTransOrigin)
                        marked = marked.Next;
                    if (marked != null) node = marked;
                }
                if (vendorReturns.TryGetValue(row.InventTransId, out var purchaseId))
                {
                    // A vendor return must consume the very receipt it sends back.
                    var receiptIds = pool.Rows.Where(x => x.InventTransId == purchaseId && x.Transaction.Qty > 0)
                        .Select(x => x.Transaction.RecId).ToHashSet();
                    while (node != null && !receiptIds.Contains(node.Value.Receipt.RecId))
                        node = pool.Model == InventModel.LIFO ? node.Previous : node.Next;
                    if (node == null) throw new PostingValidationException("Vendor return exceeds the open quantity of its original purchase receipt.");
                }
                var layer = node.Value;
                var quantity = Math.Min(needed, layer.Remaining);
                needed -= quantity;
                layer.Receipt.QtySettled += quantity;
                layer.Remaining -= quantity;
                consumed.Add((layer, quantity));
                if (layer.Remaining <= 0)
                {
                    if (layer.Receipt.Qty - layer.Receipt.QtySettled <= 0)
                        layer.Receipt.ValueOpen = InventTransOpen.No;
                    layers.Remove(node);
                }
            }
            if (needed > 0)
                throw new PostingValidationException(
                    $"Issue transaction {trans.RecId} lacks {needed} financially received units in its {pool.Model} pool.");

            // The settled cost of the issued quantity, by inventory model.
            var settledCost = pool.Model == InventModel.WeightedAvg
                ? decimal.Round(averageCost * settledQty, 2, MidpointRounding.AwayFromZero)
                : consumed.Sum(x => x.Qty * x.Layer.UnitCost);
            var target = -settledCost;

            // The provisional cost carried by exactly the quantity settled in this run.
            var provisional = trans.CostAmountPosted * (settledQty / -trans.Qty);
            var adjustment = target - provisional;

            var allocated = 0m;
            for (var i = 0; i < consumed.Count; i++)
            {
                var (layer, quantity) = consumed[i];
                var layerCost = pool.Model == InventModel.WeightedAvg
                    ? (i == consumed.Count - 1 ? settledCost - allocated
                        : decimal.Round(averageCost * quantity, 2, MidpointRounding.AwayFromZero))
                    : quantity * layer.UnitCost;
                allocated += layerCost;
                await AddSettlementAsync(companyId, voucher, cutoffDate, pool, trans, row.InventTransId,
                    layer.Receipt.RecId, -quantity, -layerCost,
                    adjustment * (quantity / settledQty), isClose, ct);
                run.SettlementsCreated++;
            }

            trans.QtySettled += -settledQty;
            trans.CostAmountAdjustment += adjustment;
            trans.CostAmountSettled += target;
            if (isClose) FinishIssue(trans, cutoffDate);
            run.IssuesSettled++;
            run.TotalAdjustment += adjustment;

            if (adjustment != 0m)
            {
                // Settlement moves value between cost of goods sold and remaining inventory.
                await AdjustInventorySummaryAsync(companyId, trans.ItemId, trans.InventDimId, adjustment, ct);
                await CreateLedgerCorrectionAsync(companyId, voucher, trans, row.InventTransId, row.Category,
                    pool.ItemGroupId, adjustment, context, ct);
                run.LedgerEntriesCreated++;
            }
        }
    }

    /// <summary>A close finishes the issue; a recalculation leaves it open for a later run.</summary>
    private static void FinishIssue(InventTrans issue, DateTime cutoffDate)
    {
        issue.ValueOpen = InventTransOpen.No;
        issue.DateClosed = cutoffDate;
    }

    private async Task AddSettlementAsync(string companyId, string voucher, DateTime cutoffDate, Pool pool,
        InventTrans trans, string inventTransId, long? receiptRecId, decimal qtySettled, decimal costSettled,
        decimal adjustment, bool isClose, CancellationToken ct)
    {
        Db.Add(new InventSettlement
        {
            DataAreaId = companyId,
            SettleTransId = (await _numbers.NextAsync("SettleTransId", companyId, ct)).Code,
            InventTransId = inventTransId, ItemId = trans.ItemId, ItemGroupId = pool.ItemGroupId,
            Voucher = voucher, TransDate = cutoffDate, TransRecId = trans.RecId,
            ReceiptTransRecId = receiptRecId,
            QtySettled = qtySettled, CostAmountSettled = costSettled, CostAmountAdjustment = adjustment,
            SettleType = isClose ? InventSettleType.FinancialClose : InventSettleType.RecalculationAdjustment,
            SettleModel = pool.Model,
            Posted = NoYes.Yes, Cancelled = NoYes.No, TransBeginTime = DateTime.UtcNow
        });
    }

    private async Task<List<Pool>> LoadPoolsAsync(string companyId, DateTime cutoffDate, string itemId, CancellationToken ct)
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
        var transferOrderTransactions = transactions.Where(x => origins.TryGetValue(x.InventTransOrigin, out var origin)
            && origin.ReferenceCategory == InventRefType.TransferOrder).ToList();
        foreach (var movement in transferOrderTransactions.GroupBy(x =>
            (origins[x.InventTransOrigin].ReferenceId, x.Voucher, x.ItemId)))
        {
            var legs = movement.ToList();
            if (string.IsNullOrWhiteSpace(movement.Key.ReferenceId) || string.IsNullOrWhiteSpace(movement.Key.Voucher)
                || legs.Count < 2 || legs.Count % 2 != 0
                || legs.Count(x => x.Qty < 0 && x.StatusIssue == StatusIssue.Sold) != legs.Count / 2
                || legs.Count(x => x.Qty > 0 && x.StatusReceipt == StatusReceipt.Purchased) != legs.Count / 2
                || legs.Sum(x => x.Qty) != 0
                || legs.Sum(x => x.CostAmountPosted + x.CostAmountAdjustment) != 0
                || legs.Any(x => !dimensions.ContainsKey(x.InventDimId))
                || legs.Select(x => dimensions[x.InventDimId].InventSiteId).Distinct().Count() != 1)
                throw new PostingValidationException("A transfer order must have balanced financial issue/receipt legs in the same item/site pool before close.");
        }
        var assignments = await Db.Set<InventModelGroupItem>().AsNoTracking()
            .Where(x => x.ItemDataAreaId == companyId).ToListAsync(ct);
        var models = await Db.Set<ItemModelGroup>().AsNoTracking()
            .Where(x => x.DataAreaId == companyId).ToDictionaryAsync(x => x.ModelGroupId, ct);
        var itemGroups = (await Db.Set<InventItemGroupItem>().AsNoTracking()
            .Where(x => x.ItemDataAreaId == companyId).ToListAsync(ct))
            .GroupBy(x => x.ItemId).ToDictionary(x => x.Key, x => x.First().ItemGroupId);

        var rows = new List<(InventTrans Transaction, string Site, string ItemGroupId, string InventTransId, InventRefType Category, InventModel Model)>();
        foreach (var transaction in transactions)
        {
            if (!dimensions.TryGetValue(transaction.InventDimId, out var dim) || string.IsNullOrWhiteSpace(dim.InventSiteId))
                throw new PostingValidationException($"Transaction {transaction.RecId} has no valid financial site dimension.");
            if (origins.TryGetValue(transaction.InventTransOrigin, out var transferOrigin) && transferSet.Contains(transferOrigin.InventTransId))
                continue; // Verified internal movement does not consume or create a site-level layer.
            if (origins.TryGetValue(transaction.InventTransOrigin, out var orderOrigin)
                && orderOrigin.ReferenceCategory == InventRefType.TransferOrder)
                continue; // The balanced transfer-order movement was verified above.
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
            if (assignment is null || !models.TryGetValue(assignment.ModelGroupId, out var model))
                throw new PostingValidationException($"Item {transaction.ItemId} does not have a verified item model group.");
            if (!SupportedModels.Contains(model.InventModel))
                throw new PostingValidationException(
                    $"Inventory model '{model.InventModel}' on item '{transaction.ItemId}' is not settled by this engine. " +
                    $"Supported models are {string.Join(", ", SupportedModels)}.");
            rows.Add((transaction, dim.InventSiteId,
                itemGroups.GetValueOrDefault(transaction.ItemId, string.Empty), origin!.InventTransId,
                origin.ReferenceCategory, model.InventModel));
        }

        return rows.GroupBy(x => (x.Transaction.ItemId, x.Site))
            .Select(g => new Pool(g.Key.ItemId, g.Key.Site, g.First().ItemGroupId, g.First().Model,
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
        var text = $"Inventory cost settlement adjustment for issue {inventTransId}";
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
        var context = await Support.LoadLedgerContextAsync(companyId, date,
            LedgerPostingModule.Inventory, _user.GetCurrentUserId(), ct);
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

    private sealed class RunTotals
    {
        public int IssuesSettled;
        public int SettlementsCreated;
        public int LedgerEntriesCreated;
        public decimal TotalAdjustment;
    }

    private sealed record Pool(string ItemId, string Site, string ItemGroupId, InventModel Model,
        List<(InventTrans Transaction, string InventTransId, InventRefType Category)> Rows);

    private sealed record LedgerContext(Ledger Ledger, FiscalCalendarPeriod Period, DateTime AccountingDate);
}
