using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Foundation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory.InventoryStatus.Entities;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;

/// <summary>
/// Inventory statuses, the sites, and per site a main warehouse with its quarantine and transit warehouses and
/// the receipt, issue, bulk and picking locations of the main warehouse.
/// </summary>
public sealed class BaselineSiteWarehouseSeeder : IBaselineSeeder
{
    public const string SeederId = "Finance.Inventory.SitesAndWarehouses";
    public string Id => SeederId;
    public IReadOnlyCollection<string> Dependencies => [BaselineCompanySeeder.SeederId];

    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var catalog = InventoryBaselineCatalog.Load();
        foreach (var status in catalog.Statuses)
            await c.AddMissingAsync<WHSInventStatus>(x => x.InventStatusId == status.StatusId, new()
            {
                InventStatusId = status.StatusId, Name = status.Name, InventStatusBlocking = status.Blocking ? NoYes.Yes : NoYes.No
            }, ct);

        foreach (var site in catalog.Sites)
        {
            await c.AddMissingAsync<InventSite>(x => x.SiteId == site.SiteId, new()
            {
                SiteId = site.SiteId, Name = site.Name, DefaultInventStatusID = catalog.DefaultStatus, TimeZone = Timezone.Local
            }, ct);
            var main = site.SiteId + "-1";
            var quarantine = site.SiteId + "-QC";
            var transit = site.SiteId + "-TRS";
            var mainWarehouse = Warehouse(main, site.Warehouse + " Main Warehouse", site.SiteId, InventLocationType.Standard, catalog.DefaultStatus);
            mainWarehouse.InventLocationIdQuarantine = quarantine;
            mainWarehouse.InventLocationIdTransit = transit;
            mainWarehouse.ItmInventLocationIdGit = transit;
            await c.AddMissingAsync<InventLocation>(x => x.InventLocationId == main, mainWarehouse, ct);
            foreach (var (id, name, type) in new[] { (quarantine, " Quarantine Warehouse", InventLocationType.Quarantine), (transit, " Transit Warehouse", InventLocationType.Transit) })
            {
                var related = Warehouse(id, site.Warehouse + name, site.SiteId, type, catalog.DefaultStatus);
                related.InventLocationIdReqMain = main;
                await c.AddMissingAsync<InventLocation>(x => x.InventLocationId == id, related, ct);
            }
            foreach (var location in catalog.WarehouseLocations)
                await c.AddMissingAsync<WMSLocation>(x => x.InventLocationId == main && x.WMSLocationId == location.WmsLocationId, new()
                {
                    InventLocationId = main, WMSLocationId = location.WmsLocationId, CheckText = location.CheckText, LocationType = location.LocationType
                }, ct);
        }
    }

    private static InventLocation Warehouse(string id, string name, string site, InventLocationType type, string status) => new()
    {
        InventLocationId = id, Name = name, InventSiteId = site, InventLocationType = type,
        InventLocationLevel = type == InventLocationType.Standard ? 0 : 1, DefaultStatusID = status,
        WmsLocationIdDefaultReceipt = "RECEIPT", WmsLocationIdDefaultIssue = "ISSUE",
        DefaultProductionInputLocation = "PROD-IN", DefaultProductionFinishGoodsLocation = "PROD-OUT", DefaultKanbanFinishedGoodsLocation = "KANBAN"
    };
}
