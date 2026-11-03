using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Invoice;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IAX.IXApi.Tests;

/// <summary>
/// Zero-rated sales tax codes. A zero-rated code is an ordinary configuration, not an absent one,
/// and the taxable turnover booked under it has to be reportable: the sales tax report reads
/// <c>TaxTrans</c>, so if nothing is written the base is lost at posting and cannot be recovered.
///
/// <para>
/// Tax resolution used to end with <c>.Where(x =&gt; x.Item2 != 0m)</c>, which discarded every code
/// whose rate was zero, and each invoice flow then skipped any code with no tax amount. Between
/// them a zero-rated line produced no tax record at all.
/// </para>
///
/// <para>
/// Two cases that look alike must stay apart, which is what these tests pin: a code with a rate row
/// that says zero is <b>in force at zero</b> and is kept; a code with no rate row covering the date
/// is <b>not in force</b> and is still dropped.
/// </para>
/// </summary>
public class ZeroRatedTaxTests
{
    private const string Area = "dat";
    private static readonly DateTime Date = new(2026, 1, 15);

    /// <summary>
    /// The defect this closes. The code is attached to both the tax group and the item tax group and
    /// has a rate row covering the date; only its percentage is zero.
    /// </summary>
    [Fact]
    public async Task A_code_whose_configured_rate_is_zero_is_returned_at_zero()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        AddCode(db, "VAT-ZERO", rate: 0m);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var codes = await new SubledgerPostingSupport(db)
            .ApplicableTaxCodesAsync(Area, "DOM", "FULL", Date, default);

        var code = Assert.Single(codes);
        Assert.Equal("VAT-ZERO", code.Code);
        Assert.Equal(0m, code.Rate);
    }

    /// <summary>
    /// The case that must keep behaving as before: no rate row covers the date, so the code is not
    /// in force and contributes nothing. Without this distinction, lifting the zero filter would
    /// have started reporting turnover under codes that were never effective.
    /// </summary>
    [Fact]
    public async Task A_code_with_no_rate_covering_the_date_is_still_dropped()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        // In force only during 2025, so it does not cover the January 2026 document date.
        AddCode(db, "VAT-OLD", rate: 15m,
            from: new DateTime(2025, 1, 1), to: new DateTime(2025, 12, 31));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var codes = await new SubledgerPostingSupport(db)
            .ApplicableTaxCodesAsync(Area, "DOM", "FULL", Date, default);

        Assert.Empty(codes);
    }

    /// <summary>A code with no rate rows at all is likewise not in force.</summary>
    [Fact]
    public async Task A_code_with_no_rate_row_at_all_is_dropped()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        AddCode(db, "VAT-UNSET", rate: null);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var codes = await new SubledgerPostingSupport(db)
            .ApplicableTaxCodesAsync(Area, "DOM", "FULL", Date, default);

        Assert.Empty(codes);
    }

    /// <summary>
    /// Apportioning a zero total across zero rates would divide by zero. It must instead yield one
    /// entry per code, each at zero, so every code still gets its <c>TaxTrans</c> row.
    /// </summary>
    [Fact]
    public void Allocating_tax_across_only_zero_rated_codes_yields_a_zero_for_each()
    {
        List<(string Code, decimal Rate)> codes = [("VAT-ZERO", 0m), ("VAT-EXEMPT", 0m)];

        var allocated = SubledgerPostingSupport.AllocateTax(codes, 0m);

        Assert.Equal(2, allocated.Count);
        Assert.All(allocated, x => Assert.Equal(0m, x.Tax));
        Assert.Equal(["VAT-ZERO", "VAT-EXEMPT"], allocated.Select(x => x.Code));
    }

    /// <summary>A mixed set still apportions by rate, so the existing behavior is unchanged.</summary>
    [Fact]
    public void Allocating_tax_across_a_zero_and_a_positive_rate_gives_it_all_to_the_positive_one()
    {
        List<(string Code, decimal Rate)> codes = [("VAT-ZERO", 0m), ("VAT-15", 15m)];

        var allocated = SubledgerPostingSupport.AllocateTax(codes, 150m);

        Assert.Equal(0m, allocated.Single(x => x.Code == "VAT-ZERO").Tax);
        Assert.Equal(150m, allocated.Single(x => x.Code == "VAT-15").Tax);
    }

    /// <summary>
    /// Through a real posting path: a charge on a zero-rated group reports its base under the code
    /// at zero tax, instead of the charge tax returning early and registering nothing. The base is
    /// what the invoice's tax loop turns into the <c>TaxTrans</c> row.
    /// </summary>
    [Fact]
    public async Task A_zero_rated_charge_reports_its_base_under_the_code_at_zero_tax()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        AddCode(db, "VAT-ZERO", rate: 0m);
        db.Add(new MarkupTable
        {
            RecId = 1, DataAreaId = Area, MarkupCode = "ShipCharge", Txt = "ShipCharge",
            ModuleType = ModuleInventPurchSales.Sales, CustType = 1, VendType = 0,
            VendorLedgerDimension = 3, TaxItemGroup = "FULL"
        });
        db.Add(new MarkupTrans
        {
            RecId = 1, DataAreaId = Area, MarkupCode = "ShipCharge", LineNum = 1,
            ModuleType = MarkupModuleType.Sales, MarkupCategory = MarkupCategory.Fixed, Value = 100m,
            TransTableId = FinanceTableNumbers.SalesTable, TransRecId = 1, CurrencyCode = "SAR"
        });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var order = await db.Set<SalesTable>().AsNoTracking().SingleAsync();
        var support = new SubledgerPostingSupport(db);
        var customer = await support.LoadDimensionAsync(Area, 1, "customer summary", default);
        var ledger = await db.Set<Ledger>().AsNoTracking().FirstAsync();
        var translation = await SubledgerCurrencyTranslation.LoadAsync(db, Area, ledger, "SAR", Date, default);
        var result = await new SalesChargePosting(db).PostAsync(Area, order, entryRecId: 1, "INV-1",
            Date, translation, support, customer, default);

        Assert.Equal(0m, result.Tax);
        var reported = Assert.Single(result.TaxByCode);
        Assert.Equal("VAT-ZERO", reported.Code);
        // The charge amount is the taxable base, recorded even though no tax arises from it.
        Assert.Equal(100m, reported.Base);
        Assert.Equal(0m, reported.Tax);
        Assert.Equal(0m, reported.Rate);
    }

    /// <summary>
    /// Attaches a tax code to the fixture's tax group and item tax group. A null rate adds no
    /// <see cref="TaxData"/> row, which is how "no rate configured" is expressed.
    /// </summary>
    private static void AddCode(DbContext db, string taxCode, decimal? rate,
        DateTime from = default, DateTime to = default)
    {
        var next = db.Set<TaxTable>().Local.Count + 1;
        db.Add(new TaxTable
        {
            RecId = next, DataAreaId = Area, TaxCode = taxCode, TaxName = taxCode,
            TaxAccountGroup = "STANDARD"
        });
        db.Add(new TaxGroupData { RecId = next, DataAreaId = Area, TaxGroup = "DOM", TaxCode = taxCode });
        db.Add(new TaxOnItem { RecId = next, DataAreaId = Area, TaxItemGroup = "FULL", TaxCode = taxCode });
        if (rate != null)
            db.Add(new TaxData
            {
                RecId = next, DataAreaId = Area, TaxCode = taxCode, TaxValue = rate.Value,
                TaxFromDate = from, TaxToDate = to
            });
    }

    private static async Task<TaxContext> NewContextAsync(SqliteConnection connection)
    {
        var db = new TaxContext(new DbContextOptionsBuilder<TaxContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new SalesTable { RecId = 1, DataAreaId = Area, SalesId = "SO-1", CustAccount = "C-001", TaxGroupId = "DOM" },
            new SalesLine { RecId = 1, DataAreaId = Area, SalesId = "SO-1", LineNum = 1, ItemId = "ITEM-1",
                SalesQty = 4, QtyOrdered = 4, LineAmount = 1000m },
            new Ledger { RecId = 1, DataAreaId = Area, AccountingCurrency = "SAR" },
            new MainAccount { RecId = 1, DataAreaId = Area, MainAccountId = "11010000", LedgerChartOfAccounts = 10 },
            new MainAccount { RecId = 2, DataAreaId = Area, MainAccountId = "410100", LedgerChartOfAccounts = 10 },
            new DimensionAttributeValueCombination { RecId = 1, DataAreaId = Area, MainAccount = 1, MainAccountValue = "11010000", DisplayValue = "11010000" },
            new DimensionAttributeValueCombination { RecId = 3, DataAreaId = Area, MainAccount = 2, MainAccountValue = "410100", DisplayValue = "410100" });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return db;
    }

    private sealed class TaxContext(DbContextOptions<TaxContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            Type[] types = [typeof(SalesTable), typeof(SalesLine), typeof(MarkupTable), typeof(MarkupTrans),
                typeof(MainAccount), typeof(DimensionAttributeValueCombination),
                typeof(LedgerStructure), typeof(DimensionHierarchy), typeof(Ledger),
                typeof(GeneralJournalAccountEntry), typeof(TaxGroupData), typeof(TaxOnItem), typeof(TaxData),
                // Listed so their own navigations are ignored too.
                typeof(TaxTable), typeof(TaxLedgerAccountGroup), typeof(UnitOfMeasure)];
            builder.Ignore<Currency>();
            builder.Ignore<ExchangeRateCurrencyPair>();
            builder.Ignore<ExchangeRateType>();
            foreach (var type in types)
            {
                var entity = builder.Entity(type);
                entity.HasKey("RecId");
                entity.Ignore("RowVersion");
                foreach (var property in type.GetProperties())
                    if (property.PropertyType != typeof(string) && property.PropertyType != typeof(byte[])
                        && !property.PropertyType.IsValueType)
                        entity.Ignore(property.Name);
            }
        }
    }
}
