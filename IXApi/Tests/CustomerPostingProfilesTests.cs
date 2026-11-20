using System.Reflection;
using FluentValidation;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PostingProfile;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IAX.IXApi.Tests;

public class CustomerPostingProfilesTests
{
    [Fact]
    public async Task Rule_creation_update_and_reopen_preserve_ledger_selection_and_reject_duplicate_scope()
    {
        await using var fixture = await Fixture.Create();
        var service = fixture.Service;
        var rule = new CustLedgerAccounts { DataAreaId = "dat", PostingProfile = "GEN",
            AccountCode = AccountCode.Group, Num = "LOCAL", SummaryLedgerDimension = 100 };
        await service.AddAsync(rule);
        fixture.Db.ChangeTracker.Clear();
        var saved = await fixture.Db.Set<CustLedgerAccounts>().SingleAsync();
        Assert.Equal(100, saved.SummaryLedgerDimension);
        saved.SummaryLedgerDimension = 200;
        await service.UpdateAsync(saved);
        fixture.Db.ChangeTracker.Clear();
        Assert.Equal(200, (await fixture.Db.Set<CustLedgerAccounts>().SingleAsync()).SummaryLedgerDimension);
        await Assert.ThrowsAsync<ValidationException>(() => service.AddAsync(new CustLedgerAccounts {
            DataAreaId = "dat", PostingProfile = "GEN", AccountCode = AccountCode.Group,
            Num = "LOCAL", SummaryLedgerDimension = 100 }));
        Assert.Single(await fixture.Db.Set<CustLedgerAccounts>().ToListAsync());
    }

    [Theory]
    [InlineData("profile")]
    [InlineData("group")]
    [InlineData("ledger")]
    [InlineData("company")]
    [InlineData("scope")]
    public async Task Invalid_references_are_rejected_without_saving(string invalid)
    {
        await using var fixture = await Fixture.Create();
        var rule = new CustLedgerAccounts { DataAreaId = "dat", PostingProfile = "GEN",
            AccountCode = AccountCode.Group, Num = "LOCAL", SummaryLedgerDimension = 100 };
        if (invalid == "profile") rule.PostingProfile = "missing";
        if (invalid == "group") rule.Num = "missing";
        if (invalid == "ledger") rule.SummaryLedgerDimension = 300; // belongs to another company
        if (invalid == "company") rule.DataAreaId = "other";
        if (invalid == "scope") rule.AccountCode = (AccountCode)99;
        await Assert.ThrowsAsync<ValidationException>(() => fixture.Service.AddAsync(rule));
        Assert.Empty(await fixture.Db.Set<CustLedgerAccounts>().ToListAsync());
    }

    [Fact]
    public async Task All_scope_clears_number_and_lookup_is_company_scoped()
    {
        await using var fixture = await Fixture.Create();
        var rule = await fixture.Service.AddAsync(new CustLedgerAccounts { DataAreaId = "dat",
            PostingProfile = "GEN", AccountCode = AccountCode.All, Num = "LOCAL" });
        Assert.Equal("", rule.Num);
        var json = System.Text.Json.JsonSerializer.Serialize(await fixture.Service.LedgerAccountsAsync(default));
        Assert.Contains("11020302", json);
        Assert.DoesNotContain("OTHER", json);
    }

    [Fact]
    public void Dto_validation_rejects_missing_scope_and_negative_ledger_identifiers()
    {
        var validator = new CustLedgerAccountsDtoValidator();
        Assert.False(validator.Validate(new CustLedgerAccountsDto {
            PostingProfile = "GEN", AccountCode = AccountCode.Group, SummaryLedgerDimension = -1 }).IsValid);
        Assert.True(validator.Validate(new CustLedgerAccountsDto {
            PostingProfile = "GEN", AccountCode = AccountCode.All }).IsValid);
        Assert.False(new CustLedgerDtoValidator().Validate(new CustLedgerDto { PostingProfile = "GEN" }).IsValid);
    }

    private sealed class Fixture : IAsyncDisposable
    {
        private readonly SqliteConnection connection = new("Data Source=:memory:");
        public ProfileContext Db { get; private set; } = null!;
        public CustLedgerAccountsService Service => new(new Work(Db), null!, new Company());
        public static async Task<Fixture> Create()
        {
            var f = new Fixture();
            await f.connection.OpenAsync();
            f.Db = new ProfileContext(new DbContextOptionsBuilder<ProfileContext>().UseSqlite(f.connection).Options);
            await f.Db.Database.EnsureCreatedAsync();
            f.Db.AddRange(new CustLedger { RecId = 1, DataAreaId = "dat", PostingProfile = "GEN", Name = "General" },
                new CustGroup { RecId = 1, DataAreaId = "dat", CustGroupId = "LOCAL" },
                new DimensionAttributeValueCombination { RecId = 100, DataAreaId = "dat", MainAccountValue = "11020302", DisplayValue = "11020302" },
                new DimensionAttributeValueCombination { RecId = 200, DataAreaId = "dat", MainAccountValue = "11020303", DisplayValue = "11020303" },
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
            Type[] types = [typeof(CustLedger), typeof(CustLedgerAccounts), typeof(CustGroup), typeof(CustTable), typeof(DimensionAttributeValueCombination)];
            foreach (var type in typeof(CustLedger).Assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract
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
