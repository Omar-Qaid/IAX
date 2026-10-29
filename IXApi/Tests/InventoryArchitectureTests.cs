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

public sealed class InventoryArchitectureTests
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
    public void Journal_name_rejects_unsupported_policies()
    {
        var validator = new InventJournalNameInputValidator();
        var valid = new InventJournalNameInputDto
        { JournalNameId = "COUNT", Description = "Count", JournalType = InventJournalType.Counting };
        Assert.True(validator.Validate(valid).IsValid);
        valid.DetailSummary = DetailSummary.Summary;
        Assert.False(validator.Validate(valid).IsValid);
        valid.DetailSummary = DetailSummary.Detail;
        valid.DeletePostedLines = true;
        Assert.False(validator.Validate(valid).IsValid);
        valid.DeletePostedLines = false;
        valid.Reservation = ItemReservation.Automatic;
        Assert.False(validator.Validate(valid).IsValid);
    }

    [Fact]
    public async Task Inventory_readiness_rejects_blocked_voucher_sequence_on_selected_journal()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.Add(new SysNumberSequence { RecId = 1, DataAreaId = "dat", NumberSequence = "VOUCHER", Txt = "Voucher",
            Format = "V-{SEQ}", AnnotatedFormat = "V-{SEQ}", InUse = 1, IsActive = true, Blocked = 1,
            NextRec = 1, Highest = 99999 });
        db.Add(new InventJournalName { RecId = 2, DataAreaId = "dat", JournalNameId = "MOV",
            Description = "Movement", JournalType = InventJournalType.Movement, VoucherNumberSequenceTable = 1 });
        db.Add(new InventParameters { RecId = 3, DataAreaId = "dat", MovementJournalNameId = "MOV" });
        await db.SaveChangesAsync();
        var identity = new TestIdentity();
        var service = new InventParametersService(new TestUnitOfWork(db), identity);
        var readiness = (await service.GetReadinessAsync())!;
        Assert.Contains(readiness.Issues, x => x.Code == "MISSING_MOVEMENT_JOURNAL");
        var journalNames = new InventJournalNameService(new TestUnitOfWork(db), identity, new InventJournalNameInputValidator());
        Assert.Empty((await journalNames.LookupsAsync(default)).NumberSequences);
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
    public async Task Invent_batch_master_is_company_scoped_and_preserves_copied_fields()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new ReleasedItem { RecId = 1, DataAreaId = "dat", ItemId = "ITEM-1" },
            new ReleasedItem { RecId = 2, DataAreaId = "other", ItemId = "ITEM-1" },
            new InventBatch { RecId = 3, DataAreaId = "other", ItemId = "ITEM-1", InventBatchId = "LOT-1" });
        await db.SaveChangesAsync();
        var identity = new TestIdentity();
        var service = new InventBatchService(new TestUnitOfWork(db), identity);

        Assert.Contains("released", (await service.CreateAsync(new InventBatchInputDto
            { ItemId = "MISSING", InventBatchId = "LOT-1" }, default)).Error);
        Assert.Contains("precede", (await service.CreateAsync(new InventBatchInputDto
            { ItemId = "ITEM-1", InventBatchId = "LOT-1", ProdDate = new DateTime(2026, 2, 2),
                ExpDate = new DateTime(2026, 2, 1) }, default)).Error);

        var created = await service.CreateAsync(new InventBatchInputDto
        {
            ItemId = " item-1 ", InventBatchId = " lot-1 ", ProdDate = new DateTime(2026, 2, 1),
            ExpDate = new DateTime(2027, 2, 1)
        }, default);
        Assert.Null(created.Error);
        Assert.Equal("LOT-1", created.Batch!.InventBatchId);
        Assert.Single(await service.ListAsync("item-1", default));
        Assert.Contains("already exists", (await service.CreateAsync(new InventBatchInputDto
            { ItemId = "ITEM-1", InventBatchId = "LOT-1" }, default)).Error);
        var row = await db.Set<InventBatch>().SingleAsync(x => x.RecId == created.Batch.RecId);
        row.PdsVendBatchId = "VENDOR-LOT";
        await db.SaveChangesAsync();
        var updated = await service.UpdateAsync(row.RecId, new InventBatchInputDto
        {
            ItemId = "ITEM-1", InventBatchId = "LOT-1", ProdDate = new DateTime(2026, 3, 1),
            ExpDate = new DateTime(2027, 3, 1)
        }, default);
        Assert.Null(updated.Error);
        Assert.Equal("VENDOR-LOT", (await db.Set<InventBatch>().SingleAsync(x => x.RecId == row.RecId)).PdsVendBatchId);
        Assert.Contains("cannot be changed", (await service.UpdateAsync(row.RecId,
            new InventBatchInputDto { ItemId = "ITEM-1", InventBatchId = "DIFFERENT" }, default)).Error);
        Assert.Null(await service.GetAsync(3, default));

        db.Add(new InventDim { DataAreaId = "dat", InventDimId = "D-1", InventBatchId = "LOT-1" });
        await db.SaveChangesAsync();
        // An unused dimension does not establish an item-specific stock reference.
        db.Add(new InventTrans { DataAreaId = "dat", ItemId = "ITEM-1", InventDimId = "D-1" });
        await db.SaveChangesAsync();
        Assert.Contains("referenced", await service.DeleteAsync(row.RecId, default));
        Assert.Equal(2, await db.Set<InventBatch>().CountAsync());
    }

    [Fact]
    public async Task Invent_batch_deletion_checks_references_for_the_same_item_only()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new ReleasedItem { DataAreaId = "dat", ItemId = "ITEM-1" },
            new ReleasedItem { DataAreaId = "dat", ItemId = "ITEM-2" },
            new InventBatch { DataAreaId = "dat", ItemId = "ITEM-1", InventBatchId = "LOT-1" },
            new InventDim { DataAreaId = "dat", InventDimId = "D-1", InventBatchId = "LOT-1" },
            new InventTrans { DataAreaId = "dat", ItemId = "ITEM-2", InventDimId = "D-1" });
        await db.SaveChangesAsync();
        var service = new InventBatchService(new TestUnitOfWork(db), new TestIdentity());
        var batch = await db.Set<InventBatch>().SingleAsync();

        Assert.Null(await service.DeleteAsync(batch.RecId, default));
        Assert.Empty(await db.Set<InventBatch>().ToListAsync());
    }

    [Fact]
    public async Task Inventory_closing_history_is_read_only_and_company_scoped()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new InventClosing { RecId = 1, DataAreaId = "dat", Voucher = "C-1", PeriodCode = "2026-10",
                TransDate = new DateTime(2026, 10, 1), RunNum = 1, InventCostStatus = InventCostStatus.Finished },
            new InventClosing { RecId = 2, DataAreaId = "other", Voucher = "C-2", PeriodCode = "2026-10",
                TransDate = new DateTime(2026, 10, 2), RunNum = 2, InventCostStatus = InventCostStatus.Finished });
        await db.SaveChangesAsync();
        var service = new InventClosingQueryService(new TestUnitOfWork(db), new TestIdentity());

        var listed = await service.ListAsync(default);

        Assert.Single(listed);
        Assert.Equal("C-1", listed[0].Voucher);
        Assert.Null(await service.GetAsync(2, default));
        Assert.Equal("2026-10", (await service.GetAsync(1, default))!.PeriodCode);
    }

    [Fact]
    public async Task Inventory_record_inquiries_are_company_scoped_and_do_not_write()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new InventDim { RecId = 1, DataAreaId = "dat", InventDimId = "D-1", InventSiteId = "JED", InventLocationId = "MAIN" },
            new InventDim { RecId = 2, DataAreaId = "other", InventDimId = "D-2", InventSiteId = "JED", InventLocationId = "MAIN" },
            new InventItemGroupItem { RecId = 3, DataAreaId = "dat", ItemDataAreaId = "dat", ItemGroupDataAreaId = "dat", ItemId = "PAPER", ItemGroupId = "OFFICE" },
            new InventItemGroupItem { RecId = 4, DataAreaId = "other", ItemDataAreaId = "other", ItemGroupDataAreaId = "other", ItemId = "PAPER", ItemGroupId = "OTHER" },
            new InventItemLocation { RecId = 5, DataAreaId = "dat", ItemId = "PAPER", InventDimId = "D-1", PickingLocationMaxQty = 10 },
            new InventItemLocation { RecId = 6, DataAreaId = "other", ItemId = "PAPER", InventDimId = "D-2" },
            new InventSettlement { RecId = 7, DataAreaId = "dat", ItemId = "PAPER", Voucher = "C-1", QtySettled = 2 },
            new InventSettlement { RecId = 8, DataAreaId = "other", ItemId = "PAPER", Voucher = "C-2", QtySettled = 3 });
        await db.SaveChangesAsync();
        var service = new InventoryRecordQueryService(new TestUnitOfWork(db), new TestIdentity());

        Assert.Equal("D-1", Assert.Single(await service.DimensionsAsync("JED", "MAIN", default)).InventDimId);
        Assert.Equal("OFFICE", Assert.Single(await service.ItemGroupsAsync("PAPER", default)).ItemGroupId);
        Assert.Equal(10, Assert.Single(await service.ItemLocationsAsync("PAPER", default)).PickingLocationMaxQty);
        Assert.Equal("C-1", Assert.Single(await service.SettlementsAsync("PAPER", "C-1", default)).Voucher);
        Assert.Empty(await service.SettlementsAsync("PAPER", "C-2", default));
    }

    [Fact]
    public async Task Item_group_assignment_upserts_for_released_item_in_selected_company()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(new ReleasedItem { DataAreaId = "dat", ItemId = "PAPER" },
            new InventItemGroup { DataAreaId = "dat", ItemGroupId = "OFFICE" },
            new InventItemGroup { DataAreaId = "dat", ItemGroupId = "SUPPLIES" },
            new InventItemGroup { DataAreaId = "other", ItemGroupId = "OTHER" });
        await db.SaveChangesAsync();
        var service = new ItemGroupAssignmentService(new TestUnitOfWork(db), new TestIdentity());

        Assert.Contains("released", (await service.AssignAsync("MISSING", new("OFFICE"), default)).Error);
        Assert.Contains("does not exist", (await service.AssignAsync("PAPER", new("OTHER"), default)).Error);
        Assert.Null((await service.AssignAsync(" paper ", new(" office "), default)).Error);
        Assert.Null((await service.AssignAsync("PAPER", new("SUPPLIES"), default)).Error);
        var row = Assert.Single(await db.Set<InventItemGroupItem>().ToListAsync());
        Assert.Equal("SUPPLIES", row.ItemGroupId);
        Assert.Equal("dat", row.ItemDataAreaId);
        Assert.Equal("dat", row.ItemGroupDataAreaId);
    }

    [Fact]
    public async Task Item_location_setup_checks_dimension_and_preserves_advanced_fields()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(new ReleasedItem { DataAreaId = "dat", ItemId = "PAPER" },
            new InventDim { DataAreaId = "dat", InventDimId = "D-1", InventSiteId = "JED", InventLocationId = "MAIN" },
            new InventDim { DataAreaId = "dat", InventDimId = "D-2", InventSiteId = "JED", InventLocationId = "MAIN", InventBatchId = "B-1" },
            new InventLocation { DataAreaId = "dat", InventSiteId = "JED", InventLocationId = "MAIN" });
        await db.SaveChangesAsync();
        var service = new InventItemLocationService(new TestUnitOfWork(db), new TestIdentity());

        Assert.Contains("only a site and warehouse", (await service.CreateAsync(new("PAPER", "D-2", 10, 2), default)).Error);
        Assert.Contains("negative", (await service.CreateAsync(new("PAPER", "D-1", -1, 0), default)).Error);
        var created = await service.CreateAsync(new(" paper ", " d-1 ", 10, 2), default);
        Assert.Null(created.Error);
        Assert.Contains("already exists", (await service.CreateAsync(new("PAPER", "D-1", 10, 2), default)).Error);
        var row = await db.Set<InventItemLocation>().SingleAsync();
        row.CountGroupId = "LEGACY";
        await db.SaveChangesAsync();
        Assert.Null((await service.UpdateAsync(created.Location!.RecId, new("PAPER", "D-1", 20, 3), default)).Error);
        Assert.Equal("LEGACY", (await db.Set<InventItemLocation>().SingleAsync()).CountGroupId);
        Assert.Null(await service.DeleteAsync(created.Location.RecId, default));
        Assert.Empty(await db.Set<InventItemLocation>().ToListAsync());
    }

    [Fact]
    public async Task Count_group_setup_validates_policy_and_prevents_deleting_assigned_group()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var service = new InventCountGroupService(new TestUnitOfWork(db), new TestIdentity());

        Assert.Contains("positive interval", (await service.CreateAsync(new("C1", "Cycle", InventCountCode.Period, 0), default)).Error);
        Assert.Contains("only valid", (await service.CreateAsync(new("C1", "Cycle", InventCountCode.Manual, 2), default)).Error);
        Assert.Contains("invalid", (await service.CreateAsync(new("C1", "Cycle", (InventCountCode)99, 0), default)).Error);
        var created = await service.CreateAsync(new(" c1 ", " Cycle ", InventCountCode.Period, 30), default);
        Assert.Null(created.Error);
        Assert.Equal("C1", created.Group!.CountGroupId);
        db.Add(new InventCountGroup { DataAreaId = "other", CountGroupId = "C2", Name = "Other" });
        await db.SaveChangesAsync();
        Assert.Null(await service.GetAsync((await db.Set<InventCountGroup>().SingleAsync(x => x.DataAreaId == "other")).RecId, default));
        Assert.Contains("already exists", (await service.CreateAsync(new("C1", "Cycle", InventCountCode.Period, 30), default)).Error);
        Assert.Contains("cannot be changed", (await service.UpdateAsync(created.Group.RecId,
            new("C2", "Cycle", InventCountCode.Manual, 0), default)).Error);
        Assert.Null((await service.UpdateAsync(created.Group.RecId,
            new("C1", "Manual", InventCountCode.Manual, 0), default)).Error);
        db.Add(new InventItemLocation { DataAreaId = "dat", ItemId = "PAPER", InventDimId = "D1", CountGroupId = "C1" });
        await db.SaveChangesAsync();
        Assert.Contains("assigned", await service.DeleteAsync(created.Group.RecId, default));
        Assert.Single(await service.ListAsync(default));
    }

    [Fact]
    public async Task Item_location_rejects_unknown_count_group_and_accepts_configured_group()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(new ReleasedItem { DataAreaId = "dat", ItemId = "PAPER" },
            new InventDim { DataAreaId = "dat", InventDimId = "D-1", InventSiteId = "JED", InventLocationId = "MAIN" },
            new InventLocation { DataAreaId = "dat", InventSiteId = "JED", InventLocationId = "MAIN" },
            new InventCountGroup { DataAreaId = "dat", CountGroupId = "C1", Name = "Cycle" },
            new InventCountGroup { DataAreaId = "other", CountGroupId = "C2", Name = "Other" });
        await db.SaveChangesAsync();
        var service = new InventItemLocationService(new TestUnitOfWork(db), new TestIdentity());

        Assert.Contains("does not exist", (await service.CreateAsync(new("PAPER", "D-1", 0, 0, "C2"), default)).Error);
        var created = await service.CreateAsync(new("PAPER", "D-1", 0, 0, " c1 "), default);
        Assert.Null(created.Error);
        Assert.Equal("C1", created.Location!.CountGroupId);
    }

    [Fact]
    public async Task Invent_serial_master_validates_item_and_references()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.Add(new ReleasedItem { RecId = 1, DataAreaId = "dat", ItemId = "PAPER" });
        await db.SaveChangesAsync();
        var service = new InventSerialService(new TestUnitOfWork(db), new TestIdentity());

        Assert.Contains("released", (await service.CreateAsync(new InventSerialInputDto("MISSING", "S-1", null), default)).Error);
        var created = await service.CreateAsync(new InventSerialInputDto(" paper ", " s-1 ", new DateTime(2026, 1, 1)), default);
        Assert.Null(created.Error);
        Assert.Contains("already exists", (await service.CreateAsync(new InventSerialInputDto("PAPER", "S-1", null), default)).Error);
        Assert.Contains("cannot be changed", (await service.UpdateAsync(created.Serial!.RecId,
            new InventSerialInputDto("PAPER", "S-2", null), default)).Error);
        db.Add(new InventDim { DataAreaId = "dat", InventDimId = "D-1", InventSerialId = "S-1" });
        await db.SaveChangesAsync();
        Assert.Contains("referenced", await service.DeleteAsync(created.Serial.RecId, default));
    }

    [Fact]
    public async Task Packaging_group_master_is_company_scoped_and_units_are_inquiry_only()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new InventPackagingGroup { RecId = 1, DataAreaId = "other", PackagingGroupId = "BOX", Name = "Other" },
            new InventPackagingUnit { RecId = 2, DataAreaId = "dat", Unit = "EA", Factor = 5 },
            new InventPackagingUnit { RecId = 3, DataAreaId = "other", Unit = "EA", Factor = 8 });
        await db.SaveChangesAsync();
        var service = new InventPackagingService(new TestUnitOfWork(db), new TestIdentity());

        Assert.Equal(5, Assert.Single(await service.UnitsAsync(default)).Factor);
        var created = await service.CreateGroupAsync(new InventPackagingGroupInputDto(" box ", "Boxes"), default);
        Assert.Null(created.Error);
        Assert.Single(await service.GroupsAsync(default));
        Assert.Contains("already exists", (await service.CreateGroupAsync(
            new InventPackagingGroupInputDto("BOX", "Duplicate"), default)).Error);
        Assert.Contains("cannot be changed", (await service.UpdateGroupAsync(created.Group!.RecId,
            new InventPackagingGroupInputDto("CASE", "Cases"), default)).Error);
        Assert.Null(await service.DeleteGroupAsync(created.Group.RecId, default));
        Assert.Empty(await service.GroupsAsync(default));
    }

    [Fact]
    public async Task Posting_profile_normalizes_tax_group_and_rejects_duplicates_and_foreign_updates()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        new InventPostingMapping().Register(TypeAdapterConfig.GlobalSettings);
        db.AddRange(
            new MainAccount { RecId = 71, MainAccountId = "1100" },
            new DimensionAttributeValueCombination { RecId = 72, DataAreaId = "dat", MainAccount = 71, MainAccountValue = "1100" },
            new TaxGroupHeading { RecId = 73, DataAreaId = "dat", TaxGroup = "VAT" });
        await db.SaveChangesAsync();
        var identity = new TestIdentity();
        var service = new InventPostingService(new TestUnitOfWork(db), identity, identity);
        var input = new InventPostingRuleDto { InventAccountType = (int)InventAccountType.InventReceipt,
            ItemCode = (int)TableGroupAll.All, CustVendCode = (int)TableGroupAll.All,
            LedgerDimension = "72", TaxGroupId = " vat " };
        var created = await service.CreateAsync(input, default);
        Assert.Null(created.Error);
        Assert.Equal("VAT", created.Rule!.TaxGroupId);
        Assert.NotNull((await service.CreateAsync(input, default)).Error);
        var id = (await db.Set<InventPosting>().SingleAsync()).RecId;
        identity.DataAreaId = "other";
        Assert.Empty((await service.ListAsync(input.InventAccountType, default))!);
        Assert.NotNull((await service.UpdateAsync(id, input, default)).Error);
        Assert.False((await service.DeleteAsync(id, default)).Deleted);
        Assert.Single(await db.Set<InventPosting>().ToListAsync());
    }

    [Fact]
    public async Task Physical_as_of_report_uses_dated_movements_and_excludes_draft_demand()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>()
            .UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new InventDim { RecId = 1001, DataAreaId = "dat", InventDimId = "D1", InventSiteId = "JED", InventLocationId = "MAIN" },
            new InventTrans { RecId = 1002, DataAreaId = "dat", ItemId = "PAPER", InventDimId = "D1", Qty = 100,
                StatusReceipt = StatusReceipt.Purchased, DatePhysical = new DateTime(2026, 10, 1, 10, 0, 0) },
            new InventTrans { RecId = 1003, DataAreaId = "dat", ItemId = "PAPER", InventDimId = "D1", Qty = -40,
                StatusIssue = StatusIssue.Deducted, DatePhysical = new DateTime(2026, 10, 2, 16, 0, 0) },
            new InventTrans { RecId = 1004, DataAreaId = "dat", ItemId = "PAPER", InventDimId = "D1", Qty = -10,
                StatusIssue = StatusIssue.Ordered },
            new InventTrans { RecId = 1005, DataAreaId = "other", ItemId = "PAPER", InventDimId = "D1", Qty = 999,
                StatusReceipt = StatusReceipt.Purchased, DatePhysical = new DateTime(2026, 10, 1) });
        await db.SaveChangesAsync();

        var service = new InventPhysicalAsOfService(new TestUnitOfWork(db), new TestIdentity());
        var beforeIssue = (await service.SearchAsync(new DateOnly(2026, 10, 1), "PAPER", "JED", "MAIN", 1, 50))!;
        Assert.Equal(100, Assert.Single(beforeIssue.Rows).PhysicalQuantity);
        var afterIssue = (await service.SearchAsync(new DateOnly(2026, 10, 2), "PAPER", "JED", "MAIN", 1, 50))!;
        Assert.Equal(60, Assert.Single(afterIssue.Rows).PhysicalQuantity);
        Assert.Equal(0, afterIssue.Rows[0].UnverifiedStateCount);
        Assert.True(afterIssue.Rows[0].DimensionFound);
    }

    [Fact]
    public async Task Physical_reconciliation_reports_differences_and_unclassified_states_without_writing_stock()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>()
            .UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new InventDim { RecId = 801, DataAreaId = "dat", InventDimId = "DIM-1", InventSiteId = "JED", InventLocationId = "MAIN" },
            new InventSum { RecId = 802, DataAreaId = "dat", ItemId = "PAPER", InventDimId = "DIM-1", PhysicalInvent = 12 },
            new InventTrans { RecId = 803, DataAreaId = "dat", ItemId = "PAPER", InventDimId = "DIM-1", Qty = 10, StatusReceipt = StatusReceipt.Purchased },
            new InventTrans { RecId = 804, DataAreaId = "dat", ItemId = "PAPER", InventDimId = "DIM-1", Qty = 3, StatusReceipt = StatusReceipt.Registered },
            new InventTrans { RecId = 805, DataAreaId = "other", ItemId = "PAPER", InventDimId = "DIM-1", Qty = 100, StatusReceipt = StatusReceipt.Purchased });
        await db.SaveChangesAsync();

        var result = (await new InventPhysicalReconciliationService(new TestUnitOfWork(db), new TestIdentity())
            .CheckAsync("PAPER", "JED", "MAIN"))!;

        Assert.Equal(1, result.ComparedKeys);
        Assert.Equal(1, result.DifferenceCount);
        Assert.Equal(1, result.UnclassifiedTransactionCount);
        Assert.False(result.NoReportedIssues);
        var row = Assert.Single(result.Rows);
        Assert.Equal(10, row.TransactionPhysical);
        Assert.Equal(12, row.SummaryPhysical);
        Assert.Equal(2, row.Difference);
        Assert.Equal(12, (await db.Set<InventSum>().SingleAsync(x => x.DataAreaId == "dat")).PhysicalInvent);
    }

    [Fact]
    public async Task Fifo_preview_allocates_financial_journal_receipts_by_site_without_writing_adjustments()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>()
            .UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var date = new DateTime(2026, 9, 30);
        db.AddRange(
            new InventModelGroup { RecId = 501, DataAreaId = "dat", ModelGroupId = "FIFO", InventModel = InventModel.FIFO },
            new InventModelGroupItem { RecId = 502, DataAreaId = "dat", ItemDataAreaId = "dat", ItemId = "PAPER", ModelGroupDataAreaId = "dat", ModelGroupId = "FIFO" },
            new InventDim { RecId = 503, DataAreaId = "dat", InventDimId = "DIM-A", InventSiteId = "JED", InventLocationId = "MAIN" },
            new InventDim { RecId = 504, DataAreaId = "dat", InventDimId = "DIM-B", InventSiteId = "JED", InventLocationId = "SHOP" },
            new InventTransOrigin { RecId = 505, DataAreaId = "dat", InventTransId = "R-A", ItemId = "PAPER", ReferenceCategory = InventRefType.InventoryJournal },
            new InventTransOrigin { RecId = 506, DataAreaId = "dat", InventTransId = "R-B", ItemId = "PAPER", ReferenceCategory = InventRefType.InventoryJournal },
            new InventTransOrigin { RecId = 507, DataAreaId = "dat", InventTransId = "ISS", ItemId = "PAPER", ReferenceCategory = InventRefType.InventoryJournal },
            new InventTrans { RecId = 508, DataAreaId = "dat", InventTransOrigin = 505, ItemId = "PAPER", InventDimId = "DIM-A", Qty = 100, CostAmountPosted = 1000, StatusReceipt = StatusReceipt.Purchased, DateFinancial = date },
            new InventTrans { RecId = 509, DataAreaId = "dat", InventTransOrigin = 506, ItemId = "PAPER", InventDimId = "DIM-B", Qty = 100, CostAmountPosted = 1400, StatusReceipt = StatusReceipt.Purchased, DateFinancial = date },
            new InventTrans { RecId = 510, DataAreaId = "dat", InventTransOrigin = 507, ItemId = "PAPER", InventDimId = "DIM-A", Qty = -150, CostAmountPosted = -1800, StatusIssue = StatusIssue.Sold, DateFinancial = date });
        await db.SaveChangesAsync();

        var result = await new FifoCostPreviewService(new TestUnitOfWork(db), new TestIdentity())
            .PreviewAsync(date);

        var issue = Assert.Single(result.Issues);
        Assert.Equal(-1700, issue.FifoCost);
        Assert.Equal(100, issue.RemainingAdjustment);
        Assert.Equal(new decimal[] { 100, 50 }, issue.Allocations.Select(x => x.Quantity));
        Assert.False(result.CanExecute);
        Assert.Equal(0, await db.Set<InventSettlement>().CountAsync());
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

    static InventoryArchitectureTests()
    {
        new InventLocationMapping().Register(TypeAdapterConfig.GlobalSettings);
        new InventTransMapping().Register(TypeAdapterConfig.GlobalSettings);
    }

    [Fact]
    public async Task Basic_item_model_group_rejects_advanced_controls()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var service = new InventModelGroupService(new TestUnitOfWork(db), new TestIdentity(), new TestIdentity());

        var valid = new InventModelGroupRecordDto
        {
            ModelGroupId = "FIFO-BASIC", Name = "Basic FIFO", InventModel = (int)InventModel.FIFO,
            StockedProduct = true, PostOnHandFinancial = true
        };
        var advanced = new InventModelGroupRecordDto
        {
            ModelGroupId = "FIFO-ADV", Name = "Advanced FIFO", InventModel = (int)InventModel.FIFO,
            StockedProduct = true, PostOnHandFinancial = true, MandatoryPick = true
        };

        Assert.Null((await service.CreateAsync(valid, default)).Error);
        Assert.Contains("outside the basic inventory scope", (await service.CreateAsync(advanced, default)).Error);
        Assert.Equal(1, await db.Set<InventModelGroup>().CountAsync());
    }

    [Fact]
    public async Task Item_group_rejects_invalid_posting_accounts_without_partial_changes()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        new InventItemGroupMapping().Register(TypeAdapterConfig.GlobalSettings);
        var identity = new TestIdentity();
        var service = new InventItemGroupService(new TestUnitOfWork(db), identity, identity);
        var invalid = new InventItemGroupRecordDto { ItemGroupId = "TRADE", Name = "Trading",
            Postings = [new InventItemGroupPostingDto { AccountType = 9, LedgerDimension = "999" }] };
        Assert.NotNull((await service.CreateAsync(invalid, default)).Error);
        Assert.Empty(await db.Set<InventItemGroup>().ToListAsync());
        var created = await service.CreateAsync(new InventItemGroupRecordDto
            { ItemGroupId = "TRADE", Name = "Trading" }, default);
        Assert.Null(created.Error);
        var update = await service.UpdateAsync(long.Parse(created.Group!.Id), new InventItemGroupRecordDto
            { ItemGroupId = "TRADE", Name = "Changed", Postings = invalid.Postings }, default);
        Assert.NotNull(update.Error);
        Assert.Equal("Trading", (await db.Set<InventItemGroup>().SingleAsync()).Name);
        Assert.Empty(await db.Set<InventPosting>().Where(x => x.ItemRelation == "TRADE").ToListAsync());
    }

    [Fact]
    public async Task Item_group_create_rolls_back_when_posting_sync_fails()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var identity = new TestIdentity();
        var service = new InventItemGroupService(new TestUnitOfWork(db, failOnComplete: 2), identity, identity);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(
            new InventItemGroupRecordDto { ItemGroupId = "TRADE", Name = "Trading" }, default));
        Assert.Empty(await db.Set<InventItemGroup>().AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task Item_model_group_supports_only_basic_stocked_fifo()
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
        { ModelGroupId = "FIFO-NEW", Name = "FIFO", InventModel = (int)InventModel.FIFO,
            StockedProduct = true, PostOnHandFinancial = true }, default);
        var weightedAverage = await service.CreateAsync(new InventModelGroupRecordDto
        { ModelGroupId = "AVG-NEW", Name = "Weighted average", InventModel = (int)InventModel.WeightedAvg }, default);
        var standardCost = await service.CreateAsync(new InventModelGroupRecordDto
        { ModelGroupId = "STD-NEW", Name = "Standard cost", InventModel = (int)InventModel.StandardCost }, default);
        var movingAverage = await service.CreateAsync(new InventModelGroupRecordDto
        { ModelGroupId = "MOV-AVG", Name = "Moving average", InventModel = (int)InventModel.MovingAverage }, default);

        Assert.Null(fifo.Error);
        Assert.Contains("FIFO only", weightedAverage.Error);
        Assert.Contains("FIFO only", standardCost.Error);
        Assert.Contains("FIFO only", movingAverage.Error);
        Assert.Single(await db.Set<InventModelGroup>().Where(x => x.ModelGroupId == "FIFO-NEW").ToListAsync());
    }

    [Fact]
    public async Task Journal_posting_rejects_missing_inventory_posting_accounts()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new InventJournalTable { RecId = 1, DataAreaId = "dat", JournalId = "J-1", JournalNameId = "ADJ", JournalType = InventJournalType.Adjustment, VoucherNumberSequenceTable = 4 },
            new InventJournalTrans { RecId = 2, DataAreaId = "dat", JournalId = "J-1", JournalType = InventJournalType.Adjustment, ItemId = "ITEM-1", InventDimId = "DIM-1", Unit = "pcs", Qty = 1, CostPrice = 10 },
            new InventJournalName { RecId = 3, DataAreaId = "dat", JournalNameId = "ADJ", JournalType = InventJournalType.Adjustment, VoucherNumberSequenceTable = 4 },
            new SysNumberSequence { RecId = 4, DataAreaId = "dat", NumberSequence = "INV-VOUCHER", Txt = "Inventory voucher", Format = "IV-#####", AnnotatedFormat = "IV-{SEQ}", InUse = 1, IsActive = true, NextRec = 1, Highest = 99999 },
            new ReleasedItem { RecId = 5, DataAreaId = "dat", ItemId = "ITEM-1", Product = 6 },
            new InventTableModule { RecId = 16, DataAreaId = "dat", ItemId = "ITEM-1", ModuleType = ModuleInventPurchSales.Inventory, UnitId = "pcs" },
            new InventModelGroup { RecId = 7, DataAreaId = "dat", ModelGroupId = "FIFO", Name = "FIFO", InventModel = InventModel.FIFO, StockedProduct = NoYes.Yes },
            new InventModelGroupItem { RecId = 8, DataAreaId = "dat", ItemId = "ITEM-1", ItemDataAreaId = "dat", ModelGroupId = "FIFO", ModelGroupDataAreaId = "dat" },
            new EcoResTrackingDimensionGroupItem { RecId = 9, DataAreaId = "dat", ItemId = "ITEM-1", ItemDataAreaId = "dat", TrackingDimensionGroup = 10 },
            new EcoResTrackingDimensionGroup { RecId = 10, Name = "No tracking" },
            new InventDim { RecId = 11, DataAreaId = "dat", InventDimId = "DIM-1", InventSiteId = "JED", InventLocationId = "MAIN" },
            new Ledger { RecId = 12, DataAreaId = "dat", FiscalCalendar = 13, AccountingCurrency = "SAR", ReportingCurrency = "SAR" },
            new FiscalCalendarPeriod { RecId = 14, DataAreaId = "dat", FiscalCalendar = 13, StartDate = DateTime.Today.AddDays(-1), EndDate = DateTime.Today.AddDays(1), Type = FiscalPeriodType.Operating },
            new LedgerFiscalCalendarPeriod { RecId = 15, DataAreaId = "dat", Ledger = 12, FiscalCalendarPeriod = 14, Status = FiscalPeriodStatus.Open });
        await db.SaveChangesAsync();
        var identity = new TestIdentity();
        var unitOfWork = new TestUnitOfWork(db);
        var posting = new InventJournalPostingService(unitOfWork, identity, identity,
            new SysNumberSequenceService(unitOfWork, identity), new TestSalesInventoryNumbers());

        var result = await posting.PostAsync("J-1", default);

        Assert.Contains("Inventory posting setup is missing", result.Error);
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
    public async Task Transaction_query_preserves_costs_labels_order_and_company_scope()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new InventTrans { RecId = 1, DataAreaId = "dat", ItemId = "ITEM", InventTransOrigin = 10, InventDimId = "DIM", Qty = -2,
                CostAmountPosted = -20, CostAmountPhysical = -4, CostAmountAdjustment = 2, StatusIssue = StatusIssue.Ordered,
                DatePhysical = new DateTime(2026, 10, 23, 14, 0, 0), DateFinancial = new DateTime(2026, 10, 24, 9, 0, 0) },
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
        var dated = (await service.SearchPageAsync("ITEM", "TX", "S1", "W1",
            new DateTime(2026, 10, 23), new DateTime(2026, 10, 23),
            new DateTime(2026, 10, 24), new DateTime(2026, 10, 24), 1, 1))!;
        Assert.Equal(1, dated.TotalCount);
        Assert.Equal(1, Assert.Single(dated.Rows).RecId);
        Assert.Equal(-4, dated.Rows[0].PhysicalCostAmount);
        Assert.Equal(-20, dated.Rows[0].PostedCostAmount);
        Assert.Equal(2, dated.Rows[0].CostAdjustmentAmount);
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
                PhysicalInvent = 10, ReservPhysical = 2, Picked = 3, AvailPhysical = 8,
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
        Assert.Equal(5, row.AvailablePhysical);
        Assert.Equal(4, row.OrderedReceipts);
        Assert.Equal(3, row.OrderedIssues);
        Assert.Equal(6, row.AvailableOrdered);
        Assert.Equal(700, row.PostedValue);
        Assert.Equal(100, row.PhysicalValue);

        db.Add(new InventDim { RecId = 7, DataAreaId = "dat", InventDimId = "DIM-VALUE", InventSiteId = "JED", InventLocationId = "MAIN" });
        db.Add(new InventDim { RecId = 8, DataAreaId = "dat", InventDimId = "DIM-ZERO", InventSiteId = "JED", InventLocationId = "MAIN" });
        db.Add(new InventSum { RecId = 9, DataAreaId = "dat", ItemId = "ITEM-001", InventDimId = "DIM-VALUE", PostedValue = 25 });
        db.Add(new InventSum { RecId = 10, DataAreaId = "dat", ItemId = "ITEM-001", InventDimId = "DIM-ZERO" });
        await db.SaveChangesAsync();
        var nonzeroPage = (await new InventOnHandService(new TestUnitOfWork(db), identity)
            .SearchPageAsync(null, null, null, null, false, 1, 50))!;
        Assert.Equal(2, nonzeroPage.TotalCount);
        Assert.Contains(nonzeroPage.Rows, x => x.InventDimId == "DIM-VALUE" && x.PostedValue == 25);
        Assert.DoesNotContain(nonzeroPage.Rows, x => x.InventDimId == "DIM-ZERO");

        identity.DataAreaId = null;
        Assert.Null(await new InventOnHandService(new TestUnitOfWork(db), identity).ListAsync());
    }

    [Fact]
    public async Task On_hand_page_filters_group_and_zero_rows_before_counting_and_paging()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new ReleasedItem { RecId = 901, DataAreaId = "dat", ItemId = "A", Name = "A" },
            new ReleasedItem { RecId = 902, DataAreaId = "dat", ItemId = "B", Name = "B" },
            new InventDim { RecId = 903, DataAreaId = "dat", InventDimId = "D1", InventSiteId = "JED", InventLocationId = "MAIN" },
            new InventItemGroupItem { RecId = 904, ItemDataAreaId = "dat", ItemGroupDataAreaId = "dat", ItemId = "A", ItemGroupId = "TRADE" },
            new InventItemGroupItem { RecId = 905, ItemDataAreaId = "dat", ItemGroupDataAreaId = "dat", ItemId = "B", ItemGroupId = "TRADE" },
            new InventTableModule { RecId = 906, DataAreaId = "dat", ItemId = "A", ModuleType = ModuleInventPurchSales.Inventory, UnitId = "EA" },
            new InventSum { RecId = 907, DataAreaId = "dat", ItemId = "A", InventDimId = "D1", PhysicalInvent = 4 },
            new InventSum { RecId = 908, DataAreaId = "dat", ItemId = "B", InventDimId = "D1", PhysicalInvent = 0 });
        await db.SaveChangesAsync();

        var service = new InventOnHandService(new TestUnitOfWork(db), new TestIdentity());
        var nonzero = (await service.SearchPageAsync(null, "TRADE", "JED", "MAIN", false, 1, 1))!;
        Assert.Equal(1, nonzero.TotalCount);
        Assert.Equal("A", Assert.Single(nonzero.Rows).ItemNumber);
        Assert.Equal("EA", nonzero.Rows[0].InventoryUnit);
        var all = (await service.SearchPageAsync(null, "TRADE", "JED", "MAIN", true, 2, 1))!;
        Assert.Equal(2, all.TotalCount);
        Assert.Equal("B", Assert.Single(all.Rows).ItemNumber);
        Assert.Equal(0, all.Rows[0].PhysicalInventory);
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
            new InventModelGroup { RecId = 10, DataAreaId = "dat", ModelGroupId = "FIFO", Name = "FIFO", InventModel = InventModel.FIFO, StockedProduct = NoYes.Yes, PostOnHandFinancial = NoYes.Yes },
            new InventModelGroupItem { RecId = 11, DataAreaId = "dat", ItemId = "ITEM-001", ItemDataAreaId = "dat", ModelGroupId = "FIFO", ModelGroupDataAreaId = "dat" },
            new EcoResTrackingDimensionGroupItem { RecId = 40, DataAreaId = "dat", ItemId = "ITEM-001", ItemDataAreaId = "dat", TrackingDimensionGroup = 41 },
            new EcoResTrackingDimensionGroup { RecId = 41, Name = "No tracking" },
            new InventTableModule { RecId = 2, DataAreaId = "other", ItemId = "ITEM-001", ModuleType = ModuleInventPurchSales.Inventory, UnitId = "box" },
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
        Assert.Equal(5, (await db.Set<InventSum>().SingleAsync(x => x.DataAreaId == "dat" && x.InventLocationId == "MAIN")).PhysicalInvent);

        var posted = await posting.PostAsync(created.Journal.JournalId, default);
        Assert.Null(posted.Error);
        Assert.True(posted.Result!.Posted);
        Assert.Equal(2, posted.Result.InventoryTransactionsCreated);
        Assert.Equal(0, posted.Result.LedgerEntriesCreated);
        Assert.Equal(3, await db.Set<InventTrans>().CountAsync());
        var transferTransactions = await db.Set<InventTrans>()
            .Where(x => x.ItemId == "ITEM-001" && x.Voucher == posted.Result.Vouchers.Single())
            .ToListAsync();
        transferTransactions = transferTransactions.OrderBy(x => x.Qty).ToList();
        Assert.Equal(new decimal[] { -2, 2 }, transferTransactions.Select(x => x.Qty));
        Assert.Equal(0, transferTransactions.Sum(x => x.Qty));
        Assert.Equal(0, transferTransactions.Sum(x => x.CostAmountPosted));
        Assert.Equal(3, (await db.Set<InventSum>().SingleAsync(x => x.InventDimId == line.InventDimId)).PhysicalInvent);
        Assert.Equal(2, (await db.Set<InventSum>().SingleAsync(x => x.InventDimId == line.ToInventDimId)).PhysicalInvent);
        var repeated = await posting.PostAsync(created.Journal.JournalId, default);
        Assert.Null(repeated.Error);
        Assert.Equal(posted.Result.Vouchers, repeated.Result!.Vouchers);
        Assert.Equal(3, await db.Set<InventTrans>().CountAsync());
        Assert.False((await service.DeleteAsync(created.Journal.JournalId, default)).Deleted);

        var excessive = await service.CreateAsync(new InventJournalCreateDto
        { JournalNameId = "TRF", Description = "Too much stock", InventSiteId = "JED", InventLocationId = "MAIN" }, default);
        for (var i = 0; i < 2; i++)
        {
            var draft = await service.AddLineAsync(excessive.Journal!.JournalId, new InventJournalLineInputDto
            { ItemId = "ITEM-001", InventSiteId = "JED", InventLocationId = "MAIN", ToInventSiteId = "JED",
                ToInventLocationId = "SHOW", Unit = "pcs", Qty = 2, TransDate = DateTime.Today }, default);
            Assert.Null(draft.Error);
        }
        Assert.Contains("exceed available stock", (await posting.PostAsync(excessive.Journal!.JournalId, default)).Error);
        Assert.Equal(3, await db.Set<InventTrans>().CountAsync());
        Assert.Equal(3, (await db.Set<InventSum>().SingleAsync(x => x.InventDimId == line.InventDimId)).PhysicalInvent);

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
        { ItemId = "ITEM-001", InventSiteId = "JED", InventLocationId = "OPEN", Unit = "pcs", Qty = 4,
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
        // FIFO uses the same-site provisional average across all three warehouses until close.
        Assert.Equal(-(230m / 12m),
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
        var duplicateCount = await service.AddLineAsync(counting.Journal.JournalId, new InventJournalLineInputDto
        { ItemId = "ITEM-001", InventSiteId = "JED", InventLocationId = "MAIN", Unit = "pcs", Counted = 0,
            TransDate = DateTime.Today }, default);
        Assert.Contains("already in the counting journal", duplicateCount.Error);
        var countPost = await posting.PostAsync(counting.Journal.JournalId, default);
        Assert.Contains("durable movement freeze", countPost.Error);
        Assert.Null(countPost.Result);
        Assert.Equal(7, (await db.Set<InventSum>().SingleAsync(x => x.InventDimId == line.InventDimId)).PhysicalInvent);
        Assert.Equal(await db.Set<InventTrans>().Where(x => x.ItemId == "ITEM-001").SumAsync(x => x.Qty),
            await db.Set<InventSum>().Where(x => x.ItemId == "ITEM-001").SumAsync(x => x.PhysicalInvent));
        Assert.Equal(0, await db.Set<GeneralJournalAccountEntry>().SumAsync(x => x.AccountingCurrencyAmount));

        Assert.Null(await posting.FreezeCountAsync(counting.Journal.JournalId, default));
        db.ChangeTracker.Clear(); // Freeze must survive a new request, not just tracked state.
        Assert.NotNull(await InventoryCountProtection.CheckAsync(db, "dat", "ITEM-001", line.InventDimId, default));
        Assert.NotNull((await service.DeleteAsync(counting.Journal.JournalId, default)).Error);
        var blockedJournal = await service.CreateAsync(new InventJournalCreateDto
        { JournalNameId = "ADJ", Description = "Blocked during count", InventSiteId = "JED", InventLocationId = "MAIN" }, default);
        Assert.Null((await service.AddLineAsync(blockedJournal.Journal!.JournalId, new InventJournalLineInputDto
        { ItemId = "ITEM-001", InventSiteId = "JED", InventLocationId = "MAIN", Unit = "pcs", Qty = 1, CostPrice = 10, TransDate = DateTime.Today }, default)).Error);
        Assert.Contains("frozen for counting", (await posting.PostAsync(blockedJournal.Journal.JournalId, default)).Error);
        Assert.Equal(7m, (await db.Set<InventSum>().SingleAsync(x => x.InventDimId == line.InventDimId)).PhysicalInvent);
        var frozenLine = await db.Set<InventJournalTrans>().SingleAsync(x => x.JournalId == counting.Journal.JournalId);
        Assert.Null(await posting.ConfirmCountAsync(counting.Journal.JournalId, new Dictionary<long, decimal> { [frozenLine.RecId] = 10m }, default));
        Assert.NotNull(await posting.ConfirmCountAsync(counting.Journal.JournalId, new Dictionary<long, decimal> { [frozenLine.RecId] = 11m }, default));
        var confirmedCount = await posting.PostAsync(counting.Journal.JournalId, default);
        Assert.Null(confirmedCount.Error);
        Assert.Equal(10m, (await db.Set<InventSum>().SingleAsync(x => x.InventDimId == line.InventDimId)).PhysicalInvent);
        var countTransactions = await db.Set<InventTrans>().CountAsync();
        Assert.Null((await posting.PostAsync(counting.Journal.JournalId, default)).Error);
        Assert.Equal(countTransactions, await db.Set<InventTrans>().CountAsync());
        Assert.Null(await InventoryCountProtection.CheckAsync(db, "dat", "ITEM-001", line.InventDimId, default));

        db.Add(new InventLocation { DataAreaId = "dat", InventLocationId = "TRANSIT", InventSiteId = "JED", Name = "In transit", InventLocationType = InventLocationType.Transit });
        await db.SaveChangesAsync();
        var transfers = new IAX.IXApi.Modules.Finance.Inventory.Transfer.InventTransferOrderService(
            uow, identity, dimensions, new TestSalesInventoryNumbers());
        var transferInput = new IAX.IXApi.Modules.Finance.Inventory.Transfer.InventTransferCreateDto(
            "TR-DEMO", "JED", "MAIN", "TRANSIT", "SHOW", DateTime.Today, DateTime.Today,
            [new("ITEM-001", "pcs", 2m)]);
        var destinationBefore = await db.Set<InventSum>().Where(x => x.InventLocationId == "SHOW").SumAsync(x => x.PhysicalInvent);
        Assert.Null((await transfers.CreateAsync(transferInput, default)).Error);
        // A transfer identifier belongs to one order.
        Assert.NotNull((await transfers.CreateAsync(transferInput, default)).Error);
        // Source, transit and destination must differ.
        Assert.NotNull((await transfers.CreateAsync(transferInput with { TransferId = "TR-BAD", InventLocationIdTo = "MAIN" }, default)).Error);
        // Nothing is in transit yet, so there is nothing to receive.
        Assert.NotNull((await transfers.ReceiveAsync("TR-DEMO", Post("EARLY", 1m), default)).Error);

        Assert.Null((await transfers.ShipAsync("TR-DEMO", Post("SHIP1", 1m), default)).Error);
        var shippedCount = await db.Set<InventTrans>().CountAsync();
        // The request identifier makes a repeated shipment a no-operation.
        Assert.Null((await transfers.ShipAsync("TR-DEMO", Post("SHIP1", 1m), default)).Error);
        Assert.Equal(shippedCount, await db.Set<InventTrans>().CountAsync());
        Assert.Equal(1m, await db.Set<InventSum>().Where(x => x.InventLocationId == "TRANSIT").SumAsync(x => x.PhysicalInvent));
        var partlyShipped = (await transfers.GetAsync("TR-DEMO", default)).Order!;
        Assert.Equal(InventTransferStatus.Shipped, partlyShipped.TransferStatus);
        Assert.Equal(1m, partlyShipped.Lines[0].QtyShipped);
        Assert.Equal(1m, partlyShipped.Lines[0].QtyRemainShip);
        // Both shipment legs own an inventory transaction.
        Assert.NotEqual(string.Empty, partlyShipped.Lines[0].InventTransId);
        Assert.NotEqual(string.Empty, partlyShipped.Lines[0].InventTransIdTransitTo);

        // A receipt cannot exceed the stock this order has in transit.
        Assert.NotNull((await transfers.ReceiveAsync("TR-DEMO", Post("OVER", 2m), default)).Error);
        Assert.Null((await transfers.ReceiveAsync("TR-DEMO", Post("RECEIVE1", 1m), default)).Error);
        Assert.Null((await transfers.ShipAsync("TR-DEMO", Post("SHIP2", 1m), default)).Error);
        // The shipment is complete, so nothing more may be shipped.
        Assert.NotNull((await transfers.ShipAsync("TR-DEMO", Post("SHIP3", 1m), default)).Error);
        Assert.Null((await transfers.ReceiveAsync("TR-DEMO", Post("RECEIVE2", 1m), default)).Error);

        Assert.Equal(0m, await db.Set<InventSum>().Where(x => x.InventLocationId == "TRANSIT").SumAsync(x => x.PhysicalInvent));
        var finished = (await transfers.GetAsync("TR-DEMO", default)).Order!;
        Assert.Equal(InventTransferStatus.Received, finished.TransferStatus);
        Assert.Equal(2m, finished.Lines[0].QtyReceived);
        Assert.Equal(0m, finished.Lines[0].QtyRemainReceive);
        Assert.Equal(InventTransferRemainStatus.Received, finished.Lines[0].RemainStatus);
        Assert.NotEqual(string.Empty, finished.Lines[0].InventTransIdTransitFrom);
        Assert.NotEqual(string.Empty, finished.Lines[0].InventTransIdReceive);
        // Source, transit and destination reconcile: the two units left MAIN and arrived at SHOW.
        Assert.Equal(destinationBefore + 2m, await db.Set<InventSum>().Where(x => x.InventLocationId == "SHOW").SumAsync(x => x.PhysicalInvent));
        // Transfers never post through an inventory journal.
        Assert.False(await db.Set<InventJournalTable>().AnyAsync(x => x.Description!.Contains("TR-DEMO")));

        static IAX.IXApi.Modules.Finance.Inventory.Transfer.InventTransferPostDto Post(string requestId, decimal qty) =>
            new(requestId, DateTime.Today, new Dictionary<decimal, decimal> { [1m] = qty });

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
        Assert.Contains("available physical inventory", rollbackResult.Error);
        Assert.Equal(beforeRollbackTransactions, await db.Set<InventTrans>().CountAsync());
        Assert.Equal(beforeRollbackEntries, await db.Set<GeneralJournalEntry>().CountAsync());
        Assert.Equal(beforeRollbackQty, await db.Set<InventSum>().Where(x => x.ItemId == "ITEM-001").SumAsync(x => x.PhysicalInvent));
        Assert.False((await service.GetAsync(rollbackJournal.Journal.JournalId, default))!.Posted);

        (await db.Set<InventModelGroup>().SingleAsync()).InventModel = InventModel.StandardCost;
        db.AddRange(
            new InventItemPrice { DataAreaId = "other", ItemId = "ITEM-001", InventDimId = line.InventDimId,
                CostingType = CostingType.StandardCost, PriceType = PreferredPriceType.CostPrice,
                ActivationDate = DateTime.Today.AddDays(-1), Price = 1000, PriceUnit = 1 },
            new InventItemPrice { DataAreaId = "dat", ItemId = "ITEM-001", InventDimId = "OTHER-DIM",
                CostingType = CostingType.StandardCost, PriceType = PreferredPriceType.CostPrice,
                ActivationDate = DateTime.Today.AddDays(-1), Price = 500, PriceUnit = 1 },
            new InventItemPrice { DataAreaId = "dat", ItemId = "ITEM-001", InventDimId = string.Empty,
                CostingType = CostingType.StandardCost, PriceType = PreferredPriceType.CostPrice,
                ActivationDate = DateTime.Today.AddDays(-1), Price = 8, PriceUnit = 1 },
            new InventItemPrice { DataAreaId = "dat", ItemId = "ITEM-001", InventDimId = line.InventDimId,
                CostingType = CostingType.StandardCost, PriceType = PreferredPriceType.CostPrice,
                ActivationDate = DateTime.Today.AddDays(-1), Price = 12, PriceUnit = 2 });
        await db.SaveChangesAsync();
        var standardJournal = await service.CreateAsync(new InventJournalCreateDto
        { JournalNameId = "ADJ", Description = "Existing standard-cost item", InventSiteId = "JED", InventLocationId = "MAIN" }, default);
        var standardLine = await service.AddLineAsync(standardJournal.Journal!.JournalId, new InventJournalLineInputDto
        { ItemId = "ITEM-001", InventSiteId = "JED", InventLocationId = "MAIN", Unit = "pcs", Qty = 1,
            CostPrice = 999, TransDate = DateTime.Today }, default);
        Assert.Null(standardLine.Error);
        var beforeVarianceTransactions = await db.Set<InventTrans>().CountAsync();
        var varianceRejected = await posting.PostAsync(standardJournal.Journal.JournalId, default);
        Assert.Contains("variance posting is not implemented", varianceRejected.Error);
        Assert.Equal(beforeVarianceTransactions, await db.Set<InventTrans>().CountAsync());
        var correctedLine = await service.UpdateLineAsync(standardJournal.Journal.JournalId,
            Assert.Single(standardLine.Journal!.Lines).RecId, new InventJournalLineInputDto
            { ItemId = "ITEM-001", InventSiteId = "JED", InventLocationId = "MAIN", Unit = "pcs", Qty = 1,
                CostPrice = 6, TransDate = DateTime.Today }, default);
        Assert.Null(correctedLine.Error);
        var standardPost = await posting.PostAsync(standardJournal.Journal.JournalId, default);
        Assert.Null(standardPost.Error);
        Assert.Equal(6, await db.Set<InventTrans>().Where(x => x.Voucher == standardPost.Result!.Vouchers.Single())
            .Select(x => x.CostAmountPosted).SingleAsync());
    }

    private sealed class InventoryContext(DbContextOptions<InventoryContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            Configure<InventSite>(builder);
            Configure<InventLocation>(builder);
            Configure<InventModelGroup>(builder);
            Configure<InventBatch>(builder);
            Configure<InventSerial>(builder);
            Configure<InventItemLocation>(builder);
            Configure<InventCountGroup>(builder);
            Configure<InventPackagingGroup>(builder);
            Configure<InventPackagingUnit>(builder);
            Configure<WMSLocation>(builder);
            Configure<InventTrans>(builder);
            Configure<IAX.IXApi.Modules.Finance.Entities.InventTransPosting>(builder);
            Configure<InventTransOrigin>(builder);
            Configure<InventSettlement>(builder);
            Configure<InventClosing>(builder);
            Configure<InventDim>(builder);
            Configure<SalesLine>(builder);
            Configure<SalesTable>(builder);
            Configure<CustPackingSlipJour>(builder);
            Configure<ReleasedItem>(builder);
            Configure<InventSum>(builder);
            Configure<InventItemBarcode>(builder);
            Configure<UnitOfMeasure>(builder);
            Configure<UnitOfMeasureConversion>(builder);
            Configure<InventTableModule>(builder);
            Configure<InventItemPurchSetup>(builder);
            Configure<InventItemSalesSetup>(builder);
            Configure<InventItemInventSetup>(builder);
            Configure<InventItemSetupSupplyType>(builder);
            Configure<InventItemPrice>(builder);
            Configure<SharedProduct>(builder);
            builder.Entity<SharedProduct>().HasQueryFilter(x => !x.IsDeleted);
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
            Configure<InventTransferTable>(builder);
            Configure<InventTransferLine>(builder);
            builder.ApplyConfiguration(new IAX.IXApi.Modules.Finance.Inventory.Transfer.InventTransferTableConfiguration());
            builder.ApplyConfiguration(new IAX.IXApi.Modules.Finance.Inventory.Transfer.InventTransferLineConfiguration());
            Configure<InventJournalTrans>(builder);
            Configure<InventModelGroup>(builder);
            Configure<InventModelGroupItem>(builder);
            Configure<EcoResTrackingDimensionGroupItem>(builder);
            Configure<EcoResTrackingDimensionGroup>(builder);
            Configure<EcoResTrackingDimensionGroupFldSetup>(builder);
            Configure<EcoResStorageDimensionGroup>(builder);
            Configure<EcoResStorageDimensionGroupItem>(builder);
            Configure<EcoResStorageDimensionGroupFldSetup>(builder);
            Configure<InventItemGroupItem>(builder);
            Configure<InventItemGroup>(builder);
            Configure<InventPosting>(builder);
            Configure<TaxGroupHeading>(builder);
            Configure<TaxItemGroupHeading>(builder);
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
        private readonly int failOnComplete;
        private int completeCount;
        private IDbContextTransaction? transaction;
        public TestUnitOfWork(DbContext context, int failOnComplete = 0)
        { this.context = context; this.failOnComplete = failOnComplete; }
        public DbContext Context => context;
        public IGenericRepository<T> Repository<T>() where T : class
        {
            var repository = DispatchProxy.Create<IGenericRepository<T>, RepositoryProxy<T>>();
            ((RepositoryProxy<T>)(object)repository).Context = context;
            return repository;
        }
        public Task<int> CompleteAsync(CancellationToken ct = default)
        {
            if (++completeCount == failOnComplete) throw new InvalidOperationException("Injected save failure.");
            return context.SaveChangesAsync(ct);
        }
        public async Task BeginTransactionAsync(CancellationToken ct = default)
        {
            if (context.Database.CurrentTransaction == null) transaction = await context.Database.BeginTransactionAsync(ct);
        }
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
