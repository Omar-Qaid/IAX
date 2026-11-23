using System.Reflection;
using FluentValidation;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Tax;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
namespace IAX.IXApi.Tests;
public class TaxAccountGroupTests
{
    [Fact]
    public async Task Save_reopen_lookup_and_duplicate_validation()
    {
        await using var f = await Fixture.Create();
        var group = new TaxLedgerAccountGroup { DataAreaId = "dat", TaxAccountGroup = "VAT-STD", Name = "VAT", TaxOutgoingLedgerDimension = 100 };
        await f.Service.AddAsync(group);
        f.Db.ChangeTracker.Clear();
        var saved = await f.Db.Set<TaxLedgerAccountGroup>().SingleAsync();
        Assert.Equal(100, saved.TaxOutgoingLedgerDimension);
        saved.TaxIncomingLedgerDimension = 100;
        await f.Service.UpdateAsync(saved);
        f.Db.ChangeTracker.Clear();
        Assert.Equal(100, (await f.Db.Set<TaxLedgerAccountGroup>().SingleAsync()).TaxIncomingLedgerDimension);
        Assert.NotNull(await f.Service.LedgerAccountsAsync(default));
        await Assert.ThrowsAsync<ValidationException>(() => f.Service.AddAsync(new TaxLedgerAccountGroup { DataAreaId = "dat", TaxAccountGroup = "VAT-STD" }));
    }
    [Theory]
    [InlineData(300)]
    [InlineData(-1)]
    [InlineData(0)]
    public async Task Invalid_account_is_rejected(long id)
    {
        await using var f = await Fixture.Create();
        await Assert.ThrowsAsync<ValidationException>(() => f.Service.AddAsync(new TaxLedgerAccountGroup { DataAreaId = "dat", TaxAccountGroup = "VAT", TaxOutgoingLedgerDimension = id }));
        Assert.Empty(await f.Db.Set<TaxLedgerAccountGroup>().ToListAsync());
    }
    [Fact]
    public async Task Used_group_cannot_be_deleted_or_renamed()
    {
        await using var f = await Fixture.Create();
        var group = new TaxLedgerAccountGroup { RecId = 1, DataAreaId = "dat", TaxAccountGroup = "VAT-STD", Name = "VAT" };
        f.Db.Add(group);
        f.Db.Add(new TaxTable { RecId = 1, DataAreaId = "dat", TaxCode = "VAT15", TaxAccountGroup = "VAT-STD" });
        await f.Db.SaveChangesAsync();
        await Assert.ThrowsAsync<ValidationException>(() => f.Service.RemoveAsync(group));
        group.TaxAccountGroup = "RENAMED";
        await Assert.ThrowsAsync<ValidationException>(() => f.Service.UpdateAsync(group));
    }
    private sealed class Fixture : IAsyncDisposable
    {
        private readonly SqliteConnection connection = new("Data Source=:memory:");
        public ProfileContext Db { get; private set; } = null!;
        public TaxLedgerAccountGroupService Service => new(new Work(Db), null!, new Company());
        public static async Task<Fixture> Create()
        {
            var f = new Fixture();
            await f.connection.OpenAsync();
            f.Db = new ProfileContext(new DbContextOptionsBuilder<ProfileContext>().UseSqlite(f.connection).Options);
            await f.Db.Database.EnsureCreatedAsync();
            f.Db.AddRange(
                new DimensionAttributeValueCombination { RecId = 100, DataAreaId = "dat", MainAccountValue = "VAT", DisplayValue = "VAT" },
                new DimensionAttributeValueCombination { RecId = 300, DataAreaId = "other", MainAccountValue = "OTHER", DisplayValue = "OTHER" });
            await f.Db.SaveChangesAsync();
            return f;
        }
        public async ValueTask DisposeAsync() { await Db.DisposeAsync(); await connection.DisposeAsync(); }
    }
    private sealed class ProfileContext(DbContextOptions<ProfileContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            Type[] types = [typeof(TaxLedgerAccountGroup), typeof(TaxTable), typeof(DimensionAttributeValueCombination)];
            foreach (var type in typeof(TaxLedgerAccountGroup).Assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract
                && typeof(IAX.IXApi.Shared.Domain.Entities.Entity<long>).IsAssignableFrom(t) && !types.Contains(t)))
                builder.Ignore(type);
            foreach (var type in types)
            {
                var entity = builder.Entity(type);
                entity.HasKey("RecId"); entity.Ignore("RowVersion");
                foreach (var property in type.GetProperties())
                    if (property.PropertyType != typeof(string) && property.PropertyType != typeof(byte[])
                        && !property.PropertyType.IsValueType) entity.Ignore(property.Name);
            }
        }
    }
    private sealed class Company : ICompanyExecutionContext
    {
        public string GetDataAreaId() => "dat";
        public bool IsRequestedCompanyAuthorized() => true;
    }
    private sealed class Work(DbContext db) : IUnitOfWork
    {
        public DbContext Context => db;
        public Task<int> CompleteAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
        public IGenericRepository<T> Repository<T>() where T : class
        {
            var proxy = DispatchProxy.Create<IGenericRepository<T>, RepositoryProxy<T>>();
            ((RepositoryProxy<T>)(object)proxy).Db = db;
            return proxy;
        }
        public Task BeginTransactionAsync(CancellationToken ct = default) => throw new NotSupportedException();
        public Task CommitTransactionAsync(CancellationToken ct = default) => throw new NotSupportedException();
        public Task RollbackTransactionAsync(CancellationToken ct = default) => throw new NotSupportedException();
        public void Dispose() { }
    }
    public class RepositoryProxy<T> : DispatchProxy where T : class
    {
        public DbContext Db { get; set; } = null!;
        protected override object? Invoke(MethodInfo? method, object?[]? args)
        {
            if (method?.Name == "AddAsync") { Db.Add((T)args![0]!); return Task.FromResult((T)args[0]!); }
            if (method?.Name == "UpdateAsync") return Task.FromResult((T)args![0]!);
            throw new NotSupportedException(method?.Name);
        }
    }
}
