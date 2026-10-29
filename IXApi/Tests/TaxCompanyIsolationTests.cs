using System.Reflection;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.Tax;
using IAX.IXApi.Shared.Domain.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace IAX.IXApi.Tests;

public class TaxCompanyIsolationTests
{
    [Fact]
    public async Task Same_tax_code_in_two_companies_does_not_leak_or_move_records()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new TaxContext(new DbContextOptionsBuilder<TaxContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(new TaxTable { RecId = 1, DataAreaId = "dat", TaxCode = "VAT", TaxName = "DAT" },
            new TaxTable { RecId = 2, DataAreaId = "ksa", TaxCode = "VAT", TaxName = "KSA" },
            new TaxData { RecId = 1, DataAreaId = "dat", TaxCode = "VAT", TaxValue = 5 },
            new TaxData { RecId = 2, DataAreaId = "ksa", TaxCode = "VAT", TaxValue = 15 });
        await db.SaveChangesAsync();
        var work = new Work(db);
        var user = new User("ksa");
        var rates = new TaxCodeRateService(work, NullLogger<TaxCodeRateService>.Instance, user);
        var service = new TaxTableService(work, user, rates, NullLogger<TaxTableService>.Instance);
        var details = (await service.GetDetailsAsync()).ToList();
        Assert.Single(details);
        Assert.Equal(15m, details[0].TaxValue);
        Assert.Equal("KSA", (await service.GetDetailAsync("VAT"))!.TaxName);
        Assert.Null(await service.GetDetailAsync("1"));
        Assert.Null(await service.UpdateFromDtoAsync("1", new TaxTableDto { TaxCode = "VAT", TaxValue = 99 }));
        Assert.False(await service.DeleteByCodeAsync("1"));

        await service.UpdateFromDtoAsync("2", new TaxTableDto { DataAreaId = "dat", TaxCode = "RENAMED", TaxName = "Updated KSA", TaxValue = 12 });
        Assert.Equal(5m, (await db.Set<TaxData>().SingleAsync(x => x.DataAreaId == "dat")).TaxValue);
        Assert.Equal(12m, (await db.Set<TaxData>().SingleAsync(x => x.DataAreaId == "ksa")).TaxValue);
        var updated = await db.Set<TaxTable>().SingleAsync(x => x.RecId == 2);
        Assert.Equal("ksa", updated.DataAreaId);
        Assert.Equal("VAT", updated.TaxCode);
        Assert.Equal("DAT", (await db.Set<TaxTable>().SingleAsync(x => x.RecId == 1)).TaxName);

        var created = await service.CreateFromDtoAsync(new TaxTableDto { DataAreaId = "dat", TaxCode = "NEW", TaxName = "New KSA", TaxValue = 7 });
        Assert.NotNull(created);
        Assert.Equal("ksa", (await db.Set<TaxTable>().SingleAsync(x => x.TaxCode == "NEW")).DataAreaId);
        Assert.Equal("ksa", (await db.Set<TaxData>().SingleAsync(x => x.TaxCode == "NEW")).DataAreaId);
        var missingCompany = new TaxCodeRateService(work, NullLogger<TaxCodeRateService>.Instance, new User(""));
        await Assert.ThrowsAsync<InvalidOperationException>(() => missingCompany.SyncTaxDataRateAsync("VAT", 99, default));
        Assert.Equal(5m, (await db.Set<TaxData>().SingleAsync(x => x.DataAreaId == "dat")).TaxValue);
        Assert.True(await service.DeleteByCodeAsync("VAT"));
        Assert.True(await db.Set<TaxTable>().AnyAsync(x => x.DataAreaId == "dat" && x.TaxCode == "VAT"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Rate_save_failure_rolls_back_create_and_update_and_allows_clean_retry(bool cancel)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new TaxContext(new DbContextOptionsBuilder<TaxContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var work = new Work(db) { FailOnSave = 2, CancelFailure = cancel };
        var user = new User("ksa");
        var rates = new TaxCodeRateService(work, NullLogger<TaxCodeRateService>.Instance, user);
        var service = new TaxTableService(work, user, rates, NullLogger<TaxTableService>.Instance);
        var dto = new TaxTableDto { TaxCode = "VAT", TaxName = "Original", TaxValue = 15 };
        var failure = await Record.ExceptionAsync(() => service.CreateFromDtoAsync(dto));
        Assert.NotNull(failure);
        if (cancel) Assert.IsType<OperationCanceledException>(failure);
        else Assert.IsType<InvalidOperationException>(failure);
        Assert.Empty(await db.Set<TaxTable>().ToListAsync());
        Assert.Empty(await db.Set<TaxData>().ToListAsync());
        Assert.False(db.ChangeTracker.HasChanges());

        await service.CreateFromDtoAsync(dto);
        var original = await db.Set<TaxTable>().SingleAsync();
        var rate = await db.Set<TaxData>().SingleAsync();
        var from = rate.TaxFromDate;
        var to = rate.TaxToDate;
        work.FailOnSave = work.SaveCount + 2;
        failure = await Record.ExceptionAsync(() => service.UpdateFromDtoAsync("VAT",
            new TaxTableDto { TaxCode = "VAT", TaxName = "Must roll back", TaxValue = 20 }));
        Assert.NotNull(failure);
        // Tracked objects must agree with the rolled-back database, including on retry.
        Assert.Equal("Original", original.TaxName);
        Assert.Equal(15m, rate.TaxValue);
        Assert.False(db.ChangeTracker.HasChanges());
        Assert.Equal("Original", (await db.Set<TaxTable>().AsNoTracking().SingleAsync()).TaxName);
        Assert.Equal(15m, (await db.Set<TaxData>().AsNoTracking().SingleAsync()).TaxValue);
        Assert.Equal(from, rate.TaxFromDate);
        Assert.Equal(to, rate.TaxToDate);
        await service.UpdateFromDtoAsync("VAT", new TaxTableDto { TaxCode = "VAT", TaxName = "Saved", TaxValue = 18 });
        Assert.Equal("Saved", (await db.Set<TaxTable>().AsNoTracking().SingleAsync()).TaxName);
        Assert.Equal(18m, (await db.Set<TaxData>().AsNoTracking().SingleAsync()).TaxValue);
    }

    private sealed class User(string area) : ICurrentUserService
    {
        public string GetCurrentUserId() => "test";
        public string GetOwnerAccountId() => "test";
        public string GetDataAreaId() => area;
    }

    private sealed class Work(DbContext db) : IUnitOfWork
    {
        public int FailOnSave { get; set; }
        public bool CancelFailure { get; set; }
        public int SaveCount { get; private set; }
        public DbContext Context => db;
        public async Task<int> CompleteAsync(CancellationToken ct = default)
        {
            var result = await db.SaveChangesAsync(ct);
            if (++SaveCount == FailOnSave)
            {
                if (CancelFailure) throw new OperationCanceledException("Injected rate save cancellation.");
                throw new InvalidOperationException("Injected rate save failure.");
            }
            return result;
        }
        public IGenericRepository<T> Repository<T>() where T : class
        {
            var proxy = DispatchProxy.Create<IGenericRepository<T>, InventoryCycleTests.CycleRepositoryProxy<T>>();
            ((InventoryCycleTests.CycleRepositoryProxy<T>)(object)proxy).Context = db;
            return proxy;
        }
        public Task BeginTransactionAsync(CancellationToken ct = default) => throw new NotSupportedException();
        public Task CommitTransactionAsync(CancellationToken ct = default) => throw new NotSupportedException();
        public Task RollbackTransactionAsync(CancellationToken ct = default) => throw new NotSupportedException();
        public void Dispose() { }
    }

    private sealed class TaxContext(DbContextOptions<TaxContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Intentionally no global company filter: exercise service-level isolation.
            foreach (var type in typeof(TaxTable).Assembly.GetTypes().Where(x => x.IsClass && !x.IsAbstract
                && typeof(Entity<long>).IsAssignableFrom(x) && x != typeof(TaxTable) && x != typeof(TaxData)))
                builder.Ignore(type);
            foreach (var type in new[] { typeof(TaxTable), typeof(TaxData) })
            {
                var entity = builder.Entity(type);
                entity.HasKey("RecId");
                entity.Ignore("RowVersion");
                foreach (var property in type.GetProperties())
                    if (property.PropertyType != typeof(string) && property.PropertyType != typeof(byte[]) && !property.PropertyType.IsValueType)
                        entity.Ignore(property.Name);
            }
        }
    }
}
