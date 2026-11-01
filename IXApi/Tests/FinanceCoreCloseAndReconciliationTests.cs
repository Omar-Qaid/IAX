using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.GeneralLedger.Bank;
using IAX.IXApi.Modules.Finance.GeneralLedger.FiscalCalendar;
using IAX.IXApi.Modules.Finance.GeneralLedger.JournalEntry;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Xunit;

namespace IAX.IXApi.Tests;

public sealed class FinanceCoreCloseAndReconciliationTests
{
    [Fact]
    public async Task Year_end_close_transfers_profit_to_equity_and_closes_periods()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContext(connection);
        var end = new DateTime(2026, 12, 31);
        db.Add(new IAX.IXApi.Modules.Finance.Entities.Ledger
            { RecId = 1, DataAreaId = "dat", FiscalCalendar = 1, ChartOfAccounts = 1, AccountingCurrency = "SAR" });
        db.Add(new FiscalCalendarYear { RecId = 1, FiscalCalendar = 1,
            StartDate = new DateTime(2026, 1, 1), EndDate = end });
        db.Add(new FiscalCalendarYear { RecId = 2, FiscalCalendar = 1,
            StartDate = new DateTime(2027, 1, 1), EndDate = new DateTime(2027, 12, 31) });
        db.Add(new FiscalCalendarPeriod { RecId = 1, FiscalCalendar = 1, FiscalCalendarYear = 1,
            StartDate = new DateTime(2026, 1, 1), EndDate = end, Type = FiscalPeriodType.Operating });
        db.Add(new FiscalCalendarPeriod { RecId = 2, FiscalCalendar = 1, FiscalCalendarYear = 2,
            StartDate = new DateTime(2027, 1, 1), EndDate = new DateTime(2027, 12, 31), Type = FiscalPeriodType.Opening });
        db.Add(new LedgerFiscalCalendarYear { RecId = 1, Ledger = 1, FiscalCalendarYear = 1,
            Status = FiscalPeriodStatus.OnHold });
        db.Add(new LedgerFiscalCalendarPeriod { RecId = 1, Ledger = 1, FiscalCalendarPeriod = 1,
            Status = FiscalPeriodStatus.OnHold });
        db.AddRange(
            new MainAccount { RecId = 1, DataAreaId = "dat", LedgerChartOfAccounts = 1, Type = MainAccountType.Asset, MainAccountId = "100" },
            new MainAccount { RecId = 2, DataAreaId = "dat", LedgerChartOfAccounts = 1, Type = MainAccountType.Revenue, MainAccountId = "400" },
            new MainAccount { RecId = 3, DataAreaId = "dat", LedgerChartOfAccounts = 1, Type = MainAccountType.Equity, MainAccountId = "300" });
        db.AddRange(
            new DimensionAttributeValueCombination { RecId = 1, DataAreaId = "dat", MainAccount = 1, DisplayValue = "100" },
            new DimensionAttributeValueCombination { RecId = 2, DataAreaId = "dat", MainAccount = 2, DisplayValue = "400" },
            new DimensionAttributeValueCombination { RecId = 3, DataAreaId = "dat", MainAccount = 3, DisplayValue = "300" });
        db.Add(new GeneralJournalEntry { RecId = 1, DataAreaId = "dat", Ledger = 1,
            PostingLayer = CurrentToOperationsPostingLayer.Current, AccountingDate = end,
            JournalNumber = "J1", SubledgerVoucher = "V1" });
        db.AddRange(
            new GeneralJournalAccountEntry { RecId = 1, DataAreaId = "dat", GeneralJournalEntry = 1,
                MainAccount = 1, LedgerDimension = 1, AccountingCurrencyAmount = 100m },
            new GeneralJournalAccountEntry { RecId = 2, DataAreaId = "dat", GeneralJournalEntry = 1,
                MainAccount = 2, LedgerDimension = 2, AccountingCurrencyAmount = -100m });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var controller = new LedgerYearEndCloseController(new Work(db), new User());
        Assert.IsType<OkObjectResult>(await controller.Close(new LedgerYearEndCloseRequest(1, 3), default));
        var opening = await db.Set<GeneralJournalEntry>().SingleAsync(x => x.JournalCategory == LedgerJournalCategory.YearEndOpening);
        Assert.Equal(new DateTime(2027, 1, 1), opening.AccountingDate);
        var lines = await db.Set<GeneralJournalAccountEntry>().Where(x => x.GeneralJournalEntry == opening.RecId).ToListAsync();
        Assert.Equal(0m, lines.Sum(x => x.AccountingCurrencyAmount));
        Assert.Contains(lines, x => x.MainAccount == 1 && x.AccountingCurrencyAmount == 100m);
        Assert.Contains(lines, x => x.MainAccount == 3 && x.AccountingCurrencyAmount == -100m);
        Assert.Empty(await db.Set<GeneralJournalEntry>().Where(x => x.JournalCategory == LedgerJournalCategory.YearEndClosing).ToListAsync());
        Assert.Equal(FiscalPeriodStatus.OnHold, (await db.Set<LedgerFiscalCalendarPeriod>().SingleAsync()).Status);
        var report = Assert.IsType<OkObjectResult>(await new LedgerFinancialStatementsController(new Work(db), new User())
            .Get(new DateTime(2026, 1, 1), end, CurrentToOperationsPostingLayer.Current, default));
        Assert.Equal(100m, report.Value!.GetType().GetProperty("netProfit")!.GetValue(report.Value));
        Assert.Equal(0m, report.Value.GetType().GetProperty("balanceSheetDifference")!.GetValue(report.Value));
        Assert.IsType<OkObjectResult>(await controller.Close(new LedgerYearEndCloseRequest(1, 3), default));
        Assert.Single(await db.Set<GeneralJournalEntry>().Where(x => x.JournalCategory == LedgerJournalCategory.YearEndOpening).ToListAsync());
        Assert.IsType<OkObjectResult>(await controller.Close(new LedgerYearEndCloseRequest(1, 3, true), default));
        Assert.Single(await db.Set<GeneralJournalEntry>().Where(x => x.JournalCategory == LedgerJournalCategory.YearEndClosing).ToListAsync());
        var nextReport = Assert.IsType<OkObjectResult>(await new LedgerFinancialStatementsController(new Work(db), new User())
            .Get(new DateTime(2027, 1, 1), new DateTime(2027, 12, 31), CurrentToOperationsPostingLayer.Current, default));
        Assert.Equal(0m, nextReport.Value!.GetType().GetProperty("balanceSheetDifference")!.GetValue(nextReport.Value));
        var (trialBalance, trialBalanceError) = await new LedgerTrialBalanceService(new Work(db), new User())
            .GetAsync(new DateTime(2027, 1, 1), new DateTime(2027, 12, 31), CurrentToOperationsPostingLayer.Current, default);
        Assert.Null(trialBalanceError);
        Assert.NotNull(trialBalance);
        Assert.True(trialBalance!.Balanced);
        Assert.Equal(0m, trialBalance.TotalDebit);
        Assert.Equal(0m, trialBalance.TotalCredit);
        Assert.Equal(100m, trialBalance.Lines.Single(x => x.MainAccountId == "100").OpeningDebit);
        Assert.Equal(100m, trialBalance.Lines.Single(x => x.MainAccountId == "300").OpeningCredit);
        db.Add(new GeneralJournalEntry { RecId = 10, DataAreaId = "dat", Ledger = 1,
            PostingLayer = CurrentToOperationsPostingLayer.Current, AccountingDate = end,
            JournalNumber = "ADJ1", SubledgerVoucher = "ADJ1" });
        db.AddRange(
            new GeneralJournalAccountEntry { RecId = 10, DataAreaId = "dat", GeneralJournalEntry = 10,
                MainAccount = 1, LedgerDimension = 1, AccountingCurrencyAmount = 25m },
            new GeneralJournalAccountEntry { RecId = 11, DataAreaId = "dat", GeneralJournalEntry = 10,
                MainAccount = 2, LedgerDimension = 2, AccountingCurrencyAmount = -25m });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        Assert.IsType<OkObjectResult>(await controller.Close(new LedgerYearEndCloseRequest(1, 3, true, true), default));
        var openings = await db.Set<GeneralJournalEntry>()
            .Where(x => x.JournalCategory == LedgerJournalCategory.YearEndOpening).ToListAsync();
        Assert.Equal(2, openings.Count);
        var adjustmentOpeningId = openings.Max(x => x.RecId);
        var adjusted = await db.Set<GeneralJournalAccountEntry>()
            .Where(x => x.GeneralJournalEntry == adjustmentOpeningId).ToListAsync();
        Assert.Contains(adjusted, x => x.MainAccount == 1 && x.AccountingCurrencyAmount == 25m);
        Assert.Contains(adjusted, x => x.MainAccount == 3 && x.AccountingCurrencyAmount == -25m);
        Assert.Equal(2, await db.Set<GeneralJournalEntry>()
            .CountAsync(x => x.JournalCategory == LedgerJournalCategory.YearEndClosing));
        Assert.Equal(FiscalPeriodStatus.YearClosed, (await db.Set<LedgerFiscalCalendarPeriod>().SingleAsync()).Status);
    }

    [Fact]
    public async Task Reconciliation_requires_statement_balance_and_marks_selected_movements()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContext(connection);
        db.Add(new BankAccountTable { RecId = 1, DataAreaId = "dat", AccountId = "BANK", CurrencyCode = "SAR" });
        db.Add(new BankAccountTrans { RecId = 1, DataAreaId = "dat", AccountId = "BANK",
            TransDate = new DateTime(2026, 1, 1), BankTransCurrencyCode = "SAR", BankTransAmountCur = 50m });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var service = new BankAccountStatementService(new Work(db), new User());
        var date = new DateTime(2026, 1, 31);
        var rejected = await service.ReconcileAsync("BANK", "S1", date, 40m, [1], default);
        Assert.Null(rejected.Statement);
        Assert.NotNull(rejected.Error);
        var accepted = await service.ReconcileAsync("BANK", "S1", date, 50m, [1], default);
        Assert.NotNull(accepted.Statement);
        Assert.Equal(NoYes.Yes, (await db.Set<BankAccountTrans>().SingleAsync()).Reconciled);
        Assert.Single((await service.GetReconciliationAsync(accepted.Statement!.RecId, default))!.Movements);
    }

    private static async Task<TestContext> NewContext(SqliteConnection connection)
    {
        var db = new TestContext(new DbContextOptionsBuilder<TestContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        return db;
    }

    private sealed class TestContext(DbContextOptions<TestContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            Type[] types = [typeof(IAX.IXApi.Modules.Finance.Entities.Ledger), typeof(FiscalCalendarYear),
                typeof(FiscalCalendarPeriod), typeof(LedgerFiscalCalendarYear), typeof(LedgerFiscalCalendarPeriod),
                typeof(MainAccount), typeof(DimensionAttributeValueCombination),
                typeof(LedgerStructure), typeof(DimensionHierarchy), typeof(GeneralJournalEntry),
                typeof(GeneralJournalAccountEntry), typeof(SubledgerVoucherGeneralJournalEntry),
                typeof(BankAccountTable), typeof(BankAccountTrans),
                typeof(BankAccountStatement)];
            foreach (var type in types)
            {
                var entity = builder.Entity(type);
                entity.HasKey("RecId");
                entity.Ignore("RowVersion");
                foreach (var property in type.GetProperties())
                    if (property.PropertyType != typeof(string) && property.PropertyType != typeof(byte[]) &&
                        !property.PropertyType.IsValueType)
                        entity.Ignore(property.Name);
            }
        }
    }

    private sealed class User : ICurrentUserService
    {
        public string GetCurrentUserId() => "tester";
        public string GetOwnerAccountId() => "owner";
        public string GetDataAreaId() => "dat";
    }

    private sealed class Work(DbContext context) : IUnitOfWork
    {
        public DbContext Context => context;
        public Task<int> CompleteAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
        public IGenericRepository<T> Repository<T>() where T : class =>
            DispatchProxy.Create<IGenericRepository<T>, UnusedRepositoryProxy>();
        public Task BeginTransactionAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task CommitTransactionAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task RollbackTransactionAsync(CancellationToken ct = default) => Task.CompletedTask;
        public void Dispose() { }
    }

    public class UnusedRepositoryProxy : DispatchProxy
    {
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) =>
            throw new NotSupportedException();
    }
}
