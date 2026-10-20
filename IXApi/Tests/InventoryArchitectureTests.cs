using System.Reflection;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Common;
using System.Text.Json;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Shared.Application.Identity;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using ReleasedItem = IAX.IXApi.Modules.Finance.Entities.InventTable;
using SharedProduct = IAX.IXApi.Modules.Finance.Inventory.InventTable.EcoResProduct;

namespace IAX.IXApi.Tests;

public sealed class InventoryArchitectureTests
{
    static InventoryArchitectureTests()
    {
        new InventLocationMapping().Register(TypeAdapterConfig.GlobalSettings);
        new InventTransMapping().Register(TypeAdapterConfig.GlobalSettings);
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

    private sealed class InventoryContext(DbContextOptions<InventoryContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            Configure<InventSite>(builder);
            Configure<InventLocation>(builder);
            Configure<InventTrans>(builder);
            Configure<InventTransOrigin>(builder);
            Configure<InventDim>(builder);
            Configure<SalesLine>(builder);
            Configure<ReleasedItem>(builder);
            Configure<InventSum>(builder);
            Configure<UnitOfMeasure>(builder);
            Configure<UnitOfMeasureConversion>(builder);
            Configure<InventTableModule>(builder);
            Configure<SharedProduct>(builder);
            Configure<InventParameters>(builder);
            Configure<InventJournalName>(builder);
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

    private sealed class TestUnitOfWork(DbContext context) : IUnitOfWork
    {
        public DbContext Context => context;
        public IGenericRepository<T> Repository<T>() where T : class
        {
            var repository = DispatchProxy.Create<IGenericRepository<T>, RepositoryProxy<T>>();
            ((RepositoryProxy<T>)(object)repository).Context = context;
            return repository;
        }
        public Task<int> CompleteAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
        public Task BeginTransactionAsync(CancellationToken ct = default) => throw new NotSupportedException();
        public Task CommitTransactionAsync(CancellationToken ct = default) => throw new NotSupportedException();
        public Task RollbackTransactionAsync(CancellationToken ct = default) => throw new NotSupportedException();
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
