using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Invoice;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IAX.IXApi.Tests;

/// <summary>
/// Sales order charges at invoicing. Charges were configured, attached to orders and then dropped:
/// no posting path referenced MarkupTrans and SumMarkup was never assigned.
///
/// A charge code carries a debit side and a credit side, despite the Cust/Vend field prefixes:
/// CustType/CustomerLedgerDimension are the debit type and account, VendType/VendorLedgerDimension
/// the credit ones. Type 0 is a ledger account and type 1 is the customer or vendor.
/// </summary>
public class SalesChargePostingTests
{
    /// <summary>
    /// The documented sales pattern: debit the customer, credit a ledger account. The customer side
    /// writes no entry of its own — the invoice posts one summary customer entry and folds the
    /// charge into it, so writing it here too would debit the customer twice.
    /// </summary>
    [Fact]
    public async Task A_charge_billed_to_the_customer_credits_its_account_and_is_returned_as_billed()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        AddCode(db, "ShipCharge", debitType: 1, creditType: 0, creditDimension: 3);
        AddOrderCharge(db, "ShipCharge", value: 100m);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await PostAsync(db);

        Assert.Equal(100m, result.BilledToCustomer);
        await db.SaveChangesAsync(); // The service stages entries; the caller's posting saves them.
        db.ChangeTracker.Clear();
        var entries = await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync();
        // Only the credit side is written here.
        var credit = Assert.Single(entries);
        Assert.Equal("410100", credit.LedgerAccount);
        Assert.Equal(-100m, credit.AccountingCurrencyAmount);
    }

    /// <summary>
    /// Both sides on ledger accounts is a charge the company absorbs: it posts a balanced pair and
    /// never reaches the customer balance or the invoice total.
    /// </summary>
    [Fact]
    public async Task A_charge_the_company_absorbs_posts_both_sides_and_bills_nothing()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        AddCode(db, "Freight", debitType: 0, creditType: 0, debitDimension: 4, creditDimension: 3);
        AddOrderCharge(db, "Freight", value: 60m);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await PostAsync(db);

        Assert.Equal(0m, result.BilledToCustomer);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var entries = await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync();
        Assert.Equal(2, entries.Count);
        Assert.Equal(0m, entries.Sum(x => x.AccountingCurrencyAmount)); // Balanced on its own.
        Assert.Contains(entries, x => x.LedgerAccount == "610100" && x.AccountingCurrencyAmount == 60m);
        Assert.Contains(entries, x => x.LedgerAccount == "410100" && x.AccountingCurrencyAmount == -60m);
    }

    [Theory]
    // Percent of the order net, which the fixture's single line sets to 1000.
    [InlineData(MarkupCategory.Percent, 10, 100)]
    // Per piece, on the fixture's quantity of 4.
    [InlineData(MarkupCategory.Pcs, 25, 100)]
    [InlineData(MarkupCategory.Fixed, 100, 100)]
    public async Task The_charge_amount_follows_its_category(MarkupCategory category, int value, int expected)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        AddCode(db, "ShipCharge", debitType: 1, creditType: 0, creditDimension: 3);
        AddOrderCharge(db, "ShipCharge", value, category);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        Assert.Equal(expected, (await PostAsync(db)).BilledToCustomer);
    }

    // A charge posts once. Invoicing the rest of the order must not bill it again.
    [Fact]
    public async Task A_posted_charge_is_not_billed_again()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        AddCode(db, "ShipCharge", debitType: 1, creditType: 0, creditDimension: 3);
        AddOrderCharge(db, "ShipCharge", value: 100m);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        Assert.Equal(100m, (await PostAsync(db)).BilledToCustomer);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        // The charge records what it posted, so the second invoice finds nothing to bill.
        var charge = await db.Set<MarkupTrans>().AsNoTracking().SingleAsync();
        Assert.Equal(100m, charge.Posted);
        Assert.Equal("INV-1", charge.Voucher);
        Assert.Equal(DocumentStatus.Invoice, charge.DocumentStatus);

        Assert.Equal(0m, (await PostAsync(db)).BilledToCustomer);
    }

    [Theory]
    [InlineData(false, 100)]
    [InlineData(true, 115)]
    public async Task Tax_on_a_billed_charge_is_returned_for_the_invoices_tax_voucher(bool includesTax, int amount)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        AddCode(db, "ShipCharge", debitType: 1, creditType: 0, creditDimension: 3, taxItemGroup: "SERVICES");
        AddOrderCharge(db, "ShipCharge", value: amount);
        (await db.Set<SalesTable>().SingleAsync()).InclTax = includesTax;
        db.AddRange(
            new TaxGroupData { RecId = 1, DataAreaId = "dat", TaxGroup = "DOM", TaxCode = "VAT15" },
            new TaxOnItem { RecId = 1, DataAreaId = "dat", TaxItemGroup = "SERVICES", TaxCode = "VAT15" },
            new TaxData { RecId = 1, DataAreaId = "dat", TaxCode = "VAT15", TaxValue = 15m });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await PostAsync(db);

        Assert.Equal(100m, result.BilledToCustomer);
        Assert.Equal(15m, result.Tax);
        Assert.Equal(-100m, Assert.Single(db.Set<GeneralJournalAccountEntry>().Local).TransactionCurrencyAmount);
        var (code, chargeBase, tax, rate, _, _) = Assert.Single(result.TaxByCode);
        Assert.Equal("VAT15", code);
        Assert.Equal(100m, chargeBase);
        Assert.Equal(15m, tax);
        Assert.Equal(15m, rate);
    }

    [Theory]
    // Item type adds the charge to the item cost: sales does not allow it and landed cost is not basic scope.
    [InlineData(2, 0, "adds the charge to the item cost")]
    [InlineData(0, 2, "adds the charge to the item cost")]
    // Crediting a customer or vendor is not a sales charge pattern.
    [InlineData(1, 1, "credits a customer or vendor account")]
    [InlineData(9, 0, "unrecognised debit type")]
    public async Task A_charge_configuration_this_path_does_not_post_is_refused(
        int debitType, int creditType, string expected)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        AddCode(db, "Odd", debitType, creditType, debitDimension: 4, creditDimension: 3);
        AddOrderCharge(db, "Odd", value: 50m);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var error = await Assert.ThrowsAsync<SubledgerPostingException>(() => PostAsync(db));
        Assert.Contains(expected, error.Message);
    }

    [Fact]
    public async Task A_ledger_side_with_no_account_configured_is_refused()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        // Billed to the customer, but the credit account is missing.
        AddCode(db, "NoAccount", debitType: 1, creditType: 0);
        AddOrderCharge(db, "NoAccount", value: 50m);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var error = await Assert.ThrowsAsync<SubledgerPostingException>(() => PostAsync(db));
        Assert.Contains("posts its credit to a ledger account but has none configured", error.Message);
    }

    [Fact]
    public async Task An_order_with_no_charges_posts_nothing()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await PostAsync(db);

        Assert.Equal(0m, result.BilledToCustomer);
        Assert.Equal(0m, result.Tax);
        Assert.Empty(result.TaxByCode);
        Assert.Empty(await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync());
    }

    private static async Task<SalesChargePosting.Result> PostAsync(DbContext db)
    {
        var order = await db.Set<SalesTable>().AsNoTracking().SingleAsync();
        var support = new SubledgerPostingSupport(db);
        var customer = await support.LoadDimensionAsync("dat", 1, "customer summary", default);
        // Order and ledger share the currency, so the translation needs no exchange rate.
        var ledger = await db.Set<Ledger>().AsNoTracking().FirstAsync();
        var translation = await SubledgerCurrencyTranslation.LoadAsync(db, "dat", ledger, "SAR",
            new DateTime(2026, 1, 15), default);
        return await new SalesChargePosting(db).PostAsync("dat", order, entryRecId: 1, "INV-1",
            new DateTime(2026, 1, 15), translation, support, customer, default);
    }

    private static void AddCode(DbContext db, string markupCode, int debitType, int creditType,
        long? debitDimension = null, long? creditDimension = null, string taxItemGroup = "") =>
        db.Add(new MarkupTable
        {
            RecId = db.Set<MarkupTable>().Local.Count + 1, DataAreaId = "dat", MarkupCode = markupCode,
            Txt = markupCode, ModuleType = ModuleInventPurchSales.Sales,
            CustType = debitType, VendType = creditType,
            CustomerLedgerDimension = debitDimension, VendorLedgerDimension = creditDimension,
            TaxItemGroup = taxItemGroup
        });

    private static void AddOrderCharge(DbContext db, string markupCode, decimal value,
        MarkupCategory category = MarkupCategory.Fixed) =>
        db.Add(new MarkupTrans
        {
            RecId = 1, DataAreaId = "dat", MarkupCode = markupCode, LineNum = 1,
            ModuleType = MarkupModuleType.Sales, MarkupCategory = category, Value = value,
            TransTableId = FinanceTableNumbers.SalesTable, TransRecId = 1, CurrencyCode = "SAR"
        });

    private static async Task<ChargeContext> NewContextAsync(SqliteConnection connection)
    {
        var db = new ChargeContext(new DbContextOptionsBuilder<ChargeContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new SalesTable { RecId = 1, DataAreaId = "dat", SalesId = "SO-1", CustAccount = "C-001", TaxGroupId = "DOM" },
            // Quantity 4 and net 1000, so Pcs and Percent charges have a basis.
            new SalesLine { RecId = 1, DataAreaId = "dat", SalesId = "SO-1", LineNum = 1, ItemId = "ITEM-1",
                SalesQty = 4, QtyOrdered = 4, LineAmount = 1000m },
            new Ledger { RecId = 1, DataAreaId = "dat", AccountingCurrency = "SAR" },
            new MainAccount { RecId = 1, DataAreaId = "dat", MainAccountId = "11010000", LedgerChartOfAccounts = 10 },
            new MainAccount { RecId = 2, DataAreaId = "dat", MainAccountId = "410100", LedgerChartOfAccounts = 10 },
            new MainAccount { RecId = 3, DataAreaId = "dat", MainAccountId = "610100", LedgerChartOfAccounts = 10 },
            new DimensionAttributeValueCombination { RecId = 1, DataAreaId = "dat", MainAccount = 1, MainAccountValue = "11010000", DisplayValue = "11010000" },
            new DimensionAttributeValueCombination { RecId = 3, DataAreaId = "dat", MainAccount = 2, MainAccountValue = "410100", DisplayValue = "410100" },
            new DimensionAttributeValueCombination { RecId = 4, DataAreaId = "dat", MainAccount = 3, MainAccountValue = "610100", DisplayValue = "610100" });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return db;
    }

    private sealed class ChargeContext(DbContextOptions<ChargeContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            Type[] types = [typeof(SalesTable), typeof(SalesLine), typeof(MarkupTable), typeof(MarkupTrans),
                typeof(MainAccount), typeof(DimensionAttributeValueCombination),
                typeof(LedgerStructure), typeof(DimensionHierarchy), typeof(Ledger),
                typeof(GeneralJournalAccountEntry), typeof(TaxGroupData), typeof(TaxOnItem), typeof(TaxData),
                // Listed so their own navigations are ignored too: the model reaches them through
                // the tax and charge references above.
                typeof(TaxTable), typeof(TaxLedgerAccountGroup), typeof(UnitOfMeasure)];
            // Reached transitively through currency references this harness does not map.
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
