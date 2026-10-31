using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IAX.IXApi.Tests;

/// <summary>
/// Purchase order charges at vendor invoicing — the payable mirror of the sales side.
///
/// A charge code's debit side is CustType/CustomerLedgerDimension and its credit side is
/// VendType/VendorLedgerDimension, despite the prefixes. The documented purchase pattern reverses
/// sales: debit a ledger account for the cost, credit the vendor, so the charge increases what is
/// owed. Unlike sales, D365 allows the Item type here (landed cost), which is outside basic scope
/// and is refused.
/// </summary>
public class PurchaseChargePostingTests
{
    [Fact]
    public async Task A_charge_the_vendor_bills_debits_its_cost_and_is_returned_as_charged()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        AddCode(db, "PFreight", debitType: 0, creditType: 1, debitDimension: 4);
        AddOrderCharge(db, "PFreight", value: 80m);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await PostAsync(db);

        Assert.Equal(80m, result.ChargedToVendor);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        // Only the debit is written; the vendor credit comes from the invoice's summary entry.
        var entry = Assert.Single(await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync());
        Assert.Equal("610100", entry.LedgerAccount);
        Assert.Equal(80m, entry.AccountingCurrencyAmount);
    }

    [Fact]
    public async Task A_charge_the_company_absorbs_posts_both_sides_and_charges_the_vendor_nothing()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        AddCode(db, "PInternal", debitType: 0, creditType: 0, debitDimension: 4, creditDimension: 3);
        AddOrderCharge(db, "PInternal", value: 40m);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await PostAsync(db);

        Assert.Equal(0m, result.ChargedToVendor);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var entries = await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync();
        Assert.Equal(2, entries.Count);
        Assert.Equal(0m, entries.Sum(x => x.AccountingCurrencyAmount));
    }

    /// <summary>
    /// The calculator used to measure a purchase charge against a sales order, because a purchase
    /// header charge carries TransTableId Unspecified, which the sales branch also accepted. The
    /// fixture's order totals 1000 over 4 units, so a 10% charge is 100 and 25 per unit is 100.
    /// </summary>
    [Theory]
    [InlineData(MarkupCategory.Percent, 10, 100)]
    [InlineData(MarkupCategory.Pcs, 25, 100)]
    [InlineData(MarkupCategory.Fixed, 80, 80)]
    public async Task The_charge_amount_is_measured_against_the_purchase_order(
        MarkupCategory category, int value, int expected)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        AddCode(db, "PFreight", debitType: 0, creditType: 1, debitDimension: 4);
        AddOrderCharge(db, "PFreight", value, category);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        Assert.Equal(expected, (await PostAsync(db)).ChargedToVendor);
    }

    [Fact]
    public async Task Input_tax_on_a_billed_charge_posts_to_the_incoming_tax_account()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        AddCode(db, "PFreight", debitType: 0, creditType: 1, debitDimension: 4, taxItemGroup: "FULL");
        AddOrderCharge(db, "PFreight", value: 100m);
        db.AddRange(
            new TaxGroupData { RecId = 1, DataAreaId = "dat", TaxGroup = "DOM", TaxCode = "VAT15" },
            new TaxOnItem { RecId = 1, DataAreaId = "dat", TaxItemGroup = "FULL", TaxCode = "VAT15" },
            new TaxData { RecId = 1, DataAreaId = "dat", TaxCode = "VAT15", TaxValue = 15m },
            new TaxTable { RecId = 1, DataAreaId = "dat", TaxCode = "VAT15", TaxAccountGroup = "VAT" },
            new TaxLedgerAccountGroup { RecId = 1, DataAreaId = "dat", TaxAccountGroup = "VAT", TaxIncomingLedgerDimension = 5 });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await PostAsync(db);

        Assert.Equal(100m, result.ChargedToVendor);
        Assert.Equal(15m, result.Tax);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var entries = await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync();
        Assert.Contains(entries, x => x.LedgerAccount == "210100" && x.AccountingCurrencyAmount == 15m);
        // Input tax, keyed by the invoice voucher.
        var tax = Assert.Single(await db.Set<TaxTrans>().AsNoTracking().ToListAsync());
        Assert.Equal(TaxDirection.Incoming, tax.TaxDirection);
        Assert.Equal(15m, tax.TaxAmount);
        Assert.Equal("V-1", tax.Voucher);
    }

    [Fact]
    public async Task A_posted_charge_is_not_billed_again()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        AddCode(db, "PFreight", debitType: 0, creditType: 1, debitDimension: 4);
        AddOrderCharge(db, "PFreight", value: 80m);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        Assert.Equal(80m, (await PostAsync(db)).ChargedToVendor);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        Assert.Equal(80m, (await db.Set<MarkupTrans>().AsNoTracking().SingleAsync()).Posted);

        Assert.Equal(0m, (await PostAsync(db)).ChargedToVendor);
    }

    [Theory]
    // Landed cost: allowed in D365 for purchases, but outside basic scope.
    [InlineData(2, 1, "adds the charge to the item cost")]
    [InlineData(0, 2, "adds the charge to the item cost")]
    // Debiting a customer or vendor is not the purchase charge pattern.
    [InlineData(1, 1, "debits a customer or vendor account")]
    [InlineData(0, 9, "unrecognised credit type")]
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

    // The seeded purchase charge codes have no accounts configured, so this is the message they hit.
    [Fact]
    public async Task A_charge_with_no_debit_account_is_refused()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        AddCode(db, "PFreight", debitType: 0, creditType: 1);
        AddOrderCharge(db, "PFreight", value: 80m);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        Assert.Contains("posts its debit to a ledger account but has none configured",
            (await Assert.ThrowsAsync<SubledgerPostingException>(() => PostAsync(db))).Message);
    }

    // A sales charge on the same record id must not be picked up by the purchase path.
    [Fact]
    public async Task A_sales_charge_is_not_read_as_a_purchase_charge()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = await NewContextAsync(connection);
        AddCode(db, "ShipCharge", debitType: 1, creditType: 0, creditDimension: 3);
        db.Add(new MarkupTrans
        {
            RecId = 1, DataAreaId = "dat", MarkupCode = "ShipCharge", LineNum = 1,
            ModuleType = MarkupModuleType.Sales, MarkupCategory = MarkupCategory.Fixed, Value = 100m,
            TransTableId = FinanceTableNumbers.SalesTable, TransRecId = 1, CurrencyCode = "SAR"
        });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await PostAsync(db);

        Assert.Equal(0m, result.ChargedToVendor);
        Assert.Empty(await db.Set<GeneralJournalAccountEntry>().AsNoTracking().ToListAsync());
    }

    private static async Task<PurchaseChargePosting.Result> PostAsync(DbContext db)
    {
        var order = await db.Set<PurchTable>().AsNoTracking().SingleAsync();
        return await new PurchaseChargePosting(db).PostAsync("dat", order, entryRecId: 1, "INV-1", "V-1",
            new DateTime(2026, 1, 15), "SAR", new SubledgerPostingSupport(db), default);
    }

    private static void AddCode(DbContext db, string markupCode, int debitType, int creditType,
        long? debitDimension = null, long? creditDimension = null, string taxItemGroup = "") =>
        db.Add(new MarkupTable
        {
            RecId = db.Set<MarkupTable>().Local.Count + 1, DataAreaId = "dat", MarkupCode = markupCode,
            Txt = markupCode, ModuleType = ModuleInventPurchSales.Purchase,
            CustType = debitType, VendType = creditType,
            CustomerLedgerDimension = debitDimension, VendorLedgerDimension = creditDimension,
            TaxItemGroup = taxItemGroup
        });

    private static void AddOrderCharge(DbContext db, string markupCode, decimal value,
        MarkupCategory category = MarkupCategory.Fixed) =>
        db.Add(new MarkupTrans
        {
            RecId = 1, DataAreaId = "dat", MarkupCode = markupCode, LineNum = 1,
            ModuleType = MarkupModuleType.Vendor, MarkupCategory = category, Value = value,
            // Purchase charges attach at header level with Unspecified: no purchase table number exists.
            TransTableId = FinanceTableNumbers.Unspecified, TransRecId = 1, CurrencyCode = "SAR"
        });

    private static async Task<PurchaseChargeContext> NewContextAsync(SqliteConnection connection)
    {
        var db = new PurchaseChargeContext(
            new DbContextOptionsBuilder<PurchaseChargeContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new PurchTable { RecId = 1, DataAreaId = "dat", PurchId = "PO-1", OrderAccount = "V-001",
                InvoiceAccount = "V-001", CurrencyCode = "SAR", TaxGroup = "DOM" },
            // 4 units totalling 1000, so Pcs and Percent charges have a basis.
            new PurchLine { RecId = 1, DataAreaId = "dat", PurchId = "PO-1", LineNumber = 1, ItemId = "ITEM-1",
                PurchQty = 4, QtyOrdered = 4, LineAmount = 1000m },
            new MainAccount { RecId = 2, DataAreaId = "dat", MainAccountId = "410100", LedgerChartOfAccounts = 10 },
            new MainAccount { RecId = 3, DataAreaId = "dat", MainAccountId = "610100", LedgerChartOfAccounts = 10 },
            new MainAccount { RecId = 4, DataAreaId = "dat", MainAccountId = "210100", LedgerChartOfAccounts = 10 },
            new DimensionAttributeValueCombination { RecId = 3, DataAreaId = "dat", MainAccount = 2, MainAccountValue = "410100", DisplayValue = "410100" },
            new DimensionAttributeValueCombination { RecId = 4, DataAreaId = "dat", MainAccount = 3, MainAccountValue = "610100", DisplayValue = "610100" },
            new DimensionAttributeValueCombination { RecId = 5, DataAreaId = "dat", MainAccount = 4, MainAccountValue = "210100", DisplayValue = "210100" });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return db;
    }

    private sealed class PurchaseChargeContext(DbContextOptions<PurchaseChargeContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            Type[] types = [typeof(PurchTable), typeof(PurchLine), typeof(MarkupTable), typeof(MarkupTrans),
                typeof(MainAccount), typeof(DimensionAttributeValueCombination),
                typeof(LedgerStructure), typeof(DimensionHierarchy), typeof(Ledger),
                typeof(GeneralJournalAccountEntry), typeof(TaxGroupData), typeof(TaxOnItem), typeof(TaxData),
                typeof(TaxTable), typeof(TaxLedgerAccountGroup), typeof(TaxTrans), typeof(UnitOfMeasure)];
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
