using System.Reflection;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
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

public sealed class InventoryArchitectureTests
{
    [Fact]
    public async Task Shared_product_creation_consumes_existing_number_sequence_in_its_transaction()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.Add(new SysNumberSequence
        {
            NumberSequence = "EcoResProduct", Txt = "Product sequence", DataAreaId = "dat", Format = "PRD-######",
            AnnotatedFormat = "{PREFIX}-{SEQ}", Lowest = 1, Highest = 999999,
            NextRec = 1, InUse = 1, Manual = 0
        });
        await db.SaveChangesAsync();
        var identity = new TestIdentity();
        var work = new TestUnitOfWork(db);
        var sequences = new SysNumberSequenceService(work, identity);
        var service = new ReleasedProductListService(work, identity,
            new InventDimensionResolver(work, new TestSalesInventoryNumbers()), sequences);

        var first = await service.CreateSharedAsync(new CreateSharedProductDto
            { Name = "First", SearchName = "First", ProductType = "Item" });
        var second = await service.CreateSharedAsync(new CreateSharedProductDto
            { Name = "Second", SearchName = "Second", ProductType = "Item" });

        Assert.Null(first.Error);
        Assert.Null(second.Error);
        Assert.Equal(new[] { "PRD-000001", "PRD-000002" },
            await db.Set<SharedProduct>().OrderBy(x => x.DisplayProductNumber)
                .Select(x => x.DisplayProductNumber).ToArrayAsync());
        Assert.Equal(3, (await db.Set<SysNumberSequence>().SingleAsync()).NextRec);
    }

    [Fact]
    public async Task Basic_product_and_inventory_statuses_block_transactions_and_availability()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var identity = new TestIdentity();
        var work = new TestUnitOfWork(db);
        var lifecycle = new ProductLifecycleService(work, identity);
        var inventoryStatuses = new InventoryStatusService(work, identity);
        var state = await lifecycle.SaveAsync(null,
            new ProductLifecycleStateInputDto("BLOCKED", "Blocked", false, true, false, true), default);
        Assert.Null(state.Error);
        var status = await inventoryStatuses.SaveAsync(null,
            new InventoryStatusInputDto("HOLD", "Quality hold", true), default);
        Assert.Null(status.Error);
        db.AddRange(
            new ReleasedItem { RecId = 810, DataAreaId = "dat", ItemId = "A-810", Name = "Item", ProductLifecycleStateId = "BLOCKED" },
            new InventDim { RecId = 811, DataAreaId = "dat", InventDimId = "DIM-810", InventStatusId = "HOLD" },
            new InventSum { RecId = 812, DataAreaId = "dat", ItemId = "A-810", InventDimId = "DIM-810",
                PhysicalInvent = 5, AvailPhysical = 5, AvailOrdered = 5 });
        await db.SaveChangesAsync();

        Assert.Contains("blocks sales", await ProductLifecyclePolicy.ValidateAsync(db, "dat", "A-810", ProductLifecycleProcess.Sales, default));
        Assert.Contains("blocks inventory", await ProductLifecyclePolicy.ValidateAsync(db, "dat", "A-810", ProductLifecycleProcess.Inventory, default));
        Assert.Null(await ProductLifecyclePolicy.ValidateAsync(db, "dat", "A-810", ProductLifecycleProcess.Purchase, default));
        var onHand = Assert.Single((await new InventOnHandService(work, identity).ListAsync())!);
        Assert.Equal(5, onHand.PhysicalInventory);
        Assert.Equal(0, onHand.AvailablePhysical);
        Assert.True(onHand.InventoryStatusBlocked);
        Assert.NotNull(await inventoryStatuses.DeleteAsync(status.Row!.RecId, default));
    }

    [Fact]
    public async Task Product_categories_reject_inactive_assignments_and_list_products_by_category()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var service = new ProductCategoryService(new TestUnitOfWork(db));
        var hierarchy = await service.SaveHierarchyAsync(null, new("Products"), default);
        Assert.Null(hierarchy.Error);
        var category = await service.SaveCategoryAsync(null, new(hierarchy.Row!.RecId, 0, "Tools"), default);
        Assert.Null(category.Error);
        db.Set<SharedProduct>().Add(new SharedProduct { RecId = 501, DisplayProductNumber = "TOOL-1" });
        await db.SaveChangesAsync();
        Assert.Null((await service.SaveAssignmentAsync(501, null, new(category.Row!.RecId), default)).Error);
        Assert.Equal("TOOL-1", Assert.Single((await service.ListProductsAsync(category.Row.RecId, default)).Rows!).ProductNumber);
        Assert.NotNull((await service.SaveAssignmentAsync(501, null, new(category.Row.RecId), default)).Error);
        Assert.NotNull((await service.SaveCategoryAsync(category.Row.RecId,
            new(hierarchy.Row.RecId, 0, "Tools", false), default)).Error);
    }

    [Fact]
    public async Task Barcode_maintenance_validates_released_item_unit_uniqueness_and_company_scope()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new ReleasedItem { RecId = 1, DataAreaId = "dat", ItemId = "ITEM-1" },
            new ReleasedItem { RecId = 2, DataAreaId = "other", ItemId = "ITEM-2" },
            new ReleasedItem { RecId = 5, DataAreaId = "dat", ItemId = "ITEM-3" },
            new UnitOfMeasure { RecId = 3, Symbol = "pcs" },
            new InventItemBarcode { RecId = 4, DataAreaId = "other", ItemId = "ITEM-2", ItemBarcode = "OTHER", UnitId = "pcs", Qty = 1 });
        await db.SaveChangesAsync();
        var service = new InventItemBarcodeService(new TestUnitOfWork(db), new TestIdentity());
        var input = new InventItemBarcodeInputDto("12345", "pcs", 1);

        Assert.NotNull((await service.CreateAsync(2, input, default)).Error);
        Assert.Contains("unit", (await service.CreateAsync(1, input with { UnitId = "missing" }, default)).Error!, StringComparison.OrdinalIgnoreCase);
        var created = await service.CreateAsync(1, input, default);
        Assert.Null(created.Error);
        Assert.Equal("ITEM-1", created.Row!.ItemId);
        Assert.Null((await service.CreateAsync(5, input, default)).Error);
        Assert.Contains("already exists", (await service.CreateAsync(1, input, default)).Error!);
        Assert.Single((await service.ListAsync(1, default)).Rows!);
        var matches = (await service.FindAsync("12345", default)).Rows!;
        Assert.Equal(2, matches.Count);
        Assert.Contains(matches, x => x.ReleasedProductRecId == 1);
        Assert.DoesNotContain(matches, x => x.Barcode.ItemId == "ITEM-2");
        var stored = await db.Set<InventItemBarcode>().SingleAsync(x => x.RecId == created.Row.RecId);
        stored.BarcodeSetupId = "LEGACY";
        stored.RetailVariantId = "LEGACY";
        await db.SaveChangesAsync();
        Assert.Null((await service.UpdateAsync(1, created.Row.RecId, input with { Qty = 2, Blocked = true }, default)).Error);
        Assert.True((await service.ListAsync(1, default)).Rows![0].Blocked);
        Assert.Single((await service.FindAsync("12345", default)).Rows!);
        Assert.Equal("LEGACY", stored.BarcodeSetupId);
        Assert.Equal("LEGACY", stored.RetailVariantId);
        Assert.False((await service.DeleteAsync(1, 4, default)).Deleted);
        Assert.True((await service.DeleteAsync(1, created.Row.RecId, default)).Deleted);
        Assert.Equal(2, await db.Set<InventItemBarcode>().CountAsync());
    }

    static InventoryArchitectureTests()
    {
        new InventLocationMapping().Register(TypeAdapterConfig.GlobalSettings);
        new InventTransMapping().Register(TypeAdapterConfig.GlobalSettings);
    }

    [Fact]
    public async Task Item_model_group_rejects_costing_methods_without_required_close_or_revaluation_process()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.Set<InventModelGroup>().Add(new InventModelGroup
        {
            DataAreaId = "dat", ModelGroupId = "FIFO", Name = "FIFO", InventModel = InventModel.FIFO,
            StockedProduct = NoYes.Yes
        });
        await db.SaveChangesAsync();
        var service = new InventModelGroupService(new TestUnitOfWork(db), new TestIdentity(), new TestIdentity());

        var fifo = await service.CreateAsync(new InventModelGroupRecordDto
        { ModelGroupId = "FIFO-NEW", Name = "FIFO", InventModel = (int)InventModel.FIFO }, default);
        var weightedAverage = await service.CreateAsync(new InventModelGroupRecordDto
        { ModelGroupId = "AVG-NEW", Name = "Weighted average", InventModel = (int)InventModel.WeightedAvg }, default);
        var standardCost = await service.CreateAsync(new InventModelGroupRecordDto
        { ModelGroupId = "STD-NEW", Name = "Standard cost", InventModel = (int)InventModel.StandardCost }, default);
        var movingAverage = await service.CreateAsync(new InventModelGroupRecordDto
        { ModelGroupId = "MOV-AVG", Name = "Moving average", InventModel = (int)InventModel.MovingAverage }, default);

        Assert.Contains("inventory close/settlement", fifo.Error);
        Assert.Contains("inventory close/settlement", weightedAverage.Error);
        Assert.Contains("active standard-cost record", standardCost.Error);
        Assert.Null(movingAverage.Error);
        Assert.Single(await db.Set<InventModelGroup>().Where(x => x.ModelGroupId == "MOV-AVG").ToListAsync());
    }

    [Fact]
    public async Task Journal_posting_rejects_periodic_costing_without_inventory_close()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new InventJournalTable { RecId = 1, DataAreaId = "dat", JournalId = "J-1", JournalNameId = "ADJ", JournalType = InventJournalType.Adjustment },
            new InventJournalTrans { RecId = 2, DataAreaId = "dat", JournalId = "J-1", JournalType = InventJournalType.Adjustment, ItemId = "ITEM-1", InventDimId = "DIM-1", Unit = "pcs", Qty = 1, CostPrice = 10 },
            new InventJournalName { RecId = 3, DataAreaId = "dat", JournalNameId = "ADJ", JournalType = InventJournalType.Adjustment, VoucherNumberSequenceTable = 4 },
            new SysNumberSequence { RecId = 4, DataAreaId = "dat", NumberSequence = "INV-VOUCHER", Txt = "Inventory voucher", Format = "IV-#####", AnnotatedFormat = "IV-{SEQ}", InUse = 1, IsActive = true, NextRec = 1, Highest = 99999 },
            new ReleasedItem { RecId = 5, DataAreaId = "dat", ItemId = "ITEM-1", Product = 6 },
            new InventModelGroup { RecId = 7, DataAreaId = "dat", ModelGroupId = "FIFO", Name = "FIFO", InventModel = InventModel.FIFO, StockedProduct = NoYes.Yes },
            new InventModelGroupItem { RecId = 8, DataAreaId = "dat", ItemId = "ITEM-1", ItemDataAreaId = "dat", ModelGroupId = "FIFO", ModelGroupDataAreaId = "dat" },
            new EcoResTrackingDimensionGroupItem { RecId = 9, DataAreaId = "dat", ItemId = "ITEM-1", ItemDataAreaId = "dat", TrackingDimensionGroup = 10 },
            new EcoResTrackingDimensionGroup { RecId = 10, Name = "No tracking" },
            new InventDim { RecId = 11, DataAreaId = "dat", InventDimId = "DIM-1", InventSiteId = "JED", InventLocationId = "MAIN" });
        await db.SaveChangesAsync();
        var identity = new TestIdentity();
        var unitOfWork = new TestUnitOfWork(db);
        var posting = new InventJournalPostingService(unitOfWork, identity, identity,
            new SysNumberSequenceService(unitOfWork, identity), new TestSalesInventoryNumbers());

        var result = await posting.PostAsync("J-1", default);

        Assert.Contains("requires inventory close/settlement", result.Error);
        Assert.Empty(await db.Set<InventTrans>().ToListAsync());
        Assert.Empty(await db.Set<InventTransOrigin>().ToListAsync());
        Assert.False((await db.Set<InventJournalTable>().SingleAsync()).Posted == NoYes.Yes);
    }

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
            InventLocationIdTransit = " ref ", WorkflowApproval = true, WhsEnabled = true, DefaultStatusId = " Available " };
        var created = await service.CreateAsync(input, default);
        Assert.Equal(LocationOperationStatus.Success, created.Status);
        Assert.Equal("MAIN", created.Location!.InventLocationId);
        Assert.Equal("Main", created.Location.Name);
        Assert.Equal("REF", created.Location.InventLocationIdTransit);
        Assert.True(created.Location.WorkflowApproval);
        Assert.True(created.Location.WhsEnabled);
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
        input.InventLocationIdTransit = "MAIN";
        var invalid = await service.UpdateAsync(created.Location.RecId, input, default);
        Assert.Equal("A warehouse cannot reference itself.", invalid.Error);
        input.InventLocationIdTransit = "MISSING";
        Assert.Equal("Warehouse 'MISSING' was not found in site 'S1'.", (await service.UpdateAsync(created.Location.RecId, input, default)).Error);
        input.InventLocationIdTransit = "";
        input.Name = " Updated ";
        Assert.Equal("Updated", (await service.UpdateAsync(created.Location.RecId, input, default)).Location!.Name);
        Assert.Equal(LocationOperationStatus.Success, await service.DeleteAsync(created.Location.RecId, default));
        Assert.Single(await service.ListAsync(default));
        Assert.Equal(2, await db.Set<InventLocation>().IgnoreQueryFilters().CountAsync());
    }

    [Fact]
    public async Task Transaction_query_preserves_costs_labels_order_and_company_scope()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new InventTrans { RecId = 1, DataAreaId = "dat", ItemId = "ITEM", InventTransOrigin = 10, InventDimId = "DIM", Qty = -2,
                CostAmountPosted = -20, CostAmountPhysical = -4, CostAmountAdjustment = 2, StatusIssue = StatusIssue.Ordered },
            new InventTrans { RecId = 2, DataAreaId = "other", ItemId = "HIDDEN" },
            new InventTrans { RecId = 3, DataAreaId = "dat", ItemId = "EMPTY", Qty = 0 },
            new InventTransOrigin { RecId = 10, DataAreaId = "dat", InventTransId = "TX", ReferenceCategory = InventRefType.SalesTable, ReferenceId = "SO1" },
            new InventDim { RecId = 11, DataAreaId = "dat", InventDimId = "DIM", InventSiteId = "S1", InventLocationId = "W1" },
            new SalesLine { RecId = 12, DataAreaId = "dat", InventTransId = "TX", SalesPrice = 42 });
        await db.SaveChangesAsync();
        var identity = new TestIdentity();
        var service = new InventTransService(new TestUnitOfWork(db), identity);
        var rows = (await service.GetListAsync())!;
        Assert.Equal(new long[] { 3, 1 }, rows.Select(x => x.RecId));
        Assert.Equal(0, rows[0].UnitCost);
        Assert.Empty(rows[0].Reference);
        Assert.Null(rows[0].PhysicalDate);
        Assert.Equal(-22, rows[1].CostAmount);
        Assert.Equal(11, rows[1].UnitCost);
        Assert.Equal(42, rows[1].UnitPrice);
        Assert.Equal("On order", rows[1].IssueStatus);
        Assert.Equal("Sales order", rows[1].Reference);
        Assert.Equal("SO1", rows[1].ReferenceNumber);
        Assert.Equal("W1", rows[1].Warehouse);
        identity.DataAreaId = null;
        Assert.Null(await service.GetListAsync());
        Assert.IsType<BadRequestObjectResult>((await new InventTransController(service).GetList()).Result);
    }

    [Fact]
    public async Task On_hand_query_returns_basic_dimensions_and_standard_summary_buckets_for_selected_company()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new ReleasedItem { RecId = 1, DataAreaId = "dat", ItemId = "ITEM-001", Name = "Office Chair" },
            new ReleasedItem { RecId = 2, DataAreaId = "other", ItemId = "ITEM-001", Name = "Hidden Chair" },
            new InventDim { RecId = 3, DataAreaId = "dat", InventDimId = "DIM-1", InventSiteId = "JED", InventLocationId = "MAIN" },
            new InventDim { RecId = 4, DataAreaId = "other", InventDimId = "DIM-1", InventSiteId = "DXB", InventLocationId = "MAIN" },
            new InventSum
            {
                RecId = 5, DataAreaId = "dat", ItemId = "ITEM-001", InventDimId = "DIM-1",
                PhysicalInvent = 10, ReservPhysical = 2, AvailPhysical = 8,
                Ordered = 4, OnOrder = 3, AvailOrdered = 9, PostedValue = 700, PhysicalValue = 100
            },
            new InventSum { RecId = 6, DataAreaId = "other", ItemId = "ITEM-001", InventDimId = "DIM-1", PhysicalInvent = 99 });
        await db.SaveChangesAsync();

        var identity = new TestIdentity();
        var rows = (await new InventOnHandService(new TestUnitOfWork(db), identity).ListAsync())!;

        var row = Assert.Single(rows);
        Assert.Equal("ITEM-001", row.ItemNumber);
        Assert.Equal("Office Chair", row.ProductName);
        Assert.Equal("JED", row.Site);
        Assert.Equal("MAIN", row.Warehouse);
        Assert.Equal(10, row.PhysicalInventory);
        Assert.Equal(2, row.PhysicalReserved);
        Assert.Equal(8, row.AvailablePhysical);
        Assert.Equal(4, row.OrderedReceipts);
        Assert.Equal(3, row.OrderedIssues);
        Assert.Equal(9, row.AvailableOrdered);
        Assert.Equal(700, row.PostedValue);
        Assert.Equal(100, row.PhysicalValue);

        identity.DataAreaId = null;
        Assert.Null(await new InventOnHandService(new TestUnitOfWork(db), identity).ListAsync());
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

        var saved = await service.SaveAsync(new InventParametersDto
        {
            DefaultUnitId = "pcs", MultiSiteDefaultInventSiteId = "jed", MultiSiteDefaultInventLocationId = "main",
            MovementJournalNameId = "MOV", CountJournalNameId = "COUNT", ReserveOnOrdered = true, MandatoryItemTaxGroup = true,
        });
        Assert.Null(saved.Error);
        Assert.Equal("JED", saved.Value!.MultiSiteDefaultInventSiteId);
        Assert.Equal("MAIN", saved.Value.MultiSiteDefaultInventLocationId);
        Assert.True(saved.Value.ReserveOnOrdered);
        var lookup = (await service.GetLookupsAsync())!;
        Assert.Single(lookup.Sites);
        Assert.Equal(2, lookup.Warehouses.Count);
        Assert.Equal(2, lookup.JournalNames.Count);
        Assert.DoesNotContain(lookup.JournalNames, x => x.Id == "HIDDEN");
        Assert.Single(await db.Set<InventParameters>().ToListAsync());
    }

    [Fact]
    public async Task Unit_setup_validates_duplicates_conversions_and_used_unit_restrictions()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.Add(new SharedProduct { RecId = 100, DisplayProductNumber = "ITEM-001", ProductType = ItemType.Item });
        await db.SaveChangesAsync();
        var service = new UnitOfMeasureService(new TestUnitOfWork(db));
        var pcs = await service.CreateAsync(new UnitOfMeasureInputDto { Symbol = "pcs", UnitOfMeasureClass = 0, SystemOfUnits = 0, DecimalPrecision = 0 });
        var box = await service.CreateAsync(new UnitOfMeasureInputDto { Symbol = "box", UnitOfMeasureClass = 0, SystemOfUnits = 0, DecimalPrecision = 0 });
        Assert.NotNull(pcs.Unit);
        Assert.NotNull(box.Unit);
        Assert.NotNull((await service.CreateAsync(new UnitOfMeasureInputDto { Symbol = "pcs" })).Error);
        var conversion = await service.SaveConversionAsync(null, new UnitOfMeasureConversionInputDto
        { FromUnitOfMeasure = box.Unit!.RecId, ToUnitOfMeasure = pcs.Unit!.RecId, Product = 100, Factor = 4 });
        Assert.Equal(4, conversion.Conversion!.Factor);
        Assert.Equal("ITEM-001", conversion.Conversion.ProductNumber);
        Assert.NotNull((await service.SaveConversionAsync(null, new UnitOfMeasureConversionInputDto
        { FromUnitOfMeasure = box.Unit.RecId, ToUnitOfMeasure = pcs.Unit.RecId, Product = 100, Factor = 4 })).Error);
        db.Add(new InventTableModule { RecId = 200, DataAreaId = "dat", ItemId = "ITEM-001", UnitId = "pcs", ModuleType = ModuleInventPurchSales.Inventory });
        await db.SaveChangesAsync();
        Assert.NotNull((await service.UpdateAsync(pcs.Unit.RecId, new UnitOfMeasureInputDto { Symbol = "ea", UnitOfMeasureClass = 0, SystemOfUnits = 0 })).Error);
        Assert.NotNull(await service.DeleteAsync(pcs.Unit.RecId));
        Assert.Null(await service.DeleteConversionAsync(conversion.Conversion.RecId));
    }

    [Fact]
    public void Released_product_conversion_prefers_product_specific_rule_and_falls_back_to_standard_rule()
    {
        var method = typeof(ReleasedProductListService).GetMethod("ConversionFactor", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        var units = new List<UnitOfMeasure>
        {
            new() { RecId = 1, Symbol = "box" },
            new() { RecId = 2, Symbol = "pcs" }
        };
        var conversions = new List<UnitOfMeasureConversion>
        {
            new() { RecId = 10, Product = 0, FromUnitOfMeasure = 1, ToUnitOfMeasure = 2, Factor = 4 },
            new() { RecId = 11, Product = 100, FromUnitOfMeasure = 1, ToUnitOfMeasure = 2, Factor = 6 }
        };

        Assert.Equal(6m, (decimal)method!.Invoke(null, [conversions, units, 100L, "box", "pcs"])!);
        Assert.Equal(4m, (decimal)method.Invoke(null, [conversions, units, 200L, "box", "pcs"])!);
        Assert.Equal(0.25m, (decimal)method.Invoke(null, [conversions, units, 200L, "pcs", "box"])!);
    }

    [Fact]
    public async Task Inventory_journal_names_are_company_scoped_and_protect_used_setup()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new SysNumberSequence { RecId = 10, DataAreaId = "dat", NumberSequence = "InventJournalVoucher", Txt = "Inventory voucher", Format = "IV-#####", AnnotatedFormat = "IV-{SEQ}", InUse = 1, IsActive = true },
            new SysNumberSequence { RecId = 11, DataAreaId = "other", NumberSequence = "Hidden", Txt = "Hidden", Format = "H-#####", AnnotatedFormat = "H-{SEQ}", InUse = 1, IsActive = true },
            new InventJournalName { RecId = 20, DataAreaId = "other", JournalNameId = "HIDDEN", Description = "Hidden", JournalType = InventJournalType.Movement });
        await db.SaveChangesAsync();
        var identity = new TestIdentity();
        var service = new InventJournalNameService(new TestUnitOfWork(db), identity, new InventJournalNameInputValidator());

        var created = await service.CreateAsync(new InventJournalNameInputDto
        {
            JournalNameId = " mov ", Description = " Movement ", JournalType = InventJournalType.Movement,
            VoucherNumberSequenceTable = 10, DetailSummary = DetailSummary.Detail
        }, default);
        Assert.Null(created.Error);
        Assert.Equal("MOV", created.JournalName!.JournalNameId);
        Assert.Equal("InventJournalVoucher", created.JournalName.VoucherNumberSequenceCode);
        Assert.Single(await service.ListAsync(default));
        Assert.Single((await service.LookupsAsync(default)).NumberSequences);
        Assert.NotNull((await service.CreateAsync(new InventJournalNameInputDto
        { JournalNameId = "MOV", Description = "Duplicate", JournalType = InventJournalType.Movement }, default)).Error);

        db.Add(new InventJournalTable { RecId = 30, DataAreaId = "dat", JournalId = "J0001", JournalNameId = "MOV", Description = "Used" });
        await db.SaveChangesAsync();
        Assert.Equal(InventJournalNameDeleteResult.InUse, await service.DeleteAsync(created.JournalName.RecId, default));
        var changedType = await service.UpdateAsync(created.JournalName.RecId, new InventJournalNameInputDto
        { JournalNameId = "MOV", Description = "Movement", JournalType = InventJournalType.Counting, VoucherNumberSequenceTable = 10 }, default);
        Assert.Contains("cannot be changed", changedType.Error);
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

    [Fact]
    public async Task Inventory_journal_lifecycle_uses_standard_tables_and_enforces_company_dimensions_and_type_rules()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new SysNumberSequence { RecId = 1, DataAreaId = "dat", NumberSequence = "InventJournalTable", Txt = "Journal", Format = "IJ-#####", AnnotatedFormat = "IJ-{SEQ}", InUse = 1, IsActive = true, NextRec = 1, Highest = 99999 },
            new InventJournalName { RecId = 2, DataAreaId = "dat", JournalNameId = "TRF", Description = "Transfer", JournalType = InventJournalType.Transfer, VoucherNumberSequenceTable = 9 },
            new InventJournalName { RecId = 13, DataAreaId = "dat", JournalNameId = "MOV", Description = "Movement", JournalType = InventJournalType.Movement, VoucherNumberSequenceTable = 9 },
            new InventJournalName { RecId = 16, DataAreaId = "dat", JournalNameId = "COUNT", Description = "Counting", JournalType = InventJournalType.Counting, VoucherNumberSequenceTable = 9 },
            new InventJournalName { RecId = 3, DataAreaId = "other", JournalNameId = "HIDDEN", Description = "Hidden", JournalType = InventJournalType.Transfer },
            new InventJournalName { RecId = 14, DataAreaId = "dat", JournalNameId = "ADJ", Description = "Adjustment", JournalType = InventJournalType.Adjustment, VoucherNumberSequenceTable = 9 },
            new InventSite { RecId = 4, DataAreaId = "dat", SiteId = "JED", Name = "Jeddah" },
            new InventLocation { RecId = 5, DataAreaId = "dat", InventLocationId = "MAIN", InventSiteId = "JED", Name = "Main" },
            new InventLocation { RecId = 6, DataAreaId = "dat", InventLocationId = "SHOW", InventSiteId = "JED", Name = "Showroom" },
            new InventLocation { RecId = 44, DataAreaId = "dat", InventLocationId = "OPEN", InventSiteId = "JED", Name = "Opening stock" },
            new ReleasedItem { RecId = 7, DataAreaId = "dat", ItemId = "ITEM-001", Name = "Office Chair" },
            new UnitOfMeasure { RecId = 8, DataAreaId = "dat", Symbol = "pcs", DecimalPrecision = 0 },
            new UnitOfMeasure { RecId = 45, DataAreaId = "dat", Symbol = "box", DecimalPrecision = 0 },
            new UnitOfMeasureConversion { RecId = 46, DataAreaId = "dat", FromUnitOfMeasure = 45, ToUnitOfMeasure = 8,
                Product = 0, Factor = 4, Numerator = 1, Denominator = 1, Rounding = UnitOfMeasureRounding.Nearest },
            new SysNumberSequence { RecId = 9, DataAreaId = "dat", NumberSequence = "InventJournalVoucher", Txt = "Voucher", Format = "IV-#####", AnnotatedFormat = "IV-{SEQ}", InUse = 1, IsActive = true, NextRec = 1, Highest = 99999 },
            new InventModelGroup { RecId = 10, DataAreaId = "dat", ModelGroupId = "FIFO", Name = "FIFO", InventModel = InventModel.FIFO, StockedProduct = NoYes.Yes },
            new InventModelGroupItem { RecId = 11, DataAreaId = "dat", ItemId = "ITEM-001", ItemDataAreaId = "dat", ModelGroupId = "FIFO", ModelGroupDataAreaId = "dat" },
            new EcoResTrackingDimensionGroupItem { RecId = 40, DataAreaId = "dat", ItemId = "ITEM-001", ItemDataAreaId = "dat", TrackingDimensionGroup = 41 },
            new EcoResTrackingDimensionGroup { RecId = 41, Name = "No tracking" },
            new InventTableModule { RecId = 12, DataAreaId = "dat", ItemId = "ITEM-001", ModuleType = ModuleInventPurchSales.Inventory, UnitId = "pcs" },
            new InventItemGroupItem { RecId = 15, DataAreaId = "dat", ItemId = "ITEM-001", ItemDataAreaId = "dat", ItemGroupId = "FURN", ItemGroupDataAreaId = "dat" },
            new MainAccount { RecId = 20, DataAreaId = "dat", MainAccountId = "11050001", Name = "Inventory", CurrencyCode = "SAR" },
            new MainAccount { RecId = 21, DataAreaId = "dat", MainAccountId = "51010001", Name = "Inventory profit", CurrencyCode = "SAR" },
            new DimensionAttributeValueCombination { RecId = 22, DataAreaId = "dat", MainAccount = 20, MainAccountValue = "11050001", DisplayValue = "11050001" },
            new DimensionAttributeValueCombination { RecId = 23, DataAreaId = "dat", MainAccount = 21, MainAccountValue = "51010001", DisplayValue = "51010001" },
            new DimensionAttributeValueCombination { RecId = 50, DataAreaId = "other", MainAccountValue = "11050001", DisplayValue = "11050001" },
            new InventPosting { RecId = 24, DataAreaId = "dat", InventAccountType = InventAccountType.InventReceipt, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 22 },
            new InventPosting { RecId = 25, DataAreaId = "dat", InventAccountType = InventAccountType.InventProfit, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 23 },
            new InventPosting { RecId = 42, DataAreaId = "dat", InventAccountType = InventAccountType.InventIssue, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 22 },
            new InventPosting { RecId = 43, DataAreaId = "dat", InventAccountType = InventAccountType.InventLoss, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 23 },
            new Ledger { RecId = 26, DataAreaId = "dat", Name = "SA01 ledger", Description = "Ledger", FiscalCalendar = 27, AccountingCurrency = "SAR", ReportingCurrency = "SAR" },
            new FiscalCalendarYear { RecId = 28, DataAreaId = "dat", FiscalCalendar = 27, Name = "Current", StartDate = DateTime.Today.AddMonths(-6), EndDate = DateTime.Today.AddMonths(6) },
            new FiscalCalendarPeriod { RecId = 29, DataAreaId = "dat", FiscalCalendar = 27, FiscalCalendarYear = 28, Name = "Current", ShortName = "CUR", Type = FiscalPeriodType.Operating, StartDate = DateTime.Today.AddDays(-10), EndDate = DateTime.Today.AddDays(10) },
            new LedgerFiscalCalendarPeriod { RecId = 30, DataAreaId = "dat", Ledger = 26, FiscalCalendarPeriod = 29, Status = FiscalPeriodStatus.Open });
        await db.SaveChangesAsync();
        var identity = new TestIdentity();
        var uow = new TestUnitOfWork(db);
        var numbers = new SysNumberSequenceService(uow, identity);
        var dimensions = new InventDimensionResolver(uow, new TestSalesInventoryNumbers());
        var service = new InventJournalService(uow, identity, numbers, dimensions);

        var created = await service.CreateAsync(new InventJournalCreateDto
        { JournalNameId = "trf", Description = "Warehouse transfer", InventSiteId = "JED", InventLocationId = "MAIN" }, default);
        Assert.Null(created.Error);
        Assert.Equal("TRF", created.Journal!.JournalNameId);
        Assert.Equal(InventJournalType.Transfer, created.Journal.JournalType);
        Assert.DoesNotContain((await service.LookupsAsync(default)).JournalNames, x => x.Code == "HIDDEN");
        var accountLookup = await service.LedgerDimensionsAsync("11050001", default);
        Assert.Contains(accountLookup, x => x.Code == "22" && x.Description == "11050001");
        Assert.DoesNotContain(accountLookup, x => x.Code == "50");

        var sameDimension = await service.AddLineAsync(created.Journal.JournalId, new InventJournalLineInputDto
        { ItemId = "ITEM-001", InventSiteId = "JED", InventLocationId = "MAIN", ToInventSiteId = "JED", ToInventLocationId = "MAIN", Unit = "pcs", Qty = 1 }, default);
        Assert.Contains("must be different", sameDimension.Error);
        var added = await service.AddLineAsync(created.Journal.JournalId, new InventJournalLineInputDto
        { ItemId = "ITEM-001", InventSiteId = "JED", InventLocationId = "MAIN", ToInventSiteId = "JED", ToInventLocationId = "SHOW", Unit = "pcs", Qty = 2 }, default);
        Assert.Null(added.Error);
        var line = Assert.Single(added.Journal!.Lines);
        Assert.Equal("MAIN", line.InventLocationId);
        Assert.Equal("SHOW", line.ToInventLocationId);
        Assert.Equal(1, added.Journal.NumOfLines);
        var posting = new InventJournalPostingService(uow, identity, identity, numbers, new TestSalesInventoryNumbers());
        var openingMain = await service.CreateAsync(new InventJournalCreateDto
        { JournalNameId = "MOV", Description = "Initial on-hand", InventSiteId = "JED", InventLocationId = "MAIN" }, default);
        var openingMainLine = await service.AddLineAsync(openingMain.Journal!.JournalId, new InventJournalLineInputDto
        { ItemId = "ITEM-001", InventSiteId = "JED", InventLocationId = "MAIN", Unit = "pcs", Qty = 5,
            CostPrice = 20, LedgerDimension = 22, TransDate = DateTime.Today }, default);
        Assert.Null(openingMainLine.Error);
        Assert.Null((await posting.PostAsync(openingMain.Journal.JournalId, default)).Error);

        var posted = await posting.PostAsync(created.Journal.JournalId, default);
        Assert.Null(posted.Error);
        Assert.True(posted.Result!.Posted);
        Assert.Equal(2, posted.Result.InventoryTransactionsCreated);
        Assert.Equal(0, posted.Result.LedgerEntriesCreated);
        Assert.Equal(3, await db.Set<InventTrans>().CountAsync());
        Assert.Equal(3, (await db.Set<InventSum>().SingleAsync(x => x.InventDimId == line.InventDimId)).PhysicalInvent);
        Assert.Equal(2, (await db.Set<InventSum>().SingleAsync(x => x.InventDimId == line.ToInventDimId)).PhysicalInvent);
        Assert.NotNull((await posting.PostAsync(created.Journal.JournalId, default)).Error);
        Assert.False((await service.DeleteAsync(created.Journal.JournalId, default)).Deleted);

        var adjustment = await service.CreateAsync(new InventJournalCreateDto
        { JournalNameId = "ADJ", Description = "Opening adjustment", InventSiteId = "JED", InventLocationId = "MAIN" }, default);
        var adjusted = await service.AddLineAsync(adjustment.Journal!.JournalId, new InventJournalLineInputDto
        { ItemId = "ITEM-001", InventSiteId = "JED", InventLocationId = "MAIN", Unit = "pcs", Qty = 3, CostPrice = 10, TransDate = DateTime.Today }, default);
        Assert.Null(adjusted.Error);
        Assert.Equal(line.InventDimId, Assert.Single(adjusted.Journal!.Lines).InventDimId);
        var adjustmentPost = await posting.PostAsync(adjustment.Journal.JournalId, default);
        Assert.Null(adjustmentPost.Error);
        Assert.Equal(1, adjustmentPost.Result!.InventoryTransactionsCreated);
        Assert.Equal(1, adjustmentPost.Result.LedgerEntriesCreated);
        var ledgerEntry = await db.Set<GeneralJournalEntry>().SingleAsync(x => x.JournalNumber == adjustment.Journal!.JournalId);
        var accountEntries = await db.Set<GeneralJournalAccountEntry>().Where(x => x.GeneralJournalEntry == ledgerEntry.RecId).ToListAsync();
        Assert.Equal(2, accountEntries.Count);
        Assert.Equal(0, accountEntries.Sum(x => x.AccountingCurrencyAmount));
        Assert.Equal(30, accountEntries.Single(x => x.PostingType == LedgerPostingType.InventReceipt).AccountingCurrencyAmount);
        Assert.Equal(6, (await db.Set<InventSum>().SingleAsync(x => x.InventDimId == line.InventDimId)).PhysicalInvent);

        // D365 initializes basic opening stock through a movement journal; this dimension starts with zero on-hand.
        var opening = await service.CreateAsync(new InventJournalCreateDto
        { JournalNameId = "MOV", Description = "Opening inventory", InventSiteId = "JED", InventLocationId = "OPEN" }, default);
        var openingLine = await service.AddLineAsync(opening.Journal!.JournalId, new InventJournalLineInputDto
        { ItemId = "ITEM-001", InventSiteId = "JED", InventLocationId = "OPEN", Unit = "box", Qty = 1,
            CostPrice = 25, LedgerDimension = 22, TransDate = DateTime.Today }, default);
        Assert.Null(openingLine.Error);
        var openingPost = await posting.PostAsync(opening.Journal.JournalId, default);
        Assert.Null(openingPost.Error);
        Assert.Equal(1, openingPost.Result!.InventoryTransactionsCreated);
        Assert.Equal(1, openingPost.Result.LedgerEntriesCreated);
        Assert.Equal(4, await db.Set<InventTrans>().Where(x => x.InventDimId == Assert.Single(openingLine.Journal!.Lines).InventDimId)
            .Select(x => x.Qty).SingleAsync());
        Assert.Equal(4, (await db.Set<InventSum>().SingleAsync(x => x.ItemId == "ITEM-001" && x.InventLocationId == "OPEN")).PhysicalInvent);

        var issue = await service.CreateAsync(new InventJournalCreateDto
        { JournalNameId = "ADJ", Description = "Issue at running average", InventSiteId = "JED", InventLocationId = "MAIN" }, default);
        var issueLine = await service.AddLineAsync(issue.Journal!.JournalId, new InventJournalLineInputDto
        { ItemId = "ITEM-001", InventSiteId = "JED", InventLocationId = "MAIN", Unit = "pcs", Qty = -1, CostPrice = 999, TransDate = DateTime.Today }, default);
        Assert.Null(issueLine.Error);
        var issuePost = await posting.PostAsync(issue.Journal.JournalId, default);
        Assert.Null(issuePost.Error);
        Assert.Equal(999, Assert.Single(issueLine.Journal!.Lines).CostPrice);
        Assert.Equal(-15m,
            await db.Set<InventTrans>().OrderByDescending(x => x.RecId).Select(x => x.CostAmountPosted).FirstAsync());
        Assert.Equal(5, (await db.Set<InventSum>().SingleAsync(x => x.InventDimId == line.InventDimId)).PhysicalInvent);

        var movement = await service.CreateAsync(new InventJournalCreateDto
        { JournalNameId = "MOV", Description = "Movement receipt", InventSiteId = "JED", InventLocationId = "MAIN" }, default);
        var movementLine = await service.AddLineAsync(movement.Journal!.JournalId, new InventJournalLineInputDto
        { ItemId = "ITEM-001", InventSiteId = "JED", InventLocationId = "MAIN", Unit = "pcs", Qty = 2, CostPrice = 20,
            LedgerDimension = 22, TransDate = DateTime.Today }, default);
        Assert.Null(movementLine.Error);
        var movementPost = await posting.PostAsync(movement.Journal.JournalId, default);
        Assert.Null(movementPost.Error);
        Assert.Equal(1, movementPost.Result!.InventoryTransactionsCreated);
        Assert.Equal(1, movementPost.Result.LedgerEntriesCreated);
        Assert.Equal(7, (await db.Set<InventSum>().SingleAsync(x => x.InventDimId == line.InventDimId)).PhysicalInvent);

        var counting = await service.CreateAsync(new InventJournalCreateDto
        { JournalNameId = "COUNT", Description = "Count difference", InventSiteId = "JED", InventLocationId = "MAIN" }, default);
        var countLine = await service.AddLineAsync(counting.Journal!.JournalId, new InventJournalLineInputDto
        { ItemId = "ITEM-001", InventSiteId = "JED", InventLocationId = "MAIN", Unit = "pcs", Counted = 6,
            TransDate = DateTime.Today }, default);
        Assert.Null(countLine.Error);
        var countPost = await posting.PostAsync(counting.Journal.JournalId, default);
        Assert.Null(countPost.Error);
        Assert.Equal(1, countPost.Result!.InventoryTransactionsCreated);
        Assert.Equal(1, countPost.Result.LedgerEntriesCreated);
        Assert.Equal(-1, (await db.Set<InventTrans>().OrderByDescending(x => x.RecId).Select(x => x.Qty).FirstAsync()));
        Assert.Equal(6, (await db.Set<InventSum>().SingleAsync(x => x.InventDimId == line.InventDimId)).PhysicalInvent);
        Assert.Equal(await db.Set<InventTrans>().Where(x => x.ItemId == "ITEM-001").SumAsync(x => x.Qty),
            await db.Set<InventSum>().Where(x => x.ItemId == "ITEM-001").SumAsync(x => x.PhysicalInvent));
        Assert.Equal(0, await db.Set<GeneralJournalAccountEntry>().SumAsync(x => x.AccountingCurrencyAmount));

        var beforeRollbackTransactions = await db.Set<InventTrans>().CountAsync();
        var beforeRollbackEntries = await db.Set<GeneralJournalEntry>().CountAsync();
        var beforeRollbackQty = await db.Set<InventSum>().Where(x => x.ItemId == "ITEM-001").SumAsync(x => x.PhysicalInvent);
        var rollbackJournal = await service.CreateAsync(new InventJournalCreateDto
        { JournalNameId = "ADJ", Description = "Atomic rollback test", InventSiteId = "JED", InventLocationId = "MAIN" }, default);
        var rollbackReceipt = await service.AddLineAsync(rollbackJournal.Journal!.JournalId, new InventJournalLineInputDto
        { ItemId = "ITEM-001", InventSiteId = "JED", InventLocationId = "MAIN", Unit = "pcs", Qty = 1,
            CostPrice = 10, TransDate = DateTime.Today }, default);
        var invalidIssue = await service.AddLineAsync(rollbackJournal.Journal.JournalId, new InventJournalLineInputDto
        { ItemId = "ITEM-001", InventSiteId = "JED", InventLocationId = "MAIN", Unit = "pcs", Qty = -100,
            TransDate = DateTime.Today }, default);
        Assert.Null(rollbackReceipt.Error);
        Assert.Null(invalidIssue.Error);
        var rollbackResult = await posting.PostAsync(rollbackJournal.Journal.JournalId, default);
        Assert.Contains("negative physical inventory", rollbackResult.Error);
        Assert.Equal(beforeRollbackTransactions, await db.Set<InventTrans>().CountAsync());
        Assert.Equal(beforeRollbackEntries, await db.Set<GeneralJournalEntry>().CountAsync());
        Assert.Equal(beforeRollbackQty, await db.Set<InventSum>().Where(x => x.ItemId == "ITEM-001").SumAsync(x => x.PhysicalInvent));
        Assert.False((await service.GetAsync(rollbackJournal.Journal.JournalId, default))!.Posted);
    }

    private sealed class InventoryContext(DbContextOptions<InventoryContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            Configure<InventSite>(builder);
            Configure<InventLocation>(builder);
            Configure<InventModelGroup>(builder);
            Configure<WMSLocation>(builder);
            Configure<InventTrans>(builder);
            Configure<InventTransOrigin>(builder);
            Configure<InventDim>(builder);
            Configure<SalesLine>(builder);
            Configure<ReleasedItem>(builder);
            Configure<InventSum>(builder);
            Configure<InventItemBarcode>(builder);
            Configure<UnitOfMeasure>(builder);
            Configure<UnitOfMeasureConversion>(builder);
            Configure<InventTableModule>(builder);
            Configure<SharedProduct>(builder);
            Configure<IAX.IXApi.Modules.Finance.Inventory.InventTable.EcoResDistinctProduct>(builder);
            Configure<IAX.IXApi.Modules.Finance.Inventory.InventTable.EcoResProductTranslation>(builder);
            Configure<EcoResCategory>(builder);
            Configure<EcoResCategoryHierarchy>(builder);
            Configure<EcoResProductCategory>(builder);
            Configure<EcoResProductLifecycleState>(builder);
            Configure<WHSInventStatus>(builder);
            Configure<InventParameters>(builder);
            Configure<InventJournalName>(builder);
            Configure<InventJournalTable>(builder);
            Configure<InventJournalTrans>(builder);
            Configure<InventModelGroup>(builder);
            Configure<InventModelGroupItem>(builder);
            Configure<EcoResTrackingDimensionGroupItem>(builder);
            Configure<EcoResTrackingDimensionGroup>(builder);
            Configure<EcoResTrackingDimensionGroupFldSetup>(builder);
            Configure<EcoResStorageDimensionGroupItem>(builder);
            Configure<EcoResStorageDimensionGroupFldSetup>(builder);
            Configure<InventItemGroupItem>(builder);
            Configure<InventPosting>(builder);
            Configure<MainAccount>(builder);
            Configure<DimensionAttributeValueCombination>(builder);
            Configure<Ledger>(builder);
            Configure<FiscalCalendarYear>(builder);
            Configure<FiscalCalendarPeriod>(builder);
            Configure<LedgerFiscalCalendarPeriod>(builder);
            Configure<GeneralJournalEntry>(builder);
            Configure<GeneralJournalAccountEntry>(builder);
            Configure<SubledgerVoucherGeneralJournalEntry>(builder);
            Configure<SysNumberSequence>(builder);
            builder.Entity<InventSite>().HasQueryFilter(x => x.DataAreaId == "dat");
            builder.Entity<InventLocation>().HasQueryFilter(x => x.DataAreaId == "dat");
        }
        private static void Configure<T>(ModelBuilder builder) where T : class
        {
            var entity = builder.Entity<T>();
            entity.HasKey("RecId");
            entity.Property<long>("RecId").ValueGeneratedOnAdd();
            entity.Ignore("RowVersion");
            foreach (var property in typeof(T).GetProperties())
                if (!property.PropertyType.IsValueType && property.PropertyType != typeof(string))
                    entity.Ignore(property.Name);
        }
    }

    private sealed class TestIdentity : ICurrentUserService, ICompanyExecutionContext
    {
        public string GetCurrentUserId() => "test";
        public string GetOwnerAccountId() => "test";
        public string? DataAreaId { get; set; } = "dat";
        public string? GetDataAreaId() => DataAreaId;
        public bool IsRequestedCompanyAuthorized() => true;
    }

    private sealed class TestSalesInventoryNumbers : ISalesInventoryNumberService
    {
        private int _dimension;
        private int _transaction;
        public Task<string> NextInventDimIdAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult($"DIM-{++_dimension:000}");
        public Task<string> NextInventTransIdAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult($"TRANS-{++_transaction:000}");
    }

    private sealed class TestUnitOfWork : IUnitOfWork
    {
        private readonly DbContext context;
        private IDbContextTransaction? transaction;
        public TestUnitOfWork(DbContext context) => this.context = context;
        public DbContext Context => context;
        public IGenericRepository<T> Repository<T>() where T : class
        {
            var repository = DispatchProxy.Create<IGenericRepository<T>, RepositoryProxy<T>>();
            ((RepositoryProxy<T>)(object)repository).Context = context;
            return repository;
        }
        public Task<int> CompleteAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
        public async Task BeginTransactionAsync(CancellationToken ct = default) => transaction ??= await context.Database.BeginTransactionAsync(ct);
        public async Task CommitTransactionAsync(CancellationToken ct = default)
        { if (transaction != null) { await transaction.CommitAsync(ct); await transaction.DisposeAsync(); transaction = null; } }
        public async Task RollbackTransactionAsync(CancellationToken ct = default)
        { if (transaction != null) { await transaction.RollbackAsync(ct); await transaction.DisposeAsync(); transaction = null; } }
        public void Dispose() { }
    }

    public class RepositoryProxy<T> : DispatchProxy where T : class
    {
        public DbContext Context { get; set; } = null!;
        protected override object? Invoke(MethodInfo? method, object?[]? args)
        {
            switch (method?.Name)
            {
                case "GetQueryable": return Context.Set<T>().AsQueryable();
                case "AddAsync": Context.Add((T)args![0]!); return Task.FromResult((T)args[0]!);
                case "UpdateAsync": return Task.FromResult((T)args![0]!);
                case "RemoveAsync": Context.Remove((T)args![0]!); return Task.CompletedTask;
                default: throw new NotSupportedException(method?.Name);
            }
        }
    }
}
