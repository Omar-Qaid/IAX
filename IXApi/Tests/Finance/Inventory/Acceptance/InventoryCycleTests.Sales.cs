using System.Reflection;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PriceDisc;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Invoice;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip.Dtos;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Payment;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Return;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Services;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory;
using IAX.IXApi.Shared.Application.Identity;
using IAX.IXApi.Shared.Domain.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace IAX.IXApi.Tests;

public sealed partial class InventoryCycleTests
{
    [Fact]
    public async Task CustOpenTrans_reads_open_rows_with_transaction_ids_dimensions_and_remaining_balances()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        db.Set<CustTable>().Add(new CustTable { RecId = 1, DataAreaId = "dat", AccountNum = "C-1", Party = 1, CurrencyCode = "SAR" });
        db.Set<DirPartyTable>().Add(new DirPartyTable { RecId = 1, DataAreaId = "dat", Name = "Customer One" });
        db.Set<Ledger>().Add(new Ledger { RecId = 1, DataAreaId = "dat", AccountingCurrency = "SAR" });
        db.Set<DimensionAttributeValueSet>().Add(new DimensionAttributeValueSet { RecId = 1, DataAreaId = "dat", WorkerValue = "W-1", DepartmentValue = "SALES" });
        db.Set<CustTrans>().AddRange(
            new CustTrans { RecId = 1, DataAreaId = "dat", AccountNum = "C-1", Invoice = "INV-1", CurrencyCode = "SAR", AmountCur = 100, AmountMst = 100, SettleAmountCur = 40, SettleAmountMst = 40, DefaultDimension = 1 },
            new CustTrans { RecId = 2, DataAreaId = "dat", AccountNum = "C-1", Voucher = "PAY-1", CurrencyCode = "SAR", AmountCur = -20, AmountMst = -20 },
            new CustTrans { RecId = 3, DataAreaId = "dat", AccountNum = "C-2", AmountCur = 999, AmountMst = 999 },
            new CustTrans { RecId = 4, DataAreaId = "other", AccountNum = "C-1", AmountCur = 888, AmountMst = 888 },
            new CustTrans { RecId = 5, DataAreaId = "dat", AccountNum = "C-1", AmountCur = 50, AmountMst = 50, SettleAmountCur = 50, SettleAmountMst = 50 });
        db.Set<CustTransOpen>().AddRange(
            new CustTransOpen { RecId = 101, DataAreaId = "dat", AccountNum = "C-1", RefRecId = 1, AmountCur = 60, AmountMst = 60, PossibleCashDisc = 3 },
            new CustTransOpen { RecId = 102, DataAreaId = "dat", AccountNum = "C-1", RefRecId = 2, AmountCur = -20, AmountMst = -20 },
            new CustTransOpen { RecId = 103, DataAreaId = "dat", AccountNum = "C-2", RefRecId = 3, AmountCur = 999, AmountMst = 999 },
            new CustTransOpen { RecId = 104, DataAreaId = "other", AccountNum = "C-1", RefRecId = 4, AmountCur = 888, AmountMst = 888 });
        db.Set<CustSettlement>().Add(new CustSettlement { RecId = 1, DataAreaId = "dat", AccountNum = "C-1", TransRecId = 1, UtilizedCashDisc = 5 });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var service = new IAX.IXApi.Modules.Finance.AccountsReceivable.Settlement.CustSettleOpenTransactionsService(new TestUnitOfWork(db), new TestCompany(), Sequence());
        var result = await service.GetOpenTransactionsAsync(" C-1 ", default);
        Assert.Equal(200, result.StatusCode);
        var data = Assert.IsType<IAX.IXApi.Modules.Finance.AccountsReceivable.Settlement.CustOpenTransDto>(result.Data);
        Assert.Equal(2, data.Transactions.Count);
        Assert.Equal(40m, data.CustomerBalanceMst);
        Assert.True(data.Reconciled);
        var invoice = Assert.Single(data.Transactions, x => x.Invoice == "INV-1");
        Assert.Equal(101, invoice.RecId);
        Assert.Equal(1, invoice.TransRecId);
        Assert.Equal(60m, invoice.AmountCur);
        Assert.Equal(100m, invoice.OriginalAmountCur);
        Assert.Equal(3m, invoice.PossibleCashDisc);
        Assert.Equal(5m, invoice.CashDiscTaken);
        Assert.Equal("W-1", invoice.Worker);
        Assert.Equal("SALES", invoice.Department);
        Assert.Empty(db.ChangeTracker.Entries());
        Assert.Equal(400, (await service.GetOpenTransactionsAsync(" ", default)).StatusCode);
        Assert.Equal(404, (await service.GetOpenTransactionsAsync("missing", default)).StatusCode);
    }

    [Fact]
    public async Task Customer_invoice_lines_are_read_only_customer_and_company_scoped()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        db.Set<CustTable>().Add(new CustTable { RecId = 1, DataAreaId = "dat", AccountNum = "C-1", Party = 1 });
        db.Set<DirPartyTable>().Add(new DirPartyTable { RecId = 1, DataAreaId = "dat", Name = "Customer one" });
        db.Set<DimensionAttributeValueSet>().Add(new DimensionAttributeValueSet { RecId = 1, DataAreaId = "dat", DepartmentValue = "SALES" });
        db.Set<DimensionAttributeValueCombination>().Add(new DimensionAttributeValueCombination { RecId = 1, DataAreaId = "dat", MainAccountValue = "410100" });
        db.Set<CustInvoiceJour>().AddRange(
            new CustInvoiceJour { RecId = 1, DataAreaId = "dat", OrderAccount = "C-1", InvoiceAccount = "BILL-TO" },
            new CustInvoiceJour { RecId = 2, DataAreaId = "dat", OrderAccount = "C-2" },
            new CustInvoiceJour { RecId = 3, DataAreaId = "other", OrderAccount = "C-1" },
            new CustInvoiceJour { RecId = 4, DataAreaId = "dat", OrderAccount = "C-1", Proforma = NoYes.Yes });
        db.Set<CustInvoiceTrans>().AddRange(
            new CustInvoiceTrans { RecId = 1, DataAreaId = "dat", ParentRecId = 1, ItemId = "ITEM", Qty = -2, LineAmount = -100, DefaultDimension = 1, LedgerDimension = 1, DiscPercent = 7.56m },
            new CustInvoiceTrans { RecId = 2, DataAreaId = "dat", ParentRecId = 2 },
            new CustInvoiceTrans { RecId = 3, DataAreaId = "other", ParentRecId = 3 },
            new CustInvoiceTrans { RecId = 4, DataAreaId = "dat", ParentRecId = 4 },
            new CustInvoiceTrans { RecId = 5, DataAreaId = "other", ParentRecId = 1 });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var service = new SalesInvoiceService(new TestUnitOfWork(db), new TestCompany(), Sequence());
        var result = await service.ListCustomerLinesAsync(" C-1 ", default);
        Assert.Equal(200, result.StatusCode);
        var json = System.Text.Json.JsonSerializer.SerializeToElement(result.Data);
        Assert.Equal("Customer one", json.GetProperty("CustomerName").GetString());
        var row = Assert.Single(json.GetProperty("Lines").EnumerateArray());
        Assert.Equal(-100m, row.GetProperty("LineAmount").GetDecimal());
        Assert.Equal(-2m, row.GetProperty("Qty").GetDecimal());
        Assert.Equal(7.56m, row.GetProperty("DiscPercent").GetDecimal());
        Assert.Equal("C-1", row.GetProperty("CustomerAccount").GetString());
        Assert.Equal("SALES", row.GetProperty("DepartmentValue").GetString());
        Assert.Equal("410100", row.GetProperty("MainAccountValue").GetString());
        Assert.Empty(db.ChangeTracker.Entries());
        Assert.Equal(400, (await service.ListCustomerLinesAsync(" ", default)).StatusCode);
        Assert.Equal(404, (await service.ListCustomerLinesAsync("missing", default)).StatusCode);
    }

    [Fact]
    public async Task Free_text_invoice_posts_services_tax_and_receivable_without_inventory()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        db.Set<Currency>().Add(new Currency { RecId = 1, DataAreaId = "dat", CurrencyCode = "SAR" });
        db.Set<PaymTerm>().Add(new PaymTerm
        {
            RecId = 1, DataAreaId = "dat", PaymTermId = "NET30", Description = "30 days", NumOfDays = 30
        });
        db.Set<CustTable>().Add(new CustTable
        {
            RecId = 1, DataAreaId = "dat", AccountNum = "C-0001", InvoiceAccount = "C-0001",
            CustGroupId = "LOCAL", CurrencyCode = "SAR", PaymTermId = "NET30", TaxGroupId = "DOMESTIC"
        });
        db.Set<Ledger>().Add(new Ledger
        {
            RecId = 1, DataAreaId = "dat", AccountingCurrency = "SAR", ReportingCurrency = "SAR", FiscalCalendar = 1
        });
        db.Set<FiscalCalendarPeriod>().Add(new FiscalCalendarPeriod
        {
            RecId = 1, FiscalCalendar = 1, FiscalCalendarYear = 1, Type = FiscalPeriodType.Operating,
            StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 12, 31)
        });
        db.Set<LedgerFiscalCalendarPeriod>().Add(new LedgerFiscalCalendarPeriod
        {
            RecId = 1, Ledger = 1, FiscalCalendarPeriod = 1, Status = FiscalPeriodStatus.Open
        });
        db.Set<MainAccount>().AddRange(
            new MainAccount { RecId = 1, DataAreaId = "dat", MainAccountId = "110100" },
            new MainAccount { RecId = 2, DataAreaId = "dat", MainAccountId = "410100" },
            new MainAccount { RecId = 3, DataAreaId = "dat", MainAccountId = "410200" },
            new MainAccount { RecId = 4, DataAreaId = "dat", MainAccountId = "210200" });
        db.Set<DimensionAttributeValueCombination>().AddRange(
            new DimensionAttributeValueCombination { RecId = 1, DataAreaId = "dat", MainAccount = 1, MainAccountValue = "110100", DisplayValue = "110100" },
            new DimensionAttributeValueCombination { RecId = 2, DataAreaId = "dat", MainAccount = 2, MainAccountValue = "410100", DisplayValue = "410100" },
            new DimensionAttributeValueCombination { RecId = 3, DataAreaId = "dat", MainAccount = 3, MainAccountValue = "410200", DisplayValue = "410200" },
            new DimensionAttributeValueCombination { RecId = 4, DataAreaId = "dat", MainAccount = 4, MainAccountValue = "210200", DisplayValue = "210200" });
        db.Set<CustLedgerAccounts>().Add(new CustLedgerAccounts
        {
            RecId = 1, DataAreaId = "dat", PostingProfile = "GEN", AccountCode = AccountCode.Group,
            Num = "LOCAL", SummaryLedgerDimension = 1
        });
        db.Set<TaxGroupHeading>().Add(new TaxGroupHeading
        {
            RecId = 1, DataAreaId = "dat", TaxGroup = "DOMESTIC", TaxGroupName = "Domestic"
        });
        db.Set<TaxItemGroupHeading>().Add(new TaxItemGroupHeading
        {
            RecId = 1, DataAreaId = "dat", TaxItemGroup = "SERVICE", Name = "Service"
        });
        db.Set<TaxGroupData>().Add(new TaxGroupData
        {
            RecId = 1, DataAreaId = "dat", TaxGroup = "DOMESTIC", TaxCode = "VAT15"
        });
        db.Set<TaxOnItem>().Add(new TaxOnItem
        {
            RecId = 1, DataAreaId = "dat", TaxItemGroup = "SERVICE", TaxCode = "VAT15"
        });
        db.Set<TaxData>().Add(new TaxData
        {
            RecId = 1, DataAreaId = "dat", TaxCode = "VAT15", TaxValue = 15m,
            TaxFromDate = new DateTime(2026, 1, 1), TaxToDate = new DateTime(2026, 12, 31)
        });
        db.Set<TaxTable>().Add(new TaxTable
        {
            RecId = 1, DataAreaId = "dat", TaxCode = "VAT15", TaxAccountGroup = "VAT"
        });
        db.Set<TaxLedgerAccountGroup>().Add(new TaxLedgerAccountGroup
        {
            RecId = 1, DataAreaId = "dat", TaxAccountGroup = "VAT", TaxOutgoingLedgerDimension = 4
        });
        await db.SaveChangesAsync();

        var service = new FreeTextInvoiceService(new TestUnitOfWork(db), new TestCompany(), Sequence());
        var created = await service.CreateAsync(new FreeTextInvoiceSaveDto
        {
            CustomerAccount = "C-0001",
            InvoiceDate = new DateTime(2026, 11, 1),
            PostingProfile = "GEN",
            Description = "November services",
            Lines =
            [
                new() { Description = "Maintenance", Quantity = 1, UnitPrice = 10000, LedgerDimension = 2, TaxItemGroup = "SERVICE" },
                new() { Description = "Consulting", Quantity = 4, UnitPrice = 500, LedgerDimension = 3, TaxItemGroup = "SERVICE" }
            ]
        }, default);

        Assert.Equal(201, created.StatusCode);
        Assert.Equal(12000m, created.Invoice!.NetAmount);
        Assert.Equal(1800m, created.Invoice.TaxAmount);
        Assert.Equal(new DateTime(2026, 12, 1), created.Invoice.DueDate);
        Assert.Equal(string.Empty, created.Invoice.InvoiceId);
        Assert.Empty(await db.Set<CustTrans>().ToListAsync());

        // Editing an ordinary draft must not look for a correction with record ID zero.
        var updated = await service.UpdateAsync(created.Invoice.RecId, new FreeTextInvoiceSaveDto
        {
            CustomerAccount = "C-0001", InvoiceDate = new DateTime(2026, 11, 1),
            PostingProfile = "GEN", Description = "Updated November services",
            Lines =
            [
                new() { Description = "Maintenance", Quantity = 1, UnitPrice = 10000, LedgerDimension = 2, TaxItemGroup = "SERVICE" },
                new() { Description = "Consulting", Quantity = 4, UnitPrice = 500, LedgerDimension = 3, TaxItemGroup = "SERVICE" }
            ]
        }, default);
        Assert.True(updated.StatusCode == 200, updated.Message);
        db.ChangeTracker.Clear();
        var reopened = await service.GetAsync(created.Invoice.RecId, default);
        Assert.Equal("Updated November services", reopened!.Description);
        Assert.Equal(new long[] { 2, 3 }, reopened.Lines.Select(x => x.LedgerDimension));
        Assert.Empty(await db.Set<CustTrans>().ToListAsync());

        var posted = await service.PostAsync(created.Invoice.RecId, default);

        Assert.Equal(200, posted.StatusCode);
        Assert.Equal("X-000001", posted.Invoice!.InvoiceId);
        Assert.Equal(13800m, posted.Invoice.InvoiceAmount);
        Assert.Equal(13800m, (await db.Set<CustTrans>().SingleAsync()).AmountCur);
        Assert.Equal(13800m, (await db.Set<CustTransOpen>().SingleAsync()).AmountCur);
        Assert.Equal(2, await db.Set<CustInvoiceTrans>().CountAsync());
        Assert.Equal(-1800m, (await db.Set<TaxTrans>().SingleAsync()).TaxAmountCur);
        Assert.Equal(new[] { -10000m, -2000m, -1800m, 13800m },
            (await db.Set<GeneralJournalAccountEntry>().Select(x => x.AccountingCurrencyAmount).ToListAsync())
                .OrderBy(x => x).ToArray());
        Assert.Empty(await db.Set<InventTrans>().ToListAsync());

        var repeated = await service.PostAsync(created.Invoice.RecId, default);
        Assert.Equal("Free text invoice was already posted.", repeated.Message);
        Assert.Single(await db.Set<CustInvoiceJour>().ToListAsync());
        Assert.Single(await db.Set<CustTrans>().ToListAsync());
    }

    [Fact]
    public async Task Reserve_and_release_track_ownership_and_availability()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        db.Set<InventDim>().Add(new InventDim { RecId = 1, DataAreaId = "dat", InventDimId = "DIM", InventSiteId = "S1", InventLocationId = "WH" });
        db.Set<InventSum>().Add(new InventSum { RecId = 1, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", PhysicalInvent = 100, ReservPhysical = 0, OnOrder = 30, AvailPhysical = 100 });
        db.Set<InventTransOrigin>().Add(new InventTransOrigin { RecId = 1, DataAreaId = "dat", InventTransId = "SO-L1", ItemId = "ITEM", ReferenceId = "SO1", ReferenceCategory = InventRefType.SalesTable });
        db.Set<InventTrans>().Add(new InventTrans { RecId = 1, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 1, Qty = -30, StatusIssue = StatusIssue.Ordered });
        await db.SaveChangesAsync();
        var service = new InventoryReservationService(new TestUnitOfWork(db), new TestCompany());

        // Reserve 20 of 30 demand against 100 on hand.
        var (reserved, error) = await service.ReserveAsync("SO-L1", 20m, false, default);
        Assert.Null(error);
        Assert.Equal(20m, reserved!.Reserved);
        Assert.Equal(80m, reserved.AvailablePhysical);
        var sum = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(20m, sum.ReservPhysical);
        Assert.Equal(10m, sum.OnOrder);
        Assert.Equal(80m, sum.AvailPhysical);
        Assert.Equal(20m, -(await db.Set<InventTrans>().SingleAsync(x => x.StatusIssue == StatusIssue.ReservPhysical)).Qty);

        // Idempotent set-target: reserving the same 20 again is a no-op.
        var (same, _) = await service.ReserveAsync("SO-L1", 20m, false, default);
        Assert.Equal(20m, same!.Reserved);
        Assert.Equal(20m, (await db.Set<InventSum>().SingleAsync()).ReservPhysical);

        // Cannot reserve above remaining demand (30).
        Assert.NotNull((await service.ReserveAsync("SO-L1", 40m, false, default)).Error);

        // Release 5 ÃƒÂ¢Ã¢â‚¬Â Ã¢â‚¬â„¢ reserved 15, available 85.
        var (released, releaseError) = await service.ReleaseAsync("SO-L1", 5m, default);
        Assert.Null(releaseError);
        Assert.Equal(15m, released!.Reserved);
        sum = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(15m, sum.ReservPhysical);
        Assert.Equal(15m, sum.OnOrder);
        Assert.Equal(85m, sum.AvailPhysical);
    }

    [Fact]
    public async Task Reserve_rejects_more_than_available_physical_stock()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        db.Set<InventDim>().Add(new InventDim { RecId = 1, DataAreaId = "dat", InventDimId = "DIM", InventSiteId = "S1", InventLocationId = "WH" });
        db.Set<InventSum>().Add(new InventSum { RecId = 1, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", PhysicalInvent = 10, ReservPhysical = 0, OnOrder = 30, AvailPhysical = 10 });
        db.Set<InventTransOrigin>().Add(new InventTransOrigin { RecId = 1, DataAreaId = "dat", InventTransId = "SO-L1", ItemId = "ITEM", ReferenceId = "SO1", ReferenceCategory = InventRefType.SalesTable });
        db.Set<InventTrans>().Add(new InventTrans { RecId = 1, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 1, Qty = -30, StatusIssue = StatusIssue.Ordered });
        await db.SaveChangesAsync();
        var service = new InventoryReservationService(new TestUnitOfWork(db), new TestCompany());

        Assert.NotNull((await service.ReserveAsync("SO-L1", 25m, false, default)).Error);
        Assert.Equal(0m, (await db.Set<InventSum>().SingleAsync()).ReservPhysical);

        // allowPartial reserves only what is available (10).
        var (partial, error) = await service.ReserveAsync("SO-L1", 25m, true, default);
        Assert.Null(error);
        Assert.Equal(10m, partial!.Reserved);
        Assert.Equal(0m, partial.AvailablePhysical);
    }

    [Fact]
    public async Task Sales_invoice_without_an_exchange_rate_posts_nothing()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedSalesMasterData(db);
        SeedForeignOrder(db);
        await db.SaveChangesAsync();
        var order = await db.Set<SalesTable>().SingleAsync(x => x.SalesId == "SO-FX");
        var invoices = new SalesInvoiceService(new TestUnitOfWork(db), new TestCompany(), Sequence());

        // No rate is configured for USD/SAR, so the invoice is refused rather than posted at an
        // assumed rate. Nothing is written.
        var result = await invoices.PostAsync(order.RecId, new SalesInvoicePostDto(new DateTime(2026, 10, 7), "fx-1"), default);
        Assert.Equal(422, result.StatusCode);
        Assert.Contains("No exchange rate is available", result.Message);
        Assert.Empty(await db.Set<CustInvoiceJour>().ToListAsync());
        Assert.Empty(await db.Set<GeneralJournalEntry>().ToListAsync());
    }

    [Fact]
    public async Task Sales_invoice_translates_a_foreign_currency_order_into_the_accounting_currency()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        SeedSalesMasterData(db);
        SeedRate(db, "USD", "SAR", 3.75m);
        await db.SaveChangesAsync();
        var numbers = new TestInventNumbers();

        // Buy in SAR, sell 10 @ 100 USD + 15% VAT: 1,000 revenue and 150 tax, so the customer owes
        // 1,150 USD. At 3.75 that is 3,750 / 562.50 / 4,312.50 in the accounting currency.
        await StockAndSellTenAsync(db, numbers, salesCurrency: "USD");

        var journal = await db.Set<CustInvoiceJour>().AsNoTracking().SingleAsync();
        Assert.Equal("USD", journal.CurrencyCode);
        Assert.Equal(1150m, journal.InvoiceAmount);
        Assert.Equal(4312.5m, journal.InvoiceAmountMst);
        Assert.Equal(1000m, journal.SalesBalance);
        Assert.Equal(3750m, journal.SalesBalanceMst);
        Assert.Equal(150m, journal.SumTax);
        Assert.Equal(562.5m, journal.SumTaxMst);

        // The customer's debt is held in both currencies.
        var custTrans = await db.Set<CustTrans>().AsNoTracking().SingleAsync(x => x.Invoice == journal.InvoiceId);
        Assert.Equal("USD", custTrans.CurrencyCode);
        Assert.Equal(1150m, custTrans.AmountCur);
        Assert.Equal(4312.5m, custTrans.AmountMst);
        var open = await db.Set<CustTransOpen>().AsNoTracking().SingleAsync(x => x.RefRecId == custTrans.RecId);
        Assert.Equal(1150m, open.AmountCur);
        Assert.Equal(4312.5m, open.AmountMst);

        // The invoice voucher covers the invoice entry and the reversal of the packing slip's
        // physical posting, so both are read together.
        var entryIds = await db.Set<GeneralJournalEntry>().AsNoTracking()
            .Where(x => x.SubledgerVoucher == journal.InvoiceId).Select(x => x.RecId).ToListAsync();
        var lines = await db.Set<GeneralJournalAccountEntry>().AsNoTracking()
            .Where(x => entryIds.Contains(x.GeneralJournalEntry)).ToListAsync();

        // Revenue keeps the order's currency on the transaction amount and is translated for the ledger.
        var revenue = lines.Single(x => x.PostingType == LedgerPostingType.SalesRevenue);
        Assert.Equal("USD", revenue.TransactionCurrencyCode);
        Assert.Equal(-1000m, revenue.TransactionCurrencyAmount);
        Assert.Equal(-3750m, revenue.AccountingCurrencyAmount);
        var customer = lines.Single(x => x.PostingType == LedgerPostingType.CustBalance);
        Assert.Equal(1150m, customer.TransactionCurrencyAmount);
        Assert.Equal(4312.5m, customer.AccountingCurrencyAmount);

        // Cost of goods came from inventory and is already an accounting-currency amount, so it is
        // posted in that currency and is not translated: 10 units at cost 60 is 600 either way.
        var cogs = lines.Single(x => x.PostingType == LedgerPostingType.SalesConsump);
        Assert.Equal("SAR", cogs.TransactionCurrencyCode);
        Assert.Equal(600m, cogs.TransactionCurrencyAmount);
        Assert.Equal(600m, cogs.AccountingCurrencyAmount);

        // The voucher balances in the accounting currency, which is the balance that must hold when
        // its lines are in more than one transaction currency.
        Assert.Equal(0m, lines.Sum(x => x.AccountingCurrencyAmount));

        // Tax is stored in both currencies, so settlement sums accounting amounts.
        var tax = await db.Set<TaxTrans>().AsNoTracking().SingleAsync(x => x.Voucher == journal.InvoiceId);
        Assert.Equal(-150m, tax.TaxAmountCur);
        Assert.Equal(-562.5m, tax.TaxAmount);
    }

    [Fact]
    public async Task A_foreign_currency_invoice_balances_exactly_when_the_rate_rounds()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        SeedSalesMasterData(db);
        // A rate chosen so that translating each line and translating the total disagree: at
        // 0.123456, revenue 1,000 gives 123.46 and tax 150 gives 18.52, summing to 141.98, while
        // the total of 1,150 translates to 141.97. Deriving the customer line from the lines
        // posted is what keeps the voucher balanced.
        SeedRate(db, "USD", "SAR", 0.123456m);
        await db.SaveChangesAsync();
        var numbers = new TestInventNumbers();

        await StockAndSellTenAsync(db, numbers, salesCurrency: "USD");

        var journal = await db.Set<CustInvoiceJour>().AsNoTracking().SingleAsync();
        var entryIds = await db.Set<GeneralJournalEntry>().AsNoTracking()
            .Where(x => x.SubledgerVoucher == journal.InvoiceId).Select(x => x.RecId).ToListAsync();
        var lines = await db.Set<GeneralJournalAccountEntry>().AsNoTracking()
            .Where(x => entryIds.Contains(x.GeneralJournalEntry)).ToListAsync();

        // The point of the test: both ledger currencies balance to the cent, even though the
        // individual lines were rounded independently.
        Assert.Equal(0m, lines.Sum(x => x.AccountingCurrencyAmount));
        Assert.Equal(0m, lines.Sum(x => x.ReportingCurrencyAmount));

        // And the customer's subledger balance is the same figure the ledger received, so the
        // two cannot drift apart.
        var customer = lines.Single(x => x.PostingType == LedgerPostingType.CustBalance);
        var custTrans = await db.Set<CustTrans>().AsNoTracking().SingleAsync(x => x.Invoice == journal.InvoiceId);
        Assert.Equal(customer.AccountingCurrencyAmount, custTrans.AmountMst);
        Assert.Equal(customer.AccountingCurrencyAmount, journal.InvoiceAmountMst);
        var open = await db.Set<CustTransOpen>().AsNoTracking().SingleAsync(x => x.RefRecId == custTrans.RecId);
        Assert.Equal(custTrans.AmountMst, open.AmountMst);

        // The transaction currency still holds the amount actually invoiced.
        Assert.Equal(1150m, custTrans.AmountCur);
    }

    [Fact]
    public async Task A_foreign_currency_credit_note_translates_revenue_and_leaves_cost_in_the_accounting_currency()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        SeedSalesMasterData(db);
        db.Set<ReturnDispositionCode>().Add(new ReturnDispositionCode
        { RecId = 1, DataAreaId = "dat", DispositionCodeId = "CREDIT", DispositionAction = DispositionAction.Credit });
        db.Set<ReturnReasonCode>().Add(new ReturnReasonCode { RecId = 1, DataAreaId = "dat", ReasonCodeId = "DAMAGED" });
        SeedRate(db, "USD", "SAR", 0.123456m);
        await db.SaveChangesAsync();
        var numbers = new TestInventNumbers();

        // Sold 10 @ 100 USD + 15% VAT; return 2 of them, so revenue 200 USD and VAT 30 USD reverse.
        var invoiceId = await StockAndSellTenAsync(db, numbers, salesCurrency: "USD");
        var sold = await db.Set<CustInvoiceTrans>().SingleAsync();
        var returns = new CustReturnService(new TestUnitOfWork(db), new TestCompany(), Sequence(), numbers);
        Assert.Equal(200, (await returns.CreateAsync(new CustReturnCreateDto(invoiceId, "DAMAGED",
            [new(sold.RecId, 2, "CREDIT")]), default)).StatusCode);
        var rma = await db.Set<SalesTable>().SingleAsync(x => x.SalesType == SalesType.ReturnItem);
        Assert.Equal("USD", rma.CurrencyCode);
        var creditLine = await db.Set<SalesLine>().SingleAsync(x => x.SalesId == rma.SalesId);
        Assert.Equal(200, (await returns.ReceiveAsync(rma.RecId,
            new CustReturnReceiptDto("rr-1", new DateTime(2026, 10, 20), [new(creditLine.RecId, 2)]), default)).StatusCode);

        var credit = await returns.CreditAsync(rma.RecId, new CustCreditNoteDto(new DateTime(2026, 10, 21), "cn-1"), default);
        Assert.True(credit.StatusCode == 200, credit.Message);

        var note = await db.Set<CustInvoiceJour>().AsNoTracking().SingleAsync(x => x.InvoiceAmount < 0);
        var entryIds = await db.Set<GeneralJournalEntry>().AsNoTracking()
            .Where(x => x.SubledgerVoucher == note.InvoiceId).Select(x => x.RecId).ToListAsync();
        var lines = await db.Set<GeneralJournalAccountEntry>().AsNoTracking()
            .Where(x => entryIds.Contains(x.GeneralJournalEntry)).ToListAsync();

        // Revenue is credited in the order currency and translated for the ledger.
        var revenue = lines.Single(x => x.PostingType == LedgerPostingType.SalesRevenue);
        Assert.Equal("USD", revenue.TransactionCurrencyCode);
        Assert.Equal(200m, revenue.TransactionCurrencyAmount);

        // The returned goods go back into inventory at their original cost, which is already an
        // accounting-currency amount and is not translated: 2 units at 60 is 120.
        var issue = lines.Single(x => x.PostingType == LedgerPostingType.SalesIssue);
        Assert.Equal("SAR", issue.TransactionCurrencyCode);
        Assert.Equal(120m, issue.AccountingCurrencyAmount);

        // Both ledger currencies balance to the cent despite independent per-line rounding.
        Assert.Equal(0m, lines.Sum(x => x.AccountingCurrencyAmount));
        Assert.Equal(0m, lines.Sum(x => x.ReportingCurrencyAmount));

        // The customer's credit in the subledger is the figure the ledger received.
        var customer = lines.Single(x => x.PostingType == LedgerPostingType.CustBalance);
        var custTrans = await db.Set<CustTrans>().AsNoTracking().SingleAsync(x => x.Voucher == note.InvoiceId);
        Assert.Equal(customer.AccountingCurrencyAmount, custTrans.AmountMst);
        Assert.Equal(customer.AccountingCurrencyAmount, note.InvoiceAmountMst);
        Assert.Equal(-230m, custTrans.AmountCur); // 200 revenue + 30 VAT, credited
    }

    [Fact]
    public async Task Sales_order_partial_delivery_partial_invoice_and_payment_reconcile()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        SeedSalesMasterData(db);
        db.Set<InventModelGroup>().Local.Single().PostPhysicalRevenue = NoYes.Yes;
        db.Set<InventModelGroup>().Local.Single().PostOnHandPhysical = NoYes.Yes;
        db.AddRange(
            new InventPosting { RecId = 13, DataAreaId = "dat", InventAccountType = InventAccountType.SalesPackingSlip, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 1 },
            new InventPosting { RecId = 14, DataAreaId = "dat", InventAccountType = InventAccountType.SalesPackingSlipOffsetAccount, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 2 });
        await db.SaveChangesAsync();
        var work = new TestUnitOfWork(db);
        var company = new TestCompany();
        var numbers = new TestInventNumbers();

        // Stock 100 PCS at 60 through the real purchase cycle.
        var (orders, _, receipts, vendorInvoices) = PurchaseServices(db, numbers);
        await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 10, 1), "S01", "WH-01", null), default);
        var po = await db.Set<PurchTable>().SingleAsync();
        await orders.AddLineAsync(po.RecId, new PurchLineInputDto("ITEM-001", 100, "PCS", 60, 1, null, null, null, null), default);
        var pl = await db.Set<PurchLine>().SingleAsync();
        await receipts.PostAsync(po.RecId, new ProductReceiptPostDto("PR-1", new DateTime(2026, 10, 2), [new(pl.RecId, 100)]), default);
        Assert.Equal(200, (await vendorInvoices.PostAsync(po.RecId, new VendorInvoicePostDto("VI-1", new DateTime(2026, 10, 3), [new(pl.RecId, 100)]), default)).StatusCode);

        // SO-0001: 10 PCS at 100, VAT 15%.
        var order = new SalesTable
        {
            DataAreaId = "dat", SalesId = "SO-0001", CustAccount = "C-001", InvoiceAccount = "C-001", CurrencyCode = "SAR",
            SalesStatus = SalesStatus.Backorder, InventSiteId = "S01", InventLocationId = "WH-01", OrderDate = new DateTime(2026, 10, 5), PaymTerm = "NET30"
        };
        db.Set<PaymTerm>().Add(new PaymTerm { DataAreaId = "dat", PaymTermId = "NET30", PaymMethod = PaymMethod.Net, NumOfDays = 30 });
        db.Set<SalesTable>().Add(order);
        await db.SaveChangesAsync();
        var line = new SalesLine
        {
            DataAreaId = "dat", SalesId = order.SalesId, LineNum = 1, ItemId = "ITEM-001", SalesQty = 10, QtyOrdered = 10,
            RemainSalesPhysical = 10, RemainSalesFinancial = 10, SalesUnit = "PCS", SalesPrice = 100, PriceUnit = 1,
            LineAmount = 1000, SalesStatus = SalesStatus.Backorder, StockedProduct = NoYes.Yes, CurrencyCode = "SAR",
            TaxGroup = "VAT15", TaxItemGroup = "VAT15", ReceiptDateRequested = new DateTime(2026, 10, 10)
        };
        var demand = new SalesInventoryDemandService(work, numbers, new InventDimensionResolver(work, numbers));
        await demand.CreateAsync(order, line, "S01", "WH-01");
        db.Set<SalesLine>().Add(line);
        await db.SaveChangesAsync();

        var reservation = new InventoryReservationService(work, company);
        Assert.Null((await reservation.ReserveAsync(line.InventTransId, 10m, false, default)).Error);
        Assert.Equal(10m, (await db.Set<InventSum>().SingleAsync()).ReservPhysical);

        Assert.Null((await reservation.SetPickedAsync(line.InventTransId, 6m, default)).Error);
        Assert.Null((await reservation.SetPickedAsync(line.InventTransId, 2m, default)).Error); // unpick restores reservation
        Assert.Null((await reservation.SetPickedAsync(line.InventTransId, 6m, default)).Error);
        Assert.Null((await reservation.SetPickedAsync(line.InventTransId, 6m, default)).Error); // target retry is idempotent
        var beforeDelivery = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(100m, beforeDelivery.PhysicalInvent);
        Assert.Equal(6m, beforeDelivery.Picked);
        Assert.Equal(4m, beforeDelivery.ReservPhysical);
        Assert.Equal(90m, beforeDelivery.AvailPhysical);
        Assert.NotNull((await reservation.SetPickedAsync(line.InventTransId, 11m, default)).Error);
        Assert.NotNull((await reservation.SetPickedAsync("another-company-source", 1m, default)).Error);

        var packingSlips = new PackingSlipJournalService(work, company, Sequence());
        var invoices = new SalesInvoiceService(work, company, Sequence());

        // Packing slip 6 -> remain physical 4, remain financial 10; reservation consumed.
        var ps1 = await packingSlips.PostAsync(order.RecId, new PostPackingSlipRequest(new DateTime(2026, 10, 6), "Picked", true, "ps-1",
            [new PackingSlipPostingLineRequest(line.RecId.ToString(), 6)]), default);
        Assert.Equal(200, ps1.StatusCode);
        line = await db.Set<SalesLine>().SingleAsync();
        Assert.Equal(4m, line.RemainSalesPhysical);
        Assert.Equal(10m, line.RemainSalesFinancial);
        var sum = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(94m, sum.PhysicalInvent);
        Assert.Equal(4m, sum.ReservPhysical);
        Assert.Equal(0m, sum.Picked);

        // Invoice 4 of the 6 delivered -> remain financial 6; invoicing beyond delivered is rejected.
        var inv1 = await invoices.PostAsync(order.RecId, new SalesInvoicePostDto(new DateTime(2026, 10, 7), "inv-1", [new(line.RecId, 4)]), default);
        Assert.Equal(200, inv1.StatusCode);
        line = await db.Set<SalesLine>().SingleAsync();
        Assert.Equal(4m, line.RemainSalesPhysical);
        Assert.Equal(6m, line.RemainSalesFinancial);
        var remainingDelivered = await db.Set<InventTrans>().SingleAsync(x => x.StatusIssue == StatusIssue.Deducted);
        Assert.Equal(-2m, remainingDelivered.Qty);
        Assert.Equal(-120m, remainingDelivered.CostAmountPhysical);
        Assert.Equal(-120m, (await db.Set<InventSum>().SingleAsync()).PhysicalValue);
        Assert.Equal(NoYes.Yes, (await db.Set<InventTransPosting>().SingleAsync(x => x.InventTransPostingType == InventTransPostingType.Physical)).IsPosted);
        Assert.Equal(422, (await invoices.PostAsync(order.RecId, new SalesInvoicePostDto(null, null, [new(line.RecId, 3)]), default)).StatusCode);
        Assert.Equal(200, (await invoices.PostAsync(order.RecId, new SalesInvoicePostDto(null, "inv-1", [new(line.RecId, 4)]), default)).StatusCode);
        Assert.Single(await db.Set<CustInvoiceJour>().ToListAsync()); // idempotent retry

        // Change posting accounts between deliveries: each invoice must reverse its own slip.
        (await db.Set<InventPosting>().SingleAsync(x => x.InventAccountType == InventAccountType.SalesPackingSlip)).LedgerDimension = 5;
        (await db.Set<InventPosting>().SingleAsync(x => x.InventAccountType == InventAccountType.SalesPackingSlipOffsetAccount)).LedgerDimension = 6;
        await db.SaveChangesAsync();
        // Packing slip 4, then invoice the remaining 6 delivered.
        Assert.Equal(200, (await packingSlips.PostAsync(order.RecId, new PostPackingSlipRequest(new DateTime(2026, 10, 8), "All", true, "ps-2",
            [new PackingSlipPostingLineRequest(line.RecId.ToString(), 4)]), default)).StatusCode);
        Assert.Equal(200, (await invoices.PostAsync(order.RecId, new SalesInvoicePostDto(new DateTime(2026, 10, 9), "inv-2"), default)).StatusCode);
        var financialPostings = await db.Set<InventTransPosting>()
            .Where(x => x.InventTransPostingType == InventTransPostingType.Financial && x.ItemId == "ITEM-001")
            .ToListAsync();
        var financialSalesOrigin = await db.Set<InventTransOrigin>().SingleAsync(x => x.InventTransId == line.InventTransId);
        var salesPostings = financialPostings.Where(x => x.InventTransOrigin == financialSalesOrigin.RecId).ToList();
        Assert.Equal(2, salesPostings.Count);
        Assert.Equal(2, salesPostings.Select(x => x.Voucher).Distinct().Count());
        Assert.All(salesPostings, x => { Assert.Equal(LedgerPostingType.SalesIssue, x.PostingType); Assert.Equal(LedgerPostingType.SalesConsump, x.PostingTypeOffset); });
        var physicalPostings = await db.Set<InventTransPosting>().Where(x => x.InventTransOrigin == financialSalesOrigin.RecId
            && x.InventTransPostingType == InventTransPostingType.Physical).ToListAsync();
        Assert.Equal(2, physicalPostings.Count);
        Assert.All(physicalPostings, x => Assert.Equal(NoYes.No, x.IsPosted));
        var physicalLedger = (await db.Set<GeneralJournalAccountEntry>().ToListAsync()).Where(x =>
            x.PostingType is LedgerPostingType.SalesPackingSlip or LedgerPostingType.SalesOffsetAccountPackingSlip);
        Assert.All(physicalLedger.GroupBy(x => x.LedgerDimension), group => Assert.Equal(0m, group.Sum(x => x.AccountingCurrencyAmount)));

        line = await db.Set<SalesLine>().SingleAsync();
        Assert.Equal(0m, line.RemainSalesPhysical);
        Assert.Equal(0m, line.RemainSalesFinancial);
        Assert.Equal(SalesStatus.Invoiced, line.SalesStatus);
        Assert.Equal(SalesStatus.Invoiced, (await db.Set<SalesTable>().SingleAsync()).SalesStatus);

        var journals = await db.Set<CustInvoiceJour>().ToListAsync();
        Assert.Equal(1000m, journals.Sum(x => x.SalesBalance));
        Assert.Equal(150m, journals.Sum(x => x.SumTax));
        Assert.Equal(1150m, journals.Sum(x => x.InvoiceAmount));
        Assert.All(journals, x => Assert.Equal(x.InvoiceDate.AddDays(30), x.DueDate));
        Assert.All(await db.Set<CustTrans>().ToListAsync(), x => Assert.Equal(x.TransDate.AddDays(30), x.DueDate));
        Assert.All(await db.Set<CustTransOpen>().ToListAsync(), x => Assert.Equal(x.TransDate.AddDays(30), x.DueDate));
        Assert.Equal(-150m, (await db.Set<TaxTrans>().ToListAsync()).Sum(x => x.TaxAmount));

        sum = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(90m, sum.PhysicalInvent);
        Assert.Equal(90m, sum.PostedQty);
        Assert.Equal(0m, sum.Deducted);
        Assert.Equal(0m, sum.ReservPhysical);
        Assert.Equal(5400m, sum.PostedValue); // 6,000 purchased - 600 COGS (10 x 60)
        var salesOrigin = await db.Set<InventTransOrigin>().SingleAsync(x => x.ReferenceCategory == InventRefType.SalesTable);
        var salesTrans = await db.Set<InventTrans>().Where(x => x.InventTransOrigin == salesOrigin.RecId).ToListAsync();
        Assert.All(salesTrans, x => Assert.Equal(StatusIssue.Sold, x.StatusIssue));
        Assert.Equal(-10m, salesTrans.Sum(x => x.Qty));
        Assert.Equal(-600m, salesTrans.Sum(x => x.CostAmountPosted));

        Assert.Equal(1150m, (await db.Set<CustTransOpen>().ToListAsync()).Sum(x => x.AmountCur));
        Assert.Equal(0m, (await db.Set<GeneralJournalAccountEntry>().ToListAsync()).Sum(x => x.AccountingCurrencyAmount));

        // Customer payment of 1,150 settles both invoices; the open receivable becomes zero.
        var payments = new CustPaymentService(work, company, Sequence());
        Assert.Equal(400, (await payments.PostAsync(new CustPaymentPostDto("C-001", "BANK-1", 1150m,
            RequestId: new string('x', FieldLengths.PaymReference + 1)), default)).StatusCode);
        var otherCustomerInvoice = new CustTrans
        {
            DataAreaId = "dat", AccountNum = "C-OTHER", Invoice = "OTHER-INV", CurrencyCode = "SAR",
            AmountCur = 50m, AmountMst = 50m, ReportingCurrencyAmount = 50m,
            TransDate = new DateTime(2026, 10, 1), DueDate = new DateTime(2026, 10, 31)
        };
        db.Add(otherCustomerInvoice);
        await db.SaveChangesAsync();
        var mislinkedOpen = new CustTransOpen
        {
            DataAreaId = "dat", AccountNum = "C-001", RefRecId = otherCustomerInvoice.RecId,
            AmountCur = 50m, AmountMst = 50m, ReportingCurrencyAmount = 50m,
            TransDate = otherCustomerInvoice.TransDate, DueDate = otherCustomerInvoice.DueDate
        };
        db.Add(mislinkedOpen);
        await db.SaveChangesAsync();
        Assert.Equal(422, (await payments.PostAsync(new CustPaymentPostDto("C-001", "BANK-1", 50m,
            new DateTime(2026, 10, 15), "BANK", "OTHER-INV", "pay-cross-customer"), default)).StatusCode);
        mislinkedOpen = await db.Set<CustTransOpen>().SingleAsync(x => x.RecId == mislinkedOpen.RecId);
        db.Remove(mislinkedOpen);
        otherCustomerInvoice = await db.Set<CustTrans>().SingleAsync(x => x.RecId == otherCustomerInvoice.RecId);
        db.Remove(otherCustomerInvoice);
        await db.SaveChangesAsync();
        var mismatchedOpen = await db.Set<CustTransOpen>().OrderBy(x => x.RecId).FirstAsync();
        mismatchedOpen.AmountMst += 1m;
        await db.SaveChangesAsync();
        Assert.Equal(422, (await payments.PostAsync(new CustPaymentPostDto("C-001", "BANK-1", 1150m,
            new DateTime(2026, 10, 15), "BANK", null, "pay-mismatch"), default)).StatusCode);
        Assert.Empty(await db.Set<BankAccountTrans>().ToListAsync());
        mismatchedOpen = await db.Set<CustTransOpen>().SingleAsync(x => x.RecId == mismatchedOpen.RecId);
        mismatchedOpen.AmountMst -= 1m;
        await db.SaveChangesAsync();
        var paid = await payments.PostAsync(new CustPaymentPostDto("C-001", "BANK-1", 1150m, new DateTime(2026, 10, 15), "BANK", null, "pay-1"), default);
        Assert.Equal(200, paid.StatusCode);
        Assert.Empty(await db.Set<CustTransOpen>().ToListAsync());
        var custTrans = await db.Set<CustTrans>().ToListAsync();
        Assert.Equal(0m, custTrans.Sum(x => x.AmountCur));
        Assert.All(custTrans.Where(x => x.TransType == LedgerTransType.Sales), x => Assert.Equal(x.AmountCur, x.SettleAmountCur));
        Assert.Equal(4, await db.Set<CustSettlement>().CountAsync());
        Assert.Equal(NoYes.Yes, (await db.Set<LedgerJournalTable>().SingleAsync()).Posted);
        Assert.Equal(0m, (await db.Set<GeneralJournalAccountEntry>().ToListAsync()).Sum(x => x.AccountingCurrencyAmount));
        Assert.Equal(200, (await payments.PostAsync(new CustPaymentPostDto("C-001", "BANK-1", 1150m, null, null, null, "pay-1"), default)).StatusCode);
        Assert.Single(await db.Set<LedgerJournalTable>().ToListAsync()); // idempotent retry
        Assert.Equal(1150m, (await db.Set<BankAccountTrans>().SingleAsync()).AmountCur);
        Assert.Equal(422, (await payments.PostAsync(new CustPaymentPostDto("C-001", "BANK-1", 1151m, RequestId: "pay-1"), default)).StatusCode);
        Assert.Equal(422, (await payments.PostAsync(new CustPaymentPostDto("OTHER", "BANK-1", 1150m, RequestId: "pay-1"), default)).StatusCode);
        Assert.Single(await db.Set<BankAccountTrans>().ToListAsync());
        db.Set<BankAccountTrans>().Remove(await db.Set<BankAccountTrans>().SingleAsync());
        await db.SaveChangesAsync();
        Assert.Equal(422, (await payments.PostAsync(new CustPaymentPostDto("C-001", "BANK-1", 1150m,
            RequestId: "pay-1"), default)).StatusCode);
        Assert.Single(await db.Set<LedgerJournalTable>().ToListAsync());
    }

    [Fact]
    public async Task Customer_return_rejects_combined_request_lines_exceeding_invoiced_quantity()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        SeedSalesMasterData(db);
        db.Set<ReturnDispositionCode>().AddRange(
            new ReturnDispositionCode { DataAreaId = "dat", DispositionCodeId = "CREDIT", DispositionAction = DispositionAction.Credit },
            new ReturnDispositionCode { DataAreaId = "dat", DispositionCodeId = "CRONLY", DispositionAction = DispositionAction.CreditOnly });
        await db.SaveChangesAsync();
        var numbers = new TestInventNumbers();
        var invoiceId = await StockAndSellTenAsync(db, numbers);
        var invoiceLine = await db.Set<CustInvoiceTrans>().SingleAsync();
        var summaryBefore = await db.Set<InventSum>().Select(x => x.Ordered).SingleAsync();
        var originCountBefore = await db.Set<InventTransOrigin>().CountAsync();
        var returns = new CustReturnService(new TestUnitOfWork(db), new TestCompany(), Sequence(), numbers);

        var result = await returns.CreateAsync(new CustReturnCreateDto(invoiceId, null,
            [new(invoiceLine.RecId, 6m, "CREDIT"), new(invoiceLine.RecId, 5m, "CRONLY")]), default);

        Assert.Equal(422, result.StatusCode);
        Assert.Contains("exceeds the returnable quantity", result.Message);
        Assert.Empty(await db.Set<SalesTable>().Where(x => x.SalesType == SalesType.ReturnItem).ToListAsync());
        Assert.Equal(originCountBefore, await db.Set<InventTransOrigin>().CountAsync());
        Assert.Equal(summaryBefore, await db.Set<InventSum>().Select(x => x.Ordered).SingleAsync());
    }

    [Fact]
    public async Task Customer_return_request_cannot_exceed_selected_partial_invoice_line()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        SeedSalesMasterData(db);
        db.Set<ReturnDispositionCode>().Add(new ReturnDispositionCode { DataAreaId = "dat", DispositionCodeId = "CRONLY", DispositionAction = DispositionAction.CreditOnly });
        await db.SaveChangesAsync();
        var numbers = new TestInventNumbers();
        var invoiceId = await StockAndSellTenAsync(db, numbers);
        var invoiceLine = await db.Set<CustInvoiceTrans>().SingleAsync();
        invoiceLine.Qty = 5m;
        invoiceLine.InventQty = 5m;
        db.Add(new CustInvoiceTrans { DataAreaId = "dat", InvoiceId = "SECOND-INVOICE", SalesId = invoiceLine.SalesId,
            InventTransId = invoiceLine.InventTransId, ItemId = invoiceLine.ItemId, Qty = 5m, InventQty = 5m });
        await db.SaveChangesAsync();
        var returns = new CustReturnService(new TestUnitOfWork(db), new TestCompany(), Sequence(), numbers);

        var result = await returns.CreateAsync(new CustReturnCreateDto(invoiceId, null,
            [new(invoiceLine.RecId, 3m, "CRONLY"), new(invoiceLine.RecId, 3m, "CRONLY")]), default);

        Assert.Equal(422, result.StatusCode);
        Assert.Contains("selected invoice line", result.Message);
        Assert.Empty(await db.Set<SalesTable>().Where(x => x.SalesType == SalesType.ReturnItem).ToListAsync());
    }

    [Fact]
    public async Task Customer_return_rejects_cumulative_excess_against_selected_partial_invoice()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        SeedSalesMasterData(db);
        db.Add(new ReturnDispositionCode { DataAreaId = "dat", DispositionCodeId = "CRONLY", DispositionAction = DispositionAction.CreditOnly });
        await db.SaveChangesAsync();
        var numbers = new TestInventNumbers();
        var invoiceId = await StockAndSellTenAsync(db, numbers);
        var invoiceLine = await db.Set<CustInvoiceTrans>().SingleAsync();
        invoiceLine.Qty = 5m;
        invoiceLine.InventQty = 5m;
        db.Add(new CustInvoiceTrans { DataAreaId = "dat", InvoiceId = "SECOND-INVOICE", SalesId = invoiceLine.SalesId,
            InventTransId = invoiceLine.InventTransId, ItemId = invoiceLine.ItemId, Qty = 5m, InventQty = 5m });
        await db.SaveChangesAsync();
        var returns = new CustReturnService(new TestUnitOfWork(db), new TestCompany(), Sequence(), numbers);

        Assert.Equal(200, (await returns.CreateAsync(new CustReturnCreateDto(invoiceId, null,
            [new(invoiceLine.RecId, 4m, "CRONLY")]), default)).StatusCode);
        var second = await returns.CreateAsync(new CustReturnCreateDto(invoiceId, null,
            [new(invoiceLine.RecId, 2m, "CRONLY")]), default);
        Assert.Equal(422, second.StatusCode);
        Assert.Contains("remaining quantity", second.Message);
        Assert.Equal(-4m, (await db.Set<SalesLine>().Where(x => x.SalesType == SalesType.ReturnItem).ToListAsync()).Sum(x => x.SalesQty));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Customer_return_keeps_attributed_partial_invoice_entitlements_separate(bool sameRequest)
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        SeedSalesMasterData(db);
        db.Add(new ReturnDispositionCode { DataAreaId = "dat", DispositionCodeId = "CRONLY", DispositionAction = DispositionAction.CreditOnly });
        await db.SaveChangesAsync();
        var numbers = new TestInventNumbers();
        var invoiceId = await StockAndSellTenAsync(db, numbers);
        var first = await db.Set<CustInvoiceTrans>().SingleAsync();
        first.Qty = first.InventQty = 5m;
        var secondInvoiceId = sameRequest ? invoiceId : "SECOND-INVOICE";
        if (!sameRequest)
            db.Add(new CustInvoiceJour { DataAreaId = "dat", InvoiceId = secondInvoiceId, SalesId = first.SalesId });
        var second = new CustInvoiceTrans { DataAreaId = "dat", InvoiceId = secondInvoiceId, SalesId = first.SalesId,
            InventTransId = first.InventTransId, ItemId = first.ItemId, Qty = 5m, InventQty = 5m };
        db.Add(second);
        await db.SaveChangesAsync();
        var service = new CustReturnService(new TestUnitOfWork(db), new TestCompany(), Sequence(), numbers);

        if (sameRequest)
            Assert.Equal(200, (await service.CreateAsync(new CustReturnCreateDto(invoiceId, null,
                [new(first.RecId, 4m, "CRONLY"), new(second.RecId, 4m, "CRONLY")]), default)).StatusCode);
        else
        {
            Assert.Equal(200, (await service.CreateAsync(new CustReturnCreateDto(invoiceId, null,
                [new(first.RecId, 4m, "CRONLY")]), default)).StatusCode);
            Assert.Equal(200, (await service.CreateAsync(new CustReturnCreateDto(secondInvoiceId, null,
                [new(second.RecId, 4m, "CRONLY")]), default)).StatusCode);
        }
        var returnLines = await db.Set<SalesLine>().Where(x => x.SalesType == SalesType.ReturnItem).ToListAsync();
        Assert.Equal(2, returnLines.Count);
        Assert.Equal(-4m, Assert.Single(returnLines, x => x.OriginalInvoiceTransRecId == first.RecId).SalesQty);
        Assert.Equal(-4m, Assert.Single(returnLines, x => x.OriginalInvoiceTransRecId == second.RecId).SalesQty);
        Assert.Equal(422, (await service.CreateAsync(new CustReturnCreateDto(secondInvoiceId, null,
            [new(second.RecId, 2m, "CRONLY")]), default)).StatusCode);
    }

    [Fact]
    public async Task Customer_return_with_unattributed_legacy_line_retains_conservative_limit()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        SeedSalesMasterData(db);
        db.Add(new ReturnDispositionCode { DataAreaId = "dat", DispositionCodeId = "CRONLY", DispositionAction = DispositionAction.CreditOnly });
        await db.SaveChangesAsync();
        var numbers = new TestInventNumbers();
        var invoiceId = await StockAndSellTenAsync(db, numbers);
        var first = await db.Set<CustInvoiceTrans>().SingleAsync();
        first.Qty = first.InventQty = 5m;
        const string secondInvoiceId = "SECOND-INVOICE";
        db.Add(new CustInvoiceJour { DataAreaId = "dat", InvoiceId = secondInvoiceId, SalesId = first.SalesId });
        var second = new CustInvoiceTrans { DataAreaId = "dat", InvoiceId = secondInvoiceId, SalesId = first.SalesId,
            InventTransId = first.InventTransId, ItemId = first.ItemId, Qty = 5m, InventQty = 5m };
        db.Add(second);
        await db.SaveChangesAsync();
        var service = new CustReturnService(new TestUnitOfWork(db), new TestCompany(), Sequence(), numbers);
        Assert.Equal(200, (await service.CreateAsync(new CustReturnCreateDto(invoiceId, null,
            [new(first.RecId, 4m, "CRONLY")]), default)).StatusCode);
        (await db.Set<SalesLine>().SingleAsync(x => x.SalesType == SalesType.ReturnItem)).OriginalInvoiceTransRecId = null;
        await db.SaveChangesAsync();

        var rejected = await service.CreateAsync(new CustReturnCreateDto(secondInvoiceId, null,
            [new(second.RecId, 2m, "CRONLY")]), default);
        Assert.Equal(422, rejected.StatusCode);
        Assert.Single(await db.Set<SalesLine>().Where(x => x.SalesType == SalesType.ReturnItem).ToListAsync());
    }

    [Fact]
    public async Task Customer_return_costs_from_the_selected_invoice_across_partial_invoices()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        SeedSalesMasterData(db);
        db.Set<ReturnDispositionCode>().Add(new ReturnDispositionCode { DataAreaId = "dat", DispositionCodeId = "CREDIT", DispositionAction = DispositionAction.Credit });
        await db.SaveChangesAsync();
        var numbers = new TestInventNumbers();
        var invoiceId = await StockAndSellTenAsync(db, numbers);
        var invoiceLine = await db.Set<CustInvoiceTrans>().SingleAsync();
        var origin = await db.Set<InventTransOrigin>().SingleAsync(x => x.InventTransId == invoiceLine.InventTransId);
        var issue = await db.Set<InventTrans>().SingleAsync(x => x.InventTransOrigin == origin.RecId && x.StatusIssue == StatusIssue.Sold);
        issue.Qty = -5m;
        issue.CostAmountPosted = -300m;
        db.Add(new InventTrans { DataAreaId = "dat", InventTransOrigin = origin.RecId, ItemId = issue.ItemId,
            InventDimId = issue.InventDimId, Qty = -5m, StatusIssue = StatusIssue.Sold, CostAmountPosted = -350m,
            InvoiceId = "SECOND-INVOICE", Voucher = "SECOND-INVOICE", DateFinancial = issue.DateFinancial, ValueOpen = InventTransOpen.Yes });
        await db.SaveChangesAsync();
        var returns = new CustReturnService(new TestUnitOfWork(db), new TestCompany(), Sequence(), numbers);
        var result = await returns.CreateAsync(new CustReturnCreateDto(invoiceId, null,
            [new(invoiceLine.RecId, 1, "CREDIT")]), default);

        // The return records the invoice line it came from, so the 60.00 cost of the selected
        // invoice is used instead of refusing the differing 70.00 cost of the other invoice.
        Assert.Equal(200, result.StatusCode);
        var returnLine = await db.Set<SalesLine>().SingleAsync(x => x.SalesType == SalesType.ReturnItem);
        Assert.Equal(invoiceLine.RecId, returnLine.OriginalInvoiceTransRecId);

        Assert.Equal(-1m, returnLine.SalesQty);
    }

    [Fact]
    public async Task Customer_return_credits_received_goods_at_original_cost_and_credit_only_without_stock()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        SeedSalesMasterData(db);
        db.Set<ReturnDispositionCode>().AddRange(
            new ReturnDispositionCode { RecId = 1, DataAreaId = "dat", DispositionCodeId = "CREDIT", DispositionAction = DispositionAction.Credit },
            new ReturnDispositionCode { RecId = 2, DataAreaId = "dat", DispositionCodeId = "CRONLY", DispositionAction = DispositionAction.CreditOnly },
            new ReturnDispositionCode { RecId = 3, DataAreaId = "dat", DispositionCodeId = "SCRAP", DispositionAction = DispositionAction.Scrap });
        db.Set<ReturnReasonCode>().Add(new ReturnReasonCode { RecId = 1, DataAreaId = "dat", ReasonCodeId = "DAMAGED" });
        await db.SaveChangesAsync();
        var numbers = new TestInventNumbers();
        var invoiceId = await StockAndSellTenAsync(db, numbers); // 100 @ 60 bought, 10 @ 100 + 15% VAT sold
        var sold = await db.Set<CustInvoiceTrans>().SingleAsync();
        var returns = new CustReturnService(new TestUnitOfWork(db), new TestCompany(), Sequence(), numbers);

        // Unsupported disposition and over-return are rejected.
        Assert.Equal(422, (await returns.CreateAsync(new CustReturnCreateDto(invoiceId, "DAMAGED", [new(sold.RecId, 1, "SCRAP")]), default)).StatusCode);
        var created = await returns.CreateAsync(new CustReturnCreateDto(invoiceId, "DAMAGED",
            [new(sold.RecId, 2, "CREDIT"), new(sold.RecId, 1, "CRONLY")]), default);
        Assert.Equal(200, created.StatusCode);
        Assert.Equal(422, (await returns.CreateAsync(new CustReturnCreateDto(invoiceId, null, [new(sold.RecId, 8, "CREDIT")]), default)).StatusCode);

        var rma = await db.Set<SalesTable>().SingleAsync(x => x.SalesType == SalesType.ReturnItem);
        Assert.False(string.IsNullOrEmpty(rma.ReturnItemNum));
        var creditLine = await db.Set<SalesLine>().SingleAsync(x => x.SalesId == rma.SalesId && x.ReturnDispositionCodeId == "CREDIT");
        var creditOnlyLine = await db.Set<SalesLine>().SingleAsync(x => x.SalesId == rma.SalesId && x.ReturnDispositionCodeId == "CRONLY");
        Assert.Equal(-2m, creditLine.SalesQty);
        Assert.Equal(sold.InventTransId, creditLine.InventTransIdReturn);
        Assert.Equal(2m, (await db.Set<InventSum>().SingleAsync()).Ordered); // expected return receipt only for the Credit line

        // Receive the 2 credited units back at the original cost of 60; credit-only lines are not received.
        Assert.Equal(422, (await returns.ReceiveAsync(rma.RecId, new CustReturnReceiptDto("rr-x", null, [new(creditOnlyLine.RecId, 1)]), default)).StatusCode);
        Assert.Equal(200, (await returns.ReceiveAsync(rma.RecId, new CustReturnReceiptDto("rr-1", new DateTime(2026, 10, 20), [new(creditLine.RecId, 2)]), default)).StatusCode);
        var sum = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(92m, sum.PhysicalInvent);
        Assert.Equal(2m, sum.Received);
        Assert.Equal(120m, sum.PhysicalValue);

        // Credit note: revenue 300 + VAT 45 reversed; received goods financially restored at 120.
        rma = await db.Set<SalesTable>().SingleAsync(x => x.RecId == rma.RecId);
        rma.CurrencyCode = "USD";
        await db.SaveChangesAsync();
        var rejectedCredit = await returns.CreditAsync(rma.RecId,
            new CustCreditNoteDto(new DateTime(2026, 10, 21), "cn-fx"), default);
        Assert.Equal(422, rejectedCredit.StatusCode);
        Assert.Empty(await db.Set<CustInvoiceJour>().Where(x => x.SalesId == rma.SalesId).ToListAsync());
        rma = await db.Set<SalesTable>().SingleAsync(x => x.RecId == rma.RecId);
        rma.CurrencyCode = "SAR";
        var creditLedger = await db.Set<Ledger>().SingleAsync();
        creditLedger.ReportingCurrency = "USD";
        await db.SaveChangesAsync();
        var rejectedReporting = await returns.CreditAsync(rma.RecId,
            new CustCreditNoteDto(new DateTime(2026, 10, 21), "cn-reporting"), default);
        Assert.Equal(422, rejectedReporting.StatusCode);
        Assert.Empty(await db.Set<CustInvoiceJour>().Where(x => x.SalesId == rma.SalesId).ToListAsync());
        rma = await db.Set<SalesTable>().SingleAsync(x => x.RecId == rma.RecId);
        creditLedger = await db.Set<Ledger>().SingleAsync();
        creditLedger.ReportingCurrency = "SAR";
        await db.SaveChangesAsync();
        var credit = await returns.CreditAsync(rma.RecId, new CustCreditNoteDto(new DateTime(2026, 10, 21), "cn-1"), default);
        Assert.Equal(200, credit.StatusCode);
        var note = await db.Set<CustInvoiceJour>().SingleAsync(x => x.SalesId == rma.SalesId);
        var returnOrigin = await db.Set<InventTransOrigin>().SingleAsync(x => x.InventTransId == creditLine.InventTransId);
        var returnPosting = await db.Set<InventTransPosting>().SingleAsync(x => x.InventTransOrigin == returnOrigin.RecId
            && x.InventTransPostingType == InventTransPostingType.Financial && x.Voucher == note.InvoiceId);
        Assert.Equal(LedgerPostingType.SalesIssue, returnPosting.PostingType);
        Assert.Equal(LedgerPostingType.SalesConsump, returnPosting.PostingTypeOffset);
        Assert.Equal(-300m, note.SalesBalance);
        Assert.Equal(-45m, note.SumTax);
        Assert.Equal(-345m, note.InvoiceAmount);
        sum = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(92m, sum.PhysicalInvent);
        Assert.Equal(92m, sum.PostedQty);
        Assert.Equal(0m, sum.Received);
        Assert.Equal(0m, sum.PhysicalValue);
        Assert.Equal(5520m, sum.PostedValue); // 6,000 - 600 sold + 120 returned
        rma = await db.Set<SalesTable>().SingleAsync(x => x.RecId == rma.RecId);
        Assert.Equal(ReturnStatusHeader.Closed, rma.ReturnStatus);
        Assert.All(await db.Set<SalesLine>().Where(x => x.SalesId == rma.SalesId).ToListAsync(),
            x => Assert.Equal(ReturnStatusLine.Invoiced, x.ReturnStatus));
        Assert.Equal(1150m - 345m, (await db.Set<CustTransOpen>().ToListAsync()).Sum(x => x.AmountCur)); // open balance after credit
        Assert.Equal(0m, (await db.Set<GeneralJournalAccountEntry>().ToListAsync()).Sum(x => x.AccountingCurrencyAmount));
        Assert.Equal(200, (await returns.CreditAsync(rma.RecId, new CustCreditNoteDto(null, "cn-1"), default)).StatusCode);
        Assert.Equal(2, await db.Set<CustInvoiceJour>().CountAsync()); // retry does not post again
    }

    /// <summary>
    /// The case the order-to-cash cycle had no ending for. Order 10, deliver 6, invoice nothing,
    /// then cancel: the 4 that will never ship is released along with its reservation, and the 6
    /// already delivered stays invoiceable. Cancelling demand touches no physical stock and posts
    /// no ledger entry.
    /// </summary>
    [Fact]
    public async Task Cancelling_a_partly_delivered_order_releases_the_remainder_and_keeps_the_delivery_invoiceable()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (work, company, numbers) = (new TestUnitOfWork(db), new TestCompany(), new TestInventNumbers());
        var (order, line, demand) = await PartlyDeliveredSalesOrderAsync(db, work, company, numbers);

        var beforeStock = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(4m, beforeStock.ReservPhysical); // the undelivered 4 are still reserved
        var ledgerCount = await db.Set<GeneralJournalAccountEntry>().CountAsync();

        var cancelled = await new SalesOrderCancellationService(work, demand, company).CancelAsync(order.RecId, default);

        Assert.Equal(SalesOrderCancelStatus.Cancelled, cancelled.Status);
        line = await db.Set<SalesLine>().SingleAsync();
        var sum = await db.Set<InventSum>().SingleAsync();
        // The reservation and the ordered demand behind the undelivered 4 are both released.
        Assert.Equal(0m, sum.ReservPhysical);
        Assert.Equal(0m, sum.OnOrder);
        Assert.Equal(0m, line.RemainSalesPhysical);
        // The delivery is still waiting to be invoiced, which is the whole point.
        Assert.Equal(6m, line.RemainSalesFinancial);
        Assert.Equal(SalesStatus.Delivered, line.SalesStatus);
        // Physical stock and the ledger are untouched: 94 left after shipping 6 of 100.
        Assert.Equal(94m, sum.PhysicalInvent);
        Assert.Equal(ledgerCount, await db.Set<GeneralJournalAccountEntry>().CountAsync());

        // And the delivered 6 still invoice afterwards, closing the order properly.
        var invoiced = await new SalesInvoiceService(work, company, Sequence())
            .PostAsync(order.RecId, new SalesInvoicePostDto(new DateTime(2026, 10, 8), "inv-cancel", [new(line.RecId, 6)]), default);
        Assert.Equal(200, invoiced.StatusCode);
        line = await db.Set<SalesLine>().SingleAsync();
        Assert.Equal(0m, line.RemainSalesFinancial);
        Assert.Equal(SalesStatus.Invoiced, (await db.Set<SalesTable>().SingleAsync(x => x.SalesId == order.SalesId)).SalesStatus);
    }

    /// <summary>
    /// Picking is a physical registration, so cancellation refuses it by name and leaves it to the
    /// existing unpick path rather than silently reversing it. After unpicking, the same
    /// cancellation succeeds.
    /// </summary>
    [Fact]
    public async Task Cancelling_an_order_with_a_picked_quantity_is_refused_until_it_is_unpicked()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (work, company, numbers) = (new TestUnitOfWork(db), new TestCompany(), new TestInventNumbers());
        var (order, line, demand) = await PartlyDeliveredSalesOrderAsync(db, work, company, numbers);
        var reservation = new InventoryReservationService(work, company);
        // Pick 3 of the 4 still undelivered.
        Assert.Null((await reservation.SetPickedAsync(line.InventTransId, 3m, default)).Error);

        var refused = await new SalesOrderCancellationService(work, demand, company).CancelAsync(order.RecId, default);

        Assert.Equal(SalesOrderCancelStatus.Refused, refused.Status);
        Assert.Contains("must be unpicked", refused.Message);
        // Rolled back: the line keeps its remainder and the pick is still there.
        line = await db.Set<SalesLine>().SingleAsync();
        Assert.Equal(4m, line.RemainSalesPhysical);
        Assert.Equal(3m, (await db.Set<InventSum>().SingleAsync()).Picked);

        // Unpick through the documented path, and the cancellation then goes through.
        Assert.Null((await reservation.SetPickedAsync(line.InventTransId, 0m, default)).Error);
        var cancelled = await new SalesOrderCancellationService(work, demand, company).CancelAsync(order.RecId, default);
        Assert.Equal(SalesOrderCancelStatus.Cancelled, cancelled.Status);
        line = await db.Set<SalesLine>().SingleAsync();
        Assert.Equal(0m, line.RemainSalesPhysical);
        Assert.Equal(6m, line.RemainSalesFinancial);
        Assert.Equal(0m, (await db.Set<InventSum>().SingleAsync()).Picked);
    }

    /// <summary>
    /// A fully delivered and invoiced order has no remainder, so cancellation is refused by name
    /// rather than quietly doing nothing.
    /// </summary>
    [Fact]
    public async Task Cancelling_a_fully_delivered_order_is_refused()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (work, company, numbers) = (new TestUnitOfWork(db), new TestCompany(), new TestInventNumbers());
        var (order, line, demand) = await PartlyDeliveredSalesOrderAsync(db, work, company, numbers);
        // Deliver and invoice everything.
        Assert.Equal(200, (await new PackingSlipJournalService(work, company, Sequence()).PostAsync(order.RecId,
            new PostPackingSlipRequest(new DateTime(2026, 10, 8), "All", true, "ps-rest",
                [new PackingSlipPostingLineRequest(line.RecId.ToString(), 4)]), default)).StatusCode);
        Assert.Equal(200, (await new SalesInvoiceService(work, company, Sequence()).PostAsync(order.RecId,
            new SalesInvoicePostDto(new DateTime(2026, 10, 9), "inv-all", [new(line.RecId, 10)]), default)).StatusCode);

        var refused = await new SalesOrderCancellationService(work, demand, company).CancelAsync(order.RecId, default);

        Assert.Equal(SalesOrderCancelStatus.NothingToCancel, refused.Status);
        Assert.Contains("no undelivered quantity left to cancel", refused.Message);
    }

    /// <summary>
    /// Order 10, deliver 6, invoice 6, then cancel the rest: nothing is left owing, so the line and
    /// the order both reach Invoiced rather than Canceled - part of the order really happened.
    /// </summary>
    [Fact]
    public async Task Cancelling_the_rest_of_a_fully_invoiced_delivery_closes_the_order_as_invoiced()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (work, company, numbers) = (new TestUnitOfWork(db), new TestCompany(), new TestInventNumbers());
        var (order, line, demand) = await PartlyDeliveredSalesOrderAsync(db, work, company, numbers);
        Assert.Equal(200, (await new SalesInvoiceService(work, company, Sequence()).PostAsync(order.RecId,
            new SalesInvoicePostDto(new DateTime(2026, 10, 7), "inv-6", [new(line.RecId, 6)]), default)).StatusCode);

        var cancelled = await new SalesOrderCancellationService(work, demand, company).CancelAsync(order.RecId, default);

        Assert.Equal(SalesOrderCancelStatus.Cancelled, cancelled.Status);
        line = await db.Set<SalesLine>().SingleAsync();
        Assert.Equal(0m, line.RemainSalesPhysical);
        Assert.Equal(0m, line.RemainSalesFinancial);
        Assert.Equal(SalesStatus.Invoiced, line.SalesStatus);
        Assert.Equal(SalesStatus.Invoiced, (await db.Set<SalesTable>().SingleAsync(x => x.SalesId == order.SalesId)).SalesStatus);
    }

    [Fact]
    public async Task Cancellation_releases_split_reserved_and_ordered_demand_without_stock_or_ledger_changes()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        SeedSalesMasterData(db);
        await db.SaveChangesAsync();
        var numbers = new TestInventNumbers();
        await StockAndSellTenAsync(db, numbers);
        var work = new TestUnitOfWork(db);
        var company = new TestCompany();
        var order = new SalesTable { DataAreaId = "dat", SalesId = "SO-CANCEL", CustAccount = "C-001", SalesStatus = SalesStatus.Backorder };
        db.Add(order);
        var line = new SalesLine { DataAreaId = "dat", SalesId = order.SalesId, ItemId = "ITEM-001", SalesQty = 10, QtyOrdered = 10,
            RemainSalesPhysical = 10, RemainSalesFinancial = 10, SalesStatus = SalesStatus.Backorder, StockedProduct = NoYes.Yes };
        var demand = new SalesInventoryDemandService(work, numbers, new InventDimensionResolver(work, numbers));
        await demand.CreateAsync(order, line, "S01", "WH-01");
        db.Add(line);
        await db.SaveChangesAsync();
        var reservation = new InventoryReservationService(work, company);
        Assert.Null((await reservation.ReserveAsync(line.InventTransId, 10, false, default)).Error);
        Assert.Null((await reservation.ReleaseAsync(line.InventTransId, 4, default)).Error);
        var ledgerCount = await db.Set<GeneralJournalAccountEntry>().CountAsync();
        Assert.Equal(SalesOrderCancelStatus.Cancelled, (await new SalesOrderCancellationService(work, demand, company).CancelAsync(order.RecId, default)).Status);
        var sum = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(0m, sum.ReservPhysical);
        Assert.Equal(0m, sum.OnOrder);
        Assert.Equal(90m, sum.PhysicalInvent);
        Assert.Equal(5400m, sum.PostedValue);
        Assert.Equal(0m, line.RemainSalesPhysical);
        Assert.Equal(ledgerCount, await db.Set<GeneralJournalAccountEntry>().CountAsync());
    }
}
