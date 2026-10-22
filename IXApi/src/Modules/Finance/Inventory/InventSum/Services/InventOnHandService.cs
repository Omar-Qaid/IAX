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
    {
        var companyId = company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(companyId)) return null;

        return await (from summary in unitOfWork.Context.Set<InventSum>().AsNoTracking()
                      join item in unitOfWork.Context.Set<ReleasedItem>().AsNoTracking()
                          on new { summary.DataAreaId, summary.ItemId }
                          equals new { item.DataAreaId, item.ItemId }
                      join dimension in unitOfWork.Context.Set<InventDim>().AsNoTracking()
                          on new { summary.DataAreaId, summary.InventDimId }
                          equals new { dimension.DataAreaId, dimension.InventDimId }
                      where summary.DataAreaId == companyId
                      orderby summary.ItemId, dimension.InventSiteId, dimension.InventLocationId
                      select new InventOnHandDto
                      {
                          Id = summary.RecId.ToString(CultureInfo.InvariantCulture),
                          RecId = summary.RecId,
                          ItemNumber = summary.ItemId,
                          ProductName = item.Name,
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
                              status.InventStatusBlocking == NoYes.Yes) ? 0m : summary.AvailPhysical,
                          OrderedReceipts = summary.Ordered,
                          OrderedIssues = summary.OnOrder,
                          AvailableOrdered = unitOfWork.Context.Set<WHSInventStatus>().Any(status =>
                              status.DataAreaId == companyId && status.InventStatusId == dimension.InventStatusId &&
                              status.InventStatusBlocking == NoYes.Yes) ? 0m : summary.AvailOrdered,
                          PostedValue = summary.PostedValue,
                          PhysicalValue = summary.PhysicalValue,
                      }).ToListAsync(ct);
    }
}
