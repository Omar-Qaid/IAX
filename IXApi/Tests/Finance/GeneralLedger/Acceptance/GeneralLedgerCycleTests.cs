using System.Reflection;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Invoice;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip.Dtos;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Payment;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PriceDisc;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Return;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Services;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.GeneralLedger.FiscalCalendar;
using IAX.IXApi.Modules.Finance.GeneralLedger.JournalEntry;
using IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable;
using IAX.IXApi.Modules.Finance.Inventory;
using IAX.IXApi.Shared.Application.Identity;
using IAX.IXApi.Shared.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using JournalHeader = IAX.IXApi.Modules.Finance.Entities.LedgerJournalTable;

namespace IAX.IXApi.Tests;

/// <summary>
/// The general ledger reference cycle from <c>CycleErpExamples/GeneralLedger/Logic_Readonly.md</c>,
/// executed end to end through the real purchase, sales, payment, settlement, costing and journal
/// services, then checked against the figures that document publishes.
///
/// Before this existed the reference's fifteen steps, its <b>6,300</b> trial balance and the
/// <b>1,200</b> it carries to retained earnings were asserted by nothing, which is why every UAT id
/// in its §6 read <c>Not run</c>.
/// </summary>
public sealed class GeneralLedgerCycleTests
{
    // Account ids and ledger dimensions used across the cycle. The reference names accounts by
    // role, not by number, so these follow the fixture while the roles follow the reference.
    private const long Inventory = 1;       // 130100 asset
    private const long CostOfSales = 2;     // 510100 expense
    private const long Vendors = 3;         // 210100 liability
    private const long GoodsRevenue = 5;    // 410100 revenue
    private const long Receivables = 6;     // 120100 asset
    private const long Bank = 7;            // 110100 asset
    private const long ServiceRevenue = 20; // 410200 revenue
    private const long Expense = 21;        // 600100 expense
    private const long AccruedExpenses = 22;// 210200 liability
    private const long RetainedEarnings = 23;// 300100 equity

    /// <summary>
    /// §2 of the reference, steps 1-15: buy 100 at 50, sell 30 at 90 plus a 300 service, deliver and
    /// invoice in two parts, collect 2,000 then 1,000, pay the vendor 2,000, close inventory cost and
    /// accrue a 300 expense. Then §49's trial balance - <b>6,300</b> each side with a net profit of
    /// <b>1,200</b> - and §5's year-end close, which carries that 1,200 to retained earnings and
    /// opens the new year balanced at <b>4,500</b>.
    /// </summary>
    [Fact]
    public async Task General_ledger_reference_cycle_balances_at_six_thousand_three_hundred_and_carries_twelve_hundred_to_equity()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        Seed(db);
        await db.SaveChangesAsync();

        var work = new TestUnitOfWork(db);
        var company = new TestCompany();
        var user = new TestUser();
        var numbers = new TestNumbers();
        var supply = new PurchInventorySupplyService(work, numbers, new InventDimensionResolver(work, numbers));
        var orders = new PurchOrderService(work, company, Sequence(), supply, new SalesUnitConversionService(work));
        var confirmations = new PurchConfirmationService(work, company, Sequence());
        var receipts = new ProductReceiptService(work, company, Sequence());
        var vendorInvoices = new VendorInvoiceService(work, company, Sequence());

        // Steps 1-3: confirm the purchase of 100 at 50, receive all of it, then invoice it.
        // Receipt posts the temporary pair; the invoice reverses it and books inventory 5,000
        // against accounts payable 5,000.
        Assert.Equal(200, (await orders.CreateAsync(
            new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 2, 1), "S01", "WH-01", null), default)).StatusCode);
        var po = await db.Set<PurchTable>().SingleAsync();
        Assert.Equal(200, (await orders.AddLineAsync(po.RecId,
            new PurchLineInputDto("ITEM-001", 100, "PCS", 50, 1, null, null, null, null), default)).StatusCode);
        Assert.Equal(200, (await confirmations.ConfirmAsync(po.RecId, default)).StatusCode);
        var poLine = await db.Set<PurchLine>().SingleAsync();
        Assert.Equal(200, (await receipts.PostAsync(po.RecId,
            new ProductReceiptPostDto("PR-001", new DateTime(2026, 2, 2), [new(poLine.RecId, 100)]), default)).StatusCode);
        Assert.Equal(100m, (await db.Set<InventSum>().SingleAsync()).PhysicalInvent);
        Assert.Equal(200, (await vendorInvoices.PostAsync(po.RecId,
            new VendorInvoicePostDto("VI-001", new DateTime(2026, 2, 3), [new(poLine.RecId, 100)]), default)).StatusCode);
        var stock = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(100m, stock.PhysicalInvent);
        Assert.Equal(5000m, stock.PostedValue);

        // Step 4: one sales order carrying the stocked item (30 at 90) and the 300 service.
        var order = new SalesTable
        {
            DataAreaId = "dat", SalesId = "SO-001", CustAccount = "C-001", CurrencyCode = "SAR",
            SalesStatus = SalesStatus.Backorder, InventSiteId = "S01", InventLocationId = "WH-01",
            OrderDate = new DateTime(2026, 2, 4), PaymTerm = "NET30"
        };
        db.Set<SalesTable>().Add(order);
        await db.SaveChangesAsync();
        var itemLine = new SalesLine
        {
            DataAreaId = "dat", SalesId = order.SalesId, LineNum = 1, ItemId = "ITEM-001", SalesQty = 30, QtyOrdered = 30,
            RemainSalesPhysical = 30, RemainSalesFinancial = 30, SalesUnit = "PCS", SalesPrice = 90, PriceUnit = 1,
            LineAmount = 2700, SalesStatus = SalesStatus.Backorder, StockedProduct = NoYes.Yes, CurrencyCode = "SAR",
            ReceiptDateRequested = new DateTime(2026, 2, 5)
        };
        await new SalesInventoryDemandService(work, numbers, new InventDimensionResolver(work, numbers))
            .CreateAsync(order, itemLine, "S01", "WH-01");
        db.Set<SalesLine>().Add(itemLine);
        var serviceLine = new SalesLine
        {
            DataAreaId = "dat", SalesId = order.SalesId, LineNum = 2, ItemId = "SRV-01", SalesQty = 1, QtyOrdered = 1,
            RemainSalesPhysical = 1, RemainSalesFinancial = 1, SalesUnit = "PCS", SalesPrice = 300, PriceUnit = 1,
            LineAmount = 300, SalesStatus = SalesStatus.Backorder, StockedProduct = NoYes.No, CurrencyCode = "SAR",
            ReceiptDateRequested = new DateTime(2026, 2, 5)
        };
        db.Set<SalesLine>().Add(serviceLine);
        await db.SaveChangesAsync();

        // Step 5: reserve the 30. On hand stays 100, available drops to 70.
        Assert.Null((await new InventoryReservationService(work, company)
            .ReserveAsync(itemLine.InventTransId, 30m, false, default)).Error);
        stock = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(100m, stock.PhysicalInvent);
        Assert.Equal(70m, stock.AvailPhysical);

        var packingSlips = new PackingSlipJournalService(work, company, Sequence());
        var salesInvoices = new SalesInvoiceService(work, company, Sequence());

        // Steps 6-7: deliver 20 (on hand 80), then invoice exactly those 20 - customer 1,800,
        // goods revenue 1,800, cost of sales 1,000.
        Assert.Equal(200, (await packingSlips.PostAsync(order.RecId, new PostPackingSlipRequest(
            new DateTime(2026, 2, 6), "All", true, "PS-001",
            [new PackingSlipPostingLineRequest(itemLine.RecId.ToString(), 20)]), default)).StatusCode);
        Assert.Equal(80m, (await db.Set<InventSum>().SingleAsync()).PhysicalInvent);
        Assert.Equal(200, (await salesInvoices.PostAsync(order.RecId,
            new SalesInvoicePostDto(new DateTime(2026, 2, 7), "SI-001", [new(itemLine.RecId, 20)]), default)).StatusCode);

        // Steps 8-9: deliver the last 10 (on hand 70), then invoice them with the service -
        // customer 1,200, goods revenue 900, service revenue 300, cost of sales 500.
        Assert.Equal(200, (await packingSlips.PostAsync(order.RecId, new PostPackingSlipRequest(
            new DateTime(2026, 2, 8), "All", true, "PS-002",
            [new PackingSlipPostingLineRequest(itemLine.RecId.ToString(), 10)]), default)).StatusCode);
        Assert.Equal(70m, (await db.Set<InventSum>().SingleAsync()).PhysicalInvent);
        var lastInvoice = await salesInvoices.PostAsync(order.RecId, new SalesInvoicePostDto(
            new DateTime(2026, 2, 9), "SI-002", [new(itemLine.RecId, 10), new(serviceLine.RecId, 1)]), default);
        Assert.True(lastInvoice.StatusCode == 200, lastInvoice.Message);
        // The service never produces an inventory transaction (MD-01, SO-04).
        Assert.DoesNotContain(await db.Set<InventTrans>().ToListAsync(), x => x.ItemId == "SRV-01");
        Assert.Equal(-1500m, (await db.Set<InventTrans>()
            .Where(x => x.StatusIssue == StatusIssue.Sold).ToListAsync()).Sum(x => x.CostAmountPosted));

        // Steps 10-12: collect 2,000, which settles 1,800 against the first invoice and 200 against
        // the second, then collect the remaining 1,000. The customer closes at zero.
        var custPayments = new CustPaymentService(work, user, company, Sequence());
        Assert.Equal(200, (await custPayments.PostAsync(new CustPaymentPostDto("C-001", "BANK-1", 2000m,
            new DateTime(2026, 2, 10), null, null, "RCPT-1"), default)).StatusCode);
        Assert.Equal(200, (await custPayments.PostAsync(new CustPaymentPostDto("C-001", "BANK-1", 1000m,
            new DateTime(2026, 2, 12), null, null, "RCPT-2"), default)).StatusCode);
        Assert.Equal(0m, (await db.Set<CustTransOpen>().ToListAsync()).Sum(x => x.AmountCur));

        // Step 13: pay the vendor 2,000, leaving 3,000 open (AP-03).
        Assert.Equal(200, (await new VendPaymentService(work, user, company, Sequence())
            .PostAsync(new VendPaymentPostDto("V-0001", "BANK-1", 2000m, "PAY-1",
                new DateTime(2026, 2, 13)), default)).StatusCode);
        Assert.Equal(-3000m, (await db.Set<VendTransOpen>().ToListAsync()).Sum(x => x.AmountCur));

        // Step 14: close inventory cost. One purchase price, so the reference expects no cost
        // difference: 70 pieces still at 50 is 3,500, and cost of sales stays 1,500.
        var close = await new InventCloseService(work, company, user, Sequence())
            .CloseItemAsync("ITEM-001", new DateTime(2026, 2, 28));
        Assert.Null(close.Error);
        await db.SaveChangesAsync();
        Assert.Equal(0m, (await db.Set<InventTrans>().ToListAsync()).Sum(x => x.CostAmountAdjustment));

        // Step 15: accrue a 300 expense through a real general journal (GL-01).
        await PostAccrualJournalAsync(db, new DateTime(2026, 2, 28));

        // §49: the trial balance before year-end close. 6,300 each side, and the control accounts -
        // receivables and the temporary receipt/delivery accounts - are flat (REC-01).
        var balances = await PostedBalancesAsync(db, new DateTime(2026, 1, 1), new DateTime(2026, 12, 31));
        Assert.Equal(1000m, balances[Bank]);
        Assert.Equal(3500m, balances[Inventory]);
        Assert.Equal(-3000m, balances[Vendors]);
        Assert.Equal(-300m, balances[AccruedExpenses]);
        Assert.Equal(-2700m, balances[GoodsRevenue]);
        Assert.Equal(-300m, balances[ServiceRevenue]);
        Assert.Equal(1500m, balances[CostOfSales]);
        Assert.Equal(300m, balances[Expense]);
        Assert.Equal(0m, balances.GetValueOrDefault(Receivables));
        Assert.Equal(6300m, balances.Values.Where(x => x > 0).Sum());
        Assert.Equal(-6300m, balances.Values.Where(x => x < 0).Sum());
        Assert.Equal(0m, balances.Values.Sum());

        // Net profit 1,200: revenue 3,000 less cost of sales 1,500 and expense 300.
        var netProfit = -(balances[GoodsRevenue] + balances[ServiceRevenue] + balances[CostOfSales] + balances[Expense]);
        Assert.Equal(1200m, netProfit);

        // The trial balance service agrees and reports itself balanced.
        var (trial, trialError) = await new LedgerTrialBalanceService(work, user)
            .GetAsync(new DateTime(2026, 1, 1), new DateTime(2026, 12, 31), CurrentToOperationsPostingLayer.Current, default);
        Assert.Null(trialError);
        Assert.True(trial!.Balanced,
            $"movement {trial.TotalDebit}/{trial.TotalCredit}, balance {trial.TotalDebitBalance}/{trial.TotalCreditBalance}");
        // The reference's 6,300 is the balance column - each account's net on its own side - not
        // the period's gross movement, which is larger because inventory is debited and relieved.
        Assert.Equal(6300m, trial.TotalDebitBalance);
        Assert.Equal(6300m, trial.TotalCreditBalance);
        Assert.Equal(3500m, trial.Lines.Single(x => x.MainAccountId == "130100").DebitBalance);
        Assert.Equal(3000m, trial.Lines.Single(x => x.MainAccountId == "210100").CreditBalance);

        // §5: year-end close. The 1,200 result goes to retained earnings and the new year opens
        // balanced at 4,500 (CLOSE-02).
        var closeResult = await new LedgerYearEndCloseController(work, user)
            .Close(new LedgerYearEndCloseRequest(1, RetainedEarningsDimension, true, true), default);
        Assert.IsType<OkObjectResult>(closeResult);
        db.ChangeTracker.Clear();

        var opening = await db.Set<GeneralJournalEntry>().AsNoTracking()
            .SingleAsync(x => x.JournalCategory == LedgerJournalCategory.YearEndOpening);
        Assert.Equal(new DateTime(2027, 1, 1), opening.AccountingDate);
        var openingLines = await db.Set<GeneralJournalAccountEntry>().AsNoTracking()
            .Where(x => x.GeneralJournalEntry == opening.RecId).ToListAsync();
        Assert.Equal(0m, openingLines.Sum(x => x.AccountingCurrencyAmount));
        Assert.Equal(1000m, openingLines.Single(x => x.MainAccount == Bank).AccountingCurrencyAmount);
        Assert.Equal(3500m, openingLines.Single(x => x.MainAccount == Inventory).AccountingCurrencyAmount);
        Assert.Equal(-3000m, openingLines.Single(x => x.MainAccount == Vendors).AccountingCurrencyAmount);
        Assert.Equal(-300m, openingLines.Single(x => x.MainAccount == AccruedExpenses).AccountingCurrencyAmount);
        // The whole point of the close: the year's 1,200 result lands on equity.
        Assert.Equal(-1200m, openingLines.Single(x => x.MainAccount == RetainedEarnings).AccountingCurrencyAmount);
        // The opening carries no income-statement account forward.
        Assert.DoesNotContain(openingLines, x => x.MainAccount == GoodsRevenue || x.MainAccount == ServiceRevenue
            || x.MainAccount == CostOfSales || x.MainAccount == Expense);
        Assert.Equal(4500m, openingLines.Where(x => x.AccountingCurrencyAmount > 0).Sum(x => x.AccountingCurrencyAmount));

        // The closing voucher zeroes the income statement inside 2026 and leaves the balance sheet
        // alone, so the closed year still reports its assets.
        var closing = await db.Set<GeneralJournalEntry>().AsNoTracking()
            .SingleAsync(x => x.JournalCategory == LedgerJournalCategory.YearEndClosing);
        var closingLines = await db.Set<GeneralJournalAccountEntry>().AsNoTracking()
            .Where(x => x.GeneralJournalEntry == closing.RecId).ToListAsync();
        Assert.Equal(0m, closingLines.Sum(x => x.AccountingCurrencyAmount));
        Assert.Equal(2700m, closingLines.Single(x => x.MainAccount == GoodsRevenue).AccountingCurrencyAmount);
        Assert.Equal(300m, closingLines.Single(x => x.MainAccount == ServiceRevenue).AccountingCurrencyAmount);
        Assert.Equal(-1500m, closingLines.Single(x => x.MainAccount == CostOfSales).AccountingCurrencyAmount);
        Assert.Equal(-300m, closingLines.Single(x => x.MainAccount == Expense).AccountingCurrencyAmount);
        Assert.Equal(-1200m, closingLines.Single(x => x.MainAccount == RetainedEarnings).AccountingCurrencyAmount);
        Assert.DoesNotContain(closingLines, x => x.MainAccount == Bank || x.MainAccount == Inventory
            || x.MainAccount == Vendors || x.MainAccount == AccruedExpenses);

        // Counting every posted line in 2026, the closing voucher included, the balance sheet is
        // untouched by the close.
        var afterClose = await PostedBalancesAsync(db, new DateTime(2026, 1, 1), new DateTime(2026, 12, 31));
        Assert.Equal(1000m, afterClose[Bank]);
        Assert.Equal(3500m, afterClose[Inventory]);
        Assert.Equal(0m, afterClose[GoodsRevenue]);
        Assert.Equal(0m, afterClose[CostOfSales]);
        Assert.Equal(-1200m, afterClose[RetainedEarnings]);
        Assert.Equal(FiscalPeriodStatus.YearClosed,
            (await db.Set<LedgerFiscalCalendarPeriod>().AsNoTracking().SingleAsync(x => x.RecId == 1)).Status);
    }

    private const long RetainedEarningsDimension = RetainedEarnings;

    /// <summary>Step 15: debit the expense and credit accrued expenses for 300, through the real journal engine.</summary>
    private static async Task PostAccrualJournalAsync(DbContext db, DateTime date)
    {
        db.Set<JournalHeader>().Add(new JournalHeader
        {
            RecId = 500, DataAreaId = "dat", JournalNum = "GJ-001", JournalName = "GEN"
        });
        db.Set<LedgerJournalName>().Add(new LedgerJournalName { RecId = 500, DataAreaId = "dat", JournalName = "GEN" });
        db.Set<LedgerJournalTrans>().Add(new LedgerJournalTrans
        {
            RecId = 500, DataAreaId = "dat", Company = "dat", JournalNum = "GJ-001", LineNum = 1, Voucher = "GJV-001",
            TransDate = date, CurrencyCode = "SAR", AccountType = LedgerJournalACType.Ledger,
            LedgerDimension = Expense, OffsetAccountType = LedgerJournalACType.Ledger,
            OffsetLedgerDimension = AccruedExpenses, AmountCurDebit = 300m,
            Txt = "Accrued expense", OffsetTxt = "Accrued expenses"
        });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var header = await db.Set<JournalHeader>().SingleAsync(x => x.JournalNum == "GJ-001");
        await new LedgerJournalPosting(db).PostAsync("dat", header, default);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
    }

    /// <summary>Signed accounting balance per main account over every posted line in the range.</summary>
    private static async Task<Dictionary<long, decimal>> PostedBalancesAsync(DbContext db, DateTime fromDate, DateTime toDate)
    {
        var rows = await (from line in db.Set<GeneralJournalAccountEntry>().AsNoTracking()
                          join entry in db.Set<GeneralJournalEntry>().AsNoTracking()
                              on line.GeneralJournalEntry equals entry.RecId
                          where entry.AccountingDate >= fromDate && entry.AccountingDate <= toDate
                          select new { line.MainAccount, line.AccountingCurrencyAmount }).ToListAsync();
        return rows.GroupBy(x => x.MainAccount)
            .ToDictionary(x => x.Key, x => x.Sum(y => y.AccountingCurrencyAmount));
    }

    // ---- Seed -------------------------------------------------------------------------------

    private static void Seed(GlCycleContext db)
    {
        // §1: ledger, calendar and the two fiscal years the close moves between.
        db.Set<Ledger>().Add(new Ledger { RecId = 1, DataAreaId = "dat", AccountingCurrency = "SAR", ChartOfAccounts = 1, FiscalCalendar = 1, DefaultExchangeRateType = 1 });
        db.Set<FiscalCalendarYear>().AddRange(
            new FiscalCalendarYear { RecId = 1, FiscalCalendar = 1, StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 12, 31) },
            new FiscalCalendarYear { RecId = 2, FiscalCalendar = 1, StartDate = new DateTime(2027, 1, 1), EndDate = new DateTime(2027, 12, 31) });
        db.Set<FiscalCalendarPeriod>().AddRange(
            new FiscalCalendarPeriod { RecId = 1, FiscalCalendar = 1, FiscalCalendarYear = 1, Type = FiscalPeriodType.Operating, StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 12, 31) },
            new FiscalCalendarPeriod { RecId = 2, FiscalCalendar = 1, FiscalCalendarYear = 2, Type = FiscalPeriodType.Opening, StartDate = new DateTime(2027, 1, 1), EndDate = new DateTime(2027, 12, 31) });
        db.Set<LedgerFiscalCalendarPeriod>().Add(new LedgerFiscalCalendarPeriod { RecId = 1, Ledger = 1, FiscalCalendarPeriod = 1, Status = FiscalPeriodStatus.Open });
        db.Set<LedgerFiscalCalendarYear>().Add(new LedgerFiscalCalendarYear { RecId = 1, Ledger = 1, FiscalCalendarYear = 1, Status = FiscalPeriodStatus.Open });

        // §1: the chart of accounts. Types matter because the year-end close classifies by them.
        Account(db, Inventory, "130100", MainAccountType.Asset);
        Account(db, CostOfSales, "510100", MainAccountType.Expense);
        Account(db, Vendors, "210100", MainAccountType.Liability);
        Account(db, GoodsRevenue, "410100", MainAccountType.Revenue);
        Account(db, Receivables, "120100", MainAccountType.Asset);
        Account(db, Bank, "110100", MainAccountType.Asset);
        Account(db, ServiceRevenue, "410200", MainAccountType.Revenue);
        Account(db, Expense, "600100", MainAccountType.Expense);
        Account(db, AccruedExpenses, "210200", MainAccountType.Liability);
        Account(db, RetainedEarnings, "300100", MainAccountType.Equity);
        Account(db, 90, "600180", MainAccountType.Expense);   // purchase expenditure for product
        Account(db, 91, "210110", MainAccountType.Liability); // accrued purchases, product receipt
        Account(db, 92, "210120", MainAccountType.Liability); // accrued cost of goods sold

        // §1 and §17: inventory posting profiles. The service item gets its own revenue account so
        // the reference's split of 2,700 goods and 300 service revenue is observable.
        Posting(db, 1, InventAccountType.InventReceipt, Inventory);
        Posting(db, 2, InventAccountType.InventIssue, CostOfSales);
        Posting(db, 3, InventAccountType.PurchReceipt, Inventory);
        Posting(db, 4, InventAccountType.PurchConsump, 90);
        Posting(db, 10, InventAccountType.SalesRevenue, GoodsRevenue);
        Posting(db, 11, InventAccountType.SalesConsump, CostOfSales);
        Posting(db, 12, InventAccountType.SalesIssue, Inventory);
        db.Set<InventPosting>().Add(new InventPosting
        {
            RecId = 13, DataAreaId = "dat", InventAccountType = InventAccountType.SalesRevenue,
            ItemCode = TableGroupAll.Table, ItemRelation = "SRV-01",
            CustVendCode = TableGroupAll.All, LedgerDimension = ServiceRevenue
        });

        // §1: vendor, customer and bank, each with its summary account.
        db.Set<VendTable>().Add(new VendTable { RecId = 1, DataAreaId = "dat", AccountNum = "V-0001", Currency = "SAR", VendGroup = "LOCAL" });
        db.Set<CustTable>().Add(new CustTable { RecId = 1, DataAreaId = "dat", AccountNum = "C-001", CustGroupId = "DOM", CurrencyCode = "SAR" });
        db.Set<VendLedgerAccounts>().Add(new VendLedgerAccounts { RecId = 1, DataAreaId = "dat", PostingProfile = "GEN", AccountCode = AccountCode.All, SummaryLedgerDimension = Vendors });
        db.Set<CustLedgerAccounts>().Add(new CustLedgerAccounts { RecId = 1, DataAreaId = "dat", PostingProfile = "GEN", AccountCode = AccountCode.All, SummaryLedgerDimension = Receivables });
        db.Set<BankAccountTable>().Add(new BankAccountTable { RecId = 1, DataAreaId = "dat", AccountId = "BANK-1", LedgerDimension = Bank, CurrencyCode = "SAR", BankAccountStatus = BankAccountStatus.Active });
        db.Set<PaymTerm>().Add(new PaymTerm { DataAreaId = "dat", PaymTermId = "NET30", PaymMethod = PaymMethod.Net, NumOfDays = 30 });

        // §16: the stocked item and the non-stocked service, on one site and warehouse.
        db.Set<InventSite>().Add(new InventSite { RecId = 1, DataAreaId = "dat", SiteId = "S01", Name = "Jeddah Main" });
        db.Set<InventLocation>().Add(new InventLocation { RecId = 1, DataAreaId = "dat", InventLocationId = "WH-01", InventSiteId = "S01" });
        db.Set<InventTable>().AddRange(
            new InventTable { RecId = 1, DataAreaId = "dat", ItemId = "ITEM-001", NameAlias = "Cotton Uniform" },
            new InventTable { RecId = 2, DataAreaId = "dat", ItemId = "SRV-01", NameAlias = "Installation", ItemType = ItemType.Service });
        db.Set<InventTableModule>().AddRange(
            new InventTableModule { RecId = 1, DataAreaId = "dat", ItemId = "ITEM-001", ModuleType = ModuleInventPurchSales.Inventory, UnitId = "PCS" },
            new InventTableModule { RecId = 2, DataAreaId = "dat", ItemId = "ITEM-001", ModuleType = ModuleInventPurchSales.Purchase, UnitId = "PCS" },
            new InventTableModule { RecId = 3, DataAreaId = "dat", ItemId = "SRV-01", ModuleType = ModuleInventPurchSales.Sales, UnitId = "PCS" });
        db.Set<UnitOfMeasure>().Add(new UnitOfMeasure { RecId = 1, Symbol = "PCS", DecimalPrecision = 0 });
        // The reference costs this example with weighted average.
        db.Set<InventModelGroup>().Add(new InventModelGroup { RecId = 1, DataAreaId = "dat", ModelGroupId = "WAVG", InventModel = InventModel.WeightedAvg, StockedProduct = NoYes.Yes });
        db.Set<InventModelGroupItem>().Add(new InventModelGroupItem { RecId = 1, ItemDataAreaId = "dat", ModelGroupDataAreaId = "dat", ModelGroupId = "WAVG", ItemId = "ITEM-001" });
        db.Set<InventItemGroupItem>().Add(new InventItemGroupItem { RecId = 1, ItemDataAreaId = "dat", ItemId = "ITEM-001", ItemGroupId = "FINISHED" });
    }

    private static void Account(GlCycleContext db, long recId, string id, MainAccountType type)
    {
        db.Set<MainAccount>().Add(new MainAccount
        {
            RecId = recId, DataAreaId = "dat", MainAccountId = id, Type = type, LedgerChartOfAccounts = 1
        });
        db.Set<DimensionAttributeValueCombination>().Add(new DimensionAttributeValueCombination
        {
            RecId = recId, DataAreaId = "dat", MainAccount = recId, MainAccountValue = id, DisplayValue = id
        });
    }

    private static void Posting(GlCycleContext db, long recId, InventAccountType type, long dimension) =>
        db.Set<InventPosting>().Add(new InventPosting
        {
            RecId = recId, DataAreaId = "dat", InventAccountType = type,
            ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = dimension
        });

    // ---- Harness ----------------------------------------------------------------------------

    private static ISysNumberSequenceService Sequence() =>
        DispatchProxy.Create<ISysNumberSequenceService, GlSequenceProxy>();

    public class GlSequenceProxy : DispatchProxy
    {
        private int _n;
        protected override object? Invoke(MethodInfo? method, object?[]? args) =>
            method?.Name == nameof(ISysNumberSequenceService.NextAsync)
                ? Task.FromResult(new NextSequenceResultDto { EntityName = "X", Value = ++_n, Code = $"X-{_n:D6}" })
                : throw new NotSupportedException(method?.Name);
    }

    private sealed class TestNumbers : ISalesInventoryNumberService
    {
        private int _dim, _trans;
        public Task<string> NextInventDimIdAsync(CancellationToken cancellationToken = default) => Task.FromResult($"DIM-{++_dim:000000}");
        public Task<string> NextInventTransIdAsync(CancellationToken cancellationToken = default) => Task.FromResult($"LOT-{++_trans:000000}");
    }

    private sealed class TestUser : ICurrentUserService
    {
        public string GetCurrentUserId() => "tester";
        public string GetOwnerAccountId() => "owner";
        public string GetDataAreaId() => "dat";
    }

    private sealed class TestCompany : ICompanyExecutionContext
    {
        public string GetDataAreaId() => "dat";
        public bool IsRequestedCompanyAuthorized() => true;
    }

    private sealed class TestUnitOfWork(DbContext context) : IUnitOfWork
    {
        private Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? _tx;
        public DbContext Context => context;
        public Task<int> CompleteAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
        public IGenericRepository<T> Repository<T>() where T : class => null!;
        public async Task BeginTransactionAsync(CancellationToken ct = default) => _tx = await context.Database.BeginTransactionAsync(ct);
        public async Task CommitTransactionAsync(CancellationToken ct = default) { if (_tx != null) { await _tx.CommitAsync(ct); _tx = null; } }
        public async Task RollbackTransactionAsync(CancellationToken ct = default) { if (_tx != null) { await _tx.RollbackAsync(ct); _tx = null; } }
        public void Dispose() { }
    }

    private sealed class Fixture : IAsyncDisposable
    {
        private readonly SqliteConnection _connection = new("Data Source=:memory:");
        public GlCycleContext Db { get; private set; } = null!;

        public static async Task<Fixture> CreateAsync()
        {
            var fixture = new Fixture();
            await fixture._connection.OpenAsync();
            fixture.Db = new GlCycleContext(new DbContextOptionsBuilder<GlCycleContext>().UseSqlite(fixture._connection).Options);
            await fixture.Db.Database.EnsureCreatedAsync();
            return fixture;
        }

        public async ValueTask DisposeAsync() { await Db.DisposeAsync(); await _connection.DisposeAsync(); }
    }

    private sealed class GlCycleContext(DbContextOptions<GlCycleContext> options) : DbContext(options)
    {
        private static readonly Type[] Included =
        [
            typeof(InventDim), typeof(InventTrans), typeof(InventTransOrigin), typeof(InventSum),
            typeof(IAX.IXApi.Modules.Finance.Entities.InventTransPosting),
            typeof(InventModelGroup), typeof(InventModelGroupItem), typeof(InventItemGroupItem),
            typeof(InventPosting), typeof(DimensionAttributeValueCombination),
            typeof(LedgerStructure), typeof(DimensionHierarchy), typeof(MainAccount),
            typeof(Ledger), typeof(FiscalCalendarPeriod), typeof(LedgerFiscalCalendarPeriod),
            typeof(FiscalCalendarYear), typeof(LedgerFiscalCalendarYear), typeof(LedgerPeriodModuleAccessControl),
            typeof(InventSettlement), typeof(InventClosing),
            // The count-freeze guard reads the counting journals before any stock movement.
            typeof(InventJournalTable), typeof(InventJournalTrans), typeof(InventJournalName),
            typeof(GeneralJournalEntry), typeof(GeneralJournalAccountEntry), typeof(SubledgerVoucherGeneralJournalEntry),
            typeof(SourceDocumentHeader), typeof(SourceDocumentLine), typeof(AccountingEvent), typeof(AccountingDistribution),
            typeof(SubledgerJournalEntry), typeof(SubledgerJournalAccountEntry), typeof(SubledgerJournalAccountEntryDistribution),
            typeof(PurchParameters), typeof(VendInvoiceInfoTable), typeof(VendInvoiceInfoLine), typeof(VendInvoiceInfoSubLine),
            typeof(PurchTable), typeof(PurchLine), typeof(PurchPrepayTable), typeof(EcoResCategory),
            typeof(VendPurchOrderJour), typeof(VendPurchOrderTrans),
            typeof(VendPackingSlipJour), typeof(VendPackingSlipTrans), typeof(VendPackingSlipVersion), typeof(VendPackingSlipTransHistory),
            typeof(VendInvoiceJour), typeof(VendInvoiceTrans), typeof(VendInvoicePackingSlipQuantityMatch),
            typeof(PaymTerm), typeof(VendTrans), typeof(VendTransOpen), typeof(VendSettlement), typeof(PurchReceiptAccrual),
            typeof(VendLedgerAccounts), typeof(InventTransOriginPurchLine), typeof(VendTable), typeof(VendParameters), typeof(VendPaymModeTable),
            typeof(InventSite), typeof(InventLocation), typeof(InventTable), typeof(InventTableModule),
            typeof(UnitOfMeasure), typeof(UnitOfMeasureConversion),
            typeof(SalesTable), typeof(SalesLine), typeof(CustTable), typeof(CustParameters),
            typeof(CustPackingSlipJour), typeof(CustPackingSlipTrans), typeof(CustPackingSlipVersion), typeof(CustPackingSlipTransHistory),
            typeof(CustInvoiceTable), typeof(CustInvoiceLine), typeof(CustInvoiceJour), typeof(CustInvoiceTrans),
            typeof(CustRelatedInvoice), typeof(CustTrans), typeof(CustTransOpen), typeof(CustSettlement), typeof(CustLedgerAccounts),
            typeof(Currency), typeof(ExchangeRate), typeof(ExchangeRateType), typeof(ExchangeRateCurrencyPair),
            typeof(DirPartyTable), typeof(CustPaymModeTable), typeof(TaxGroupHeading), typeof(TaxItemGroupHeading),
            typeof(TaxGroupData), typeof(TaxOnItem), typeof(TaxData), typeof(TaxTable), typeof(TaxLedgerAccountGroup), typeof(TaxTrans),
            typeof(InventTransOriginSalesLine), typeof(BankAccountTable), typeof(BankAccountTrans),
            typeof(LedgerJournalTable), typeof(LedgerJournalTrans), typeof(LedgerJournalName),
            typeof(IAX.IXApi.Modules.Finance.Entities.LedgerParameters), typeof(CompanyInfo), typeof(MainAccountLegalEntity),
            typeof(ReturnDispositionCode), typeof(ReturnReasonCode),
            typeof(PriceDiscTable), typeof(PriceDiscAdmTable), typeof(PriceDiscAdmTrans),
            typeof(PriceDiscAdmName), typeof(PriceDiscGroup),
            typeof(MarkupTable), typeof(MarkupTrans),
            typeof(IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities.EcoResStorageDimensionGroupItem),
            typeof(IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities.EcoResStorageDimensionGroupFldSetup),
            typeof(IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities.EcoResTrackingDimensionGroupItem),
            typeof(IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities.EcoResTrackingDimensionGroupFldSetup),
            typeof(InventBatch), typeof(InventSerial), typeof(InventItemLocation)
        ];

        protected override void OnModelCreating(ModelBuilder builder)
        {
            var included = new HashSet<Type>(Included);
            foreach (var type in typeof(InventTrans).Assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && typeof(Entity<long>).IsAssignableFrom(t) && !included.Contains(t)))
                builder.Ignore(type);
            foreach (var type in Included) Configure(builder, type);
            builder.Entity<CustPackingSlipVersion>().Ignore(x => x.DataAreaId);
            builder.Entity<VendPackingSlipVersion>().Ignore(x => x.DataAreaId);
            builder.Entity<VendPackingSlipTransHistory>().HasOne(x => x.PackingSlipLine)
                .WithMany().HasForeignKey(x => x.VendPackingSlipTrans);
            builder.Entity<CustPackingSlipTransHistory>().HasOne(x => x.PackingSlipLine)
                .WithMany().HasForeignKey(x => x.CustPackingSlipTrans).OnDelete(DeleteBehavior.Restrict);
        }

        private static void Configure(ModelBuilder builder, Type entity)
        {
            var e = builder.Entity(entity);
            e.HasKey(nameof(Entity<long>.RecId));
            e.Property(nameof(Entity<long>.RecId)).ValueGeneratedOnAdd();
            e.Ignore(nameof(Entity<long>.RowVersion));
            foreach (var property in entity.GetProperties())
                if (property.PropertyType != typeof(string) && property.PropertyType != typeof(byte[])
                    && !property.PropertyType.IsValueType)
                    e.Ignore(property.Name);
        }
    }
}
