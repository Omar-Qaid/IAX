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
            var period = builder.Entity<LedgerFiscalCalendarPeriod>();
            period.HasKey(x => x.RecId);
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
                case "UpdateAsync": return Task.FromResult((T)args![0]!);
                case "RemoveAsync": Context.Remove((T)args![0]!); return Task.CompletedTask;
                default: throw new NotSupportedException(method?.Name);
            }
        }
    }
}
