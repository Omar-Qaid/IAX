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
}
