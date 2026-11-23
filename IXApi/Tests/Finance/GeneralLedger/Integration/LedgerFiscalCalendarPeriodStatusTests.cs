using System.Reflection;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.GeneralLedger;
using IAX.IXApi.Modules.Finance.GeneralLedger.FiscalCalendar;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IAX.IXApi.Tests;

/// <summary>
/// Ledger period status is what the posting engine reads to allow or refuse posting into a month,
/// so the rules that change it are tested on their own, not only through a posting path.
/// </summary>
public class LedgerFiscalCalendarPeriodStatusTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Changing_period_status_refuses_pending_changes_without_discarding_them(bool periodChange)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        if (periodChange)
            (await db.Set<LedgerFiscalCalendarPeriod>().SingleAsync(x => x.RecId == 1)).Status = FiscalPeriodStatus.OnHold;
        else
            (await db.Set<FiscalCalendarPeriod>().SingleAsync(x => x.RecId == 1)).Name = "Unsaved period name";
        var (result, error) = await new LedgerFiscalCalendarPeriodService(new TestUnitOfWork(db), new TestUser())
            .SetStatusAsync(1, FiscalPeriodStatus.Closed);
        Assert.Null(result);
        Assert.Equal("Save or discard pending changes before changing ledger period status.", error);
        Assert.True(db.ChangeTracker.HasChanges());
        Assert.Equal(FiscalPeriodStatus.Open, (await db.Set<LedgerFiscalCalendarPeriod>().AsNoTracking().SingleAsync(x => x.RecId == 1)).Status);
        Assert.Equal("January 2026", (await db.Set<FiscalCalendarPeriod>().AsNoTracking().SingleAsync(x => x.RecId == 1)).Name);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Module_access_update_can_hold_posting_for_its_period(bool bulk)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        var row = new LedgerPeriodModuleAccessControl { RecId = 1, DataAreaId = "dat", LedgerFiscalCalendarPeriod = 1 };
        db.Add(row);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        row.LedgerAccessLevel = LedgerPeriodAccessLevel.None;
        var service = new LedgerPeriodModuleAccessControlService(new TestUnitOfWork(db), new TestUser());
        if (bulk) await service.UpdateRangeAsync([row]);
        else await service.UpdateAsync(row);
        Assert.Equal(LedgerPeriodAccessLevel.None,
            (await db.Set<LedgerPeriodModuleAccessControl>().AsNoTracking().SingleAsync()).LedgerAccessLevel);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Module_access_update_requires_a_period_in_the_same_company(bool bulk, bool otherCompany)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        var row = new LedgerPeriodModuleAccessControl { RecId = 1, DataAreaId = "dat", LedgerFiscalCalendarPeriod = 1 };
        db.Add(row);
        if (otherCompany) db.Add(new LedgerFiscalCalendarPeriod
            { RecId = 9, DataAreaId = "other", Ledger = 9, FiscalCalendarPeriod = 1, Status = FiscalPeriodStatus.Open });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        row.LedgerFiscalCalendarPeriod = 9;
        var service = new LedgerPeriodModuleAccessControlService(new TestUnitOfWork(db), new TestUser());
        var error = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            if (bulk) await service.UpdateRangeAsync([row]);
            else await service.UpdateAsync(row);
        });
        Assert.Equal("The ledger fiscal period was not found.", error.Message);
        Assert.Equal(1, (await db.Set<LedgerPeriodModuleAccessControl>().AsNoTracking().SingleAsync()).LedgerFiscalCalendarPeriod);
    }

    public static IEnumerable<object[]> InvalidModuleAccessCases()
    {
        foreach (var field in new[] { "LedgerAccessLevel", "TaxAccessLevel", "BankAccessLevel", "CustAccessLevel", "VendAccessLevel", "InventAccessLevel" })
            foreach (var operation in new[] { "add", "addRange", "update", "updateRange" })
                yield return new object[] { field, operation };
    }

    [Theory]
    [MemberData(nameof(InvalidModuleAccessCases))]
    public async Task Module_access_writes_refuse_undefined_levels(string field, string operation)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        var service = new LedgerPeriodModuleAccessControlService(new TestUnitOfWork(db), new TestUser());
        var row = new LedgerPeriodModuleAccessControl { RecId = 1, DataAreaId = "dat", LedgerFiscalCalendarPeriod = 1 };
        if (operation.StartsWith("update"))
        {
            db.Add(row);
            await db.SaveChangesAsync();
            db.ChangeTracker.Clear();
        }
        typeof(LedgerPeriodModuleAccessControl).GetProperty(field)!.SetValue(row, (LedgerPeriodAccessLevel)99);
        var error = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            switch (operation)
            {
                case "add": await service.AddAsync(row); break;
                case "addRange": await service.AddRangeAsync([row]); break;
                case "update": await service.UpdateAsync(row); break;
                case "updateRange": await service.UpdateRangeAsync([row]); break;
            }
        });
        Assert.Equal("Module access levels must be All, None, or UserGroup.", error.Message);
        var persisted = await db.Set<LedgerPeriodModuleAccessControl>().AsNoTracking().SingleOrDefaultAsync();
        if (operation.StartsWith("update"))
            Assert.Equal((LedgerPeriodAccessLevel)0, typeof(LedgerPeriodModuleAccessControl).GetProperty(field)!.GetValue(persisted));
        else Assert.Null(persisted);
    }

    [Fact]
    public async Task Module_access_creation_defaults_to_the_current_company()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        var service = new LedgerPeriodModuleAccessControlService(new TestUnitOfWork(db), new TestUser());
        var row = await service.AddAsync(new LedgerPeriodModuleAccessControl
            { DataAreaId = "", LedgerFiscalCalendarPeriod = 1 });
        Assert.Equal("dat", row.DataAreaId);
        Assert.Single(await db.Set<LedgerPeriodModuleAccessControl>().ToListAsync());
    }

    [Theory]
    [InlineData(false, "")]
    [InlineData(true, "")]
    [InlineData(false, "dat")]
    [InlineData(true, "dat")]
    public async Task Module_access_creation_refuses_duplicate_pending_or_batch_rows(bool pending, string area)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        var service = new LedgerPeriodModuleAccessControlService(new TestUnitOfWork(db), new TestUser());
        var first = new LedgerPeriodModuleAccessControl { RecId = 1, DataAreaId = "dat", LedgerFiscalCalendarPeriod = 1 };
        var second = new LedgerPeriodModuleAccessControl { RecId = 2, DataAreaId = area, LedgerFiscalCalendarPeriod = 1 };
        if (pending) db.Add(first);
        var error = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            if (pending) await service.AddAsync(second);
            else await service.AddRangeAsync([first, second]);
        });
        Assert.Contains("Module access is already configured", error.Message);
        Assert.Empty(await db.Set<LedgerPeriodModuleAccessControl>().AsNoTracking().ToListAsync());
    }

    [Fact]
    public void Production_period_configuration_has_a_unique_ledger_period_index()
    {
        var model = new ModelBuilder();
        model.ApplyConfiguration(new LedgerFiscalCalendarConfiguration.LedgerFiscalCalendarPeriodConfiguration());
        var entity = model.Model.FindEntityType(typeof(LedgerFiscalCalendarPeriod));
        Assert.NotNull(entity);
        Assert.Contains(entity!.GetIndexes(), index => index.IsUnique &&
            index.Properties.Select(property => property.Name).SequenceEqual(
                new[] { nameof(LedgerFiscalCalendarPeriod.Ledger), nameof(LedgerFiscalCalendarPeriod.FiscalCalendarPeriod) }));
    }

    [Fact]
    public async Task Database_rejects_duplicate_periods_even_when_service_checks_are_bypassed()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        db.Add(new LedgerFiscalCalendarPeriod { RecId = 2, DataAreaId = "dat", Ledger = 1,
            FiscalCalendarPeriod = 1, Status = FiscalPeriodStatus.OnHold });
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task Creation_refuses_a_duplicate_period_already_pending_in_the_context()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        db.Add(new LedgerFiscalCalendarPeriod { RecId = 2, DataAreaId = "dat", Ledger = 1,
            FiscalCalendarPeriod = 2, Status = FiscalPeriodStatus.Open });
        var service = new LedgerFiscalCalendarPeriodService(new TestUnitOfWork(db), new TestUser());
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddAsync(
            new LedgerFiscalCalendarPeriod { RecId = 3, DataAreaId = "dat", Ledger = 1,
                FiscalCalendarPeriod = 2, Status = FiscalPeriodStatus.OnHold }));
        Assert.Contains("already has a status row", error.Message);
        Assert.Single(await db.Set<LedgerFiscalCalendarPeriod>().AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task Bulk_create_rejects_duplicate_ledger_period_pairs_without_saving_any_rows()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        var service = new LedgerFiscalCalendarPeriodService(new TestUnitOfWork(db), new TestUser());
        var rows = new[]
        {
            new LedgerFiscalCalendarPeriod { RecId = 2, DataAreaId = "dat", Ledger = 1, FiscalCalendarPeriod = 2, Status = FiscalPeriodStatus.Open },
            new LedgerFiscalCalendarPeriod { RecId = 3, DataAreaId = "dat", Ledger = 1, FiscalCalendarPeriod = 2, Status = FiscalPeriodStatus.OnHold }
        };

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddRangeAsync(rows));

        Assert.Contains("already has a status row", error.Message);
        Assert.Single(await db.Set<LedgerFiscalCalendarPeriod>().AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task Bulk_create_accepts_distinct_ledger_period_pairs()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        var service = new LedgerFiscalCalendarPeriodService(new TestUnitOfWork(db), new TestUser());

        await service.AddRangeAsync([
            new LedgerFiscalCalendarPeriod { RecId = 2, DataAreaId = "dat", Ledger = 1, FiscalCalendarPeriod = 2, Status = FiscalPeriodStatus.Open },
            new LedgerFiscalCalendarPeriod { RecId = 3, DataAreaId = "dat", Ledger = 2, FiscalCalendarPeriod = 2, Status = FiscalPeriodStatus.OnHold }
        ]);

        Assert.Equal(3, await db.Set<LedgerFiscalCalendarPeriod>().CountAsync());
    }

    [Fact]
    public async Task A_stale_tracked_open_period_cannot_reopen_a_persisted_closed_period()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        await db.Set<LedgerFiscalCalendarPeriod>().SingleAsync(x => x.RecId == 1);
        await db.Set<LedgerFiscalCalendarPeriod>().Where(x => x.RecId == 1)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, FiscalPeriodStatus.Closed));

        var (result, error) = await new LedgerFiscalCalendarPeriodService(new TestUnitOfWork(db), new TestUser())
            .SetStatusAsync(1, FiscalPeriodStatus.Open);
        Assert.Null(result);
        Assert.Contains("cannot be reopened", error);
        Assert.Equal(FiscalPeriodStatus.Closed,
            (await db.Set<LedgerFiscalCalendarPeriod>().AsNoTracking().SingleAsync(x => x.RecId == 1)).Status);
    }

    [Fact]
    public async Task A_period_in_another_company_cannot_be_changed()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        db.Add(new LedgerFiscalCalendarPeriod { RecId = 9, DataAreaId = "other", Ledger = 9,
            FiscalCalendarPeriod = 1, Status = FiscalPeriodStatus.Open });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var (result, error) = await new LedgerFiscalCalendarPeriodService(new TestUnitOfWork(db), new TestUser())
            .SetStatusAsync(9, FiscalPeriodStatus.Closed);
        Assert.Null(result);
        Assert.Null(error);
        Assert.Equal(FiscalPeriodStatus.Open,
            (await db.Set<LedgerFiscalCalendarPeriod>().AsNoTracking().SingleAsync(x => x.RecId == 9)).Status);
    }

    [Fact]
    public async Task A_period_cannot_be_created_with_an_undefined_status()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new LedgerFiscalCalendarPeriodService(new TestUnitOfWork(db), new TestUser()).AddAsync(
                new LedgerFiscalCalendarPeriod { DataAreaId = "dat", Ledger = 1,
                    FiscalCalendarPeriod = 2, Status = (FiscalPeriodStatus)99 }));
        Assert.Contains("create it Open or On hold", error.Message);
        Assert.Single(await db.Set<LedgerFiscalCalendarPeriod>().ToListAsync());
    }

    [Theory]
    // On hold is the reversible month-end restriction, so it converts both ways.
    [InlineData(FiscalPeriodStatus.Open, FiscalPeriodStatus.OnHold, null)]
    [InlineData(FiscalPeriodStatus.OnHold, FiscalPeriodStatus.Open, null)]
    // Either state may be closed permanently.
    [InlineData(FiscalPeriodStatus.Open, FiscalPeriodStatus.Closed, null)]
    [InlineData(FiscalPeriodStatus.OnHold, FiscalPeriodStatus.Closed, null)]
    // Setting the status a period already holds does nothing and is not an error.
    [InlineData(FiscalPeriodStatus.Closed, FiscalPeriodStatus.Closed, null)]
    [InlineData(FiscalPeriodStatus.YearClosed, FiscalPeriodStatus.YearClosed, null)]
    // Permanently closed cannot be reopened, by any route.
    [InlineData(FiscalPeriodStatus.Closed, FiscalPeriodStatus.Open, "cannot be reopened")]
    [InlineData(FiscalPeriodStatus.Closed, FiscalPeriodStatus.OnHold, "cannot be reopened")]
    [InlineData(FiscalPeriodStatus.Closed, FiscalPeriodStatus.YearClosed, "cannot be reopened")]
    // Year closed belongs to the year end close process, which is not implemented here.
    [InlineData(FiscalPeriodStatus.YearClosed, FiscalPeriodStatus.Open, "year end close process")]
    [InlineData(FiscalPeriodStatus.Open, FiscalPeriodStatus.YearClosed, "year end close process")]
    [InlineData(FiscalPeriodStatus.OnHold, FiscalPeriodStatus.YearClosed, "year end close process")]
    public void Transition_rules_follow_the_core_cycle(FiscalPeriodStatus from, FiscalPeriodStatus to, string? expected)
    {
        var result = FiscalPeriodStatusTransition.Check(from, to);
        if (expected == null) Assert.Null(result);
        else Assert.Contains(expected, result);
    }

    [Fact]
    public void An_undefined_status_is_not_a_period_status()
    {
        Assert.Contains("not a ledger period status",
            FiscalPeriodStatusTransition.Check(FiscalPeriodStatus.Open, (FiscalPeriodStatus)99));
    }

    [Fact]
    public async Task Holding_and_reopening_a_period_is_persisted_and_reports_the_previous_status()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        var service = new LedgerFiscalCalendarPeriodService(new TestUnitOfWork(db), new TestUser());

        var (held, error) = await service.SetStatusAsync(1, FiscalPeriodStatus.OnHold, default);
        Assert.Null(error);
        Assert.Equal(FiscalPeriodStatus.Open, held!.PreviousStatus);
        Assert.Equal(FiscalPeriodStatus.OnHold, held.Status);
        Assert.Equal("January 2026", held.PeriodName);
        Assert.Equal(new DateTime(2026, 1, 1), held.StartDate);
        Assert.Equal(FiscalPeriodStatus.OnHold,
            (await db.Set<LedgerFiscalCalendarPeriod>().AsNoTracking().SingleAsync(x => x.RecId == 1)).Status);

        var (reopened, reopenError) = await service.SetStatusAsync(1, FiscalPeriodStatus.Open, default);
        Assert.Null(reopenError);
        Assert.Equal(FiscalPeriodStatus.Open, reopened!.Status);
    }

    [Fact]
    public async Task A_permanently_closed_period_is_never_reopened_and_its_stored_status_is_unchanged()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        var service = new LedgerFiscalCalendarPeriodService(new TestUnitOfWork(db), new TestUser());

        Assert.Null((await service.SetStatusAsync(1, FiscalPeriodStatus.Closed, default)).Error);
        db.ChangeTracker.Clear();

        var (result, error) = await service.SetStatusAsync(1, FiscalPeriodStatus.Open, default);
        Assert.Null(result);
        Assert.Contains("cannot be reopened", error);
        Assert.Equal(FiscalPeriodStatus.Closed,
            (await db.Set<LedgerFiscalCalendarPeriod>().AsNoTracking().SingleAsync(x => x.RecId == 1)).Status);
    }

    // A null result with a null error is "not in this company", which the controller turns into 404.
    [Fact]
    public async Task An_unknown_period_is_reported_as_missing_rather_than_refused()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        var service = new LedgerFiscalCalendarPeriodService(new TestUnitOfWork(db), new TestUser());

        var (result, error) = await service.SetStatusAsync(404, FiscalPeriodStatus.OnHold, default);
        Assert.Null(result);
        Assert.Null(error);
    }

    [Fact]
    public async Task Generic_update_and_delete_cannot_change_a_period_status()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        var service = new LedgerFiscalCalendarPeriodService(new TestUnitOfWork(db), new TestUser());
        var row = await db.Set<LedgerFiscalCalendarPeriod>().AsNoTracking().SingleAsync(x => x.RecId == 1);
        row.Status = FiscalPeriodStatus.Open;

        await Assert.ThrowsAsync<NotSupportedException>(() => service.UpdateAsync(row, default));
        await Assert.ThrowsAsync<NotSupportedException>(() => service.UpdateRangeAsync([row], default));
        await Assert.ThrowsAsync<NotSupportedException>(() => service.RemoveAsync(row, default));
        await Assert.ThrowsAsync<NotSupportedException>(() => service.RemoveRangeAsync([row], default));
    }

    [Fact]
    public async Task A_second_status_row_for_the_same_ledger_period_is_refused()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        var service = new LedgerFiscalCalendarPeriodService(new TestUnitOfWork(db), new TestUser());

        // A duplicate would make the period's status, and therefore posting, ambiguous.
        var duplicate = new LedgerFiscalCalendarPeriod
        {
            RecId = 2, DataAreaId = "dat", Ledger = 1, FiscalCalendarPeriod = 1, Status = FiscalPeriodStatus.Open
        };
        var refused = await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddAsync(duplicate, default));
        Assert.Contains("already has a status row", refused.Message);

        // A period may not be born closed; it is closed by the guarded transition.
        var bornClosed = new LedgerFiscalCalendarPeriod
        {
            RecId = 3, DataAreaId = "dat", Ledger = 1, FiscalCalendarPeriod = 2, Status = FiscalPeriodStatus.Closed
        };
        Assert.Contains("cannot be created as Closed",
            (await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddAsync(bornClosed, default))).Message);

        db.ChangeTracker.Clear();
        var added = await service.AddAsync(new LedgerFiscalCalendarPeriod
        {
            RecId = 4, DataAreaId = "dat", Ledger = 1, FiscalCalendarPeriod = 2, Status = FiscalPeriodStatus.Open
        }, default);
        Assert.Equal(FiscalPeriodStatus.Open, added.Status);
    }

    [Fact]
    public void The_controller_exposes_the_status_action_and_no_generic_update_or_delete()
    {
        var actions = typeof(LedgerFiscalCalendarPeriodController)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(x => x.DeclaringType == typeof(LedgerFiscalCalendarPeriodController)
                && !x.IsDefined(typeof(NonActionAttribute), inherit: false))
            .Select(x => x.Name)
            .ToHashSet();

        Assert.Contains(nameof(LedgerFiscalCalendarPeriodController.SetStatus), actions);
        foreach (var blocked in new[] { "Update", "UpdateRange", "Delete", "DeleteRange" })
        {
            var method = typeof(LedgerFiscalCalendarPeriodController).GetMethod(blocked,
                BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(method);
            Assert.Equal(typeof(LedgerFiscalCalendarPeriodController), method!.DeclaringType);
            Assert.True(method.IsDefined(typeof(NonActionAttribute), inherit: false), blocked);
        }

        // Reads stay available, and the feature still uses the project's generic controller base.
        Assert.Equal(typeof(BaseController<LedgerFiscalCalendarPeriod, LedgerFiscalCalendarPeriodDto>),
            typeof(LedgerFiscalCalendarPeriodController).BaseType);
    }

    private static async Task<PeriodContext> NewContextAsync(SqliteConnection connection)
    {
        var db = new PeriodContext(new DbContextOptionsBuilder<PeriodContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new FiscalCalendarPeriod
            {
                RecId = 1, FiscalCalendar = 1, Name = "January 2026", Type = FiscalPeriodType.Operating,
                StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 1, 31)
            },
            new FiscalCalendarPeriod
            {
                RecId = 2, FiscalCalendar = 1, Name = "February 2026", Type = FiscalPeriodType.Operating,
                StartDate = new DateTime(2026, 2, 1), EndDate = new DateTime(2026, 2, 28)
            },
            new LedgerFiscalCalendarPeriod
            {
                RecId = 1, DataAreaId = "dat", Ledger = 1, FiscalCalendarPeriod = 1, Status = FiscalPeriodStatus.Open
            });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return db;
    }

    private sealed class PeriodContext(DbContextOptions<PeriodContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            var access = builder.Entity<LedgerPeriodModuleAccessControl>();
            access.HasKey(x => x.RecId);
            access.Ignore(x => x.RowVersion);
            var period = builder.Entity<LedgerFiscalCalendarPeriod>();
            period.HasKey(x => x.RecId);
            period.HasIndex(x => new { x.Ledger, x.FiscalCalendarPeriod }).IsUnique();
            period.Ignore(x => x.RowVersion);
            period.Ignore(x => x.LedgerTable);
            period.Ignore(x => x.FiscalCalendarPeriodTable);

            var calendar = builder.Entity<FiscalCalendarPeriod>();
            calendar.HasKey(x => x.RecId);
            calendar.Ignore(x => x.RowVersion);
            calendar.Ignore(x => x.FiscalCalendarTable);
            calendar.Ignore(x => x.FiscalCalendarYearTable);
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
        public IGenericRepository<T> Repository<T>() where T : class
        {
            var repository = DispatchProxy.Create<IGenericRepository<T>, PeriodRepositoryProxy<T>>();
            ((PeriodRepositoryProxy<T>)(object)repository).Context = context;
            return repository;
        }
        public Task<int> CompleteAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
        public Task BeginTransactionAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task CommitTransactionAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task RollbackTransactionAsync(CancellationToken ct = default) => Task.CompletedTask;
        public void Dispose() { }
    }

    public class PeriodRepositoryProxy<T> : DispatchProxy where T : class
    {
        public DbContext Context { get; set; } = null!;
        protected override object? Invoke(MethodInfo? method, object?[]? args)
        {
            switch (method?.Name)
            {
                case "GetQueryable": return Context.Set<T>().AsQueryable();
                case "AddAsync": Context.Add((T)args![0]!); return Task.FromResult((T)args[0]!);
                case "AddRangeAsync":
                    var rows = ((IEnumerable<T>)args![0]!).ToList();
                    Context.AddRange(rows);
                    return Task.FromResult<IEnumerable<T>>(rows);
                case "UpdateAsync": Context.Update((T)args![0]!); return Task.FromResult((T)args[0]!);
                case "UpdateRangeAsync":
                    var updates = ((IEnumerable<T>)args![0]!).ToList();
                    Context.UpdateRange(updates);
                    return Task.FromResult<IEnumerable<T>>(updates);
                case "RemoveAsync": Context.Remove((T)args![0]!); return Task.CompletedTask;
                default: throw new NotSupportedException(method?.Name);
            }
        }
    }
}
