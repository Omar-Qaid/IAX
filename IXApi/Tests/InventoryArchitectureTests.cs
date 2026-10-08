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
