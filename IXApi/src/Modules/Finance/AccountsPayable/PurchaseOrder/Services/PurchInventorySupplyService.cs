using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

/// <summary>
/// Purchase-side counterpart of SalesInventoryDemandService: a PO line gets a stable inventory
/// origin and an Ordered receipt transaction; InventSum.Ordered tracks expected supply.
/// </summary>
public sealed class PurchInventorySupplyService(IUnitOfWork unitOfWork, ISalesInventoryNumberService numbers,
    IInventDimensionResolver dimensions) : IPurchInventorySupplyService
{
    private DbContext Db => unitOfWork.Context;

    public async Task CreateAsync(PurchTable order, PurchLine line, string inventSiteId, string inventLocationId, CancellationToken ct = default)
    {
        var dimension = await dimensions.ResolveAsync(order.DataAreaId, inventSiteId, inventLocationId, ct);
        var transactionId = await numbers.NextInventTransIdAsync(ct);
        var origin = new InventTransOrigin
        {
            InventTransId = transactionId, ItemId = line.ItemId, ItemInventDimId = dimension.InventDimId,
            ReferenceCategory = InventRefType.PurchaseOrder, ReferenceId = order.PurchId, DataAreaId = order.DataAreaId
        };
        Db.Set<InventTransOrigin>().Add(origin);
        await Db.SaveChangesAsync(ct);
        Db.Set<InventTransOriginPurchLine>().Add(new InventTransOriginPurchLine
        {
            InventTransOrigin = origin.RecId, PurchLineDataAreaId = line.DataAreaId,
            PurchLineInventTransId = transactionId, DataAreaId = order.DataAreaId
        });

        line.InventDimId = dimension.InventDimId;
        line.InventTransId = transactionId;

        var summary = await Db.Set<InventSum>().SingleOrDefaultAsync(x => x.DataAreaId == order.DataAreaId
            && x.ItemId == line.ItemId && x.InventDimId == dimension.InventDimId, ct);
        if (summary == null)
        {
            summary = new InventSum
            {
                DataAreaId = order.DataAreaId, ItemId = line.ItemId, InventDimId = dimension.InventDimId,
                InventSiteId = dimension.InventSiteId, InventLocationId = dimension.InventLocationId
            };
            Db.Set<InventSum>().Add(summary);
        }
        summary.Ordered += line.QtyOrdered;
        summary.AvailOrdered = summary.AvailPhysical + summary.Ordered - summary.OnOrder - summary.ReservOrdered;
        summary.LastUpdDateExpected = line.DeliveryDate;

        Db.Set<InventTrans>().Add(new InventTrans
        {
            InventTransOrigin = origin.RecId, ItemId = line.ItemId, InventDimId = dimension.InventDimId,
            Qty = line.QtyOrdered, StatusReceipt = StatusReceipt.Ordered, StatusIssue = StatusIssue.None,
            ValueOpen = InventTransOpen.Yes, CurrencyCode = order.CurrencyCode,
            DateExpected = line.DeliveryDate, DataAreaId = order.DataAreaId
        });
    }

    /// <summary>
    /// The reverse of the Ordered receipt <see cref="CreateAsync"/> writes, for the quantity that is
    /// still only expected. Receiving splits the Ordered transaction and moves the received part to
    /// Received, so the transactions still carrying <see cref="StatusReceipt.Ordered"/> are exactly
    /// the unreceived remainder and nothing else is touched here.
    /// <para>
    /// No ledger entry arises: ordered supply is neither physical nor financial inventory, so
    /// cancelling it has no accounting effect and needs no voucher or fiscal period check.
    /// </para>
    /// </summary>
    public async Task<decimal> CancelRemainingAsync(PurchTable order, PurchLine line, CancellationToken ct = default)
    {
        var remaining = Math.Max(0m, line.RemainInventPhysical);
        if (remaining == 0m || string.IsNullOrWhiteSpace(line.InventTransId)) return 0m;

        var origin = await Db.Set<InventTransOrigin>().FirstOrDefaultAsync(x => x.DataAreaId == order.DataAreaId
            && x.InventTransId == line.InventTransId, ct)
            ?? throw new InvalidOperationException($"Inventory origin is missing for item {line.ItemId}.");
        if (origin.ReferenceCategory != InventRefType.PurchaseOrder || origin.ReferenceId != order.PurchId
            || origin.ItemId != line.ItemId)
            throw new InvalidOperationException($"The inventory origin does not belong to purchase line {line.LineNumber}.");

        var ordered = await Db.Set<InventTrans>().Where(x => x.DataAreaId == order.DataAreaId
            && x.InventTransOrigin == origin.RecId && x.StatusReceipt == StatusReceipt.Ordered).ToListAsync(ct);
        // The line's own remainder and its inventory transactions must agree. If they do not, the
        // supply is not understood well enough to release the right quantity, so refuse rather than
        // cancel an amount that might belong to a receipt.
        var openQty = ordered.Sum(x => x.Qty);
        if (openQty != remaining)
            throw new InvalidOperationException(
                $"Ordered inventory for item {line.ItemId} is {openQty}, which does not match the uninvoiced remainder {remaining}; the purchase line cannot be cancelled.");
        if (ordered.Any(x => x.ItemId != line.ItemId || x.InventDimId != line.InventDimId || x.Qty < 0))
            throw new InvalidOperationException(
                $"Cancellation requires matching purchase-line inventory dimensions and ordered quantities for item {line.ItemId}.");

        var summary = await Db.Set<InventSum>().SingleOrDefaultAsync(x => x.DataAreaId == order.DataAreaId
            && x.ItemId == line.ItemId && x.InventDimId == line.InventDimId, ct)
            ?? throw new InvalidOperationException($"Inventory summary is missing for item {line.ItemId}.");
        if (summary.Ordered < remaining)
            throw new InvalidOperationException(
                $"On-hand ordered quantity for item {line.ItemId} is less than this purchase line expects; the purchase line cannot be cancelled.");
        summary.Ordered -= remaining;
        summary.AvailOrdered = summary.AvailPhysical + summary.Ordered - summary.OnOrder - summary.ReservOrdered;

        foreach (var transaction in ordered)
        {
            transaction.Qty = 0;
            transaction.StatusReceipt = StatusReceipt.None;
            transaction.ValueOpen = InventTransOpen.No;
        }
        return remaining;
    }
}
