using IAX.IXApi.Infrastructure.Persistence.Seeding.Modules.Finance;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IXApi.Tests;

public sealed class FiscalCalendarSeederTests
{
    [Fact]
    public async Task Rerun_preserves_custom_period_dates_and_repairs_only_missing_periods()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new Fixture(new DbContextOptionsBuilder<Fixture>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var foreignCalendar = new FiscalCalendar { CalendarId = "HBMC", Description = "Another company's calendar", DataAreaId = "oth" };
        db.Add(foreignCalendar);
        await db.SaveChangesAsync();
        var seed = new FiscalCalendarSeeder();
        await seed.SeedDataAsync(db);
        var period = await db.Set<FiscalCalendarPeriod>().SingleAsync(x => x.Name == "Period 1");
        period.EndDate = period.EndDate.AddDays(-1);
        var customizedEnd = period.EndDate;
        db.Remove(await db.Set<FiscalCalendarPeriod>().SingleAsync(x => x.Name == "Period 2"));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        await seed.SeedDataAsync(db);
        await seed.SeedDataAsync(db);
        Assert.Equal(14, await db.Set<FiscalCalendarPeriod>().CountAsync());
        Assert.Equal(2, await db.Set<FiscalCalendar>().CountAsync());
        Assert.DoesNotContain(await db.Set<FiscalCalendarYear>().ToListAsync(), x => x.FiscalCalendar == foreignCalendar.RecId);
        Assert.Equal(customizedEnd, (await db.Set<FiscalCalendarPeriod>().SingleAsync(x => x.Name == "Period 1")).EndDate);
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
            var types = new[] { typeof(FiscalCalendar), typeof(FiscalCalendarYear), typeof(FiscalCalendarPeriod),
                typeof(Ledger), typeof(LedgerFiscalCalendarYear), typeof(LedgerFiscalCalendarPeriod) };
            foreach (var type in types) b.Entity(type);
            foreach (var type in b.Model.GetEntityTypes().Where(x => !types.Contains(x.ClrType)).Select(x => x.ClrType).ToList()) b.Ignore(type);
            foreach (var type in types)
            {
                b.Entity(type).HasKey("RecId");
                b.Entity(type).Property<byte[]>("RowVersion").IsConcurrencyToken(false).ValueGeneratedNever();
            }
        }
    }
}
