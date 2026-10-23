using System.Globalization;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Inventory.InventoryStatus.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;
using ReleasedItem = IAX.IXApi.Modules.Finance.Entities.InventTable;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventOnHandService(
    IUnitOfWork unitOfWork,
    ICompanyExecutionContext company) : IInventOnHandService
{
    public async Task<List<InventOnHandDto>?> ListAsync(CancellationToken ct = default)
        => await SearchAsync(null, null, null, ct);

    public async Task<List<InventOnHandDto>?> SearchAsync(string? itemId, string? siteId, string? warehouseId,
        CancellationToken ct = default)
    {
        var companyId = company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(companyId)) return null;
        return await Query(companyId, itemId, null, siteId, warehouseId, true).ToListAsync(ct);
    }

    public async Task<InventOnHandPageDto?> SearchPageAsync(string? itemId, string? itemGroupId, string? siteId,
        string? warehouseId, bool includeZero, int page, int pageSize, CancellationToken ct = default)
    {
        var companyId = company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(companyId)) return null;
        var query = Query(companyId, itemId, itemGroupId, siteId, warehouseId, includeZero);
        var total = await query.CountAsync(ct);
        var rows = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new InventOnHandPageDto(page, pageSize, total, rows);
    }

    private IQueryable<InventOnHandDto> Query(string companyId, string? itemId, string? itemGroupId,
        string? siteId, string? warehouseId, bool includeZero)
    {
        itemId = itemId?.Trim();
        itemGroupId = itemGroupId?.Trim();
        siteId = siteId?.Trim();
        warehouseId = warehouseId?.Trim();
        return from summary in unitOfWork.Context.Set<InventSum>().AsNoTracking()
                      join item in unitOfWork.Context.Set<ReleasedItem>().AsNoTracking()
                          on new { summary.DataAreaId, summary.ItemId }
                          equals new { item.DataAreaId, item.ItemId }
                      join dimension in unitOfWork.Context.Set<InventDim>().AsNoTracking()
                          on new { summary.DataAreaId, summary.InventDimId }
                          equals new { dimension.DataAreaId, dimension.InventDimId }
                      where summary.DataAreaId == companyId
                          && (itemId == null || itemId == "" || summary.ItemId == itemId)
                          && (itemGroupId == null || itemGroupId == "" || unitOfWork.Context.Set<InventItemGroupItem>()
                              .Any(assignment => assignment.ItemDataAreaId == companyId && assignment.ItemGroupDataAreaId == companyId
                                  && assignment.ItemId == summary.ItemId && assignment.ItemGroupId == itemGroupId))
                          && (siteId == null || siteId == "" || dimension.InventSiteId == siteId)
                          && (warehouseId == null || warehouseId == "" || dimension.InventLocationId == warehouseId)
                          && (includeZero || summary.PhysicalInvent != 0 || summary.ReservPhysical != 0
                              || summary.Ordered != 0 || summary.OnOrder != 0 || summary.ReservOrdered != 0)
                      orderby summary.ItemId, dimension.InventSiteId, dimension.InventLocationId, summary.RecId
                      select new InventOnHandDto
                      {
                          Id = summary.RecId.ToString(CultureInfo.InvariantCulture),
                          RecId = summary.RecId,
                          ItemNumber = summary.ItemId,
                          ProductName = item.Name,
                          InventoryUnit = unitOfWork.Context.Set<InventTableModule>()
                              .Where(module => module.DataAreaId == companyId && module.ItemId == summary.ItemId
                                  && module.ModuleType == ModuleInventPurchSales.Inventory)
                              .Select(module => module.UnitId).FirstOrDefault() ?? string.Empty,
                          InventDimId = summary.InventDimId,
                          Site = dimension.InventSiteId,
                          Warehouse = dimension.InventLocationId,
                          InventoryStatusId = dimension.InventStatusId,
                          InventoryStatusBlocked = unitOfWork.Context.Set<WHSInventStatus>().Any(status =>
                              status.DataAreaId == companyId && status.InventStatusId == dimension.InventStatusId &&
                              status.InventStatusBlocking == NoYes.Yes),
                          PhysicalInventory = summary.PhysicalInvent,
                          PhysicalReserved = summary.ReservPhysical,
                          AvailablePhysical = unitOfWork.Context.Set<WHSInventStatus>().Any(status =>
                              status.DataAreaId == companyId && status.InventStatusId == dimension.InventStatusId &&
                              status.InventStatusBlocking == NoYes.Yes) ? 0m : summary.PhysicalInvent - summary.ReservPhysical,
                          OrderedReceipts = summary.Ordered,
                          OrderedIssues = summary.OnOrder,
                          AvailableOrdered = unitOfWork.Context.Set<WHSInventStatus>().Any(status =>
                              status.DataAreaId == companyId && status.InventStatusId == dimension.InventStatusId &&
                              status.InventStatusBlocking == NoYes.Yes) ? 0m :
                                  summary.PhysicalInvent - summary.ReservPhysical + summary.Ordered - summary.OnOrder - summary.ReservOrdered,
                          PostedValue = summary.PostedValue,
                          PhysicalValue = summary.PhysicalValue,
                      };
    }
}
