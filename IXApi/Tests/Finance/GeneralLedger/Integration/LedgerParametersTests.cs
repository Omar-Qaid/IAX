using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.GeneralLedger.LedgerParameters;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using ParametersEntity = IAX.IXApi.Modules.Finance.Entities.LedgerParameters;

namespace IAX.IXApi.Tests;

public class LedgerParametersTests
{
    [Fact]
    public async Task Creates_the_parameter_record_on_first_access_and_keeps_one_per_company()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        var service = new LedgerParametersService(new TestUnitOfWork(db), new TestUser());

        Assert.Empty(await db.Set<ParametersEntity>().AsNoTracking().ToListAsync());
        var (first, error) = await service.GetAsync(default);
        Assert.Null(error);
        Assert.NotNull(first);
        // Defaults are the cleared record; nothing is switched on by this project's choice.
        Assert.Equal(NoYes.No, first!.EnableLedgerSettlement);
        Assert.Equal(NoYes.No, first.TransactionReversalCorrection);
        Assert.Equal(0m, first.MaxRoundingDifferenceMst);

        await service.GetAsync(default);
        Assert.Single(await db.Set<ParametersEntity>().AsNoTracking().ToListAsync());

        var (updated, updateError) = await service.UpdateAsync(first with
        {
            EnableLedgerSettlement = NoYes.Yes,
            TransactionReversalCorrection = NoYes.Yes,
            MaxRoundingDifferenceMst = 0.05m
        }, default);
        Assert.Null(updateError);
        Assert.Equal(NoYes.Yes, updated!.EnableLedgerSettlement);
        Assert.Equal(0.05m, updated.MaxRoundingDifferenceMst);
        Assert.Single(await db.Set<ParametersEntity>().AsNoTracking().ToListAsync());

        Assert.Contains("cannot be negative",
            (await service.UpdateAsync(first with { MaxRoundingDifferenceMst = -1m }, default)).Error);
    }

    // A posting path must not write a parameter record for a company that has never opened the page.
    [Fact]
    public async Task Peek_reads_without_creating_a_record()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);

        Assert.Null(await LedgerParametersService.PeekAsync(db, "dat", default));
        Assert.Empty(await db.Set<ParametersEntity>().AsNoTracking().ToListAsync());

        db.Add(new ParametersEntity { RecId = 1, DataAreaId = "dat", EnableLedgerSettlement = NoYes.Yes });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        Assert.Equal(NoYes.Yes, (await LedgerParametersService.PeekAsync(db, "dat", default))!.EnableLedgerSettlement);
        Assert.Null(await LedgerParametersService.PeekAsync(db, "other", default));
    }

    private static async Task<ParametersContext> NewContextAsync(SqliteConnection connection)
    {
        var db = new ParametersContext(new DbContextOptionsBuilder<ParametersContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        return db;
    }

    private sealed class ParametersContext(DbContextOptions<ParametersContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            var entity = builder.Entity<ParametersEntity>();
            entity.HasKey(x => x.RecId);
            entity.Ignore(x => x.RowVersion);
        }
    }

    private sealed class TestUser : ICurrentUserService
    {
        public string GetCurrentUserId() => "tester";
        public string GetOwnerAccountId() => "owner";
        public string GetDataAreaId() => "dat";
    }

    private sealed class TestUnitOfWork(DbContext context) : IUnitOfWork
    {
        public DbContext Context => context;
        public Task<int> CompleteAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
        public IGenericRepository<T> Repository<T>() where T : class => throw new NotSupportedException();
        public Task BeginTransactionAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task CommitTransactionAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task RollbackTransactionAsync(CancellationToken ct = default) => Task.CompletedTask;
        public void Dispose() { }
    }
}
