using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Roles;
using IAX.IXApi.Modules.Identity.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;

/// <summary>
/// Provides a small, repeatable inventory topology for sales-order examples.
/// Each site owns a main, quarantine, and transit warehouse.
/// </summary>
public sealed  class InventorySiteWarehouseSeeder : IAX.IXApi.Infrastructure.Persistence.Seeding.Core.BaseModuleSeeder
{
    public override string Id => "Finance.Inventory.SitesAndWarehouses";
    public override int Version => 1;
    public override IAX.IXApi.Infrastructure.Persistence.Seeding.Core.SeederCategory Category => IAX.IXApi.Infrastructure.Persistence.Seeding.Core.SeederCategory.Reference;
    public override System.Collections.Generic.IReadOnlyCollection<string> DependsOn => new[] { "Finance.Foundation.CompanyInfo" };
    public override IAX.IXApi.Infrastructure.Persistence.Seeding.Core.SeederExecutionPolicy Policy => IAX.IXApi.Infrastructure.Persistence.Seeding.Core.SeederExecutionPolicy.Repeatable;
    public override string Scope => "dat";

    public override System.Threading.Tasks.Task SeedAsync(IAX.IXApi.Infrastructure.Persistence.Seeding.Core.SeedingContext context, System.Threading.CancellationToken cancellationToken)
    {
        return SeedAsync(context.Db, context.Roles!, context.Users!, cancellationToken);
    }
    private const string DataAreaId = "dat";

    public override async Task SeedAsync(
        ApplicationDbContext db,
        RoleManager<AspNetRole> roles,
        UserManager<AspNetUser> users,
        CancellationToken ct)
    {
        var siteSeeds = new[]
        {
            new InventSite { SiteId = "RIY", Name = "Riyadh", DefaultInventStatusID = "AVAILABLE", TimeZone = Timezone.Local, DataAreaId = DataAreaId },
            new InventSite { SiteId = "JED", Name = "Jeddah", DefaultInventStatusID = "AVAILABLE", TimeZone = Timezone.Local, DataAreaId = DataAreaId },
            new InventSite { SiteId = "DMM", Name = "Dammam", DefaultInventStatusID = "AVAILABLE", TimeZone = Timezone.Local, DataAreaId = DataAreaId }
        };

        var existingSiteIds = await db.InventSites
            .IgnoreQueryFilters()
            .Where(site => site.DataAreaId == DataAreaId)
            .Select(site => site.SiteId)
            .ToListAsync(ct);

        var sitesToAdd = siteSeeds
            .Where(site => !existingSiteIds.Contains(site.SiteId))
            .ToList();

        if (sitesToAdd.Count > 0)
        {
            await db.InventSites.AddRangeAsync(sitesToAdd, ct);
            await db.SaveChangesAsync(ct);
        }

        var warehouseSeeds = new[]
        {
            MainWarehouse("RIY-1", "Riyadh Main Warehouse", "RIY", "RIY-QC", "RIY-TRS"),
            RelatedWarehouse("RIY-QC", "Riyadh Quarantine Warehouse", "RIY", InventLocationType.Quarantine, "RIY-1"),
            RelatedWarehouse("RIY-TRS", "Riyadh Transit Warehouse", "RIY", InventLocationType.Transit, "RIY-1"),
            MainWarehouse("JED-1", "Jeddah Main Warehouse", "JED", "JED-QC", "JED-TRS"),
            RelatedWarehouse("JED-QC", "Jeddah Quarantine Warehouse", "JED", InventLocationType.Quarantine, "JED-1"),
            RelatedWarehouse("JED-TRS", "Jeddah Transit Warehouse", "JED", InventLocationType.Transit, "JED-1"),
            MainWarehouse("DMM-1", "Dammam Main Warehouse", "DMM", "DMM-QC", "DMM-TRS"),
            RelatedWarehouse("DMM-QC", "Dammam Quarantine Warehouse", "DMM", InventLocationType.Quarantine, "DMM-1"),
            RelatedWarehouse("DMM-TRS", "Dammam Transit Warehouse", "DMM", InventLocationType.Transit, "DMM-1")
        };

        var existingWarehouseIds = await db.InventLocations
            .IgnoreQueryFilters()
            .Where(location => location.DataAreaId == DataAreaId)
            .Select(location => location.InventLocationId)
            .ToListAsync(ct);

        var warehousesToAdd = warehouseSeeds
            .Where(location => !existingWarehouseIds.Contains(location.InventLocationId))
            .ToList();

        if (warehousesToAdd.Count > 0)
        {
            await db.InventLocations.AddRangeAsync(warehousesToAdd, ct);
            await db.SaveChangesAsync(ct);
        }

        var wmsLocationSeeds = new[]
        {
            new WMSLocation { DataAreaId = DataAreaId, InventLocationId = "JED-1", WMSLocationId = "RECEIPT", CheckText = "Inbound Receipt Dock", LocationType = 1 },
            new WMSLocation { DataAreaId = DataAreaId, InventLocationId = "JED-1", WMSLocationId = "ISSUE", CheckText = "Outbound Shipping Dock", LocationType = 2 },
            new WMSLocation { DataAreaId = DataAreaId, InventLocationId = "JED-1", WMSLocationId = "BULK-01", CheckText = "Bulk Storage 01", LocationType = 0 },
            new WMSLocation { DataAreaId = DataAreaId, InventLocationId = "JED-1", WMSLocationId = "PICK-01", CheckText = "Picking Location 01", LocationType = 3 },

            new WMSLocation { DataAreaId = DataAreaId, InventLocationId = "RIY-1", WMSLocationId = "RECEIPT", CheckText = "Inbound Receipt Dock", LocationType = 1 },
            new WMSLocation { DataAreaId = DataAreaId, InventLocationId = "RIY-1", WMSLocationId = "ISSUE", CheckText = "Outbound Shipping Dock", LocationType = 2 },
            new WMSLocation { DataAreaId = DataAreaId, InventLocationId = "RIY-1", WMSLocationId = "BULK-01", CheckText = "Bulk Storage 01", LocationType = 0 },
            new WMSLocation { DataAreaId = DataAreaId, InventLocationId = "RIY-1", WMSLocationId = "PICK-01", CheckText = "Picking Location 01", LocationType = 3 },

            new WMSLocation { DataAreaId = DataAreaId, InventLocationId = "DMM-1", WMSLocationId = "RECEIPT", CheckText = "Inbound Receipt Dock", LocationType = 1 },
            new WMSLocation { DataAreaId = DataAreaId, InventLocationId = "DMM-1", WMSLocationId = "ISSUE", CheckText = "Outbound Shipping Dock", LocationType = 2 },
            new WMSLocation { DataAreaId = DataAreaId, InventLocationId = "DMM-1", WMSLocationId = "BULK-01", CheckText = "Bulk Storage 01", LocationType = 0 },
            new WMSLocation { DataAreaId = DataAreaId, InventLocationId = "DMM-1", WMSLocationId = "PICK-01", CheckText = "Picking Location 01", LocationType = 3 },
        };

        var existingWmsLocations = await db.Set<WMSLocation>()
            .IgnoreQueryFilters()
            .Where(x => x.DataAreaId == DataAreaId)
            .Select(x => new { x.InventLocationId, x.WMSLocationId })
            .ToListAsync(ct);

        var wmsToAdd = wmsLocationSeeds
            .Where(x => !existingWmsLocations.Any(e => e.InventLocationId == x.InventLocationId && e.WMSLocationId == x.WMSLocationId))
            .ToList();

        if (wmsToAdd.Count > 0)
        {
            await db.Set<WMSLocation>().AddRangeAsync(wmsToAdd, ct);
            await db.SaveChangesAsync(ct);
        }
    }

    private static InventLocation MainWarehouse(
        string warehouseId,
        string name,
        string siteId,
        string quarantineWarehouseId,
        string transitWarehouseId)
    {
        var warehouse = CreateWarehouse(warehouseId, name, siteId, InventLocationType.Standard);
        warehouse.InventLocationIdQuarantine = quarantineWarehouseId;
        warehouse.InventLocationIdTransit = transitWarehouseId;
        warehouse.ItmInventLocationIdGit = transitWarehouseId;
        return warehouse;
    }

    private static InventLocation RelatedWarehouse(
        string warehouseId,
        string name,
        string siteId,
        InventLocationType type,
        string mainWarehouseId)
    {
        var warehouse = CreateWarehouse(warehouseId, name, siteId, type);
        warehouse.InventLocationIdReqMain = mainWarehouseId;
        return warehouse;
    }

    private static InventLocation CreateWarehouse(
        string warehouseId,
        string name,
        string siteId,
        InventLocationType type) => new()
        {
            InventLocationId = warehouseId,
            Name = name,
            InventSiteId = siteId,
            InventLocationType = type,
            InventLocationLevel = type == InventLocationType.Standard ? 0 : 1,
            DefaultStatusID = "AVAILABLE",
            WmsLocationIdDefaultReceipt = "RECEIPT",
            WmsLocationIdDefaultIssue = "ISSUE",
            DefaultProductionInputLocation = "PROD-IN",
            DefaultProductionFinishGoodsLocation = "PROD-OUT",
            DefaultKanbanFinishedGoodsLocation = "KANBAN",
            DataAreaId = DataAreaId
        };
}
