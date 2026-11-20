using IAX.IXApi.Infrastructure.Persistence.Seeding.Modules.Finance;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IXApi.Tests;

/// <summary>
/// The ledger is what every subledger posting resolves first, so these cover the three things
/// <c>SubledgerPostingSupport.LoadLedgerContextAsync</c> refuses to post without: exactly one
/// ledger for the company, a non-empty accounting currency, and an open status on its periods.
/// </summary>
public sealed class LedgerSeederTests
{
    [Fact]
    public async Task Seeds_one_ledger_with_an_accounting_currency_and_open_periods()
    {
        await using var db = await FixtureAsync();
        await new LedgerSeeder().SeedDataAsync(db);

        var ledger = await db.Set<Ledger>().SingleAsync();
        Assert.Equal("dat", ledger.DataAreaId);
        Assert.Equal("SAR", ledger.AccountingCurrency);
        Assert.NotEqual(0, ledger.FiscalCalendar);
        Assert.NotEqual(0, ledger.PrimaryForLegalEntity);

        // Every period of the calendar must carry an open ledger status, or a posting dated
        // inside it fails even though the ledger exists.
        var periods = await db.Set<FiscalCalendarPeriod>().CountAsync();
        var statuses = await db.Set<LedgerFiscalCalendarPeriod>().ToListAsync();
        Assert.Equal(periods, statuses.Count);
        Assert.All(statuses, x => Assert.Equal(FiscalPeriodStatus.Open, x.Status));
        Assert.All(statuses, x => Assert.Equal(ledger.RecId, x.Ledger));
        Assert.Equal(FiscalPeriodStatus.Open, (await db.Set<LedgerFiscalCalendarYear>().SingleAsync()).Status);
    }

    [Fact]
    public async Task Rerun_adds_nothing_and_keeps_a_closed_period_closed()
    {
        await using var db = await FixtureAsync();
        var seeder = new LedgerSeeder();
        await seeder.SeedDataAsync(db);

        // An operator closing a period is a business decision a rerun must not undo.
        var closed = await db.Set<LedgerFiscalCalendarPeriod>().FirstAsync();
        closed.Status = FiscalPeriodStatus.Closed;
        await db.SaveChangesAsync();
        var periodCount = await db.Set<LedgerFiscalCalendarPeriod>().CountAsync();
        db.ChangeTracker.Clear();

        await seeder.SeedDataAsync(db);
        await seeder.SeedDataAsync(db);

        Assert.Equal(1, await db.Set<Ledger>().CountAsync());
        Assert.Equal(1, await db.Set<LedgerFiscalCalendarYear>().CountAsync());
        Assert.Equal(periodCount, await db.Set<LedgerFiscalCalendarPeriod>().CountAsync());
        Assert.Equal(FiscalPeriodStatus.Closed,
            (await db.Set<LedgerFiscalCalendarPeriod>().SingleAsync(x => x.RecId == closed.RecId)).Status);
    }

    [Fact]
    public async Task Repairs_an_existing_ledger_that_has_no_accounting_currency()
    {
        await using var db = await FixtureAsync();
        db.Add(new Ledger { Name = "Existing", Description = "Operator ledger", DataAreaId = "dat" });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        await new LedgerSeeder().SeedDataAsync(db);

        var ledger = await db.Set<Ledger>().SingleAsync();
        Assert.Equal("Existing", ledger.Name);          // the operator's naming stands
        Assert.Equal("SAR", ledger.AccountingCurrency); // only what posting requires is converged
        Assert.NotEqual(0, ledger.FiscalCalendar);
    }

    [Fact]
    public async Task Does_nothing_when_the_company_or_calendar_has_not_been_seeded()
    {
        await using var db = await FixtureAsync(withCalendar: false);
        await new LedgerSeeder().SeedDataAsync(db);
        Assert.Empty(await db.Set<Ledger>().ToListAsync());
    }

    private static async Task<Fixture> FixtureAsync(bool withCalendar = true)
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var db = new Fixture(new DbContextOptionsBuilder<Fixture>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.Add(new CompanyInfo { DataArea = "dat", Name = "Test company", DataAreaId = "dat" });
        db.Add(new ExchangeRateType { Name = "Default", Description = "Default", DataAreaId = "dat" });
        await db.SaveChangesAsync();
        if (withCalendar)
        {
            await new FiscalCalendarSeeder().SeedDataAsync(db);
            db.ChangeTracker.Clear();
        }
        return db;
    }

    private sealed class Fixture(DbContextOptions<Fixture> options) : DbContext(options)
    {
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries().Where(x => x.State == EntityState.Added))
                entry.Property("RowVersion").CurrentValue = Array.Empty<byte>();
            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder b)
        {
            var types = new[]
            {
                typeof(FiscalCalendar), typeof(FiscalCalendarYear), typeof(FiscalCalendarPeriod),
                typeof(Ledger), typeof(LedgerFiscalCalendarYear), typeof(LedgerFiscalCalendarPeriod),
                typeof(CompanyInfo), typeof(ExchangeRateType),
            };
            foreach (var type in types) b.Entity(type);
            foreach (var type in b.Model.GetEntityTypes().Where(x => !types.Contains(x.ClrType))
                         .Select(x => x.ClrType).ToList())
                b.Ignore(type);
            foreach (var type in types)
            {
                b.Entity(type).HasKey("RecId");
                b.Entity(type).Property<byte[]>("RowVersion").IsConcurrencyToken(false).ValueGeneratedNever();
            }
        }
    }
}
