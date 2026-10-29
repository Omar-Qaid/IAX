using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;
using ReleasedItem = IAX.IXApi.Modules.Finance.Entities.InventTable;

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
        if (header.TransferStatus == InventTransferStatus.Received)
            throw new InvalidOperationException("The transfer order is fully received.");

        var transDate = (input.TransDate ?? DateTime.UtcNow).Date;
        var voucher = $"{id}:{requestId}";
        // The request identifier makes a repeated post request a no-operation, never a double post.
        if (await Db.Set<InventTrans>().AsNoTracking().AnyAsync(x => x.DataAreaId == area && x.Voucher == voucher, ct))
            return header;

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

            var fromDim = await dimensions.ResolveAsync(area, site, fromWarehouse, ct);
            var toDim = await dimensions.ResolveAsync(area, site, toWarehouse, ct);
            var fromSum = await Movement.GetOrCreateSummaryAsync(area, line.ItemId, fromDim, ct);
            var toSum = await Movement.GetOrCreateSummaryAsync(area, line.ItemId, toDim, ct);
            if (fromSum.AvailPhysical < quantity)
                throw new InvalidOperationException(receive
                    ? $"Line {lineNum}: transit stock is insufficient."
                    : $"Line {lineNum}: available stock at the source warehouse is insufficient.");

            // A transfer moves cost with the goods; it does not revalue them.
            var unitCost = InventoryMovement.RunningAverageCost(fromSum);
            var currency = accountingCurrency;
            var issueTransId = await Movement.CreateTransactionAsync(area, line.ItemId, fromDim.InventDimId,
                -quantity, unitCost, currency, voucher, transDate, InventRefType.TransferOrder, id, ct);
            var receiptTransId = await Movement.CreateTransactionAsync(area, line.ItemId, toDim.InventDimId,
                quantity, unitCost, currency, voucher, transDate, InventRefType.TransferOrder, id, ct);
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
