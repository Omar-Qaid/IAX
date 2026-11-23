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
    public async Task Shared_product_creation_persists_distinct_product_and_translations()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        new EcoResProductMapping().Register(TypeAdapterConfig.GlobalSettings);
        var service = new EcoResProductService(new TestUnitOfWork(db), new TestIdentity(), new EcoResProductInputValidator());

        var result = await service.CreateAsync(new EcoResProductInputDto
        {
            ProductNumber = "paper-a4", Name = "Paper A4", NameAlias = "ورق A4",
            SearchName = "Paper", ProductType = "Item"
        });

        Assert.Null(result.Error);
        Assert.Equal("PAPER-A4", result.Product!.ProductNumber);
        Assert.Equal("Paper A4", result.Product.Name);
        Assert.Single(await db.Set<IAX.IXApi.Modules.Finance.Inventory.InventTable.EcoResDistinctProduct>().ToListAsync());
        Assert.Equal(2, await db.Set<IAX.IXApi.Modules.Finance.Inventory.InventTable.EcoResProductTranslation>().CountAsync());
    }

    [Fact]
    public async Task Shared_product_creation_rejects_soft_deleted_duplicate_number()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.Add(new SharedProduct { RecId = 1, DisplayProductNumber = "PAPER-A4", IsDeleted = true });
        await db.SaveChangesAsync();
        var service = new EcoResProductService(new TestUnitOfWork(db), new TestIdentity(), new EcoResProductInputValidator());

        var result = await service.CreateAsync(new EcoResProductInputDto
        { ProductNumber = "paper-a4", Name = "Paper A4", ProductType = "Item" });

        Assert.Null(result.Product);
        Assert.Contains("already exists", result.Error);
        Assert.Single(await db.Set<SharedProduct>().IgnoreQueryFilters().ToListAsync());
    }

    [Fact]
    public async Task Shared_product_creation_rolls_back_all_rows_when_translation_save_fails()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var service = new EcoResProductService(new TestUnitOfWork(db, failOnComplete: 2),
            new TestIdentity(), new EcoResProductInputValidator());

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(new EcoResProductInputDto
        { ProductNumber = "PAPER-A4", Name = "Paper A4", ProductType = "Item" }));

        Assert.Empty(await db.Set<SharedProduct>().ToListAsync());
        Assert.Empty(await db.Set<IAX.IXApi.Modules.Finance.Inventory.InventTable.EcoResDistinctProduct>().ToListAsync());
        Assert.Empty(await db.Set<IAX.IXApi.Modules.Finance.Inventory.InventTable.EcoResProductTranslation>().ToListAsync());
    }

    [Fact]
    public async Task Released_product_rejects_incomplete_creation_and_invalid_release_without_writes()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var identity = new TestIdentity();
        var service = new ReleasedProductListService(new TestUnitOfWork(db), identity, null!, null!);
        var incomplete = await service.CreateReleasedAsync(new CreateReleasedProductDto { Name = "Paper" });
        Assert.Contains("configuration", incomplete.Error);
        var invalid = await service.ReleaseAsync(new ReleaseProductDto { ProductId = "invalid" });
        Assert.Contains("identifier", invalid.Error);
        identity.DataAreaId = null;
        var noCompany = await service.ReleaseAsync(new ReleaseProductDto { ProductId = "1" });
        Assert.Contains("company", noCompany.Error);
        Assert.Empty(await db.Set<SharedProduct>().ToListAsync());
        Assert.Empty(await db.Set<ReleasedItem>().ToListAsync());
        Assert.Empty(await db.Set<InventTableModule>().ToListAsync());
        Assert.Empty(await db.Set<InventTrans>().ToListAsync());
    }

    [Fact]
    public async Task Released_product_lookup_identifies_legacy_fifo_group_that_needs_setup()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new InventModelGroup { RecId = 1, DataAreaId = "dat", ModelGroupId = "FIFO", Name = "FIFO stocked items",
                InventModel = InventModel.FIFO, StockedProduct = NoYes.Yes },
            new InventModelGroup { RecId = 2, DataAreaId = "dat", ModelGroupId = "FIFO-READY", Name = "Ready",
                InventModel = InventModel.FIFO, StockedProduct = NoYes.Yes, PostOnHandFinancial = NoYes.Yes },
            new InventModelGroup { RecId = 3, DataAreaId = "other", ModelGroupId = "HIDDEN", Name = "Other company" });
        await db.SaveChangesAsync();
        var service = new ReleasedProductListService(new TestUnitOfWork(db), new TestIdentity(), null!, null!);

        var groups = (await service.GetLookupsAsync())!.ModelGroups;

        Assert.Contains(groups, x => x.Value == "FIFO" && x.Label.Contains("setup required"));
        Assert.Contains(groups, x => x.Value == "FIFO-READY" && !x.Label.Contains("setup required"));
        Assert.DoesNotContain(groups, x => x.Value == "HIDDEN");
    }

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
    public async Task Release_product_creates_one_company_item_with_three_module_rows_and_is_idempotent()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(new SharedProduct { RecId = 1, DisplayProductNumber = "PAPER-A4", ProductType = ItemType.Item },
            new IAX.IXApi.Modules.Finance.Inventory.InventTable.EcoResDistinctProduct { RecId = 1 },
            new IAX.IXApi.Modules.Finance.Inventory.InventTable.EcoResProductTranslation
                { RecId = 2, Product = 1, LanguageId = "en-us", Name = "Paper A4" });
        await db.SaveChangesAsync();
        new ReleasedProductMapping().Register(TypeAdapterConfig.GlobalSettings);
        var identity = new TestIdentity();
        var work = new TestUnitOfWork(db);
        var service = new ReleasedProductListService(work, identity,
            new InventDimensionResolver(work, new TestSalesInventoryNumbers()),
            new SysNumberSequenceService(work, identity));

        var first = await service.ReleaseAsync(new ReleaseProductDto { ProductId = "1", ItemNumber = "PAPER-A4" });
        var repeated = await service.ReleaseAsync(new ReleaseProductDto { ProductId = "1", ItemNumber = "PAPER-A4" });

        Assert.Null(first.Error);
        Assert.Null(repeated.Error);
        Assert.Equal(first.Product!.Id, repeated.Product!.Id);
        Assert.Equal("PAPER-A4", first.Product.ItemNumber);
        Assert.Single(await db.Set<ReleasedItem>().Where(x => x.DataAreaId == "dat").ToListAsync());
        Assert.Equal(3, await db.Set<InventTableModule>().CountAsync(x => x.DataAreaId == "dat" && x.ItemId == "PAPER-A4"));
        db.Add(new UnitOfMeasure { RecId = 3, Symbol = "pcs", DecimalPrecision = 0 });
        await db.SaveChangesAsync();
        var invalidCommercial = await service.AssignUnitsAsync(long.Parse(first.Product.Id), new ReleasedProductUnitAssignmentDto
        {
            InventoryUnit = "pcs", PurchaseUnit = "pcs", SalesUnit = "pcs",
            Commercial = new ReleasedProductCommercialAssignmentDto { PurchasePrice = -1 }
        });
        Assert.Equal("Purchase and sales prices cannot be negative, and price quantities must be greater than zero.", invalidCommercial.Error);
        Assert.All(await db.Set<InventTableModule>().Where(x => x.DataAreaId == "dat" && x.ItemId == "PAPER-A4").ToListAsync(),
            module => Assert.Equal(string.Empty, module.UnitId));
        db.Add(new ReleasedItem { RecId = 4, DataAreaId = "dat", ItemId = "LEGACY", Product = 0 });
        await db.SaveChangesAsync();
        var legacyUnits = new ReleasedProductUnitAssignmentDto
        { InventoryUnit = "pcs", PurchaseUnit = "pcs", SalesUnit = "pcs" };
        Assert.Equal("The released item does not reference an existing shared product.",
            (await service.AssignUnitsAsync(4, legacyUnits)).Error);
        Assert.Equal("The released item does not reference an existing shared product.",
            (await service.ConfigureAsync(4, new ConfigureReleasedProductDto
            { InventoryUnit = "pcs", PurchaseUnit = "pcs", SalesUnit = "pcs" })).Error);
        Assert.Empty(await db.Set<UnitOfMeasureConversion>().ToListAsync());
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
    public async Task Sales_issue_rechecks_product_and_inventory_status_before_allocating_a_packing_slip()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new SalesTable { RecId = 901, DataAreaId = "dat", SalesId = "SO-901", SalesStatus = SalesStatus.Backorder },
            new SalesLine { RecId = 902, DataAreaId = "dat", SalesId = "SO-901", ItemId = "ITEM-901",
                SalesStatus = SalesStatus.Backorder, StockedProduct = NoYes.Yes, InventDimId = "DIM-901",
                InventTransId = "TRANS-901", RemainSalesPhysical = 1, RemainInventPhysical = 1 },
            new ReleasedItem { RecId = 903, DataAreaId = "dat", ItemId = "ITEM-901", ProductLifecycleStateId = "HOLD" },
            new EcoResProductLifecycleState { RecId = 904, StateId = "HOLD", Description = "Hold", BlockSales = NoYes.Yes },
            new InventDim { RecId = 905, DataAreaId = "dat", InventDimId = "DIM-901", InventStatusId = "AVAILABLE" },
            new WHSInventStatus { RecId = 906, DataAreaId = "dat", InventStatusId = "AVAILABLE", Name = "Available" });
        await db.SaveChangesAsync();
        var service = new PackingSlipJournalService(new TestUnitOfWork(db), new TestIdentity(), null!);
        var request = new PostPackingSlipRequest(null, "all", true, "REQUEST-901",
            [new PackingSlipPostingLineRequest("902", 1)]);

        var productBlocked = await service.PostAsync(901, request, default);
        Assert.Equal(422, productBlocked.StatusCode);
        Assert.Contains("blocks sales", productBlocked.Message);

        (await db.Set<ReleasedItem>().SingleAsync()).ProductLifecycleStateId = string.Empty;
        (await db.Set<WHSInventStatus>().SingleAsync()).InventStatusBlocking = NoYes.Yes;
        await db.SaveChangesAsync();
        var inventoryBlocked = await service.PostAsync(901, request, default);
        Assert.Equal(422, inventoryBlocked.StatusCode);
        Assert.Contains("blocked for issue", inventoryBlocked.Message);

        (await db.Set<WHSInventStatus>().SingleAsync()).InventStatusBlocking = NoYes.No;
        var salesLine = await db.Set<SalesLine>().SingleAsync();
        salesLine.SalesQty = 1;
        salesLine.QtyOrdered = 1;
        db.Add(new InventSum { DataAreaId = "dat", ItemId = "ITEM-901", InventDimId = "DIM-901",
            PhysicalInvent = 10, ReservPhysical = 10, AvailPhysical = 0 });
        await db.SaveChangesAsync();
        var reservedStock = await service.PostAsync(901, request, default);
        Assert.Equal(422, reservedStock.StatusCode);
        Assert.Contains("other reservations", reservedStock.Message);
        (await db.Set<SalesTable>().SingleAsync()).SalesType = SalesType.ReturnItem;
        await db.SaveChangesAsync();
        var returnAsDelivery = await service.PostAsync(901, request, default);
        Assert.Equal(422, returnAsDelivery.StatusCode);
        Assert.Contains("return receipts", returnAsDelivery.Message);
        Assert.Empty(await db.Set<CustPackingSlipJour>().ToListAsync());
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
            new InventTableModule { RecId = 6, DataAreaId = "dat", ItemId = "ITEM-1", ModuleType = ModuleInventPurchSales.Inventory, UnitId = "pcs" },
            new InventTableModule { RecId = 7, DataAreaId = "dat", ItemId = "ITEM-3", ModuleType = ModuleInventPurchSales.Inventory, UnitId = "pcs" },
            new InventItemBarcode { RecId = 4, DataAreaId = "other", ItemId = "ITEM-2", ItemBarcode = "OTHER", UnitId = "pcs", Qty = 1 });
        await db.SaveChangesAsync();
        var service = new InventItemBarcodeService(new TestUnitOfWork(db), new TestIdentity());
        var input = new InventItemBarcodeInputDto("12345", "pcs", 1);

        Assert.NotNull((await service.CreateAsync(2, input, default)).Error);
        Assert.Contains("unit", (await service.CreateAsync(1, input with { UnitId = "missing" }, default)).Error!, StringComparison.OrdinalIgnoreCase);
        var created = await service.CreateAsync(1, input, default);
        Assert.Null(created.Error);
        Assert.Equal("ITEM-1", created.Row!.ItemId);
        Assert.Contains("already maps", (await service.CreateAsync(5, input, default)).Error!);
        Assert.Contains("already maps", (await service.CreateAsync(1, input, default)).Error!);
        Assert.Single((await service.ListAsync(1, default)).Rows!);
        var matches = (await service.FindAsync("12345", default)).Rows!;
        Assert.Single(matches);
        Assert.Contains(matches, x => x.ReleasedProductRecId == 1);
        Assert.DoesNotContain(matches, x => x.Barcode.ItemId == "ITEM-2");
        var stored = await db.Set<InventItemBarcode>().SingleAsync(x => x.RecId == created.Row.RecId);
        stored.BarcodeSetupId = "LEGACY";
        stored.RetailVariantId = "LEGACY";
        await db.SaveChangesAsync();
        Assert.Null((await service.UpdateAsync(1, created.Row.RecId, input with { Qty = 2, Blocked = true }, default)).Error);
        Assert.True((await service.ListAsync(1, default)).Rows![0].Blocked);
        Assert.Empty((await service.FindAsync("12345", default)).Rows!);
        Assert.Equal("LEGACY", stored.BarcodeSetupId);
        Assert.Equal("LEGACY", stored.RetailVariantId);
        Assert.False((await service.DeleteAsync(1, 4, default)).Deleted);
        Assert.True((await service.DeleteAsync(1, created.Row.RecId, default)).Deleted);
        Assert.Equal(1, await db.Set<InventItemBarcode>().CountAsync());
    }

    [Fact]
    public async Task Barcode_lookup_preserves_leading_zeros_and_uses_product_fraction_conversion()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new SharedProduct { RecId = 100, DisplayProductNumber = "PAPER", ProductType = ItemType.Item },
            new ReleasedItem { RecId = 101, DataAreaId = "dat", ItemId = "PAPER", Product = 100 },
            new UnitOfMeasure { RecId = 102, Symbol = "BOX", UnitOfMeasureClass = UnitOfMeasureClass.Quantity },
            new UnitOfMeasure { RecId = 103, Symbol = "EA", UnitOfMeasureClass = UnitOfMeasureClass.Quantity },
            new InventTableModule { RecId = 104, DataAreaId = "dat", ItemId = "PAPER", ModuleType = ModuleInventPurchSales.Inventory, UnitId = "EA" },
            new UnitOfMeasureConversion { RecId = 105, Product = 100, FromUnitOfMeasure = 102, ToUnitOfMeasure = 103,
                Numerator = 10, Denominator = 1 });
        await db.SaveChangesAsync();
        var service = new InventItemBarcodeService(new TestUnitOfWork(db), new TestIdentity());
        var created = await service.CreateAsync(101, new InventItemBarcodeInputDto("001234", "BOX", 2), default);
        Assert.Null(created.Error);
        var found = Assert.Single((await service.FindAsync("001234", default)).Rows!);
        Assert.Equal("001234", found.Barcode.ItemBarcode);
        Assert.Equal("EA", found.InventoryUnit);
        Assert.Equal(20, found.InventoryQuantity);
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
        Assert.NotNull((await service.CreateAsync(new UnitOfMeasureInputDto { Symbol = "PCS" })).Error);
        var conversion = await service.SaveConversionAsync(null, new UnitOfMeasureConversionInputDto
        { FromUnitOfMeasure = box.Unit!.RecId, ToUnitOfMeasure = pcs.Unit!.RecId, Product = 100, Factor = 4 });
        Assert.Equal(4, conversion.Conversion!.Factor);
        Assert.Equal("ITEM-001", conversion.Conversion.ProductNumber);
        Assert.NotNull((await service.SaveConversionAsync(null, new UnitOfMeasureConversionInputDto
        { FromUnitOfMeasure = box.Unit.RecId, ToUnitOfMeasure = pcs.Unit.RecId, Product = 100, Factor = 4 })).Error);
        Assert.NotNull((await service.SaveConversionAsync(null, new UnitOfMeasureConversionInputDto
        { FromUnitOfMeasure = pcs.Unit.RecId, ToUnitOfMeasure = box.Unit.RecId, Product = 100, Factor = 0.25m })).Error);
        Assert.NotNull((await service.SaveConversionAsync(null, new UnitOfMeasureConversionInputDto
        { FromUnitOfMeasure = box.Unit.RecId, ToUnitOfMeasure = pcs.Unit.RecId, Product = 0,
            Factor = 4, InnerOffset = 1 })).Error);
        Assert.NotNull((await service.SaveConversionAsync(null, new UnitOfMeasureConversionInputDto
        { FromUnitOfMeasure = box.Unit.RecId, ToUnitOfMeasure = pcs.Unit.RecId, Product = 0,
            Factor = 4, Numerator = 4, Denominator = 1 })).Error);
        var fraction = await service.SaveConversionAsync(null, new UnitOfMeasureConversionInputDto
        { FromUnitOfMeasure = box.Unit.RecId, ToUnitOfMeasure = pcs.Unit.RecId, Product = 0,
            Numerator = 10, Denominator = 1 });
        Assert.NotNull(fraction.Conversion);
        db.Add(new InventTableModule { RecId = 200, DataAreaId = "dat", ItemId = "ITEM-001", UnitId = "pcs", ModuleType = ModuleInventPurchSales.Inventory });
        await db.SaveChangesAsync();
        Assert.NotNull((await service.UpdateAsync(pcs.Unit.RecId, new UnitOfMeasureInputDto { Symbol = "ea", UnitOfMeasureClass = 0, SystemOfUnits = 0 })).Error);
        Assert.NotNull((await service.UpdateAsync(pcs.Unit.RecId, new UnitOfMeasureInputDto { Symbol = "PCS", UnitOfMeasureClass = 0, SystemOfUnits = 0 })).Error);
        Assert.NotNull((await service.UpdateAsync(pcs.Unit.RecId, new UnitOfMeasureInputDto
        { Symbol = "pcs", UnitOfMeasureClass = 0, SystemOfUnits = 0, DecimalPrecision = 2 })).Error);
        Assert.NotNull(await service.DeleteAsync(pcs.Unit.RecId));
        Assert.Null(await service.DeleteConversionAsync(conversion.Conversion.RecId));
        Assert.Null(await service.DeleteConversionAsync(fraction.Conversion!.RecId));
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
        Assert.Equal(0m, (decimal)method.Invoke(null, [conversions, units, 200L, "", "pcs"])!);
        conversions[0].InnerOffset = 1;
        Assert.Equal(0m, (decimal)method.Invoke(null, [conversions, units, 200L, "box", "pcs"])!);
    }
}
