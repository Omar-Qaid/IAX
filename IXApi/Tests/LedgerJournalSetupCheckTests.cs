using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Foundation.Currency;
using IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IAX.IXApi.Tests;

public class LedgerJournalSetupCheckTests
{
    [Fact]
    public async Task Checks_company_period_and_chart_without_writing_or_authorizing_posting()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new CheckContext(new DbContextOptionsBuilder<CheckContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new LedgerJournalTable { RecId = 1, DataAreaId = "dat", JournalNum = "J1", JournalName = "GEN" },
            new LedgerJournalName { RecId = 1, DataAreaId = "dat", JournalName = "GEN" },
            new Ledger { RecId = 1, DataAreaId = "dat", ChartOfAccounts = 10, FiscalCalendar = 1, AccountingCurrency = "SAR" },
            new FiscalCalendarPeriod { RecId = 1, FiscalCalendar = 1, Type = FiscalPeriodType.Operating,
                StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 1, 31) },
            new LedgerFiscalCalendarPeriod { RecId = 1, Ledger = 1, FiscalCalendarPeriod = 1, Status = FiscalPeriodStatus.Open },
            new MainAccount { RecId = 1, LedgerChartOfAccounts = 10 },
            new DimensionAttributeValueCombination { RecId = 1, DataAreaId = "dat", MainAccount = 1 },
            new LedgerJournalTrans { RecId = 1, DataAreaId = "dat", Company = "dat", JournalNum = "J1", Voucher = "V1",
                TransDate = new DateTime(2026, 1, 31), CurrencyCode = "SAR", LedgerDimension = 1, AmountCurDebit = 100 });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var check = new LedgerJournalSetupCheck(db);
        Assert.Null(await check.CheckAsync("other", 1, default));
        Assert.Null(await check.CheckAsync("", 1, default));
        var valid = Assert.IsType<LedgerJournalSetupCheckDto>(await check.CheckAsync("dat", 1, default));
        Assert.Empty(valid.Errors);
        Assert.False(valid.PostingValidated);
        Assert.NotEmpty(valid.UnverifiedChecks);
        Assert.Empty(db.ChangeTracker.Entries());

        var account = await db.Set<MainAccount>().SingleAsync();
        account.LedgerChartOfAccounts = 20;
        var period = await db.Set<LedgerFiscalCalendarPeriod>().SingleAsync();
        // Missing ledger access must fail even if the shared calendar period exists.
        db.Remove(period);
        await db.SaveChangesAsync();
        var invalid = (await check.CheckAsync("dat", 1, default))!;
        Assert.Contains(invalid.Errors, x => x.Contains("chart of accounts"));
        Assert.Contains(invalid.Errors, x => x.Contains("not open"));

        var line = await db.Set<LedgerJournalTrans>().SingleAsync();
        line.AccountType = LedgerJournalACType.Vend;
        line.LedgerDimension = 999;
        line.CurrencyCode = "USD";
        line.TransDate = new DateTime(2026, 2, 1);
        await db.SaveChangesAsync();
        var unsupported = (await check.CheckAsync("dat", 1, default))!;
        Assert.Contains(unsupported.Errors, x => x.Contains("operating period"));
        Assert.Contains(unsupported.UnverifiedChecks, x => x.Contains("dynamic account"));
        Assert.Contains(unsupported.UnverifiedChecks, x => x.Contains("exchange rates"));
        Assert.DoesNotContain(unsupported.Errors, x => x.Contains("main account"));
    }

    [Fact]
    public async Task Rate_checks_use_transaction_currency_and_separate_reporting_type_with_standard_fallback()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new CheckContext(new DbContextOptionsBuilder<CheckContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new ExchangeRateType { RecId = 1, DataAreaId = "dat", IsActive = true },
            new ExchangeRateType { RecId = 2, DataAreaId = "dat", IsActive = true },
            new ExchangeRateCurrencyPair { RecId = 1, DataAreaId = "dat", ExchangeRateType = 1, FromCurrencyCode = "USD", ToCurrencyCode = "SAR", IsActive = true },
            new ExchangeRateCurrencyPair { RecId = 2, DataAreaId = "dat", ExchangeRateType = 2, FromCurrencyCode = "USD", ToCurrencyCode = "EUR", IsActive = true },
            new ExchangeRate { RecId = 1, DataAreaId = "dat", ExchangeRateCurrencyPair = 1, ExchangeRateValue = 3.75m, ValidFrom = new DateTime(2026, 1, 1), IsActive = true },
            new ExchangeRate { RecId = 2, DataAreaId = "dat", ExchangeRateCurrencyPair = 2, ExchangeRateValue = 0.9m, ValidFrom = new DateTime(2026, 1, 1), IsActive = true });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var ledger = new Ledger { AccountingCurrency = "SAR", ReportingCurrency = "EUR", DefaultExchangeRateType = 1, ReportingCurrencyExchangeRateType = 2 };
        var line = new LedgerJournalTrans { Company = "dat", CurrencyCode = "USD", TransDate = new DateTime(2026, 1, 31) };
        var check = new LedgerExchangeRateCheck(db);
        Assert.Empty((await check.CheckAsync("dat", ledger, [line], default)).Errors);
        Assert.Empty(db.ChangeTracker.Entries());
        ledger.ReportingCurrencyExchangeRateType = 0;
        var missing = await check.CheckAsync("dat", ledger, [line], default);
        Assert.Single(missing.Errors);
        Assert.Contains("Reporting USD/EUR", missing.Errors[0]);
        // Default reporting type falls back to the accounting type, not an accounting-to-reporting pair.
        var reportingPair = await db.Set<ExchangeRateCurrencyPair>().SingleAsync(x => x.RecId == 2);
        reportingPair.ExchangeRateType = 1;
        await db.SaveChangesAsync();
        Assert.Empty((await check.CheckAsync("dat", ledger, [line], default)).Errors);
        var rate = await db.Set<ExchangeRate>().SingleAsync(x => x.RecId == 2);
        rate.ValidTo = new DateTime(2026, 1, 30);
        await db.SaveChangesAsync();
        Assert.Contains((await check.CheckAsync("dat", ledger, [line], default)).Errors, x => x.Contains("No exchange rate is effective"));
        line.ReportingCurrencyExchRate = 90;
        var overridden = await check.CheckAsync("dat", ledger, [line], default);
        Assert.Empty(overridden.Errors);
        Assert.Contains(overridden.Unverified, x => x.Contains("overrides"));
        Assert.Contains((await check.CheckAsync("other", ledger, [new LedgerJournalTrans { CurrencyCode = "USD", TransDate = line.TransDate }], default)).Errors,
            x => x.Contains("selected company"));
        Assert.Equal(0.9m, rate.ExchangeRateValue);
    }

    private sealed class CheckContext(DbContextOptions<CheckContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            Type[] types = [typeof(LedgerJournalTable), typeof(LedgerJournalName), typeof(LedgerJournalTrans),
                typeof(Ledger), typeof(FiscalCalendarPeriod), typeof(LedgerFiscalCalendarPeriod),
                typeof(MainAccount), typeof(DimensionAttributeValueCombination), typeof(ExchangeRateType),
                typeof(ExchangeRateCurrencyPair), typeof(ExchangeRate),
                // The check reports general ledger posting controls it does not apply.
                typeof(IAX.IXApi.Modules.Finance.Entities.LedgerParameters)];
            foreach (var type in types)
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
