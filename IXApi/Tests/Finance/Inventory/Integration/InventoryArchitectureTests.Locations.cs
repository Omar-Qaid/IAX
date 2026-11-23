using System.Reflection;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip.Dtos;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Services;
using IAX.IXApi.Modules.Finance.Common;
using System.Text.Json;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory;
using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using IAX.IXApi.Modules.Finance.Inventory.ProductCategories;
using IAX.IXApi.Modules.Finance.Inventory.ProductCategories.Entities;
using IAX.IXApi.Modules.Finance.Inventory.InventoryStatus;
using IAX.IXApi.Modules.Finance.Inventory.InventoryStatus.Entities;
using IAX.IXApi.Modules.Finance.Inventory.ProductLifecycle;
using IAX.IXApi.Modules.Finance.Inventory.ProductLifecycle.Entities;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Shared.Application.Identity;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;
using ReleasedItem = IAX.IXApi.Modules.Finance.Entities.InventTable;
using SharedProduct = IAX.IXApi.Modules.Finance.Inventory.InventTable.EcoResProduct;

namespace IAX.IXApi.Tests;

public sealed partial class InventoryArchitectureTests
{
    [Fact]
    public async Task Warehouse_create_update_and_delete_preserve_guards_and_company_scope()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(new InventSite { RecId = 1, DataAreaId = "dat", SiteId = "S1", Name = "Site" },
            new InventLocation { RecId = 20, DataAreaId = "other", InventLocationId = "MAIN", InventSiteId = "S1", Name = "Other" },
            new InventLocation { RecId = 21, DataAreaId = "dat", InventLocationId = "REF", InventSiteId = "S1", Name = "Reference" });
        await db.SaveChangesAsync();
        var identity = new TestIdentity();
        var service = new InventLocationService(new TestUnitOfWork(db), identity, identity, new InventLocationUpdateValidator());
        var input = new LocationInputDto { InventLocationId = " main ", InventSiteId = " s1 ", Name = " Main ",
            InventLocationIdReqMain = " ref ", WorkflowApproval = true, DefaultStatusId = " Available " };
        var created = await service.CreateAsync(input, default);
        Assert.Equal(LocationOperationStatus.Success, created.Status);
        Assert.Equal("MAIN", created.Location!.InventLocationId);
        Assert.Equal("Main", created.Location.Name);
        Assert.Equal("REF", created.Location.InventLocationIdReqMain);
        Assert.True(created.Location.WorkflowApproval);
        Assert.False(created.Location.WhsEnabled);
        Assert.Equal("Available", created.Location.DefaultStatusId);
        var lookupJson = JsonSerializer.SerializeToElement(await service.LookupsAsync(default));
        Assert.Equal(new[] { "sites", "warehouses" }, lookupJson.EnumerateObject().Select(x => x.Name).OrderBy(x => x, StringComparer.Ordinal));
        var siteLookup = Assert.Single(lookupJson.GetProperty("sites").EnumerateArray());
        Assert.Equal(new[] { "code", "id", "name" }, siteLookup.EnumerateObject().Select(x => x.Name).OrderBy(x => x, StringComparer.Ordinal));
        Assert.Equal("S1", siteLookup.GetProperty("id").GetString());
        Assert.Equal(2, lookupJson.GetProperty("warehouses").GetArrayLength());
        var warehouseLookup = lookupJson.GetProperty("warehouses").EnumerateArray().Single(x => x.GetProperty("id").GetString() == "MAIN");
        Assert.Equal(new[] { "code", "id", "name", "siteId" }, warehouseLookup.EnumerateObject().Select(x => x.Name).OrderBy(x => x, StringComparer.Ordinal));
        Assert.Equal("S1", warehouseLookup.GetProperty("siteId").GetString());
        Assert.Equal(created.Location.RecId.ToString(), created.Location.Id);
        Assert.Equal(LocationOperationStatus.Duplicate, (await service.CreateAsync(input, default)).Status);
        Assert.Equal(LocationOperationStatus.Referenced, await service.DeleteAsync(21, default));
        Assert.Equal(LocationOperationStatus.NotFound, (await service.UpdateAsync(20, input, default)).Status);
        Assert.Equal(LocationOperationStatus.NotFound, await service.DeleteAsync(20, default));
        input.InventLocationId = "NEW";
        Assert.Equal(LocationOperationStatus.CodeChanged, (await service.UpdateAsync(created.Location.RecId, input, default)).Status);
        input.InventLocationId = "MAIN";
        input.InventLocationIdReqMain = "MAIN";
        var invalid = await service.UpdateAsync(created.Location.RecId, input, default);
        Assert.Equal("A warehouse cannot reference itself.", invalid.Error);
        input.InventLocationIdReqMain = "MISSING";
        Assert.Equal("Warehouse 'MISSING' was not found in site 'S1'.", (await service.UpdateAsync(created.Location.RecId, input, default)).Error);
        input.InventLocationIdReqMain = "";
        input.Name = " Updated ";
        Assert.Equal("Updated", (await service.UpdateAsync(created.Location.RecId, input, default)).Location!.Name);
        db.Add(new InventSite { RecId = 2, DataAreaId = "dat", SiteId = "S2", Name = "Second" });
        db.Add(new InventJournalTable { RecId = 30, DataAreaId = "dat", JournalId = "J-1", JournalNameId = "MOV",
            Description = "Draft", InventSiteId = "S1", InventLocationId = "MAIN" });
        await db.SaveChangesAsync();
        input.InventSiteId = "S2";
        Assert.Equal(LocationOperationStatus.InvalidReference,
            (await service.UpdateAsync(created.Location.RecId, input, default)).Status);
        Assert.Equal(LocationOperationStatus.Referenced, await service.DeleteAsync(created.Location.RecId, default));
        db.Remove(await db.Set<InventJournalTable>().SingleAsync(x => x.JournalId == "J-1"));
        await db.SaveChangesAsync();
        Assert.Equal(LocationOperationStatus.Success, await service.DeleteAsync(created.Location.RecId, default));
        Assert.Single(await service.ListAsync(default));
        Assert.Equal(2, await db.Set<InventLocation>().IgnoreQueryFilters().CountAsync());
    }

    [Fact]
    public async Task Site_with_draft_journal_cannot_be_deleted()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.Add(new InventSite { RecId = 1, DataAreaId = "dat", SiteId = "S1", Name = "Site" });
        db.Add(new InventJournalTable { RecId = 2, DataAreaId = "dat", JournalId = "J-1", JournalNameId = "MOV",
            Description = "Draft", InventSiteId = "S1", InventLocationId = "" });
        await db.SaveChangesAsync();
        var identity = new TestIdentity();
        var service = new InventSiteService(new TestUnitOfWork(db), identity, identity, new InventSiteUpdateValidator());
        Assert.Equal(InventSiteDeleteResult.HasWarehouses, await service.DeleteAsync(1, default));
        Assert.NotNull(await db.Set<InventSite>().SingleOrDefaultAsync(x => x.RecId == 1));
    }

    [Fact]
    public async Task Inventory_parameters_are_company_scoped_and_validate_basic_D365_dependencies()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new UnitOfMeasure { RecId = 1, Symbol = "pcs" },
            new InventSite { RecId = 2, DataAreaId = "dat", SiteId = "JED", Name = "Jeddah" },
            new InventSite { RecId = 3, DataAreaId = "other", SiteId = "DXB", Name = "Dubai" },
            new InventLocation { RecId = 4, DataAreaId = "dat", InventLocationId = "MAIN", InventSiteId = "JED", Name = "Main" },
            new InventLocation { RecId = 5, DataAreaId = "dat", InventLocationId = "WRONG", InventSiteId = "OTHER", Name = "Wrong site" },
            new InventJournalName { RecId = 6, DataAreaId = "dat", JournalNameId = "MOV", Description = "Movement", JournalType = InventJournalType.Movement },
            new InventJournalName { RecId = 7, DataAreaId = "dat", JournalNameId = "COUNT", Description = "Counting", JournalType = InventJournalType.Counting },
            new InventJournalName { RecId = 8, DataAreaId = "other", JournalNameId = "HIDDEN", Description = "Hidden", JournalType = InventJournalType.Movement });
        await db.SaveChangesAsync();
        var identity = new TestIdentity();
        var service = new InventParametersService(new TestUnitOfWork(db), identity);

        var invalidWarehouse = await service.SaveAsync(new InventParametersDto { DefaultUnitId = "pcs", MultiSiteDefaultInventSiteId = "JED", MultiSiteDefaultInventLocationId = "WRONG" });
        Assert.Contains("must belong", invalidWarehouse.Error);
        var invalidJournal = await service.SaveAsync(new InventParametersDto { MovementJournalNameId = "COUNT" });
        Assert.Contains("does not exist for Movement", invalidJournal.Error);
        var orderedReservation = await service.SaveAsync(new InventParametersDto { ReserveOnOrdered = true });
        Assert.Contains("disabled in the basic inventory scope", orderedReservation.Error);

        var saved = await service.SaveAsync(new InventParametersDto
        {
            DefaultUnitId = "pcs", MultiSiteDefaultInventSiteId = "jed", MultiSiteDefaultInventLocationId = "main",
            MovementJournalNameId = " mov ", CountJournalNameId = " count ", MandatoryItemTaxGroup = true,
        });
        Assert.Null(saved.Error);
        Assert.Equal("JED", saved.Value!.MultiSiteDefaultInventSiteId);
        Assert.Equal("MAIN", saved.Value.MultiSiteDefaultInventLocationId);
        Assert.Equal("MOV", saved.Value.MovementJournalNameId);
        Assert.Equal("COUNT", saved.Value.CountJournalNameId);
        Assert.False(saved.Value.ReserveOnOrdered);
        var lookup = (await service.GetLookupsAsync())!;
        Assert.Single(lookup.Sites);
        Assert.Equal(2, lookup.Warehouses.Count);
        Assert.Equal(2, lookup.JournalNames.Count);
        Assert.DoesNotContain(lookup.JournalNames, x => x.Id == "HIDDEN");
        Assert.Single(await db.Set<InventParameters>().ToListAsync());
    }

    [Fact]
    public async Task Basic_inventory_locations_are_company_scoped_and_protect_used_dimensions()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new InventLocation { RecId = 1, DataAreaId = "dat", InventLocationId = "MAIN", InventSiteId = "JED", Name = "Main" },
            new InventLocation { RecId = 2, DataAreaId = "other", InventLocationId = "OTHER", InventSiteId = "JED", Name = "Other" });
        await db.SaveChangesAsync();
        var service = new WMSLocationService(new TestUnitOfWork(db), new TestIdentity());

        var invalidWarehouse = await service.CreateAsync(new WMSLocationInputDto { InventLocationId = "OTHER", WMSLocationId = "A-01" }, default);
        Assert.Contains("current company", invalidWarehouse.Error);
        var created = await service.CreateAsync(new WMSLocationInputDto { InventLocationId = "MAIN", WMSLocationId = "A-01", CheckText = "Aisle one" }, default);
        Assert.Null(created.Error);
        Assert.Equal("Main", created.Location!.WarehouseName);
        Assert.Single(await service.ListAsync(default));
        Assert.NotNull((await service.CreateAsync(new WMSLocationInputDto { InventLocationId = "MAIN", WMSLocationId = "A-01" }, default)).Error);
        var second = await service.CreateAsync(new WMSLocationInputDto { InventLocationId = "MAIN", WMSLocationId = "A-02", InputLocation = "A-01" }, default);
        Assert.Null(second.Error);
        Assert.Contains("cannot be changed", (await service.UpdateAsync(created.Location.RecId,
            new WMSLocationInputDto { InventLocationId = "MAIN", WMSLocationId = "A-03" }, default)).Error);

        db.Add(new InventDim { RecId = 3, DataAreaId = "dat", InventDimId = "D-1", InventLocationId = "MAIN", WmsLocationId = "A-01" });
        await db.SaveChangesAsync();
        Assert.Contains("cannot be deleted", await service.DeleteAsync(created.Location.RecId, default));
        Assert.Null(await service.DeleteAsync(second.Location!.RecId, default));
    }
}
