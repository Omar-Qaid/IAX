using System.Reflection;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Confirm;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Invoice;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip.Dtos;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Payment;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Return;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Services;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Settlement;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.GeneralLedger.FiscalCalendar;
using IAX.IXApi.Modules.Finance.GeneralLedger.JournalEntry;
using IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable;
using IAX.IXApi.Modules.Finance.Inventory;
using IAX.IXApi.Modules.Finance.Inventory.Transfer;
using IAX.IXApi.Shared.Application.Identity;
using IAX.IXApi.Shared.Domain.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;
using JournalHeader = IAX.IXApi.Modules.Finance.Entities.LedgerJournalTable;

namespace IAX.IXApi.Tests;

/// <summary>
/// The core ERP reference cycle from <c>src/Modules/Finance/FullExmaple.md</c> and
/// <c>FullExampleDetails.md</c>, executed end to end through the real purchase, service receipt,
/// vendor invoice, general journal, transfer order, sales confirmation, reservation, packing slip,
/// sales invoice, free text invoice, purchase return, customer return (physical and credit only),
/// payment, settlement, counting journal, inventory close, trial balance and period-status
/// services, then checked against every figure the reference publishes.
///
/// The reference's acceptance balances are inventory <b>56 pieces at 2,800</b>, vendors
/// <b>1,030</b>, customers <b>1,150</b>, bank <b>8,980</b> and a profit of <b>1,900</b>, with
/// the balance sheet closing at <b>12,930</b> on each side. Its UAT ids (PO-02, SO-03, AP-04,
/// GL-07 and the rest) are the checkpoints below.
/// </summary>
public sealed class FullExampleCycleTests
{
    // Main accounts. The reference names accounts by role; the numbers follow the repository's
    // other cycle fixtures. Each id is both the MainAccount and its ledger dimension.
    private const long Inventory = 1;             // 130100 asset
    private const long CostOfSales = 2;           // 510100 expense
    private const long Vendors = 3;               // 210100 liability
    private const long Receivables = 4;           // 120100 asset
    private const long Bank = 5;                  // 110100 asset
    private const long Capital = 6;               // 300100 equity
    private const long GoodsRevenue = 7;          // 410100 revenue
    private const long ServiceRevenue = 8;        // 410200 revenue
    private const long Maintenance = 9;           // 600100 expense (purchased service)
    private const long AdminExpense = 10;         // 600200 expense (direct vendor invoice)
    private const long BankCharges = 11;          // 600300 expense
    private const long CountLoss = 12;            // 520100 expense
    private const long ReceivedNotInvoiced = 13;  // 130110 asset: product receipt, physical
    private const long PurchaseAccrual = 14;      // 210110 liability: product receipt, physical
    private const long DeliveredNotInvoiced = 15; // 130120 asset: packing slip, physical
    private const long CogsDelivered = 16;        // 510110 expense: packing slip, physical
    private const long PurchaseExpenditure = 17;  // 600180 expense: purchase expenditure for product
    private const long CountProfit = 18;          // 520200 revenue
    // A journal offset that names the bank account rather than a main account.
    private const long BankOffsetReference = 50;

    private static DateTime Nov(int day) => new(2026, 11, day);

    [Fact]
    public async Task Core_erp_reference_cycle_closes_at_fifty_six_pieces_and_a_profit_of_nineteen_hundred()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        Seed(db);
        await db.SaveChangesAsync();

        var work = new TestUnitOfWork(db);
        var company = new TestCompany();
        var user = new TestUser();
        var numbers = new TestNumbers();
        // One sequence for every service, so no two documents share an allocated identifier.
        var sequence = Sequence();
        var resolver = new InventDimensionResolver(work, numbers);
        var orders = new PurchOrderService(work, company, sequence,
            new PurchInventorySupplyService(work, numbers, resolver), new SalesUnitConversionService(work));
        var confirmations = new PurchConfirmationService(work, company, sequence);
        var receipts = new ProductReceiptService(work, company, sequence);
        var vendorInvoices = new VendorInvoiceService(work, company, sequence);
        var purchaseReturns = new PurchReturnService(work, company, sequence, numbers);
        var vendorPayments = new VendPaymentService(work, user, company, sequence);
        var vendorSettlements = new VendSettleOpenTransactionsService(work, company, sequence);
        var transfers = new InventTransferOrderService(work, company, resolver, numbers, sequence);
        var demand = new SalesInventoryDemandService(work, numbers, resolver);
        var salesConfirmations = new SalesOrderConfirmationService(work, sequence, company, new SalesOrderConfirmationValidator());
        var reservations = new InventoryReservationService(work, company);
        var packingSlips = new PackingSlipJournalService(work, company, sequence);
        var salesInvoices = new SalesInvoiceService(work, company, sequence);
        var freeTextInvoices = new FreeTextInvoiceService(work, company, sequence);
        var customerReturns = new CustReturnService(work, company, sequence, numbers);
        var customerPayments = new CustPaymentService(work, user, company, sequence);
        var customerSettlements = new CustSettleOpenTransactionsService(work, company, sequence);
        var journals = new InventJournalService(work, company, sequence, resolver);
        var journalPosting = new InventJournalPostingService(work, company, user, sequence, numbers);
        var close = new InventCloseService(work, company, user, sequence);

        // ---- GL-01: opening balance. Bank 10,000 against share capital, through a Bank line. ----
        Assert.Null(await PostJournalAsync(db, 9000, "GJ-OPEN", Nov(1), new LedgerJournalTrans
        {
            LineNum = 1, Voucher = "GJV-0001", AccountType = LedgerJournalACType.Bank, LedgerDimensionName = "BANK-SAR",
            OffsetAccountType = LedgerJournalACType.Ledger, OffsetLedgerDimension = Capital, AmountCurDebit = 10000m,
            Txt = "Opening bank balance", OffsetTxt = "Share capital"
        }));
        var balances = await PostedBalancesAsync(db);
        Assert.Equal(10000m, balances[Bank]);
        Assert.Equal(-10000m, balances[Capital]);
        Assert.Equal(10000m, (await db.Set<BankAccountTrans>().ToListAsync()).Sum(x => x.AmountCur));
        Assert.Empty(await db.Set<CustTrans>().ToListAsync());
        Assert.Empty(await db.Set<VendTrans>().ToListAsync());

        // ---- PO-01: PO-001 for 100 at 50 into A, confirmed. Supply 100, on hand 0, no vendor balance.
        Assert.Equal(200, (await orders.CreateAsync(
            new PurchOrderCreateDto("V-001", null, "SAR", Nov(3), "S1", "A", null, Nov(2)), default)).StatusCode);
        var po = await db.Set<PurchTable>().SingleAsync();
        Assert.Equal(200, (await orders.AddLineAsync(po.RecId,
            new PurchLineInputDto("ITEM-01", 100, "EA", 50, 1, null, null, null, null), default)).StatusCode);
        Assert.Equal(200, (await confirmations.ConfirmAsync(po.RecId, default)).StatusCode);
        var poLine = await db.Set<PurchLine>().SingleAsync(x => x.ItemId == "ITEM-01");
        Assert.Equal((100m, 100m), (poLine.RemainPurchPhysical, poLine.RemainPurchFinancial));
        Assert.Equal(100m, (await db.Set<InventTrans>().SingleAsync()).Qty);
        Assert.Equal(StatusReceipt.Ordered, (await db.Set<InventTrans>().SingleAsync()).StatusReceipt);
        Assert.Equal(0m, await OnHandAsync(db, "A"));
        Assert.Single(await db.Set<VendPurchOrderJour>().ToListAsync());
        Assert.Empty(await db.Set<VendTrans>().ToListAsync());

        // ---- PO-02: receive 60. A=60, 60 Received and 40 Ordered, accrual 3,000, vendor still zero.
        Assert.Equal(200, (await receipts.PostAsync(po.RecId,
            new ProductReceiptPostDto("PR-001", Nov(3), [new(poLine.RecId, 60)]), default)).StatusCode);
        Assert.Equal(60m, await OnHandAsync(db, "A"));
        Assert.Equal(60m, (await db.Set<InventTrans>().Where(x => x.StatusReceipt == StatusReceipt.Received).ToListAsync()).Sum(x => x.Qty));
        Assert.Equal(40m, (await db.Set<InventTrans>().Where(x => x.StatusReceipt == StatusReceipt.Ordered).ToListAsync()).Sum(x => x.Qty));
        poLine = await db.Set<PurchLine>().SingleAsync(x => x.ItemId == "ITEM-01");
        Assert.Equal((40m, 100m), (poLine.RemainPurchPhysical, poLine.RemainPurchFinancial));
        balances = await PostedBalancesAsync(db);
        Assert.Equal(3000m, balances[ReceivedNotInvoiced]);
        Assert.Equal(-3000m, balances[PurchaseAccrual]);
        Assert.Empty(await db.Set<VendTrans>().ToListAsync());

        // ---- PO-03: invoice VI-001 for the 60. Purchased 60, remaining 40/40, accrual reversed, vendor 3,000.
        Assert.Equal(200, (await vendorInvoices.PostAsync(po.RecId,
            new VendorInvoicePostDto("VI-001", Nov(4), [new(poLine.RecId, 60)]), default)).StatusCode);
        Assert.Equal(60m, await OnHandAsync(db, "A"));
        Assert.Equal(60m, (await db.Set<InventTrans>().Where(x => x.StatusReceipt == StatusReceipt.Purchased).ToListAsync()).Sum(x => x.Qty));
        Assert.Equal(40m, (await db.Set<InventTrans>().Where(x => x.StatusReceipt == StatusReceipt.Ordered).ToListAsync()).Sum(x => x.Qty));
        poLine = await db.Set<PurchLine>().SingleAsync(x => x.ItemId == "ITEM-01");
        Assert.Equal((40m, 40m), (poLine.RemainPurchPhysical, poLine.RemainPurchFinancial));
        balances = await PostedBalancesAsync(db);
        Assert.Equal(0m, balances[ReceivedNotInvoiced]);
        Assert.Equal(0m, balances[PurchaseAccrual]);
        Assert.Equal(3000m, balances[Inventory]);
        Assert.Equal(-3000m, balances[Vendors]);
        Assert.Equal(-3000m, await VendorOpenAsync(db));
        var stock = await db.Set<InventSum>().SingleAsync(x => x.InventLocationId == "A");
        Assert.Equal(3000m, stock.PostedValue);
        Assert.Equal(0m, stock.PhysicalValue);

        // ---- PO-04: receive the last 40 (accrual 2,000, remaining 0/40), then invoice VI-002.
        Assert.Equal(200, (await receipts.PostAsync(po.RecId,
            new ProductReceiptPostDto("PR-002", Nov(5), [new(poLine.RecId, 40)]), default)).StatusCode);
        Assert.Equal(100m, await OnHandAsync(db, "A"));
        poLine = await db.Set<PurchLine>().SingleAsync(x => x.ItemId == "ITEM-01");
        Assert.Equal((0m, 40m), (poLine.RemainPurchPhysical, poLine.RemainPurchFinancial));
        balances = await PostedBalancesAsync(db);
        Assert.Equal(-2000m, balances[PurchaseAccrual]);
        Assert.Equal(200, (await vendorInvoices.PostAsync(po.RecId,
            new VendorInvoicePostDto("VI-002", Nov(6), [new(poLine.RecId, 40)]), default)).StatusCode);
        poLine = await db.Set<PurchLine>().SingleAsync(x => x.ItemId == "ITEM-01");
        Assert.Equal((0m, 0m), (poLine.RemainPurchPhysical, poLine.RemainPurchFinancial));
        Assert.Equal(PurchStatus.Invoiced, poLine.PurchStatus);
        Assert.Equal(100m, (await db.Set<InventTrans>().Where(x => x.StatusReceipt == StatusReceipt.Purchased).ToListAsync()).Sum(x => x.Qty));
        balances = await PostedBalancesAsync(db);
        Assert.Equal(0m, balances[PurchaseAccrual]);
        Assert.Equal(0m, balances[ReceivedNotInvoiced]);
        Assert.Equal(5000m, balances[Inventory]);
        Assert.Equal(-5000m, balances[Vendors]);
        // INV-01: everything received and invoiced, 100 at 5,000.
        Assert.Equal(5000m, (await db.Set<InventSum>().ToListAsync()).Sum(x => x.PostedValue));

        // ---- PO-05: a maintenance service for 200 on its own order. The service is received
        // (accepted) and invoiced; it never touches stock, and acceptance books no accrual here.
        Assert.Equal(200, (await orders.CreateAsync(
            new PurchOrderCreateDto("V-001", null, "SAR", Nov(7), "S1", "A", null, Nov(6)), default)).StatusCode);
        var servicePo = await db.Set<PurchTable>().OrderByDescending(x => x.RecId).FirstAsync();
        Assert.NotEqual(po.RecId, servicePo.RecId);
        Assert.Equal(200, (await orders.AddLineAsync(servicePo.RecId,
            new PurchLineInputDto("SRV-MAINT", 1, "EA", 200, 1, null, null, null, "Maintenance"), default)).StatusCode);
        Assert.Equal(200, (await confirmations.ConfirmAsync(servicePo.RecId, default)).StatusCode);
        var serviceLinePo = await db.Set<PurchLine>().SingleAsync(x => x.ItemId == "SRV-MAINT");
        Assert.DoesNotContain(await db.Set<InventTrans>().ToListAsync(), x => x.ItemId == "SRV-MAINT");
        Assert.Equal(200, (await receipts.PostAsync(servicePo.RecId,
            new ProductReceiptPostDto("SR-001", Nov(7), [new(serviceLinePo.RecId, 1)]), default)).StatusCode);
        balances = await PostedBalancesAsync(db);
        Assert.Equal(0m, balances[PurchaseAccrual]);                     // acceptance posts nothing
        Assert.Equal(200, (await vendorInvoices.PostAsync(servicePo.RecId,
            new VendorInvoicePostDto("VI-S01", Nov(7), [new(serviceLinePo.RecId, 1)]), default)).StatusCode);
        balances = await PostedBalancesAsync(db);
        Assert.Equal(200m, balances[Maintenance]);
        Assert.Equal(-5200m, balances[Vendors]);
        Assert.DoesNotContain(await db.Set<InventTrans>().ToListAsync(), x => x.ItemId == "SRV-MAINT");
        Assert.Equal(5000m, (await db.Set<InventSum>().ToListAsync()).Sum(x => x.PostedValue));

        // ---- AP-01: a direct vendor invoice of 80 through the journal, Vend line with a ledger offset.
        Assert.Null(await PostJournalAsync(db, 9100, "VIJ-001", Nov(8), new LedgerJournalTrans
        {
            LineNum = 1, Voucher = "GJV-0002", AccountType = LedgerJournalACType.Vend, LedgerDimensionName = "V-001",
            Invoice = "VI-N01", OffsetAccountType = LedgerJournalACType.Ledger, OffsetLedgerDimension = AdminExpense,
            AmountCurCredit = 80m, Txt = "Office supplies", OffsetTxt = "Administrative expense"
        }));
        balances = await PostedBalancesAsync(db);
        Assert.Equal(80m, balances[AdminExpense]);
        Assert.Equal(-5280m, balances[Vendors]);
        Assert.Equal(-5280m, await VendorOpenAsync(db));
        Assert.Equal(-80m, (await db.Set<VendTrans>().SingleAsync(x => x.Invoice == "VI-N01")).AmountCur);

        // ---- INV-02..04: transfer 30 from A to B through TRANSIT. Ownership stays 100 throughout.
        var transfer = await transfers.CreateAsync(new InventTransferCreateDto("TR-001", "S1", "A", "TRANSIT", "B",
            Nov(9), Nov(10), [new("ITEM-01", "EA", 30m)]), default);
        Assert.Null(transfer.Error);
        Assert.Equal((100m, 0m, 0m), await OnHandAsync(db));
        var shipped = await transfers.ShipAsync("TR-001", new InventTransferPostDto("TR-001-SHIP", Nov(9),
            new Dictionary<decimal, decimal> { [1m] = 30m }), default);
        Assert.Null(shipped.Error);
        Assert.Equal((70m, 0m, 30m), await OnHandAsync(db));
        var received = await transfers.ReceiveAsync("TR-001", new InventTransferPostDto("TR-001-RECV", Nov(10),
            new Dictionary<decimal, decimal> { [1m] = 30m }), default);
        Assert.Null(received.Error);
        Assert.Equal((70m, 30m, 0m), await OnHandAsync(db));
        Assert.Equal(100m, (await db.Set<InventSum>().ToListAsync()).Sum(x => x.PhysicalInvent));
        Assert.Equal(5000m, (await db.Set<InventSum>().ToListAsync()).Sum(x => x.PostedValue));
        balances = await PostedBalancesAsync(db);
        Assert.Equal(5000m, balances[Inventory]);                        // no ledger effect inside one site

        // ---- SO-01: SO-001, 40 of the item at 100 plus the 300 service, confirmed. No receivable yet.
        var order = new SalesTable
        {
            DataAreaId = "dat", SalesId = "SO-001", CustAccount = "C-001", InvoiceAccount = "C-001", CurrencyCode = "SAR",
            SalesStatus = SalesStatus.Backorder, InventSiteId = "S1", InventLocationId = "A",
            OrderDate = Nov(10), PaymTerm = "NET30"
        };
        db.Set<SalesTable>().Add(order);
        await db.SaveChangesAsync();
        var itemLine = new SalesLine
        {
            DataAreaId = "dat", SalesId = order.SalesId, LineNum = 1, ItemId = "ITEM-01", SalesQty = 40, QtyOrdered = 40,
            RemainSalesPhysical = 40, RemainSalesFinancial = 40, SalesUnit = "EA", SalesPrice = 100, PriceUnit = 1,
            LineAmount = 4000, SalesStatus = SalesStatus.Backorder, StockedProduct = NoYes.Yes, CurrencyCode = "SAR",
            ReceiptDateRequested = Nov(11)
        };
        await demand.CreateAsync(order, itemLine, "S1", "A");
        db.Set<SalesLine>().Add(itemLine);
        var serviceLine = new SalesLine
        {
            DataAreaId = "dat", SalesId = order.SalesId, LineNum = 2, ItemId = "SRV-01", SalesQty = 1, QtyOrdered = 1,
            RemainSalesPhysical = 1, RemainSalesFinancial = 1, SalesUnit = "EA", SalesPrice = 300, PriceUnit = 1,
            LineAmount = 300, SalesStatus = SalesStatus.Backorder, StockedProduct = NoYes.No, CurrencyCode = "SAR",
            ReceiptDateRequested = Nov(11)
        };
        db.Set<SalesLine>().Add(serviceLine);
        await db.SaveChangesAsync();
        Assert.Equal(-40m, (await db.Set<InventTrans>().SingleAsync(x => x.StatusIssue == StatusIssue.Ordered)).Qty);
        var confirmed = await salesConfirmations.PostAsync(order.RecId, new PostConfirmationRequest(Nov(10), true, "CONF-001"), default);
        Assert.True(confirmed.StatusCode == 200, confirmed.Message);
        Assert.Single(await db.Set<CustConfirmJour>().ToListAsync());
        Assert.Equal(2, await db.Set<CustConfirmTrans>().CountAsync());
        Assert.Empty(await db.Set<CustTrans>().ToListAsync());
        Assert.Equal(70m, await OnHandAsync(db, "A"));

        // Reserve the 40: physical stays 70, available drops to 30 (reference §4.3).
        Assert.Null((await reservations.ReserveAsync(itemLine.InventTransId, 40m, false, default)).Error);
        stock = await db.Set<InventSum>().SingleAsync(x => x.InventLocationId == "A");
        Assert.Equal(70m, stock.PhysicalInvent);
        Assert.Equal(30m, stock.AvailPhysical);

        // ---- SO-02: packing slip for 25. A=45, Deducted 25, provisional cost 1,250 on the delivery pair.
        Assert.Equal(200, (await packingSlips.PostAsync(order.RecId, new PostPackingSlipRequest(
            Nov(11), "All", true, "PS-001", [new PackingSlipPostingLineRequest(itemLine.RecId.ToString(), 25)]), default)).StatusCode);
        Assert.Equal(45m, await OnHandAsync(db, "A"));
        Assert.Equal(-25m, (await db.Set<InventTrans>().Where(x => x.StatusIssue == StatusIssue.Deducted).ToListAsync()).Sum(x => x.Qty));
        itemLine = await db.Set<SalesLine>().SingleAsync(x => x.RecId == itemLine.RecId);
        Assert.Equal((15m, 40m), (itemLine.RemainSalesPhysical, itemLine.RemainSalesFinancial));
        balances = await PostedBalancesAsync(db);
        Assert.Equal(1250m, balances[CogsDelivered]);
        Assert.Equal(-1250m, balances[DeliveredNotInvoiced]);
        Assert.Empty(await db.Set<CustTrans>().ToListAsync());

        // ---- SO-03: invoice those 25. Sold 25, customer 2,500, revenue 2,500, cost 1,250, on hand still 45.
        var si001Post = await salesInvoices.PostAsync(order.RecId, new SalesInvoicePostDto(Nov(12), "SI-001", [new(itemLine.RecId, 25)]), default);
        Assert.True(si001Post.StatusCode == 200, si001Post.Message);
        var si001 = (await db.Set<CustInvoiceJour>().SingleAsync(x => x.ParmId == "SI-001")).InvoiceId;
        Assert.Equal(45m, await OnHandAsync(db, "A"));
        Assert.Equal(-25m, (await SalesIssuesAsync(db, itemLine.InventTransId)).Sum(x => x.Qty));
        Assert.Equal(-1250m, (await SalesIssuesAsync(db, itemLine.InventTransId)).Sum(x => x.CostAmountPosted));
        itemLine = await db.Set<SalesLine>().SingleAsync(x => x.RecId == itemLine.RecId);
        Assert.Equal((15m, 15m), (itemLine.RemainSalesPhysical, itemLine.RemainSalesFinancial));
        balances = await PostedBalancesAsync(db);
        Assert.Equal(2500m, balances[Receivables]);
        Assert.Equal(-2500m, balances[GoodsRevenue]);
        Assert.Equal(1250m, balances[CostOfSales]);
        Assert.Equal(0m, balances[CogsDelivered]);
        Assert.Equal(0m, balances[DeliveredNotInvoiced]);
        Assert.Equal(2500m, (await db.Set<CustTransOpen>().ToListAsync()).Sum(x => x.AmountCur));

        // ---- SO-04: deliver the last 15 then invoice them with the service. Customer 1,800: 1,500 goods, 300 service, cost 750.
        Assert.Equal(200, (await packingSlips.PostAsync(order.RecId, new PostPackingSlipRequest(
            Nov(13), "All", true, "PS-002", [new PackingSlipPostingLineRequest(itemLine.RecId.ToString(), 15)]), default)).StatusCode);
        Assert.Equal(30m, await OnHandAsync(db, "A"));
        var si002Post = await salesInvoices.PostAsync(order.RecId,
            new SalesInvoicePostDto(Nov(14), "SI-002", [new(itemLine.RecId, 15), new(serviceLine.RecId, 1)]), default);
        Assert.True(si002Post.StatusCode == 200, si002Post.Message);
        var si002 = (await db.Set<CustInvoiceJour>().SingleAsync(x => x.ParmId == "SI-002")).InvoiceId;
        Assert.DoesNotContain(await db.Set<InventTrans>().ToListAsync(), x => x.ItemId == "SRV-01");
        itemLine = await db.Set<SalesLine>().SingleAsync(x => x.RecId == itemLine.RecId);
        Assert.Equal((0m, 0m), (itemLine.RemainSalesPhysical, itemLine.RemainSalesFinancial));
        balances = await PostedBalancesAsync(db);
        Assert.Equal(4300m, balances[Receivables]);
        Assert.Equal(-4000m, balances[GoodsRevenue]);
        Assert.Equal(-300m, balances[ServiceRevenue]);
        Assert.Equal(2000m, balances[CostOfSales]);
        Assert.Equal(3000m, balances[Inventory]);
        Assert.Equal(1800m, (await db.Set<CustTrans>().SingleAsync(x => x.Invoice == si002)).AmountCur);
        // INV-05: 40 sold at a cost of 2,000; 3,000 is left.
        Assert.Equal((30m, 30m, 0m), await OnHandAsync(db));
        Assert.Equal(3000m, (await db.Set<InventSum>().ToListAsync()).Sum(x => x.PostedValue));

        // ---- AR-01: a free text invoice of 100 straight to service revenue. No stock movement.
        var fti = await freeTextInvoices.CreateAsync(new FreeTextInvoiceSaveDto
        {
            CustomerAccount = "C-001", InvoiceDate = Nov(15), CurrencyCode = "SAR", PaymentTerms = "NET30",
            Description = "Additional service",
            Lines = [new FreeTextInvoiceLineInputDto { Description = "Additional service", Quantity = 1, UnitPrice = 100, LedgerDimension = ServiceRevenue }]
        }, default);
        Assert.True(fti.StatusCode is 200 or 201, fti.Message);
        var ftiPost = await freeTextInvoices.PostAsync(fti.Invoice!.RecId, default);
        Assert.True(ftiPost.StatusCode == 200, ftiPost.Message);
        var ftiInvoice = ftiPost.Invoice!.InvoiceId;
        Assert.Equal(100m, (await db.Set<CustTrans>().SingleAsync(x => x.Invoice == ftiInvoice)).AmountCur);
        balances = await PostedBalancesAsync(db);
        Assert.Equal(4400m, balances[Receivables]);
        Assert.Equal(-400m, balances[ServiceRevenue]);
        Assert.Equal((30m, 30m, 0m), await OnHandAsync(db));             // a free text invoice moves no stock

        // ---- AP-02 / INV-06: return 5 to the vendor against VI-002, ship them, then the credit note. A=25, inventory -250.
        var vi002Line = await db.Set<VendInvoiceTrans>().SingleAsync(x => x.InvoiceId == "VI-002");
        Assert.Equal(200, (await purchaseReturns.CreateAsync(new PurchReturnCreateDto("VI-002", "RMA-V-001", [new(vi002Line.RecId, 5)]), default)).StatusCode);
        var vendorReturn = await db.Set<PurchTable>().SingleAsync(x => x.PurchaseType == PurchaseType.ReturnItem);
        var vendorReturnLine = await db.Set<PurchLine>().SingleAsync(x => x.PurchQty < 0);
        Assert.Equal(200, (await purchaseReturns.ShipAsync(vendorReturn.RecId,
            new PurchReturnShipDto("RS-001", Nov(16), [new(vendorReturnLine.RecId, 5)]), default)).StatusCode);
        Assert.Equal(25m, await OnHandAsync(db, "A"));
        Assert.Equal(-5280m, await VendorOpenAsync(db));                 // nothing financial yet
        var vcn = await purchaseReturns.CreditAsync(vendorReturn.RecId, new VendCreditNoteDto("VCN-001", Nov(17)), default);
        Assert.True(vcn.StatusCode == 200, vcn.Message);
        Assert.Equal(250m, (await db.Set<VendTrans>().SingleAsync(x => x.Invoice == "VCN-001")).AmountCur);
        Assert.Equal(-5030m, await VendorOpenAsync(db));
        balances = await PostedBalancesAsync(db);
        Assert.Equal(2750m, balances[Inventory]);
        Assert.Equal(-5030m, balances[Vendors]);
        Assert.Equal(2750m, (await db.Set<InventSum>().ToListAsync()).Sum(x => x.PostedValue));

        // ---- AR-02 / INV-07: the customer returns 2 of SI-002 to stock, then the credit note. A=27, inventory +100, cost of sales -100.
        var si002ItemLine = await db.Set<CustInvoiceTrans>().SingleAsync(x => x.InvoiceId == si002 && x.ItemId == "ITEM-01");
        Assert.Equal(200, (await customerReturns.CreateAsync(new CustReturnCreateDto(si002, null, [new(si002ItemLine.RecId, 2, "CREDIT")]), default)).StatusCode);
        var rma = await db.Set<SalesTable>().SingleAsync(x => x.SalesType == SalesType.ReturnItem);
        var rmaLine = await db.Set<SalesLine>().SingleAsync(x => x.SalesId == rma.SalesId);
        Assert.Equal(-2m, rmaLine.SalesQty);                             // the document reverses the sale
        Assert.Equal(200, (await customerReturns.ReceiveAsync(rma.RecId,
            new CustReturnReceiptDto("RR-001", Nov(18), [new(rmaLine.RecId, 2)]), default)).StatusCode);
        Assert.Equal(27m, await OnHandAsync(db, "A"));
        var returnOrigin = (await db.Set<InventTransOrigin>().SingleAsync(o => o.InventTransId == rmaLine.InventTransId)).RecId;
        Assert.Equal(2m, (await db.Set<InventTrans>().SingleAsync(x => x.InventTransOrigin == returnOrigin)).Qty); // the movement is +2
        var cn1 = await customerReturns.CreditAsync(rma.RecId, new CustCreditNoteDto(Nov(19), "CN-001"), default);
        Assert.True(cn1.StatusCode == 200, cn1.Message);
        var cn1Invoice = (await db.Set<CustInvoiceJour>().SingleAsync(x => x.ParmId == "CN-001")).InvoiceId;
        Assert.Equal(-200m, (await db.Set<CustTrans>().SingleAsync(x => x.Invoice == cn1Invoice)).AmountCur);
        balances = await PostedBalancesAsync(db);
        Assert.Equal(4200m, balances[Receivables]);
        Assert.Equal(-3800m, balances[GoodsRevenue]);
        Assert.Equal(1900m, balances[CostOfSales]);
        Assert.Equal(2850m, balances[Inventory]);
        Assert.Equal(2850m, (await db.Set<InventSum>().ToListAsync()).Sum(x => x.PostedValue));

        // ---- AR-03: credit only, 50 on the sale. Quantity, inventory value and cost of sales stay put.
        // The return line carries no price of its own here, so the 50 is half a unit of the 100 line.
        Assert.Equal(200, (await customerReturns.CreateAsync(new CustReturnCreateDto(si002, null, [new(si002ItemLine.RecId, 0.5m, "CREDITONLY")]), default)).StatusCode);
        var creditOnly = await db.Set<SalesTable>().SingleAsync(x => x.SalesType == SalesType.ReturnItem && x.RecId != rma.RecId);
        var cn2 = await customerReturns.CreditAsync(creditOnly.RecId, new CustCreditNoteDto(Nov(20), "CN-002"), default);
        Assert.True(cn2.StatusCode == 200, cn2.Message);
        var cn2Invoice = (await db.Set<CustInvoiceJour>().SingleAsync(x => x.ParmId == "CN-002")).InvoiceId;
        Assert.Equal(-50m, (await db.Set<CustTrans>().SingleAsync(x => x.Invoice == cn2Invoice)).AmountCur);
        Assert.Equal(27m, await OnHandAsync(db, "A"));
        balances = await PostedBalancesAsync(db);
        Assert.Equal(4150m, balances[Receivables]);
        Assert.Equal(-3750m, balances[GoodsRevenue]);
        Assert.Equal(1900m, balances[CostOfSales]);
        Assert.Equal(2850m, balances[Inventory]);

        // ---- AP-03: pay 3,000 and mark VI-001. VI-001 closes.
        Assert.Equal(200, (await vendorPayments.PostAsync(new VendPaymentPostDto("V-001", "BANK-SAR", 3000m, "PAY-001", Nov(21), null, "VI-001"), default)).StatusCode);
        var vi001Trans = await db.Set<VendTrans>().SingleAsync(x => x.Invoice == "VI-001");
        Assert.Empty(await db.Set<VendTransOpen>().Where(x => x.RefRecId == vi001Trans.RecId).ToListAsync());
        Assert.Equal(-2030m, await VendorOpenAsync(db));
        balances = await PostedBalancesAsync(db);
        Assert.Equal(7000m, balances[Bank]);

        // ---- AP-04: pay 1,000 on account, then settle it and the 250 credit note against VI-002. VI-002 keeps 750; vendor 1,030.
        Assert.Equal(200, (await vendorPayments.PostAsync(new VendPaymentPostDto("V-001", "BANK-SAR", 1000m, "PAY-002", Nov(22), null, null, false), default)).StatusCode);
        var pay2 = await db.Set<VendTrans>().SingleAsync(x => x.AmountCur == 1000m && (x.Invoice == null || x.Invoice == ""));
        var vi002Trans = await db.Set<VendTrans>().SingleAsync(x => x.Invoice == "VI-002");
        var vcnTrans = await db.Set<VendTrans>().SingleAsync(x => x.Invoice == "VCN-001");
        Assert.Equal(-1030m, await VendorOpenAsync(db));                 // the payment is open until settled
        db.ChangeTracker.Clear();
        var vendorSettle = await vendorSettlements.SettleAsync(new VendSettleOpenInput("V-001", Nov(22), [pay2.RecId, vi002Trans.RecId, vcnTrans.RecId]), default);
        Assert.True(vendorSettle.StatusCode == 200, vendorSettle.Message);
        Assert.Equal(-750m, (await db.Set<VendTransOpen>().SingleAsync(x => x.RefRecId == vi002Trans.RecId)).AmountCur);
        Assert.Empty(await db.Set<VendTransOpen>().Where(x => x.RefRecId == pay2.RecId || x.RefRecId == vcnTrans.RecId).ToListAsync());
        Assert.Equal(-1030m, await VendorOpenAsync(db));
        var serviceInvoiceId = (await db.Set<VendTrans>().SingleAsync(v => v.Invoice == "VI-S01")).RecId;
        var journalInvoiceId = (await db.Set<VendTrans>().SingleAsync(v => v.Invoice == "VI-N01")).RecId;
        Assert.Equal(-200m, (await db.Set<VendTransOpen>().SingleAsync(x => x.RefRecId == serviceInvoiceId)).AmountCur);
        Assert.Equal(-80m, (await db.Set<VendTransOpen>().SingleAsync(x => x.RefRecId == journalInvoiceId)).AmountCur);
        balances = await PostedBalancesAsync(db);
        Assert.Equal(6000m, balances[Bank]);
        Assert.Equal(-1030m, balances[Vendors]);                         // settlement moves no ledger balance

        // ---- AR-04: collect 2,000 against SI-001. 500 stays open on it.
        Assert.Equal(200, (await customerPayments.PostAsync(new CustPaymentPostDto("C-001", "BANK-SAR", 2000m, Nov(23), null, si001, "RCPT-001"), default)).StatusCode);
        var si001Trans = await db.Set<CustTrans>().SingleAsync(x => x.Invoice == si001);
        Assert.Equal(500m, (await db.Set<CustTransOpen>().SingleAsync(x => x.RefRecId == si001Trans.RecId)).AmountCur);
        balances = await PostedBalancesAsync(db);
        Assert.Equal(8000m, balances[Bank]);
        Assert.Equal(2150m, balances[Receivables]);

        // ---- AR-05: collect 1,000 on account, allocate 500 to SI-001 and 500 to SI-002, then settle both credit notes against SI-002.
        Assert.Equal(200, (await customerPayments.PostAsync(new CustPaymentPostDto("C-001", "BANK-SAR", 1000m, Nov(24), null, null, "RCPT-002", false), default)).StatusCode);
        var rcpt2 = await db.Set<CustTrans>().SingleAsync(x => x.AmountCur == -1000m && (x.Invoice == null || x.Invoice == ""));
        var si002Trans = await db.Set<CustTrans>().SingleAsync(x => x.Invoice == si002);
        Assert.Equal(1150m, (await db.Set<CustTransOpen>().ToListAsync()).Sum(x => x.AmountCur));
        db.ChangeTracker.Clear();
        var customerSettle = await customerSettlements.SettleAsync(new CustSettleOpenInput("C-001", Nov(24), [rcpt2.RecId, si001Trans.RecId, si002Trans.RecId]), default);
        Assert.True(customerSettle.StatusCode == 200, customerSettle.Message);
        Assert.Empty(await db.Set<CustTransOpen>().Where(x => x.RefRecId == si001Trans.RecId || x.RefRecId == rcpt2.RecId).ToListAsync());
        Assert.Equal(1300m, (await db.Set<CustTransOpen>().SingleAsync(x => x.RefRecId == si002Trans.RecId)).AmountCur);
        var cn1Trans = await db.Set<CustTrans>().SingleAsync(x => x.Invoice == cn1Invoice);
        var cn2Trans = await db.Set<CustTrans>().SingleAsync(x => x.Invoice == cn2Invoice);
        db.ChangeTracker.Clear();
        customerSettle = await customerSettlements.SettleAsync(new CustSettleOpenInput("C-001", Nov(24), [cn1Trans.RecId, cn2Trans.RecId, si002Trans.RecId]), default);
        Assert.True(customerSettle.StatusCode == 200, customerSettle.Message);
        Assert.Equal(1050m, (await db.Set<CustTransOpen>().SingleAsync(x => x.RefRecId == si002Trans.RecId)).AmountCur);
        var ftiTrans = await db.Set<CustTrans>().SingleAsync(x => x.Invoice == ftiInvoice);
        Assert.Equal(100m, (await db.Set<CustTransOpen>().SingleAsync(x => x.RefRecId == ftiTrans.RecId)).AmountCur);
        Assert.Equal(1150m, (await db.Set<CustTransOpen>().ToListAsync()).Sum(x => x.AmountCur));
        balances = await PostedBalancesAsync(db);
        Assert.Equal(9000m, balances[Bank]);
        Assert.Equal(1150m, balances[Receivables]);

        // ---- INV-08: count B. The system says 30, the count says 29: one piece lost at 50.
        var count = await journals.CreateAsync(new InventJournalCreateDto
        { JournalNameId = "COUNT", Description = "Count warehouse B", InventSiteId = "S1", InventLocationId = "B" }, default);
        Assert.Null(count.Error);
        var countLine = await journals.AddLineAsync(count.Journal!.JournalId, new InventJournalLineInputDto
        { ItemId = "ITEM-01", InventSiteId = "S1", InventLocationId = "B", Unit = "EA", Counted = 29, TransDate = Nov(25) }, default);
        Assert.Null(countLine.Error);
        Assert.Equal(30m, await OnHandAsync(db, "B"));                   // saving the count moves nothing
        Assert.Null(await journalPosting.FreezeCountAsync(count.Journal.JournalId, default));
        db.ChangeTracker.Clear();
        var countLineId = (await db.Set<InventJournalTrans>().SingleAsync(x => x.JournalId == count.Journal.JournalId)).RecId;
        Assert.Null(await journalPosting.ConfirmCountAsync(count.Journal.JournalId, new Dictionary<long, decimal> { [countLineId] = 29m }, default));
        var countPost = await journalPosting.PostAsync(count.Journal.JournalId, default);
        Assert.Null(countPost.Error);
        Assert.Equal((27m, 29m, 0m), await OnHandAsync(db));
        Assert.Equal(-1m, (await db.Set<InventJournalTrans>().SingleAsync(x => x.RecId == countLineId)).Qty);
        balances = await PostedBalancesAsync(db);
        Assert.Equal(50m, balances[CountLoss]);
        Assert.Equal(2800m, balances[Inventory]);
        Assert.Equal(2800m, (await db.Set<InventSum>().ToListAsync()).Sum(x => x.PostedValue));

        // ---- GL-03: a direct bank charge of 20 through the general journal, Bank as the offset.
        Assert.Null(await PostJournalAsync(db, 9200, "GJ-BANK", Nov(26), new LedgerJournalTrans
        {
            LineNum = 1, Voucher = "GJV-0003", AccountType = LedgerJournalACType.Ledger, LedgerDimension = BankCharges,
            OffsetAccountType = LedgerJournalACType.Bank, OffsetLedgerDimension = BankOffsetReference,
            AmountCurDebit = 20m, Txt = "Bank charges", OffsetTxt = "Bank charges"
        }));
        balances = await PostedBalancesAsync(db);
        Assert.Equal(20m, balances[BankCharges]);
        Assert.Equal(8980m, balances[Bank]);
        Assert.Equal(1150m, balances[Receivables]);
        Assert.Equal(-1030m, balances[Vendors]);
        Assert.Equal(2800m, balances[Inventory]);

        // ---- INV-09: close the month. One purchase price, so no cost difference anywhere.
        db.ChangeTracker.Clear();
        var closed = await close.CloseAsync(Nov(30), default);
        Assert.True(closed.Error == null, closed.Error);
        Assert.Equal(0m, (await db.Set<InventTrans>().ToListAsync()).Sum(x => x.CostAmountAdjustment));
        Assert.Empty(closed.Result!.UnsettledIssues);
        // Transfer legs stay value-open by design: close verifies them as balanced internal
        // movements and settles only sourced issues, so the check is scoped to sales issues.
        Assert.DoesNotContain(await (from t in db.Set<InventTrans>()
                                     join o in db.Set<InventTransOrigin>() on t.InventTransOrigin equals o.RecId
                                     where t.ItemId == "ITEM-01" && t.StatusIssue == StatusIssue.Sold
                                        && o.ReferenceCategory == InventRefType.SalesTable
                                     select t).ToListAsync(),
            x => x.ValueOpen != InventTransOpen.No);
        Assert.Equal((27m, 29m, 0m), await OnHandAsync(db));
        Assert.Equal(56m, (await db.Set<InventSum>().ToListAsync()).Sum(x => x.PhysicalInvent));
        Assert.Equal(2800m, (await db.Set<InventSum>().ToListAsync()).Sum(x => x.PostedValue));
        Assert.Equal(0m, (await db.Set<InventSum>().ToListAsync()).Sum(x => x.PhysicalValue));
        var report = await new InventValueReportService(work, company).RunAsync(new DateOnly(2026, 11, 30), null, null, null, null, false, 1, 50, default);
        Assert.Equal(2800m, report!.TotalFinancialValue);

        // ---- GL-02: every posted voucher balances.
        var entries = await (from line in db.Set<GeneralJournalAccountEntry>().AsNoTracking()
                             join entry in db.Set<GeneralJournalEntry>().AsNoTracking() on line.GeneralJournalEntry equals entry.RecId
                             select new { entry.RecId, line.AccountingCurrencyAmount }).ToListAsync();
        Assert.All(entries.GroupBy(x => x.RecId), voucher => Assert.Equal(0m, voucher.Sum(x => x.AccountingCurrencyAmount)));

        // ---- GL-04..GL-06: the subledgers agree with their control accounts and the transitional accounts are flat.
        balances = await PostedBalancesAsync(db);
        Assert.Equal(8980m, balances[Bank]);
        Assert.Equal(8980m, (await db.Set<BankAccountTrans>().ToListAsync()).Sum(x => x.AmountCur));
        Assert.Equal(-1030m, balances[Vendors]);
        Assert.Equal(-1030m, await VendorOpenAsync(db));
        Assert.Equal(1150m, balances[Receivables]);
        Assert.Equal(1150m, (await db.Set<CustTransOpen>().ToListAsync()).Sum(x => x.AmountCur));
        Assert.Equal(2800m, balances[Inventory]);
        Assert.Equal(0m, balances.GetValueOrDefault(ReceivedNotInvoiced));
        Assert.Equal(0m, balances.GetValueOrDefault(PurchaseAccrual));
        Assert.Equal(0m, balances.GetValueOrDefault(DeliveredNotInvoiced));
        Assert.Equal(0m, balances.GetValueOrDefault(CogsDelivered));
        Assert.Equal(0m, balances.GetValueOrDefault(PurchaseExpenditure));
        Assert.Equal(0m, balances.GetValueOrDefault(CountProfit));

        // ---- GL-07: the trial balance and the result. Revenue 4,150, cost of sales 1,900, count loss 50, other expenses 300: profit 1,900.
        Assert.Equal(-3750m, balances[GoodsRevenue]);
        Assert.Equal(-400m, balances[ServiceRevenue]);
        Assert.Equal(1900m, balances[CostOfSales]);
        Assert.Equal(50m, balances[CountLoss]);
        Assert.Equal(200m, balances[Maintenance]);
        Assert.Equal(80m, balances[AdminExpense]);
        Assert.Equal(20m, balances[BankCharges]);
        Assert.Equal(-10000m, balances[Capital]);
        var netRevenue = -(balances[GoodsRevenue] + balances[ServiceRevenue]);
        var netProfit = netRevenue - balances[CostOfSales] - balances[CountLoss] - balances[Maintenance] - balances[AdminExpense] - balances[BankCharges];
        Assert.Equal(4150m, netRevenue);
        Assert.Equal(1900m, netProfit);
        // Assets 12,930 = vendors 1,030 + capital 10,000 + profit 1,900.
        Assert.Equal(12930m, balances[Bank] + balances[Receivables] + balances[Inventory]);
        Assert.Equal(12930m, -balances[Vendors] - balances[Capital] + netProfit);
        Assert.Equal(0m, balances.Values.Sum());

        var (trial, trialError) = await new LedgerTrialBalanceService(work, user)
            .GetAsync(Nov(1), Nov(30), CurrentToOperationsPostingLayer.Current, default);
        Assert.Null(trialError);
        Assert.True(trial!.Balanced, $"balance {trial.TotalDebitBalance}/{trial.TotalCreditBalance}");
        Assert.Equal(15180m, trial.TotalDebitBalance);
        Assert.Equal(15180m, trial.TotalCreditBalance);
        Assert.Equal(2800m, trial.Lines.Single(x => x.MainAccountId == "130100").DebitBalance);
        Assert.Equal(8980m, trial.Lines.Single(x => x.MainAccountId == "110100").DebitBalance);
        Assert.Equal(1150m, trial.Lines.Single(x => x.MainAccountId == "120100").DebitBalance);
        Assert.Equal(1030m, trial.Lines.Single(x => x.MainAccountId == "210100").CreditBalance);

        // ---- GL-07, second half: put November on hold. A November posting is refused, December still posts.
        db.ChangeTracker.Clear();
        var (status, statusError) = await new LedgerFiscalCalendarPeriodService(work, user).SetStatusAsync(1, FiscalPeriodStatus.OnHold);
        Assert.Null(statusError);
        Assert.NotNull(status);
        var refused = await PostJournalAsync(db, 9300, "GJ-LATE", Nov(28), new LedgerJournalTrans
        {
            LineNum = 1, Voucher = "GJV-0004", AccountType = LedgerJournalACType.Ledger, LedgerDimension = AdminExpense,
            OffsetAccountType = LedgerJournalACType.Ledger, OffsetLedgerDimension = PurchaseAccrual, AmountCurDebit = 10m,
            Txt = "Late accrual", OffsetTxt = "Late accrual"
        });
        Assert.Contains("is not open", refused);
        Assert.Equal(balances.OrderBy(x => x.Key), (await PostedBalancesAsync(db)).OrderBy(x => x.Key));
        Assert.Null(await PostJournalAsync(db, 9400, "GJ-DEC", new DateTime(2026, 12, 1), new LedgerJournalTrans
        {
            LineNum = 1, Voucher = "GJV-0005", AccountType = LedgerJournalACType.Ledger, LedgerDimension = AdminExpense,
            OffsetAccountType = LedgerJournalACType.Ledger, OffsetLedgerDimension = PurchaseAccrual, AmountCurDebit = 10m,
            Txt = "December accrual", OffsetTxt = "December accrual"
        }));
        Assert.Equal(balances.OrderBy(x => x.Key), (await PostedBalancesAsync(db)).OrderBy(x => x.Key)); // November is untouched by December
        Assert.Equal(10m, (await PostedBalancesAsync(db, new DateTime(2026, 12, 1), new DateTime(2026, 12, 31)))[AdminExpense]);
    }

    // ---- Helpers ----------------------------------------------------------------------------

    /// <summary>Posts one general journal through the real engine; returns the refusal, or null when it posted.</summary>
    private static async Task<string?> PostJournalAsync(DbContext db, long recId, string journalNum, DateTime date, params LedgerJournalTrans[] lines)
    {
        db.ChangeTracker.Clear();
        db.Set<JournalHeader>().Add(new JournalHeader { RecId = recId, DataAreaId = "dat", JournalNum = journalNum, JournalName = "GEN" });
        foreach (var line in lines)
        {
            line.RecId = recId * 10 + (long)line.LineNum;
            line.DataAreaId = "dat";
            line.Company = "dat";
            line.JournalNum = journalNum;
            line.TransDate = date;
            line.CurrencyCode = "SAR";
            db.Set<LedgerJournalTrans>().Add(line);
        }
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var header = await db.Set<JournalHeader>().SingleAsync(x => x.JournalNum == journalNum);
        try
        {
            await new LedgerJournalPosting(db).PostAsync("dat", header, default);
        }
        catch (SubledgerPostingException ex)
        {
            db.ChangeTracker.Clear();
            return ex.Message;
        }
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return null;
    }

    /// <summary>Signed accounting balance per main account over every posted line in the range (November by default).</summary>
    private static async Task<Dictionary<long, decimal>> PostedBalancesAsync(DbContext db, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var start = fromDate ?? Nov(1);
        var end = toDate ?? Nov(30);
        var rows = await (from line in db.Set<GeneralJournalAccountEntry>().AsNoTracking()
                          join entry in db.Set<GeneralJournalEntry>().AsNoTracking() on line.GeneralJournalEntry equals entry.RecId
                          where entry.AccountingDate >= start && entry.AccountingDate <= end
                          select new { line.MainAccount, line.AccountingCurrencyAmount }).ToListAsync();
        return rows.GroupBy(x => x.MainAccount).ToDictionary(x => x.Key, x => x.Sum(y => y.AccountingCurrencyAmount));
    }

    private static async Task<decimal> OnHandAsync(DbContext db, string warehouse) =>
        (await db.Set<InventSum>().AsNoTracking().Where(x => x.ItemId == "ITEM-01" && x.InventLocationId == warehouse).ToListAsync())
            .Sum(x => x.PhysicalInvent);

    /// <summary>On hand in A, B and TRANSIT, in the order the reference's §4 table uses.</summary>
    private static async Task<(decimal A, decimal B, decimal Transit)> OnHandAsync(DbContext db) =>
        (await OnHandAsync(db, "A"), await OnHandAsync(db, "B"), await OnHandAsync(db, "TRANSIT"));

    /// <summary>The sold movements of one sales line. Transfer legs are also <c>Sold</c>, so status alone is not enough.</summary>
    private static async Task<List<InventTrans>> SalesIssuesAsync(DbContext db, string inventTransId)
    {
        var origin = (await db.Set<InventTransOrigin>().AsNoTracking().SingleAsync(x => x.InventTransId == inventTransId)).RecId;
        return await db.Set<InventTrans>().AsNoTracking()
            .Where(x => x.InventTransOrigin == origin && x.StatusIssue == StatusIssue.Sold).ToListAsync();
    }

    private static async Task<decimal> VendorOpenAsync(DbContext db) =>
        (await db.Set<VendTransOpen>().AsNoTracking().ToListAsync()).Sum(x => x.AmountCur);

    // ---- Seed -------------------------------------------------------------------------------

    private static void Seed(FullCycleContext db)
    {
        // §1 General ledger: company ledger in SAR, one fiscal year, November and December open.
        db.Set<Ledger>().Add(new Ledger { RecId = 1, DataAreaId = "dat", AccountingCurrency = "SAR", ChartOfAccounts = 1, FiscalCalendar = 1, DefaultExchangeRateType = 1 });
        db.Set<FiscalCalendarYear>().Add(new FiscalCalendarYear { RecId = 1, FiscalCalendar = 1, StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 12, 31) });
        db.Set<FiscalCalendarPeriod>().AddRange(
            new FiscalCalendarPeriod { RecId = 1, FiscalCalendar = 1, FiscalCalendarYear = 1, Type = FiscalPeriodType.Operating, Name = "November 2026", StartDate = Nov(1), EndDate = Nov(30) },
            new FiscalCalendarPeriod { RecId = 2, FiscalCalendar = 1, FiscalCalendarYear = 1, Type = FiscalPeriodType.Operating, Name = "December 2026", StartDate = new DateTime(2026, 12, 1), EndDate = new DateTime(2026, 12, 31) });
        db.Set<LedgerFiscalCalendarPeriod>().AddRange(
            new LedgerFiscalCalendarPeriod { RecId = 1, Ledger = 1, FiscalCalendarPeriod = 1, Status = FiscalPeriodStatus.Open },
            new LedgerFiscalCalendarPeriod { RecId = 2, Ledger = 1, FiscalCalendarPeriod = 2, Status = FiscalPeriodStatus.Open });
        db.Set<LedgerFiscalCalendarYear>().Add(new LedgerFiscalCalendarYear { RecId = 1, Ledger = 1, FiscalCalendarYear = 1, Status = FiscalPeriodStatus.Open });

        // §1 chart of accounts, by role.
        Account(db, Inventory, "130100", MainAccountType.Asset);
        Account(db, CostOfSales, "510100", MainAccountType.Expense);
        Account(db, Vendors, "210100", MainAccountType.Liability);
        Account(db, Receivables, "120100", MainAccountType.Asset);
        Account(db, Bank, "110100", MainAccountType.Asset);
        Account(db, Capital, "300100", MainAccountType.Equity);
        Account(db, GoodsRevenue, "410100", MainAccountType.Revenue);
        Account(db, ServiceRevenue, "410200", MainAccountType.Revenue);
        Account(db, Maintenance, "600100", MainAccountType.Expense);
        Account(db, AdminExpense, "600200", MainAccountType.Expense);
        Account(db, BankCharges, "600300", MainAccountType.Expense);
        Account(db, CountLoss, "520100", MainAccountType.Expense);
        Account(db, ReceivedNotInvoiced, "130110", MainAccountType.Asset);
        Account(db, PurchaseAccrual, "210110", MainAccountType.Liability);
        Account(db, DeliveredNotInvoiced, "130120", MainAccountType.Asset);
        Account(db, CogsDelivered, "510110", MainAccountType.Expense);
        Account(db, PurchaseExpenditure, "600180", MainAccountType.Expense);
        Account(db, CountProfit, "520200", MainAccountType.Revenue);
        // A journal offset naming the bank account: a combination with no main account.
        db.Set<DimensionAttributeValueCombination>().Add(new DimensionAttributeValueCombination
        { RecId = BankOffsetReference, DataAreaId = "dat", DisplayValue = "BANK-SAR" });

        // §1 inventory posting. The transitional pairs get their own accounts so the reference's
        // "transitional accounts are zero" check is observable, and the two services resolve
        // item-specific rules: maintenance to its expense, the sold service to service revenue.
        Posting(db, 101, InventAccountType.InventReceipt, Inventory);
        Posting(db, 102, InventAccountType.InventIssue, Inventory);
        Posting(db, 103, InventAccountType.InventProfit, CountProfit);
        Posting(db, 104, InventAccountType.InventLoss, CountLoss);
        Posting(db, 105, InventAccountType.PurchReceipt, Inventory);
        Posting(db, 106, InventAccountType.PurchConsump, PurchaseExpenditure);
        Posting(db, 107, InventAccountType.PurchConsump, Maintenance, "SRV-MAINT");
        Posting(db, 108, InventAccountType.PurchPackingSlip, ReceivedNotInvoiced);
        Posting(db, 109, InventAccountType.PurchPackingSlipOffsetAccount, PurchaseAccrual);
        Posting(db, 110, InventAccountType.SalesRevenue, GoodsRevenue);
        Posting(db, 111, InventAccountType.SalesRevenue, ServiceRevenue, "SRV-01");
        Posting(db, 112, InventAccountType.SalesConsump, CostOfSales);
        Posting(db, 113, InventAccountType.SalesIssue, Inventory);
        Posting(db, 114, InventAccountType.SalesPackingSlip, DeliveredNotInvoiced);
        Posting(db, 115, InventAccountType.SalesPackingSlipOffsetAccount, CogsDelivered);

        // §1 parties, posting profiles, bank, terms, return dispositions, journal names.
        db.Set<DirPartyTable>().AddRange(
            new DirPartyTable { RecId = 1, Name = "Reference customer" },
            new DirPartyTable { RecId = 2, Name = "Reference vendor" });
        db.Set<CustTable>().Add(new CustTable { RecId = 1, DataAreaId = "dat", AccountNum = "C-001", CustGroupId = "DOM", CurrencyCode = "SAR", PaymTermId = "NET30" });
        db.Set<VendTable>().Add(new VendTable { RecId = 1, DataAreaId = "dat", AccountNum = "V-001", Currency = "SAR", VendGroup = "LOCAL" });
        db.Set<VendLedgerAccounts>().Add(new VendLedgerAccounts { RecId = 1, DataAreaId = "dat", PostingProfile = "GEN", AccountCode = AccountCode.All, SummaryLedgerDimension = Vendors });
        db.Set<CustLedgerAccounts>().Add(new CustLedgerAccounts { RecId = 1, DataAreaId = "dat", PostingProfile = "GEN", AccountCode = AccountCode.All, SummaryLedgerDimension = Receivables });
        db.Set<BankAccountTable>().Add(new BankAccountTable { RecId = 1, DataAreaId = "dat", AccountId = "BANK-SAR", LedgerDimension = Bank, CurrencyCode = "SAR", BankAccountStatus = BankAccountStatus.Active });
        db.Set<PaymTerm>().Add(new PaymTerm { DataAreaId = "dat", PaymTermId = "NET30", PaymMethod = PaymMethod.Net, NumOfDays = 30 });
        db.Set<Currency>().Add(new Currency { RecId = 1, DataAreaId = "dat", CurrencyCode = "SAR" });
        db.Set<ReturnDispositionCode>().AddRange(
            new ReturnDispositionCode { RecId = 1, DataAreaId = "dat", DispositionCodeId = "CREDIT", Description = "Credit", DispositionAction = DispositionAction.Credit },
            new ReturnDispositionCode { RecId = 2, DataAreaId = "dat", DispositionCodeId = "CREDITONLY", Description = "Credit only", DispositionAction = DispositionAction.CreditOnly });
        db.Set<LedgerJournalName>().Add(new LedgerJournalName { RecId = 1, DataAreaId = "dat", JournalName = "GEN" });
        db.Set<SysNumberSequence>().Add(new SysNumberSequence
        {
            RecId = 1, DataAreaId = "dat", NumberSequence = "INVVOUCHER", Txt = "Inventory voucher", Format = "IV-{SEQ}",
            AnnotatedFormat = "IV-{SEQ}", InUse = 1, IsActive = true, NextRec = 1, Highest = 999999
        });
        db.Set<InventJournalName>().Add(new InventJournalName { RecId = 1, DataAreaId = "dat", JournalNameId = "COUNT", Description = "Counting", JournalType = InventJournalType.Counting, VoucherNumberSequenceTable = 1 });

        // §1 inventory: site S1 with A, B and a transit warehouse; the stocked item on weighted
        // average with physical posting on both sides, and two non-stocked services.
        db.Set<InventSite>().Add(new InventSite { RecId = 1, DataAreaId = "dat", SiteId = "S1", Name = "Main site" });
        db.Set<InventLocation>().AddRange(
            new InventLocation { RecId = 1, DataAreaId = "dat", InventLocationId = "A", InventSiteId = "S1", Name = "Warehouse A" },
            new InventLocation { RecId = 2, DataAreaId = "dat", InventLocationId = "B", InventSiteId = "S1", Name = "Warehouse B" },
            new InventLocation { RecId = 3, DataAreaId = "dat", InventLocationId = "TRANSIT", InventSiteId = "S1", Name = "In transit", InventLocationType = InventLocationType.Transit });
        db.Set<UnitOfMeasure>().Add(new UnitOfMeasure { RecId = 1, Symbol = "EA", DecimalPrecision = 0 });
        db.Set<InventTable>().AddRange(
            new InventTable { RecId = 1, DataAreaId = "dat", ItemId = "ITEM-01", NameAlias = "Stocked item" },
            new InventTable { RecId = 2, DataAreaId = "dat", ItemId = "SRV-01", NameAlias = "Sold service", ItemType = ItemType.Service },
            new InventTable { RecId = 3, DataAreaId = "dat", ItemId = "SRV-MAINT", NameAlias = "Maintenance service", ItemType = ItemType.Service });
        db.Set<InventTableModule>().AddRange(
            new InventTableModule { RecId = 1, DataAreaId = "dat", ItemId = "ITEM-01", ModuleType = ModuleInventPurchSales.Inventory, UnitId = "EA" },
            new InventTableModule { RecId = 2, DataAreaId = "dat", ItemId = "ITEM-01", ModuleType = ModuleInventPurchSales.Purchase, UnitId = "EA" },
            new InventTableModule { RecId = 3, DataAreaId = "dat", ItemId = "ITEM-01", ModuleType = ModuleInventPurchSales.Sales, UnitId = "EA" },
            new InventTableModule { RecId = 4, DataAreaId = "dat", ItemId = "SRV-01", ModuleType = ModuleInventPurchSales.Sales, UnitId = "EA" },
            new InventTableModule { RecId = 5, DataAreaId = "dat", ItemId = "SRV-MAINT", ModuleType = ModuleInventPurchSales.Inventory, UnitId = "EA" },
            new InventTableModule { RecId = 6, DataAreaId = "dat", ItemId = "SRV-MAINT", ModuleType = ModuleInventPurchSales.Purchase, UnitId = "EA" });
        db.Set<InventModelGroup>().AddRange(
            new InventModelGroup
            {
                RecId = 1, DataAreaId = "dat", ModelGroupId = "WAVG", InventModel = InventModel.WeightedAvg, StockedProduct = NoYes.Yes,
                PostOnHandPhysical = NoYes.Yes, PostOnHandFinancial = NoYes.Yes, PostPhysicalPurchase = NoYes.Yes, PostPhysicalRevenue = NoYes.Yes
            },
            new InventModelGroup { RecId = 2, DataAreaId = "dat", ModelGroupId = "SRV", InventModel = InventModel.WeightedAvg, StockedProduct = NoYes.No });
        db.Set<InventModelGroupItem>().AddRange(
            new InventModelGroupItem { RecId = 1, ItemDataAreaId = "dat", ModelGroupDataAreaId = "dat", ModelGroupId = "WAVG", ItemId = "ITEM-01" },
            new InventModelGroupItem { RecId = 2, ItemDataAreaId = "dat", ModelGroupDataAreaId = "dat", ModelGroupId = "SRV", ItemId = "SRV-MAINT" });
        db.Set<InventItemGroupItem>().AddRange(
            new InventItemGroupItem { RecId = 1, ItemDataAreaId = "dat", ItemId = "ITEM-01", ItemGroupId = "TRADE" },
            new InventItemGroupItem { RecId = 2, ItemDataAreaId = "dat", ItemId = "SRV-MAINT", ItemGroupId = "SERVICES" },
            new InventItemGroupItem { RecId = 3, ItemDataAreaId = "dat", ItemId = "SRV-01", ItemGroupId = "SERVICES" });
        // Tracking is assigned but no dimension is active, so a site/warehouse journal line is valid.
        db.Set<IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities.EcoResTrackingDimensionGroupItem>().Add(
            new IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities.EcoResTrackingDimensionGroupItem
            { RecId = 1, ItemDataAreaId = "dat", ItemId = "ITEM-01", TrackingDimensionGroup = 1 });
    }

    private static void Account(FullCycleContext db, long recId, string id, MainAccountType type)
    {
        db.Set<MainAccount>().Add(new MainAccount { RecId = recId, DataAreaId = "dat", MainAccountId = id, Type = type, LedgerChartOfAccounts = 1 });
        db.Set<DimensionAttributeValueCombination>().Add(new DimensionAttributeValueCombination
        { RecId = recId, DataAreaId = "dat", MainAccount = recId, MainAccountValue = id, DisplayValue = id });
    }

    private static void Posting(FullCycleContext db, long recId, InventAccountType type, long dimension, string? itemId = null) =>
        db.Set<InventPosting>().Add(new InventPosting
        {
            RecId = recId, DataAreaId = "dat", InventAccountType = type,
            ItemCode = itemId == null ? TableGroupAll.All : TableGroupAll.Table, ItemRelation = itemId ?? string.Empty,
            CustVendCode = TableGroupAll.All, LedgerDimension = dimension
        });

    // ---- Harness ----------------------------------------------------------------------------

    private static ISysNumberSequenceService Sequence() =>
        DispatchProxy.Create<ISysNumberSequenceService, FullCycleSequenceProxy>();

    public class FullCycleSequenceProxy : DispatchProxy
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
        private IDbContextTransaction? _tx;
        public DbContext Context => context;
        public Task<int> CompleteAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
        public IGenericRepository<T> Repository<T>() where T : class
        {
            var repository = DispatchProxy.Create<IGenericRepository<T>, FullCycleRepositoryProxy<T>>();
            ((FullCycleRepositoryProxy<T>)(object)repository).Context = context;
            return repository;
        }
        public async Task BeginTransactionAsync(CancellationToken ct = default) => _tx = await context.Database.BeginTransactionAsync(ct);
        public async Task CommitTransactionAsync(CancellationToken ct = default) { if (_tx != null) { await _tx.CommitAsync(ct); _tx = null; } }
        public async Task RollbackTransactionAsync(CancellationToken ct = default) { if (_tx != null) { await _tx.RollbackAsync(ct); _tx = null; } }
        public void Dispose() { }
    }

    public class FullCycleRepositoryProxy<T> : DispatchProxy where T : class
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

    private sealed class Fixture : IAsyncDisposable
    {
        private readonly SqliteConnection _connection = new("Data Source=:memory:");
        public FullCycleContext Db { get; private set; } = null!;

        public static async Task<Fixture> CreateAsync()
        {
            var fixture = new Fixture();
            await fixture._connection.OpenAsync();
            fixture.Db = new FullCycleContext(new DbContextOptionsBuilder<FullCycleContext>().UseSqlite(fixture._connection).Options);
            await fixture.Db.Database.EnsureCreatedAsync();
            return fixture;
        }

        public async ValueTask DisposeAsync() { await Db.DisposeAsync(); await _connection.DisposeAsync(); }
    }

    private sealed class FullCycleContext(DbContextOptions<FullCycleContext> options) : DbContext(options)
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
            typeof(CustConfirmJour), typeof(CustConfirmTrans),
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
            typeof(InventBatch), typeof(InventSerial), typeof(InventItemLocation),
            // The transfer order and its four movement origins.
            typeof(InventTransferTable), typeof(InventTransferLine), typeof(InventTransferJour),
            typeof(InventTransOriginTransferShip), typeof(InventTransOriginTransferReceive),
            typeof(InventTransOriginTransferTransitFrom), typeof(InventTransOriginTransferTransitTo),
            // The counting journal allocates its voucher from a number sequence it validates first.
            typeof(SysNumberSequence)
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
