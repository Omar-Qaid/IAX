using System.Reflection;
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

public sealed class InventSiteTests
{
    static InventSiteTests() => new InventSiteMapping().Register(TypeAdapterConfig.GlobalSettings);

    [Fact]
    public async Task Create_and_update_preserve_normalization_contract_and_immutable_code()
    {
        await using var fixture = await Fixture.CreateAsync();
        var input = new InventSiteController.SiteInput
        {
            SiteId = " north ", Name = " North site ", DefaultInventStatusId = " Available ",
            TimeZone = 42, IsReceivingWarehouseOverrideAllowed = true, DefaultDimension = 123
        };
        var created = ReadSite(await fixture.Controller.Create(input, default));
        Assert.Equal("NORTH", created.SiteId);
        Assert.Equal("North site", created.Name);
        Assert.Equal("Available", created.DefaultInventStatusId);
        Assert.Equal(42, created.TimeZone);
        Assert.True(created.IsReceivingWarehouseOverrideAllowed);
        Assert.Equal(123, created.DefaultDimension);
        Assert.Empty(created.Warehouses);
        Assert.Equal(created.RecId.ToString(), created.Id);
        var json = JsonSerializer.SerializeToElement(created, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.Equal(new[] { "defaultDimension", "defaultInventStatusId", "id", "isReceivingWarehouseOverrideAllowed", "name", "recId", "siteId", "timeZone", "warehouses" },
            json.EnumerateObject().Select(property => property.Name).OrderBy(name => name, StringComparer.Ordinal));

        Assert.IsType<ConflictObjectResult>(await fixture.Controller.Create(input, default));
        input.Name = " Updated ";
        var updated = ReadSite(await fixture.Controller.Update(created.RecId, input, default));
        Assert.Equal("NORTH", updated.SiteId);
        Assert.Equal("Updated", updated.Name);
        input.SiteId = "SOUTH";
        input.Name = "Must not save";
        Assert.IsType<UnprocessableEntityObjectResult>(await fixture.Controller.Update(created.RecId, input, default));
        Assert.Equal("Updated", (await fixture.Db.Set<InventSite>().SingleAsync()).Name);
        Assert.IsType<NotFoundObjectResult>(await fixture.Controller.Update(999, input, default));
    }

    [Fact]
    public async Task List_and_delete_preserve_warehouse_guard_and_company_filters()
    {
        await using var fixture = await Fixture.CreateAsync();
        fixture.Db.AddRange(
            new InventSite { RecId = 10, DataAreaId = "dat", SiteId = "S1", Name = "Local" },
            new InventSite { RecId = 20, DataAreaId = "other", SiteId = "S1", Name = "Other" },
            new InventLocation { RecId = 30, DataAreaId = "dat", InventSiteId = "S1", InventLocationId = "W1", Name = "Local warehouse" },
            new InventLocation { RecId = 40, DataAreaId = "other", InventSiteId = "S1", InventLocationId = "W2", Name = "Other warehouse" });
        await fixture.Db.SaveChangesAsync();
        var result = Assert.IsType<OkObjectResult>(await fixture.Controller.List(default));
        var rows = Assert.IsType<List<InventSiteDto>>(Assert.IsType<APIResponse<object>>(result.Value).Data);
        var site = Assert.Single(rows);
        Assert.Equal("Local", site.Name);
        var warehouse = Assert.Single(site.Warehouses);
        Assert.Equal("30", warehouse.Id);
        Assert.Equal("W1", warehouse.InventLocationId);
        Assert.Equal("S1", warehouse.InventSiteId);
        Assert.IsType<ConflictObjectResult>(await fixture.Controller.Delete(10, default));
        Assert.IsType<NotFoundObjectResult>(await fixture.Controller.Delete(20, default));
        fixture.Db.Remove(await fixture.Db.Set<InventLocation>().SingleAsync());
        await fixture.Db.SaveChangesAsync();
        Assert.IsType<OkObjectResult>(await fixture.Controller.Delete(10, default));
        Assert.Empty(await fixture.Db.Set<InventSite>().ToListAsync());
        Assert.Equal(20, (await fixture.Db.Set<InventSite>().IgnoreQueryFilters().SingleAsync()).RecId);
    }

    private static InventSiteDto ReadSite(IActionResult result) =>
        Assert.IsType<InventSiteDto>(Assert.IsType<APIResponse<object>>(Assert.IsType<OkObjectResult>(result).Value).Data);

    private sealed class Fixture : IAsyncDisposable
    {
        private readonly SqliteConnection _connection = new("Data Source=:memory:");
        public SiteContext Db { get; private set; } = null!;
        public InventSiteController Controller { get; private set; } = null!;
        public static async Task<Fixture> CreateAsync()
        {
            var fixture = new Fixture();
            await fixture._connection.OpenAsync();
            fixture.Db = new SiteContext(new DbContextOptionsBuilder<SiteContext>().UseSqlite(fixture._connection).Options);
            await fixture.Db.Database.EnsureCreatedAsync();
            var identity = new TestIdentity();
            fixture.Controller = new InventSiteController(new InventSiteService(
                new TestUnitOfWork(fixture.Db), identity, identity, new InventSiteUpdateValidator()));
            return fixture;
        }
        public async ValueTask DisposeAsync() { await Db.DisposeAsync(); await _connection.DisposeAsync(); }
    }

    private sealed class SiteContext(DbContextOptions<SiteContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            Configure<InventSite>(builder);
            Configure<InventLocation>(builder);
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
        public string GetDataAreaId() => "dat";
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
