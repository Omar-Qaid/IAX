using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory.ProductLifecycle;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;
using ReleasedItem = IAX.IXApi.Modules.Finance.Entities.InventTable;
using ItemModelGroup = IAX.IXApi.Modules.Finance.Entities.InventModelGroup;

namespace IAX.IXApi.Modules.Finance.Inventory.Transfer;

/// <summary>
/// Transfer orders as a document lifecycle. Shipment moves stock out of the source warehouse and
/// into transit; receipt moves it out of transit and into the destination. Each of the four legs
/// owns its own inventory transaction, recorded on the line, so source, transit and destination
/// always reconcile and stock left in transit at period end is visible as transit stock.
///
/// A shipment is not an ordinary negative journal line: no inventory journal is created here.
/// The movements go through the shared inventory movement component, so on-hand balances always
/// come from posted inventory transactions.
/// </summary>
public sealed class InventTransferOrderService(
    IUnitOfWork unitOfWork,
    ICompanyExecutionContext company,
    IInventDimensionResolver dimensions,
    ISalesInventoryNumberService inventoryNumbers) : IInventTransferOrderService
{
    private DbContext Db => unitOfWork.Context;
    private InventoryMovement Movement => new(unitOfWork, inventoryNumbers);

    public Task<(InventTransferOrderDto? Order, string? Error)> CreateAsync(InventTransferCreateDto input, CancellationToken ct) =>
        ExecuteAsync(area => CreateCoreAsync(area, input, ct), ct);

    public Task<(InventTransferOrderDto? Order, string? Error)> ShipAsync(string transferId, InventTransferPostDto input, CancellationToken ct) =>
        ExecuteAsync(area => PostCoreAsync(area, transferId, input, receive: false, ct), ct);

    public Task<(InventTransferOrderDto? Order, string? Error)> ReceiveAsync(string transferId, InventTransferPostDto input, CancellationToken ct) =>
        ExecuteAsync(area => PostCoreAsync(area, transferId, input, receive: true, ct), ct);

    public async Task<(InventTransferOrderDto? Order, string? Error)> GetAsync(string transferId, CancellationToken ct)
    {
        var area = company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(area)) return (null, "A company must be selected.");
        var header = await Db.Set<InventTransferTable>().AsNoTracking()
            .SingleOrDefaultAsync(x => x.DataAreaId == area && x.TransferId == transferId.Trim(), ct);
        return header == null ? (null, null) : (await MapAsync(area, header, ct), null);
    }

    private async Task<InventTransferTable> CreateCoreAsync(string area, InventTransferCreateDto input, CancellationToken ct)
    {
        var transferId = (input.TransferId ?? string.Empty).Trim();
        if (transferId.Length == 0 || transferId.Length > FieldLengths.TransferId)
            throw new InvalidOperationException($"A transfer identifier of at most {FieldLengths.TransferId} characters is required.");
        if (input.Lines == null || input.Lines.Count == 0)
            throw new InvalidOperationException("A transfer order needs at least one line.");
        await LockAsync(area, transferId, ct);

        if (await Db.Set<InventTransferTable>().AnyAsync(x => x.DataAreaId == area && x.TransferId == transferId, ct))
            throw new InvalidOperationException($"Transfer order '{transferId}' already exists.");

        var site = (input.InventSiteId ?? string.Empty).Trim();
        var from = (input.InventLocationIdFrom ?? string.Empty).Trim();
        var transit = (input.InventLocationIdTransit ?? string.Empty).Trim();
        var to = (input.InventLocationIdTo ?? string.Empty).Trim();
        if (new[] { from, transit, to }.Distinct(StringComparer.Ordinal).Count() != 3)
            throw new InvalidOperationException("Source, transit and destination must be distinct warehouses.");
        foreach (var (id, expected) in new[] { (from, InventLocationType.Standard), (transit, InventLocationType.Transit), (to, InventLocationType.Standard) })
        {
            var warehouse = await Db.Set<InventLocation>().AsNoTracking()
                .SingleOrDefaultAsync(x => x.DataAreaId == area && x.InventLocationId == id && x.InventSiteId == site, ct);
            if (warehouse == null || warehouse.InventLocationType != expected)
                throw new InvalidOperationException("A transfer needs standard source and destination warehouses and a transit warehouse in the same site.");
        }

        var shipDate = (input.ShipDate ?? DateTime.UtcNow).Date;
        var receiveDate = (input.ReceiveDate ?? shipDate).Date;
        if (receiveDate < shipDate) throw new InvalidOperationException("The receive date cannot precede the ship date.");

        var header = new InventTransferTable
        {
            DataAreaId = area, TransferId = transferId, TransferStatus = InventTransferStatus.Created,
            InventLocationIdFrom = from, InventLocationIdTransit = transit, InventLocationIdTo = to,
            ShipDate = shipDate, ReceiveDate = receiveDate
        };
        Db.Add(header);

        var lineNum = 0m;
        foreach (var line in input.Lines)
        {
            var itemId = (line.ItemId ?? string.Empty).Trim();
            var unitId = (line.UnitId ?? string.Empty).Trim();
            if (line.QtyTransfer <= 0 || decimal.Round(line.QtyTransfer, 4) != line.QtyTransfer)
                throw new InvalidOperationException($"Line for item '{itemId}' needs a positive transfer quantity.");
            if (!await Db.Set<ReleasedItem>().AsNoTracking().AnyAsync(x => x.DataAreaId == area && x.ItemId == itemId, ct)
                || !await Db.Set<InventTableModule>().AsNoTracking().AnyAsync(x => x.DataAreaId == area && x.ItemId == itemId
                    && x.ModuleType == ModuleInventPurchSales.Inventory && x.UnitId == unitId, ct))
                throw new InvalidOperationException($"Item '{itemId}' must be released with '{unitId}' as its inventory unit.");
            var lifecycleError = await ProductLifecyclePolicy.ValidateAsync(Db, area, itemId,
                ProductLifecycleProcess.Inventory, ct);
            if (lifecycleError != null) throw new InvalidOperationException(lifecycleError);
            await RequireStockedItemAsync(area, itemId, ct);
            await RequireUntrackedItemAsync(area, itemId, ct);
            // The line's dimension is the source: the transit and destination legs resolve their own.
            var dim = await dimensions.ResolveAsync(area, site, from, ct);
            Db.Add(new InventTransferLine
            {
                DataAreaId = area, TransferId = transferId, LineNum = ++lineNum, ItemId = itemId, UnitId = unitId,
                InventDimId = dim.InventDimId, QtyTransfer = line.QtyTransfer, QtyRemainShip = line.QtyTransfer,
                QtyRemainReceive = line.QtyTransfer, ShipDate = shipDate, ReceiveDate = receiveDate,
                RemainStatus = InventTransferRemainStatus.None
            });
        }
        return header;
    }

    /// <summary>
    /// A transfer order carries a site and warehouse dimension only. An item whose tracking
    /// dimension group requires a batch or serial number on physical inventory is therefore not
    /// transferable through this document, and saying so is better than recording stock under a
    /// blank batch that the inventory journal would have rejected.
    /// </summary>
    private async Task RequireUntrackedItemAsync(string area, string itemId, CancellationToken ct)
    {
        var group = await InventoryDimensionPolicy.TrackingGroupAsync(Db, area, itemId, ct);
        if (group is null)
            throw new InvalidOperationException($"A tracking dimension group is required for item '{itemId}'.");
        if (await InventoryDimensionPolicy.RequiresBatchOrSerialAsync(Db, group.Value, ct))
            throw new InvalidOperationException(
                $"Item '{itemId}' is batch or serial tracked. Basic transfer orders carry site and warehouse dimensions only, so use an inventory transfer journal for tracked items.");
    }

    private async Task RequireStockedItemAsync(string area, string itemId, CancellationToken ct)
    {
        var modelGroupId = await Db.Set<InventModelGroupItem>().AsNoTracking()
            .Where(x => x.ItemDataAreaId == area && x.ItemId == itemId)
            .Select(x => x.ModelGroupId).FirstOrDefaultAsync(ct);
        var model = string.IsNullOrWhiteSpace(modelGroupId) ? null :
            await Db.Set<ItemModelGroup>().AsNoTracking()
                .Where(x => x.DataAreaId == area && x.ModelGroupId == modelGroupId)
                .FirstOrDefaultAsync(ct);
        if (model?.StockedProduct != NoYes.Yes)
            throw new InvalidOperationException($"Item '{itemId}' requires a stocked item model group for inventory transfer.");
        if (model.InventModel is not (InventModel.FIFO or InventModel.LIFO or InventModel.WeightedAvg))
            throw new InvalidOperationException("Inventory transfer requires FIFO, LIFO, or weighted average costing in the basic inventory cycle.");
    }

    private async Task<InventTransferTable> PostCoreAsync(string area, string transferId, InventTransferPostDto input, bool receive, CancellationToken ct)
    {
        var id = (transferId ?? string.Empty).Trim();
        var requestId = (input.RequestId ?? string.Empty).Trim();
        if (requestId.Length == 0) throw new InvalidOperationException("A posting request identifier is required.");
        if (input.Quantities == null || input.Quantities.Count == 0)
            throw new InvalidOperationException("Select the quantities to post.");
        await LockAsync(area, id, ct);

        var header = await Db.Set<InventTransferTable>().SingleOrDefaultAsync(x => x.DataAreaId == area && x.TransferId == id, ct)
            ?? throw new InvalidOperationException("Transfer order was not found in the selected company.");
        if (header.TransferStatus == InventTransferStatus.Cancelled)
            throw new InvalidOperationException("The transfer order is cancelled.");

        var transDate = (input.TransDate ?? DateTime.UtcNow).Date;
        var voucher = $"{id}:{requestId}";
        // The request identifier makes a repeated post request a no-operation, never a double post.
        if (await Db.Set<InventTrans>().AsNoTracking().AnyAsync(x => x.DataAreaId == area && x.Voucher == voucher, ct))
            return header;
        if (header.TransferStatus == InventTransferStatus.Received)
            throw new InvalidOperationException("The transfer order is fully received.");

        var lines = await Db.Set<InventTransferLine>().Where(x => x.DataAreaId == area && x.TransferId == id).ToListAsync(ct);
        var site = await Db.Set<InventDim>().AsNoTracking()
            .Where(x => x.DataAreaId == area && x.InventDimId == lines[0].InventDimId)
            .Select(x => x.InventSiteId).FirstAsync(ct);
        var accountingCurrency = await Db.Set<Entities.Ledger>().AsNoTracking()
            .Where(x => x.DataAreaId == area).Select(x => x.AccountingCurrency).FirstOrDefaultAsync(ct) ?? string.Empty;
        var fromWarehouse = receive ? header.InventLocationIdTransit : header.InventLocationIdFrom;
        var toWarehouse = receive ? header.InventLocationIdTo : header.InventLocationIdTransit;

        foreach (var (lineNum, quantity) in input.Quantities.OrderBy(x => x.Key))
        {
            var line = lines.SingleOrDefault(x => x.LineNum == lineNum)
                ?? throw new InvalidOperationException($"Line {lineNum} does not belong to this transfer order.");
            if (quantity <= 0 || decimal.Round(quantity, 4) != quantity)
                throw new InvalidOperationException($"Line {lineNum} needs a positive quantity to post.");
            var allowed = receive ? line.QtyShipped - line.QtyReceived : line.QtyRemainShip;
            if (quantity > allowed)
                throw new InvalidOperationException(receive
                    ? $"Line {lineNum}: the receipt exceeds this line's stock in transit."
                    : $"Line {lineNum}: the shipment exceeds the remaining transfer quantity.");

            // The dimension the order can carry is site and warehouse only, so an item that needs a
            // batch or serial number is refused here rather than moved under a blank one.
            await RequireUntrackedItemAsync(area, line.ItemId, ct);
            var fromDim = await dimensions.ResolveAsync(area, site, fromWarehouse, ct);
            var toDim = await dimensions.ResolveAsync(area, site, toWarehouse, ct);
            var lifecycleError = await ProductLifecyclePolicy.ValidateAsync(Db, area, line.ItemId,
                ProductLifecycleProcess.Inventory, ct);
            if (lifecycleError != null) throw new InvalidOperationException(lifecycleError);
            await RequireStockedItemAsync(area, line.ItemId, ct);
            var boundaryError = await InventoryFinancialBoundary.CheckAsync(Db, area, line.ItemId, transDate, ct);
            if (boundaryError != null) throw new InvalidOperationException(boundaryError);
            foreach (var dim in new[] { fromDim, toDim }.OrderBy(x => x.InventDimId, StringComparer.Ordinal))
            {
                var freezeError = await InventoryCountProtection.CheckAsync(Db, area, line.ItemId, dim.InventDimId, ct);
                if (freezeError != null) throw new InvalidOperationException(freezeError);
            }
            foreach (var (dim, signedQty) in new[] { (fromDim, -quantity), (toDim, quantity) })
            {
                var statusError = await InventoryDimensionPolicy.CheckStatusAsync(Db, area, dim, signedQty, ct);
                if (statusError != null) throw new InvalidOperationException($"Line {lineNum}: {statusError}");
            }
            var fromSum = await Movement.GetOrCreateSummaryAsync(area, line.ItemId, fromDim, ct);
            var toSum = await Movement.GetOrCreateSummaryAsync(area, line.ItemId, toDim, ct);
            if ((receive ? fromSum.PhysicalInvent : fromSum.AvailPhysical) < quantity)
                throw new InvalidOperationException(receive
                    ? $"Line {lineNum}: transit stock is insufficient."
                    : $"Line {lineNum}: available stock at the source warehouse is insufficient.");

            // Shipment takes the site's provisional cost into transit. Receipt uses this order's
            // remaining transit value, so other orders and later purchases cannot change it.
            var unitCost = receive
                ? await TransitUnitCostAsync(area, id, line.LineNum, line.ItemId, fromDim.InventDimId, quantity, ct)
                : await InventoryMovement.SiteRunningAverageCostAsync(Db, area, line.ItemId, site, ct);
            var currency = accountingCurrency;
            var issueTransId = receive ? string.Empty : await Movement.CreateTransactionAsync(area, line.ItemId, fromDim.InventDimId,
                -quantity, unitCost, currency, voucher, transDate, InventRefType.TransferOrder, id, ct);
            var receiptTransId = receive ? string.Empty : await Movement.CreateTransactionAsync(area, line.ItemId, toDim.InventDimId,
                quantity, unitCost, currency, voucher, transDate, InventRefType.TransferOrder, id, ct);
            if (receive)
            {
                (issueTransId, receiptTransId) = await ReceivePendingLegsAsync(area, id, line.LineNum, line.ItemId,
                    fromDim.InventDimId, toDim.InventDimId, quantity, unitCost, currency, voucher, transDate, ct);
                fromSum.ReservPhysical -= quantity;
                toSum.Ordered -= quantity;
            }
            InventoryMovement.ApplySummary(fromSum, -quantity, unitCost);
            InventoryMovement.ApplySummary(toSum, quantity, unitCost);

            if (receive)
            {
                line.InventTransIdTransitFrom = issueTransId;
                line.InventTransIdReceive = receiptTransId;
                line.QtyReceiveNow = quantity;
                line.QtyReceived += quantity;
                line.QtyRemainReceive = line.QtyTransfer - line.QtyReceived;
                line.ReceiveDate = transDate;
            }
            else
            {
                LinkTransitTo(area, id, line.LineNum, receiptTransId);
                line.InventTransIdTransitFrom = await CreatePendingLegAsync(area, id, line.ItemId,
                    toDim.InventDimId, -quantity, currency, voucher, transDate, ct);
                LinkTransitFrom(area, id, line.LineNum, line.InventTransIdTransitFrom);
                var destinationDim = await dimensions.ResolveAsync(area, site, header.InventLocationIdTo, ct);
                line.InventTransIdReceive = await CreatePendingLegAsync(area, id, line.ItemId,
                    destinationDim.InventDimId, quantity, currency, voucher, transDate, ct);
                LinkReceive(area, id, line.LineNum, line.InventTransIdReceive);
                toSum.ReservPhysical += quantity;
                toSum.AvailPhysical = toSum.PhysicalInvent - toSum.ReservPhysical - toSum.Picked;
                toSum.AvailOrdered = toSum.AvailPhysical + toSum.Ordered - toSum.OnOrder - toSum.ReservOrdered;
                var destinationSum = await Movement.GetOrCreateSummaryAsync(area, line.ItemId, destinationDim, ct);
                destinationSum.Ordered += quantity;
                destinationSum.AvailOrdered = destinationSum.AvailPhysical + destinationSum.Ordered
                    - destinationSum.OnOrder - destinationSum.ReservOrdered;
                line.InventTransId = issueTransId;
                line.InventTransIdTransitTo = receiptTransId;
                line.QtyShipNow = quantity;
                line.QtyShipped += quantity;
                line.QtyRemainShip = line.QtyTransfer - line.QtyShipped;
                line.ShipDate = transDate;
            }
            line.RemainStatus = line.QtyRemainReceive == 0 ? InventTransferRemainStatus.Received
                : line.QtyRemainShip == 0 ? InventTransferRemainStatus.Shipped
                : InventTransferRemainStatus.None;
        }

        header.TransferStatus = lines.All(x => x.QtyRemainReceive == 0) ? InventTransferStatus.Received
            : lines.Any(x => x.QtyShipped > 0) ? InventTransferStatus.Shipped
            : InventTransferStatus.Created;
        if (receive) header.ReceiveDate = transDate; else header.ShipDate = transDate;
        return header;
    }

    private async Task<decimal> TransitUnitCostAsync(string area, string transferId, decimal lineNum, string itemId,
        string transitDimId, decimal quantity, CancellationToken ct)
    {
        var receiptIds = await Db.Set<InventTransOriginTransferTransitTo>().AsNoTracking()
            .Where(x => x.TransferTransitToDataAreaId == area && x.TransferTransitToId == transferId
                && x.TransferTransitToLineNum == lineNum)
            .Select(x => x.InventTransOrigin).ToListAsync(ct);
        var issueIds = await Db.Set<InventTransOriginTransferTransitFrom>().AsNoTracking()
            .Where(x => x.TransferTransitFromDataAreaId == area && x.TransferTransitFromId == transferId
                && x.TransferTransitFromLineNum == lineNum)
            .Select(x => x.InventTransOrigin).ToListAsync(ct);
        var originIds = receiptIds.Concat(issueIds).ToList();
        originIds.AddRange(Db.ChangeTracker.Entries<InventTransOriginTransferTransitTo>()
            .Where(x => x.State == EntityState.Added && x.Entity.TransferTransitToDataAreaId == area
                && x.Entity.TransferTransitToId == transferId && x.Entity.TransferTransitToLineNum == lineNum)
            .Select(x => x.Entity.InventTransOrigin));
        originIds.AddRange(Db.ChangeTracker.Entries<InventTransOriginTransferTransitFrom>()
            .Where(x => x.State == EntityState.Added && x.Entity.TransferTransitFromDataAreaId == area
                && x.Entity.TransferTransitFromId == transferId && x.Entity.TransferTransitFromLineNum == lineNum)
            .Select(x => x.Entity.InventTransOrigin));
        var movements = await Db.Set<InventTrans>()
            .Where(x => x.DataAreaId == area && x.ItemId == itemId && x.InventDimId == transitDimId
                && originIds.Contains(x.InventTransOrigin)
                && (x.StatusReceipt == StatusReceipt.Purchased || x.StatusIssue == StatusIssue.Sold))
            .ToListAsync(ct);
        movements.AddRange(Db.ChangeTracker.Entries<InventTrans>()
            .Where(x => x.State == EntityState.Added && x.Entity.DataAreaId == area
                && x.Entity.ItemId == itemId && x.Entity.InventDimId == transitDimId
                && originIds.Contains(x.Entity.InventTransOrigin)
                && (x.Entity.StatusReceipt == StatusReceipt.Purchased || x.Entity.StatusIssue == StatusIssue.Sold))
            .Select(x => x.Entity));
        var remainingQty = movements.Sum(x => x.Qty);
        if (remainingQty < quantity)
            throw new InvalidOperationException($"Transfer order '{transferId}' has no financially posted transit quantity for item '{itemId}'.");
        var remainingValue = movements.Sum(x => x.CostAmountPosted + x.CostAmountAdjustment);
        return remainingValue / remainingQty;
    }

    private async Task<string> CreatePendingLegAsync(string area, string transferId, string itemId,
        string dimId, decimal qty, string currency, string voucher, DateTime expectedDate, CancellationToken ct)
    {
        var transId = await Movement.CreateTransactionAsync(area, itemId, dimId, qty, 0m, currency,
            voucher, expectedDate, InventRefType.TransferOrder, transferId, ct);
        var origin = Db.Set<InventTransOrigin>().Local.Single(x => x.DataAreaId == area && x.InventTransId == transId);
        var transaction = Db.Set<InventTrans>().Local.Single(x => x.InventTransOrigin == origin.RecId);
        transaction.StatusIssue = qty < 0 ? StatusIssue.ReservPhysical : StatusIssue.None;
        transaction.StatusReceipt = qty > 0 ? StatusReceipt.Ordered : StatusReceipt.None;
        transaction.DateFinancial = default;
        transaction.DatePhysical = default;
        transaction.DateExpected = expectedDate;
        return transId;
    }

    private long OriginId(string area, string transId) => Db.Set<InventTransOrigin>().Local
        .Single(x => x.DataAreaId == area && x.InventTransId == transId).RecId;

    private void LinkTransitTo(string area, string transferId, decimal lineNum, string transId) =>
        Db.Add(new InventTransOriginTransferTransitTo
        {
            DataAreaId = area, InventTransOrigin = OriginId(area, transId),
            TransferTransitToDataAreaId = area, TransferTransitToId = transferId, TransferTransitToLineNum = lineNum
        });

    private void LinkTransitFrom(string area, string transferId, decimal lineNum, string transId) =>
        Db.Add(new InventTransOriginTransferTransitFrom
        {
            DataAreaId = area, InventTransOrigin = OriginId(area, transId),
            TransferTransitFromDataAreaId = area, TransferTransitFromId = transferId, TransferTransitFromLineNum = lineNum
        });

    private void LinkReceive(string area, string transferId, decimal lineNum, string transId) =>
        Db.Add(new InventTransOriginTransferReceive
        {
            DataAreaId = area, InventTransOrigin = OriginId(area, transId),
            TransferReceiveDataAreaId = area, TransferReceiveId = transferId, TransferReceiveLineNum = lineNum
        });

    private async Task<(string IssueId, string ReceiptId)> ReceivePendingLegsAsync(string area, string transferId,
        decimal lineNum,
        string itemId, string transitDimId, string destinationDimId, decimal quantity, decimal unitCost,
        string currency, string voucher, DateTime transDate, CancellationToken ct)
    {
        var origins = await Db.Set<InventTransOrigin>().AsNoTracking()
            .Where(x => x.DataAreaId == area && x.ReferenceCategory == InventRefType.TransferOrder
                && x.ReferenceId == transferId && x.ItemId == itemId)
            .ToDictionaryAsync(x => x.RecId, ct);
        var issueOriginIds = await Db.Set<InventTransOriginTransferTransitFrom>().AsNoTracking()
            .Where(x => x.TransferTransitFromDataAreaId == area && x.TransferTransitFromId == transferId
                && x.TransferTransitFromLineNum == lineNum)
            .Select(x => x.InventTransOrigin).ToListAsync(ct);
        var receiptOriginIds = await Db.Set<InventTransOriginTransferReceive>().AsNoTracking()
            .Where(x => x.TransferReceiveDataAreaId == area && x.TransferReceiveId == transferId
                && x.TransferReceiveLineNum == lineNum)
            .Select(x => x.InventTransOrigin).ToListAsync(ct);
        var originIds = issueOriginIds.Concat(receiptOriginIds).ToList();
        originIds.AddRange(Db.ChangeTracker.Entries<InventTransOriginTransferTransitFrom>()
            .Where(x => x.State == EntityState.Added && x.Entity.TransferTransitFromDataAreaId == area
                && x.Entity.TransferTransitFromId == transferId && x.Entity.TransferTransitFromLineNum == lineNum)
            .Select(x => x.Entity.InventTransOrigin));
        originIds.AddRange(Db.ChangeTracker.Entries<InventTransOriginTransferReceive>()
            .Where(x => x.State == EntityState.Added && x.Entity.TransferReceiveDataAreaId == area
                && x.Entity.TransferReceiveId == transferId && x.Entity.TransferReceiveLineNum == lineNum)
            .Select(x => x.Entity.InventTransOrigin));
        var pending = await Db.Set<InventTrans>()
            .Where(x => x.DataAreaId == area && x.ItemId == itemId && originIds.Contains(x.InventTransOrigin)
                && (x.StatusIssue == StatusIssue.ReservPhysical || x.StatusReceipt == StatusReceipt.Ordered))
            .ToListAsync(ct);
        pending.AddRange(Db.ChangeTracker.Entries<InventTrans>()
            .Where(x => x.State == EntityState.Added && x.Entity.DataAreaId == area
                && x.Entity.ItemId == itemId && originIds.Contains(x.Entity.InventTransOrigin)
                && (x.Entity.StatusIssue == StatusIssue.ReservPhysical || x.Entity.StatusReceipt == StatusReceipt.Ordered))
            .Select(x => x.Entity));
        var issues = pending.Where(x => x.InventDimId == transitDimId && x.StatusIssue == StatusIssue.ReservPhysical)
            .OrderBy(x => x.DateExpected).ThenBy(x => x.RecId).ToList();
        var receipts = pending.Where(x => x.InventDimId == destinationDimId && x.StatusReceipt == StatusReceipt.Ordered)
            .ToList();
        var remaining = quantity;
        var lastIssueId = string.Empty;
        var lastReceiptId = string.Empty;
        foreach (var issue in issues)
        {
            if (remaining == 0) break;
            var receipt = receipts.FirstOrDefault(x => x.Voucher == issue.Voucher && x.Qty == -issue.Qty);
            if (receipt == null)
                throw new InvalidOperationException($"Transfer order '{transferId}' has unmatched pending transit legs for item '{itemId}'.");
            receipts.Remove(receipt);
            var take = Math.Min(remaining, -issue.Qty);
            if (take == -issue.Qty)
            {
                issue.StatusIssue = StatusIssue.Sold;
                receipt.StatusReceipt = StatusReceipt.Purchased;
                issue.CostAmountPosted = -take * unitCost;
                receipt.CostAmountPosted = take * unitCost;
                foreach (var transaction in new[] { issue, receipt })
                {
                    transaction.Voucher = voucher;
                    transaction.VoucherPhysical = voucher;
                    transaction.DateStatus = transDate;
                    transaction.DatePhysical = transDate;
                    transaction.DateFinancial = transDate;
                    transaction.DateInvent = transDate;
                }
                lastIssueId = origins[issue.InventTransOrigin].InventTransId;
                lastReceiptId = origins[receipt.InventTransOrigin].InventTransId;
            }
            else
            {
                // Keep the unreceived portion reserved and post a separate transaction for the received portion.
                issue.Qty += take;
                receipt.Qty -= take;
                lastIssueId = await Movement.CreateTransactionAsync(area, itemId, transitDimId,
                    -take, unitCost, currency, voucher, transDate, InventRefType.TransferOrder, transferId, ct);
                LinkTransitFrom(area, transferId, lineNum, lastIssueId);
                lastReceiptId = await Movement.CreateTransactionAsync(area, itemId, destinationDimId,
                    take, unitCost, currency, voucher, transDate, InventRefType.TransferOrder, transferId, ct);
                LinkReceive(area, transferId, lineNum, lastReceiptId);
            }
            remaining -= take;
        }
        if (remaining != 0)
            throw new InvalidOperationException($"Transfer order '{transferId}' has insufficient reserved transit quantity for item '{itemId}'.");
        return (lastIssueId, lastReceiptId);
    }

    private async Task<InventTransferOrderDto> MapAsync(string area, InventTransferTable header, CancellationToken ct)
    {
        var lines = (await Db.Set<InventTransferLine>().AsNoTracking()
            .Where(x => x.DataAreaId == area && x.TransferId == header.TransferId).ToListAsync(ct))
            .OrderBy(x => x.LineNum).ToList();
        return new(header.TransferId, header.TransferStatus, header.InventLocationIdFrom,
            header.InventLocationIdTransit, header.InventLocationIdTo, header.ShipDate, header.ReceiveDate,
            lines.Select(x => new InventTransferLineDto(x.LineNum, x.ItemId, x.UnitId, x.QtyTransfer,
                x.QtyShipped, x.QtyRemainShip, x.QtyReceived, x.QtyRemainReceive, x.RemainStatus,
                x.InventTransId, x.InventTransIdTransitTo, x.InventTransIdTransitFrom, x.InventTransIdReceive)).ToList());
    }

    private async Task LockAsync(string area, string transferId, CancellationToken ct)
    {
        var error = await InventoryCountProtection.CheckAsync(Db, area, "TransferOrder:" + transferId, "", ct);
        if (error != null) throw new InvalidOperationException(error);
    }

    private Task<(InventTransferOrderDto? Order, string? Error)> ExecuteAsync(
        Func<string, Task<InventTransferTable>> action, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            var area = company.GetDataAreaId();
            if (string.IsNullOrWhiteSpace(area)) return ((InventTransferOrderDto?)null, (string?)"A company must be selected.");
            await unitOfWork.BeginTransactionAsync(ct);
            try
            {
                var header = await action(area);
                await unitOfWork.CompleteAsync(ct);
                var result = await MapAsync(area, header, ct);
                await unitOfWork.CommitTransactionAsync(ct);
                return ((InventTransferOrderDto?)result, (string?)null);
            }
            catch (InvalidOperationException ex)
            {
                await unitOfWork.RollbackTransactionAsync(ct);
                Db.ChangeTracker.Clear();
                return ((InventTransferOrderDto?)null, (string?)ex.Message);
            }
            catch
            {
                await unitOfWork.RollbackTransactionAsync(ct);
                Db.ChangeTracker.Clear();
                throw;
            }
        });
}
