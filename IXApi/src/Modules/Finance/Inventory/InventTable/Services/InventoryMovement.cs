using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Inventory;

/// <summary>
/// The physical inventory movement itself: an inventory origin, its inventory transaction and the
/// on-hand summary it changes. Every posting operation that moves stock goes through here, so
/// on-hand balances always result from a posted inventory transaction and never from an
/// independent summary update.
/// </summary>
internal sealed class InventoryMovement(IUnitOfWork unitOfWork, ISalesInventoryNumberService inventoryNumbers)
{
    private DbContext Db => unitOfWork.Context;

    /// <summary>The on-hand summary for an item and dimension combination, created when first used.</summary>
    public async Task<InventSum> GetOrCreateSummaryAsync(string companyId, string itemId, InventDim dim, CancellationToken ct)
    {
        var sum = await Db.Set<InventSum>().FirstOrDefaultAsync(x => x.DataAreaId == companyId && x.ItemId == itemId && x.InventDimId == dim.InventDimId, ct);
        if (sum != null) return sum;
        sum = new InventSum
        {
            DataAreaId = companyId, ItemId = itemId, InventDimId = dim.InventDimId,
            InventSiteId = dim.InventSiteId, InventLocationId = dim.InventLocationId,
            WmsLocationId = dim.WmsLocationId, LicensePlateId = dim.LicensePlateId,
            ConfigId = dim.ConfigId, InventSizeId = dim.InventSizeId, InventColorId = dim.InventColorId,
            InventStyleId = dim.InventStyleId, InventVersionId = dim.InventVersionId,
            InventBatchId = dim.InventBatchId, InventSerialId = dim.InventSerialId, InventStatusId = dim.InventStatusId
        };
        Db.Add(sum);
        return sum;
    }

    /// <summary>
    /// Creates the inventory origin and its transaction for one movement, and returns the
    /// allocated inventory transaction identifier so the source document can keep the reference.
    /// </summary>
    public async Task<string> CreateTransactionAsync(string companyId, string itemId, string dimId,
        decimal qty, decimal unitCost, string currency, string voucher, DateTime transDate,
        InventRefType referenceCategory, string referenceId, CancellationToken ct)
    {
        var transId = await inventoryNumbers.NextInventTransIdAsync(ct);
        var origin = new InventTransOrigin
        {
            DataAreaId = companyId, InventTransId = transId, ItemId = itemId, ItemInventDimId = dimId,
            ReferenceId = referenceId, ReferenceCategory = referenceCategory
        };
        Db.Add(origin);
        await unitOfWork.CompleteAsync(ct);
        Db.Add(new InventTrans
        {
            DataAreaId = companyId, ItemId = itemId, InventDimId = dimId, InventTransOrigin = origin.RecId,
            Qty = qty, QtySettled = 0, CostAmountPosted = qty * unitCost, CurrencyCode = currency,
            Voucher = voucher, VoucherPhysical = voucher, InvoiceId = string.Empty, PackingSlipId = string.Empty,
            StatusReceipt = qty > 0 ? StatusReceipt.Purchased : StatusReceipt.None,
            StatusIssue = qty < 0 ? StatusIssue.Sold : StatusIssue.None,
            ValueOpen = InventTransOpen.Yes, DateStatus = transDate, DatePhysical = transDate,
            DateFinancial = transDate, DateInvent = transDate, ProjId = string.Empty,
            ProjCategoryId = string.Empty, ProjAdjustRefId = string.Empty, PickingRouteId = string.Empty,
            TransChildRefId = string.Empty
        });
        return transId;
    }

    /// <summary>Applies a posted movement to the on-hand summary.</summary>
    public static void ApplySummary(InventSum sum, decimal qty, decimal unitCost)
    {
        sum.PostedQty += qty;
        sum.PhysicalInvent += qty;
        sum.AvailPhysical = sum.PhysicalInvent - sum.ReservPhysical - sum.Picked;
        sum.AvailOrdered = sum.AvailPhysical + sum.Ordered - sum.OnOrder - sum.ReservOrdered;
        sum.PostedValue += qty * unitCost;
        sum.LastUpdDatePhysical = DateTime.UtcNow;
        sum.Closed = sum.PhysicalInvent == 0 ? NoYes.Yes : NoYes.No;
        sum.ClosedQty = sum.PhysicalInvent == 0 ? NoYes.Yes : NoYes.No;
    }

    /// <summary>The running average cost carried by on-hand stock.</summary>
    public static decimal RunningAverageCost(InventSum sum) =>
        sum.PhysicalInvent == 0 ? 0m : Math.Abs((sum.PostedValue + sum.PhysicalValue) / sum.PhysicalInvent);
}
