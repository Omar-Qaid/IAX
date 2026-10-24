using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventoryRecordQueryService(IUnitOfWork unitOfWork, ICompanyExecutionContext company)
    : IInventoryRecordQueryService
{
    private DbContext Db => unitOfWork.Context;
    private string Area => company.GetDataAreaId() ?? throw new InvalidOperationException("A company must be selected.");
    private static string Normalize(string? value) => (value ?? string.Empty).Trim().ToUpperInvariant();

    public async Task<IReadOnlyList<InventoryDimensionRecordDto>> DimensionsAsync(string? siteId, string? warehouseId, CancellationToken ct)
    {
        var area = Area;
        var site = Normalize(siteId);
        var warehouse = Normalize(warehouseId);
        var query = Db.Set<InventDim>().AsNoTracking().Where(x => x.DataAreaId == area);
        if (site.Length > 0) query = query.Where(x => x.InventSiteId == site);
        if (warehouse.Length > 0) query = query.Where(x => x.InventLocationId == warehouse);
        return await query.OrderBy(x => x.InventDimId).Take(200)
            .Select(x => new InventoryDimensionRecordDto(x.RecId, x.InventDimId, x.InventSiteId,
                x.InventLocationId, x.InventBatchId, x.InventSerialId, x.InventStatusId)).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ItemGroupAssignmentRecordDto>> ItemGroupsAsync(string? itemId, CancellationToken ct)
    {
        var area = Area;
        var item = Normalize(itemId);
        var query = Db.Set<InventItemGroupItem>().AsNoTracking()
            .Where(x => x.DataAreaId == area && x.ItemDataAreaId == area && x.ItemGroupDataAreaId == area);
        if (item.Length > 0) query = query.Where(x => x.ItemId == item);
        return await query.OrderBy(x => x.ItemId).Take(200)
            .Select(x => new ItemGroupAssignmentRecordDto(x.RecId, x.ItemId, x.ItemGroupId)).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ItemLocationRecordDto>> ItemLocationsAsync(string? itemId, CancellationToken ct)
    {
        var area = Area;
        var item = Normalize(itemId);
        var query = Db.Set<InventItemLocation>().AsNoTracking().Where(x => x.DataAreaId == area);
        if (item.Length > 0) query = query.Where(x => x.ItemId == item);
        return await query.OrderBy(x => x.ItemId).ThenBy(x => x.InventDimId).Take(200)
            .Select(x => new ItemLocationRecordDto(x.RecId, x.ItemId, x.InventDimId,
                x.PickingLocationMaxQty, x.PickingLocationRefillMin, x.CountGroupId,
                x.WmsLocationIdDefaultReceipt, x.WmsLocationIdDefaultIssue)).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<InventorySettlementRecordDto>> SettlementsAsync(string? itemId, string? voucher, CancellationToken ct)
    {
        var area = Area;
        var item = Normalize(itemId);
        var selectedVoucher = Normalize(voucher);
        var query = Db.Set<InventSettlement>().AsNoTracking().Where(x => x.DataAreaId == area);
        if (item.Length > 0) query = query.Where(x => x.ItemId == item);
        if (selectedVoucher.Length > 0) query = query.Where(x => x.Voucher == selectedVoucher);
        return await query.OrderByDescending(x => x.TransDate).ThenByDescending(x => x.RecId).Take(200)
            .Select(x => new InventorySettlementRecordDto(x.RecId, x.ItemId, x.InventTransId,
                x.SettleTransId, x.Voucher, x.TransDate, x.QtySettled, x.CostAmountSettled,
                x.CostAmountAdjustment, x.SettleType, x.Posted == NoYes.Yes, x.Cancelled == NoYes.Yes)).ToListAsync(ct);
    }
}
