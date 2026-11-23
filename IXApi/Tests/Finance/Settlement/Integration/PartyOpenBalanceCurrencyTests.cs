using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IAX.IXApi.Tests;

public sealed class PartyOpenBalanceCurrencyTests
{
    [Fact]
    public async Task Currency_rows_do_not_mix_units_and_keep_stored_reporting_and_accounting_amounts()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await CreateAsync(connection);
        db.AddRange(
            new CustTrans { RecId = 1, DataAreaId = "dat", AccountNum = "C-001", CurrencyCode = "USD", AmountCur = 200, SettleAmountCur = 100, AmountMst = 750, SettleAmountMst = 375, ReportingCurrencyAmount = 180, SettleAmountReporting = 90 },
            new CustTransOpen { RecId = 1, DataAreaId = "dat", AccountNum = "C-001", RefRecId = 1, AmountCur = 100, AmountMst = 375 },
            new CustTrans { RecId = 2, DataAreaId = "dat", AccountNum = "C-001", CurrencyCode = "EUR", AmountCur = 100, AmountMst = 400, ReportingCurrencyAmount = 100 },
            new CustTransOpen { RecId = 2, DataAreaId = "dat", AccountNum = "C-001", RefRecId = 2, AmountCur = 100, AmountMst = 400 },
            new CustTrans { RecId = 3, DataAreaId = "other", AccountNum = "C-001", CurrencyCode = "USD", AmountCur = 9999, AmountMst = 9999 },
            new CustTransOpen { RecId = 3, DataAreaId = "other", AccountNum = "C-001", RefRecId = 3, AmountCur = 9999, AmountMst = 9999 });
        await db.SaveChangesAsync();
        var result = await PartyOpenBalanceCurrencyReader.ReadAsync(db, "dat", " C-001 ", false, default);
        Assert.NotNull(result);
        Assert.Equal("Customer", result.Name);
        Assert.Equal("العميل", result.NameAlias);
        Assert.Equal("SAR", result.AccountingCurrency);
        Assert.Equal(775m, result.BalanceMst);
        Assert.Equal(190m, result.BalanceReporting);
        Assert.Equal(1000m, result.CreditMax);
        Assert.True(result.Reconciled);
        Assert.Equal(2, result.Currencies.Count);
        Assert.All(result.Currencies, row => Assert.Equal(100m, row.AmountCur));
        Assert.Equal(new[] { "V-001" }, result.RelatedAccounts);
        Assert.Null(await PartyOpenBalanceCurrencyReader.ReadAsync(db, "other", "C-001", false, default));
    }

    [Fact]
    public async Task Vendor_balances_preserve_credit_sign_and_do_not_invent_missing_reporting_amounts()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await CreateAsync(connection);
        db.AddRange(
            new VendTrans { RecId = 1, DataAreaId = "dat", AccountNum = "V-001", CurrencyCode = "SAR", AmountCur = -250, AmountMst = -250 },
            new VendTransOpen { RecId = 1, DataAreaId = "dat", AccountNum = "V-001", RefRecId = 1, AmountCur = -250, AmountMst = -250 });
        await db.SaveChangesAsync();
        var result = await PartyOpenBalanceCurrencyReader.ReadAsync(db, "dat", "V-001", true, default);
        Assert.NotNull(result);
        Assert.Equal(-250m, result.BalanceMst);
        Assert.Null(result.BalanceReporting);
        Assert.Null(Assert.Single(result.Currencies).AmountReporting);
        Assert.Equal(new[] { "C-001" }, result.RelatedAccounts);
        Assert.True(result.Reconciled);
    }

    [Fact]
    public async Task Shared_party_does_not_imply_a_customer_vendor_link_and_inconsistent_open_rows_are_flagged()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await CreateAsync(connection);
        (await db.Set<CustTable>().SingleAsync()).VendAccount = "";
        db.AddRange(new CustTrans { RecId = 1, DataAreaId = "dat", AccountNum = "C-001", CurrencyCode = "SAR", AmountCur = 100, AmountMst = 100 },
            new CustTransOpen { RecId = 1, DataAreaId = "dat", AccountNum = "C-001", RefRecId = 1, AmountCur = 75, AmountMst = 75 });
        await db.SaveChangesAsync();
        var result = await PartyOpenBalanceCurrencyReader.ReadAsync(db, "dat", "C-001", false, default);
        Assert.NotNull(result);
        Assert.Empty(result.RelatedAccounts);
        Assert.Equal(75m, result.BalanceMst);
        Assert.False(result.Reconciled);
    }

    private static async Task<BalanceContext> CreateAsync(SqliteConnection connection)
    {
        var db = new BalanceContext(new DbContextOptionsBuilder<BalanceContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(new Ledger { RecId = 1, DataAreaId = "dat", AccountingCurrency = "SAR", ReportingCurrency = "EUR" },
            new DirPartyTable { RecId = 1, Name = "Customer", NameAlias = "العميل" },
            new CustTable { RecId = 1, DataAreaId = "dat", AccountNum = "C-001", Party = 1, CurrencyCode = "SAR", CreditMax = 1000, VendAccount = "V-001" },
            new VendTable { RecId = 1, DataAreaId = "dat", AccountNum = "V-001", Party = 1, Currency = "SAR" });
        await db.SaveChangesAsync();
        return db;
    }

    private sealed class BalanceContext(DbContextOptions<BalanceContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            Type[] types = [typeof(Ledger), typeof(DirPartyTable), typeof(CustTable), typeof(VendTable),
                typeof(CustTrans), typeof(CustTransOpen), typeof(VendTrans), typeof(VendTransOpen)];
            foreach (var type in typeof(CustTable).Assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract
                && typeof(IAX.IXApi.Shared.Domain.Entities.Entity<long>).IsAssignableFrom(t) && !types.Contains(t)))
                builder.Ignore(type);
            foreach (var type in types)
            {
                var entity = builder.Entity(type);
                entity.HasKey("RecId"); entity.Ignore("RowVersion");
                foreach (var property in type.GetProperties())
                    if (property.PropertyType != typeof(string) && property.PropertyType != typeof(byte[]) && !property.PropertyType.IsValueType)
                        entity.Ignore(property.Name);
            }
        }
    }
}
