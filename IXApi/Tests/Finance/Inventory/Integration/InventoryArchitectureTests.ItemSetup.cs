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
        // The models the cost-settlement engine settles are accepted.
        var weightedAverage = await service.CreateAsync(new InventModelGroupRecordDto
        { ModelGroupId = "AVG-NEW", Name = "Weighted average", InventModel = (int)InventModel.WeightedAvg,
            StockedProduct = true, PostOnHandFinancial = true }, default);
        var lifo = await service.CreateAsync(new InventModelGroupRecordDto
        { ModelGroupId = "LIFO-NEW", Name = "LIFO", InventModel = (int)InventModel.LIFO,
            StockedProduct = true, PostOnHandFinancial = true }, default);
        // The models it does not settle are rejected rather than silently mis-costed.
        var standardCost = await service.CreateAsync(new InventModelGroupRecordDto
        { ModelGroupId = "STD-NEW", Name = "Standard cost", InventModel = (int)InventModel.StandardCost,
            StockedProduct = true, PostOnHandFinancial = true }, default);
        var movingAverage = await service.CreateAsync(new InventModelGroupRecordDto
        { ModelGroupId = "MOV-AVG", Name = "Moving average", InventModel = (int)InventModel.MovingAverage,
            StockedProduct = true, PostOnHandFinancial = true }, default);

        Assert.Null(fifo.Error);
        Assert.Null(weightedAverage.Error);
        Assert.Null(lifo.Error);
        Assert.Contains("settles", standardCost.Error);
        Assert.Contains("settles", movingAverage.Error);
        Assert.Equal(new[] { "AVG-NEW", "FIFO-NEW", "LIFO-NEW" },
            await db.Set<InventModelGroup>().Where(x => x.ModelGroupId.EndsWith("-NEW"))
                .OrderBy(x => x.ModelGroupId).Select(x => x.ModelGroupId).ToArrayAsync());
    }
}
