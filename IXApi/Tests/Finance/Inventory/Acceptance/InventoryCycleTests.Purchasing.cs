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
    public async Task Vendor_transactions_are_read_only_and_company_scoped()
    {
        await using var fixture = await Fixture.CreateAsync();
        fixture.Db.Set<VendTrans>().AddRange(
            new VendTrans { RecId = 1, DataAreaId = "dat", AccountNum = "V001", TransDate = new DateTime(2026, 1, 1), AmountCur = -100 },
            new VendTrans { RecId = 2, DataAreaId = "usmf", AccountNum = "V001", TransDate = new DateTime(2026, 1, 2), AmountCur = -200 });
        await fixture.Db.SaveChangesAsync();

        var service = new VendTransReadService(new TestUnitOfWork(fixture.Db), new TestCompany());
        var page = await service.SearchAsync(new VendTransQueryDto { AccountNum = " V001 " }, default);

        Assert.Equal(1, page.TotalCount);
        Assert.Equal(1, Assert.Single(page.Rows).RecId);
        Assert.Null(await service.GetAsync(2, default));
        Assert.Equal(-100m, (await service.GetAsync(1, default))!.AmountCur);
    }

    [Fact]
    public async Task Vendor_return_request_validation_preserves_errors_before_posting()
    {
        await using var fixture = await Fixture.CreateAsync();
        var service = new PurchReturnService(new TestUnitOfWork(fixture.Db), new TestCompany(),
            Sequence(), new TestInventNumbers());

        var create = await service.CreateAsync(new PurchReturnCreateDto("VI-1", null, []), default);
        var ship = await service.ShipAsync(1, new PurchReturnShipDto("", null, []), default);
        var credit = await service.CreditAsync(1, new VendCreditNoteDto(""), default);

        Assert.Equal((400, "At least one return line is required."), (create.StatusCode, create.Message));
        Assert.Equal((400, "A return shipment number is required."), (ship.StatusCode, ship.Message));
        Assert.Equal((400, "A vendor credit note number is required."), (credit.StatusCode, credit.Message));
        Assert.Empty(await fixture.Db.Set<PurchTable>().ToListAsync());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Purchase_order_partial_receipts_and_invoices_complete_the_cycle(bool taxable)
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        if (taxable) SeedPurchaseTax(db);
        db.Set<PaymTerm>().Add(new PaymTerm { DataAreaId = "dat", PaymTermId = "Monthly", NumOfMonths = 1, PaymMethod = PaymMethod.Net });
        db.Set<VendTable>().Local.Single().PaymTermId = "Monthly";
        await db.SaveChangesAsync();
        var (orders, confirm, receipts, invoices) = PurchaseServices(db);

        var created = await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 10, 28), "S01", "WH-01", null), default);
        Assert.Equal(200, created.StatusCode);
        var po = await db.Set<PurchTable>().SingleAsync();
        var added = await orders.AddLineAsync(po.RecId, new PurchLineInputDto("ITEM-001", 100, "PCS", 50, 1, null, null, null, null), default);
        Assert.Equal(200, added.StatusCode);
        var line = await db.Set<PurchLine>().SingleAsync();

        // Step 1: approved PO -> ordered inbound 100, no physical stock.
        var sum = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(100m, sum.Ordered);
        Assert.Equal(0m, sum.PhysicalInvent);
        Assert.Equal(StatusReceipt.Ordered, (await db.Set<InventTrans>().SingleAsync()).StatusReceipt);

        // Step 2: confirmation stores a version, no stock effect.
        Assert.Equal(200, (await confirm.ConfirmAsync(po.RecId, default)).StatusCode);
        Assert.Single(await db.Set<VendPurchOrderJour>().ToListAsync());
        Assert.Equal(0m, (await db.Set<InventSum>().SingleAsync()).PhysicalInvent);

        // Step 3: product receipt 60 -> physical 60, remain physical 40, remain financial 100.
        Assert.Equal(200, (await receipts.PostAsync(po.RecId, new ProductReceiptPostDto("PR-0001", new DateTime(2026, 10, 28), [new(line.RecId, 60)]), default)).StatusCode);
        line = await db.Set<PurchLine>().SingleAsync();
        sum = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(60m, sum.PhysicalInvent);
        Assert.Equal(40m, sum.Ordered);
        Assert.Equal(40m, line.RemainPurchPhysical);
        Assert.Equal(100m, line.RemainPurchFinancial);

        // Negative: invoice more than received, over-receipt, duplicate receipt number.
        Assert.Equal(422, (await invoices.PostAsync(po.RecId, new VendorInvoicePostDto("INV-X", new DateTime(2026, 10, 29), [new(line.RecId, 61)]), default)).StatusCode);
        Assert.Equal(422, (await receipts.PostAsync(po.RecId, new ProductReceiptPostDto("PR-0009", null, [new(line.RecId, 41)]), default)).StatusCode);
        Assert.Equal(422, (await receipts.PostAsync(po.RecId, new ProductReceiptPostDto("PR-0001", null, [new(line.RecId, 1)]), default)).StatusCode);

        // Step 4: invoice 60 -> no extra physical stock, financial value 3,000, remain financial 40.
        Assert.Equal(200, (await invoices.PostAsync(po.RecId, new VendorInvoicePostDto("INV-0001", new DateTime(2026, 10, 29), [new(line.RecId, 60)]), default)).StatusCode);
        sum = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(60m, sum.PhysicalInvent);
        Assert.Equal(3000m, sum.PostedValue);
        Assert.Equal(40m, (await db.Set<PurchLine>().SingleAsync()).RemainPurchFinancial);
        Assert.Equal(422, (await invoices.PostAsync(po.RecId, new VendorInvoicePostDto("INV-0001", null, [new(line.RecId, 1)]), default)).StatusCode);

        // Steps 5-6: receive and invoice the remaining 40.
        Assert.Equal(200, (await receipts.PostAsync(po.RecId, new ProductReceiptPostDto("PR-0002", new DateTime(2026, 10, 30), [new(line.RecId, 40)]), default)).StatusCode);
        Assert.Equal(200, (await invoices.PostAsync(po.RecId, new VendorInvoicePostDto("INV-0002", new DateTime(2026, 10, 31), [new(line.RecId, 40)]), default)).StatusCode);

        line = await db.Set<PurchLine>().SingleAsync();
        po = await db.Set<PurchTable>().SingleAsync();
        sum = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(0m, line.RemainPurchPhysical);
        Assert.Equal(0m, line.RemainPurchFinancial);
        Assert.Equal(PurchStatus.Invoiced, po.PurchStatus);
        Assert.Equal(100m, sum.PhysicalInvent);
        Assert.Equal(0m, sum.Ordered);
        Assert.Equal(0m, sum.Received);
        Assert.Equal(5000m, sum.PostedValue);
        Assert.Equal(0m, sum.PhysicalValue);

        var trans = await db.Set<InventTrans>().ToListAsync();
        Assert.All(trans, t => Assert.Equal(StatusReceipt.Purchased, t.StatusReceipt));
        Assert.Equal(100m, trans.Sum(t => t.Qty));
        Assert.Equal(5000m, trans.Sum(t => t.CostAmountPosted));
        Assert.Equal(2, await db.Set<VendPackingSlipJour>().CountAsync());
        Assert.Equal(2, await db.Set<VendInvoiceJour>().CountAsync());
        Assert.Equal(taxable ? -5750m : -5000m, (await db.Set<VendTrans>().ToListAsync()).Sum(x => x.AmountCur));
        Assert.All(await db.Set<VendTrans>().ToListAsync(), row => Assert.Equal(row.TransDate.AddMonths(1), row.DueDate));
        Assert.Equal(2, await db.Set<VendInvoicePackingSlipQuantityMatch>().CountAsync());
        Assert.Equal(taxable ? 750m : 0m, (await db.Set<TaxTrans>().ToListAsync()).Sum(x => x.TaxAmount));
        var ledger = await db.Set<GeneralJournalAccountEntry>().ToListAsync();
        Assert.Equal(0m, ledger.Sum(x => x.AccountingCurrencyAmount));                     // balanced
        Assert.Equal(taxable ? 5750m : 5000m, ledger.Where(x => x.AccountingCurrencyAmount > 0).Sum(x => x.AccountingCurrencyAmount));
    }

    /// <summary>
    /// The rule §10 states outright: a registration and the product receipt that follows it are
    /// <b>not</b> two receipts of the same goods. Register 60 of 100, then receive the whole 100;
    /// physical on-hand ends at 100, not 160, and the registered bucket is emptied again.
    /// </summary>
    [Fact]
    public async Task Registering_an_arrival_then_receiving_does_not_count_the_quantity_twice()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        await db.SaveChangesAsync();
        var (orders, _, receipts, _) = PurchaseServices(db);
        var registrations = new PurchRegistrationService(new TestUnitOfWork(db), new TestCompany());

        Assert.Equal(200, (await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 10, 28), "S01", "WH-01", null), default)).StatusCode);
        var po = await db.Set<PurchTable>().SingleAsync();
        Assert.Equal(200, (await orders.AddLineAsync(po.RecId, new PurchLineInputDto("ITEM-001", 100, "PCS", 50, 1, null, null, null, null), default)).StatusCode);
        var line = await db.Set<PurchLine>().SingleAsync();
        var ledgerBefore = (await db.Set<GeneralJournalAccountEntry>().ToListAsync()).Count;

        var result = await registrations.RegisterAsync(po.RecId,
            new PurchRegistrationPostDto(new DateTime(2026, 10, 28), [new(line.RecId, 60)]), default);

        Assert.True(result.StatusCode == 200, result.Message);
        var sum = await db.Set<InventSum>().SingleAsync();
        // Registered is its own bucket: it leaves ordered and does not become physical on-hand.
        Assert.Equal(60m, sum.Registered);
        Assert.Equal(40m, sum.Ordered);
        Assert.Equal(0m, sum.PhysicalInvent);
        Assert.Equal(0m, sum.PhysicalValue);
        // Registration posts nothing and does not consume the line's remainder.
        Assert.Equal(ledgerBefore, (await db.Set<GeneralJournalAccountEntry>().ToListAsync()).Count);
        Assert.Empty(await db.Set<VendTrans>().ToListAsync());
        Assert.Empty(await db.Set<VendPackingSlipJour>().ToListAsync());
        line = await db.Set<PurchLine>().SingleAsync();
        Assert.Equal(100m, line.RemainPurchPhysical);

        // The receipt then posts the whole 100 once, consuming the registered 60 first.
        Assert.Equal(200, (await receipts.PostAsync(po.RecId,
            new ProductReceiptPostDto("PR-0001", new DateTime(2026, 10, 29), [new(line.RecId, 100)]), default)).StatusCode);

        sum = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(100m, sum.PhysicalInvent);          // not 160
        Assert.Equal(0m, sum.Registered);                // the bucket is emptied, not stranded
        Assert.Equal(0m, sum.Ordered);
        Assert.Equal(5000m, sum.PhysicalValue);
        Assert.Equal(0m, (await db.Set<PurchLine>().SingleAsync()).RemainPurchPhysical);
        // Every ordered or registered row has moved on; none is left open.
        Assert.DoesNotContain(await db.Set<InventTrans>().ToListAsync(),
            t => t.StatusReceipt is StatusReceipt.Ordered or StatusReceipt.Registered);
    }

    /// <summary>
    /// Registering the same goods twice runs out of ordered supply and is refused, which is what
    /// stops a registration from inflating on-hand by repetition.
    /// </summary>
    [Fact]
    public async Task Registering_more_than_the_ordered_supply_is_refused()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        await db.SaveChangesAsync();
        var (orders, _, _, _) = PurchaseServices(db);
        var registrations = new PurchRegistrationService(new TestUnitOfWork(db), new TestCompany());

        Assert.Equal(200, (await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 10, 28), "S01", "WH-01", null), default)).StatusCode);
        var po = await db.Set<PurchTable>().SingleAsync();
        Assert.Equal(200, (await orders.AddLineAsync(po.RecId, new PurchLineInputDto("ITEM-001", 100, "PCS", 50, 1, null, null, null, null), default)).StatusCode);
        var line = await db.Set<PurchLine>().SingleAsync();

        Assert.Equal(200, (await registrations.RegisterAsync(po.RecId,
            new PurchRegistrationPostDto(new DateTime(2026, 10, 28), [new(line.RecId, 100)]), default)).StatusCode);
        var refused = await registrations.RegisterAsync(po.RecId,
            new PurchRegistrationPostDto(new DateTime(2026, 10, 28), [new(line.RecId, 100)]), default);

        Assert.Equal(422, refused.StatusCode);
        Assert.Contains("may already be registered or received", refused.Message);
        // Rolled back: the first registration stands and nothing is doubled.
        var sum = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(100m, sum.Registered);
        Assert.Equal(0m, sum.Ordered);

        // Over-registering against the line's remainder is refused before inventory is touched.
        var tooMuch = await registrations.RegisterAsync(po.RecId,
            new PurchRegistrationPostDto(null, [new(line.RecId, 101)]), default);
        Assert.Equal(422, tooMuch.StatusCode);
        Assert.Contains("exceeds the remaining quantity", tooMuch.Message);
    }

    /// <summary>
    /// The case the cycle had no ending for. Order 100, receive 60, invoice nothing, then cancel:
    /// the 40 that will never arrive is released from ordered supply, and the 60 already delivered
    /// stays invoiceable. Nothing already posted is disturbed and no ledger entry arises, because
    /// ordered supply is neither physical nor financial inventory.
    /// </summary>
    [Fact]
    public async Task Cancelling_a_partly_received_order_releases_the_remainder_and_keeps_the_receipt_invoiceable()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        await db.SaveChangesAsync();
        var (orders, _, receipts, invoices) = PurchaseServices(db);

        Assert.Equal(200, (await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 10, 28), "S01", "WH-01", null), default)).StatusCode);
        var po = await db.Set<PurchTable>().SingleAsync();
        Assert.Equal(200, (await orders.AddLineAsync(po.RecId, new PurchLineInputDto("ITEM-001", 100, "PCS", 50, 1, null, null, null, null), default)).StatusCode);
        var line = await db.Set<PurchLine>().SingleAsync();
        Assert.Equal(200, (await receipts.PostAsync(po.RecId, new ProductReceiptPostDto("PR-0001", new DateTime(2026, 10, 28), [new(line.RecId, 60)]), default)).StatusCode);
        var ledgerBefore = (await db.Set<GeneralJournalAccountEntry>().ToListAsync()).Count;

        var cancelled = await orders.CancelRemainderAsync(po.RecId, default);

        Assert.Equal(200, cancelled.StatusCode);
        line = await db.Set<PurchLine>().SingleAsync();
        var sum = await db.Set<InventSum>().SingleAsync();
        // The 40 expected units are gone from ordered supply; the 60 received are untouched.
        Assert.Equal(0m, sum.Ordered);
        Assert.Equal(60m, sum.PhysicalInvent);
        Assert.Equal(0m, line.RemainPurchPhysical);
        // The receipt is still waiting to be invoiced, which is the whole point.
        Assert.Equal(60m, line.RemainPurchFinancial);
        Assert.Equal(PurchStatus.Received, line.PurchStatus);
        // No ordered inventory transaction is left open.
        Assert.DoesNotContain(await db.Set<InventTrans>().ToListAsync(), t => t.StatusReceipt == StatusReceipt.Ordered);
        // Cancelling ordered supply has no accounting effect.
        Assert.Equal(ledgerBefore, (await db.Set<GeneralJournalAccountEntry>().ToListAsync()).Count);

        // And the delivered 60 can still be invoiced afterwards, closing the order properly.
        Assert.Equal(200, (await invoices.PostAsync(po.RecId, new VendorInvoicePostDto("INV-0001", new DateTime(2026, 10, 29), [new(line.RecId, 60)]), default)).StatusCode);
        line = await db.Set<PurchLine>().SingleAsync();
        Assert.Equal(0m, line.RemainPurchFinancial);
        Assert.Equal(PurchStatus.Invoiced, (await db.Set<PurchTable>().SingleAsync()).PurchStatus);
        Assert.Equal(3000m, (await db.Set<InventSum>().SingleAsync()).PostedValue);
    }

    /// <summary>
    /// An order that received nothing is cancelled outright: the line and the order both reach
    /// Canceled and all the ordered supply is released.
    /// </summary>
    [Fact]
    public async Task Cancelling_an_untouched_order_cancels_the_line_and_the_order()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        await db.SaveChangesAsync();
        var (orders, _, receipts, _) = PurchaseServices(db);

        Assert.Equal(200, (await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 10, 28), "S01", "WH-01", null), default)).StatusCode);
        var po = await db.Set<PurchTable>().SingleAsync();
        Assert.Equal(200, (await orders.AddLineAsync(po.RecId, new PurchLineInputDto("ITEM-001", 100, "PCS", 50, 1, null, null, null, null), default)).StatusCode);
        var line = await db.Set<PurchLine>().SingleAsync();
        Assert.Equal(100m, (await db.Set<InventSum>().SingleAsync()).Ordered);

        Assert.Equal(200, (await orders.CancelRemainderAsync(po.RecId, default)).StatusCode);

        line = await db.Set<PurchLine>().SingleAsync();
        po = await db.Set<PurchTable>().SingleAsync();
        Assert.Equal(PurchStatus.Canceled, line.PurchStatus);
        Assert.Equal(PurchStatus.Canceled, po.PurchStatus);
        Assert.Equal(0m, line.RemainPurchPhysical);
        Assert.Equal(0m, line.RemainPurchFinancial);
        var sum = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(0m, sum.Ordered);
        Assert.Equal(0m, sum.PhysicalInvent);

        // Cancelling twice, and receiving against a cancelled order, are both refused.
        Assert.Equal(422, (await orders.CancelRemainderAsync(po.RecId, default)).StatusCode);
        Assert.Equal(422, (await receipts.PostAsync(po.RecId, new ProductReceiptPostDto("PR-0002", null, [new(line.RecId, 10)]), default)).StatusCode);
    }

    /// <summary>
    /// A fully received and invoiced order has no remainder, so cancellation is refused by name
    /// rather than quietly doing nothing.
    /// </summary>
    [Fact]
    public async Task Cancelling_a_fully_received_order_is_refused()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        await db.SaveChangesAsync();
        var (orders, _, receipts, invoices) = PurchaseServices(db);

        Assert.Equal(200, (await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 10, 28), "S01", "WH-01", null), default)).StatusCode);
        var po = await db.Set<PurchTable>().SingleAsync();
        Assert.Equal(200, (await orders.AddLineAsync(po.RecId, new PurchLineInputDto("ITEM-001", 10, "PCS", 50, 1, null, null, null, null), default)).StatusCode);
        var line = await db.Set<PurchLine>().SingleAsync();
        Assert.Equal(200, (await receipts.PostAsync(po.RecId, new ProductReceiptPostDto("PR-0001", new DateTime(2026, 10, 28), [new(line.RecId, 10)]), default)).StatusCode);
        Assert.Equal(200, (await invoices.PostAsync(po.RecId, new VendorInvoicePostDto("INV-0001", new DateTime(2026, 10, 29), [new(line.RecId, 10)]), default)).StatusCode);

        var refused = await orders.CancelRemainderAsync(po.RecId, default);

        Assert.Equal(422, refused.StatusCode);
        Assert.Contains("no unreceived quantity left to cancel", refused.Message);
        // The invoiced order is left exactly as it was.
        Assert.Equal(PurchStatus.Invoiced, (await db.Set<PurchTable>().SingleAsync()).PurchStatus);
    }

    /// <summary>
    /// If the line's remainder and its ordered inventory transactions disagree, the supply is not
    /// understood well enough to release the right quantity, so the whole cancellation rolls back
    /// rather than cancelling a quantity that might belong to a receipt.
    /// </summary>
    [Fact]
    public async Task Cancellation_is_refused_when_ordered_inventory_does_not_match_the_remainder()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        await db.SaveChangesAsync();
        var (orders, _, _, _) = PurchaseServices(db);

        Assert.Equal(200, (await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 10, 28), "S01", "WH-01", null), default)).StatusCode);
        var po = await db.Set<PurchTable>().SingleAsync();
        Assert.Equal(200, (await orders.AddLineAsync(po.RecId, new PurchLineInputDto("ITEM-001", 100, "PCS", 50, 1, null, null, null, null), default)).StatusCode);
        // Desynchronise the ordered transaction from the line, as stale data would be.
        var ordered = await db.Set<InventTrans>().SingleAsync();
        ordered.Qty = 70m;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var refused = await orders.CancelRemainderAsync(po.RecId, default);

        Assert.Equal(422, refused.StatusCode);
        Assert.Contains("does not match the uninvoiced remainder", refused.Message);
        // Rolled back: the line keeps its remainder and the order stays open.
        var line = await db.Set<PurchLine>().SingleAsync();
        Assert.Equal(100m, line.RemainPurchPhysical);
        Assert.Equal(PurchStatus.Backorder, (await db.Set<PurchTable>().SingleAsync()).PurchStatus);
    }

    /// <summary>
    /// Reducing an open quantity is not cancelling the line: order 100, reduce by 40, and 60 stays
    /// expected and invoiceable. The released 40 leaves ordered supply, the line keeps the status it
    /// had because it is still waiting for goods, and the ordered inventory transaction keeps its
    /// remaining 60 as Ordered rather than being closed.
    /// </summary>
    [Fact]
    public async Task Reducing_a_line_quantity_releases_only_that_part_and_keeps_the_rest_expected()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        await db.SaveChangesAsync();
        var (orders, _, receipts, invoices) = PurchaseServices(db);

        Assert.Equal(200, (await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 10, 28), "S01", "WH-01", null), default)).StatusCode);
        var po = await db.Set<PurchTable>().SingleAsync();
        Assert.Equal(200, (await orders.AddLineAsync(po.RecId, new PurchLineInputDto("ITEM-001", 100, "PCS", 50, 1, null, null, null, null), default)).StatusCode);
        var line = await db.Set<PurchLine>().SingleAsync();
        var ledgerBefore = (await db.Set<GeneralJournalAccountEntry>().ToListAsync()).Count;

        var reduced = await orders.CancelLineRemainderAsync(line.RecId, 40m, default);

        Assert.Equal(200, reduced.StatusCode);
        Assert.Contains("quantity reduced", reduced.Message);
        line = await db.Set<PurchLine>().SingleAsync();
        // 60 is still coming and still invoiceable; only the 40 was released.
        Assert.Equal(60m, line.RemainPurchPhysical);
        Assert.Equal(60m, line.RemainPurchFinancial);
        Assert.Equal(60m, (await db.Set<InventSum>().SingleAsync()).Ordered);
        // A line still expecting goods keeps the status it had: only the quantity was trimmed.
        Assert.Equal(PurchStatus.Backorder, line.PurchStatus);
        Assert.Equal(PurchStatus.Backorder, (await db.Set<PurchTable>().SingleAsync()).PurchStatus);
        // The part still expected stays Ordered rather than being closed out.
        var ordered = await db.Set<InventTrans>().SingleAsync(x => x.StatusReceipt == StatusReceipt.Ordered);
        Assert.Equal(60m, ordered.Qty);
        // Releasing ordered supply has no accounting effect.
        Assert.Equal(ledgerBefore, (await db.Set<GeneralJournalAccountEntry>().ToListAsync()).Count);

        // The reduced quantity still receives and invoices normally, closing the order at 60.
        Assert.Equal(200, (await receipts.PostAsync(po.RecId, new ProductReceiptPostDto("PR-0001", new DateTime(2026, 10, 29), [new(line.RecId, 60)]), default)).StatusCode);
        Assert.Equal(200, (await invoices.PostAsync(po.RecId, new VendorInvoicePostDto("INV-0001", new DateTime(2026, 10, 30), [new(line.RecId, 60)]), default)).StatusCode);
        Assert.Equal(PurchStatus.Invoiced, (await db.Set<PurchTable>().SingleAsync()).PurchStatus);
        Assert.Equal(3000m, (await db.Set<InventSum>().SingleAsync()).PostedValue);
    }

    /// <summary>
    /// Cancelling one line of a two-line order leaves the other line untouched and the order open,
    /// which is what order-level cancellation could not express.
    /// </summary>
    [Fact]
    public async Task Cancelling_one_line_leaves_the_other_line_and_the_order_open()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        await db.SaveChangesAsync();
        var (orders, _, _, _) = PurchaseServices(db);

        Assert.Equal(200, (await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 10, 28), "S01", "WH-01", null), default)).StatusCode);
        var po = await db.Set<PurchTable>().SingleAsync();
        Assert.Equal(200, (await orders.AddLineAsync(po.RecId, new PurchLineInputDto("ITEM-001", 100, "PCS", 50, 1, null, null, null, null), default)).StatusCode);
        Assert.Equal(200, (await orders.AddLineAsync(po.RecId, new PurchLineInputDto("ITEM-001", 20, "PCS", 50, 1, null, null, null, null), default)).StatusCode);
        var lines = (await db.Set<PurchLine>().ToListAsync()).OrderBy(x => x.LineNumber).ToList();
        var first = lines[0];
        // Both lines are the same item, so they share one InventSum: 100 + 20 ordered.
        Assert.Equal(120m, (await db.Set<InventSum>().SingleAsync()).Ordered);

        Assert.Equal(200, (await orders.CancelLineRemainderAsync(first.RecId, null, default)).StatusCode);

        lines = (await db.Set<PurchLine>().ToListAsync()).OrderBy(x => x.LineNumber).ToList();
        first = lines[0];
        var second = lines[1];
        Assert.Equal(PurchStatus.Canceled, first.PurchStatus);
        // The untouched line keeps its whole quantity, and only its 20 is left on order.
        Assert.Equal(PurchStatus.Backorder, second.PurchStatus);
        Assert.Equal(20m, second.RemainPurchPhysical);
        Assert.Equal(20m, second.RemainPurchFinancial);
        Assert.Equal(20m, (await db.Set<InventSum>().SingleAsync()).Ordered);
        // One cancelled line does not close an order that still expects the other.
        Assert.Equal(PurchStatus.Backorder, (await db.Set<PurchTable>().SingleAsync()).PurchStatus);

        // Cancelling the same line twice is refused by name.
        var refused = await orders.CancelLineRemainderAsync(first.RecId, null, default);
        Assert.Equal(422, refused.StatusCode);
        Assert.Contains("already canceled", refused.Message);
    }

    /// <summary>
    /// Asking to cancel more than the line has left, or nothing at all, is refused with the figures
    /// named and the line left exactly as it was.
    /// </summary>
    [Fact]
    public async Task Reducing_a_line_by_more_than_it_expects_is_refused()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        await db.SaveChangesAsync();
        var (orders, _, receipts, _) = PurchaseServices(db);

        Assert.Equal(200, (await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 10, 28), "S01", "WH-01", null), default)).StatusCode);
        var po = await db.Set<PurchTable>().SingleAsync();
        Assert.Equal(200, (await orders.AddLineAsync(po.RecId, new PurchLineInputDto("ITEM-001", 100, "PCS", 50, 1, null, null, null, null), default)).StatusCode);
        var line = await db.Set<PurchLine>().SingleAsync();
        // 60 received leaves 40 unreceived, so 50 is more than the line can release.
        Assert.Equal(200, (await receipts.PostAsync(po.RecId, new ProductReceiptPostDto("PR-0001", new DateTime(2026, 10, 28), [new(line.RecId, 60)]), default)).StatusCode);

        var refused = await orders.CancelLineRemainderAsync(line.RecId, 50m, default);

        Assert.Equal(422, refused.StatusCode);
        Assert.Contains("which is less than the 50 requested", refused.Message);
        Assert.Equal(422, (await orders.CancelLineRemainderAsync(line.RecId, 0m, default)).StatusCode);
        // Rolled back on both counts: the remainder and the ordered supply are unchanged.
        line = await db.Set<PurchLine>().SingleAsync();
        Assert.Equal(40m, line.RemainPurchPhysical);
        Assert.Equal(100m, line.RemainPurchFinancial);
        Assert.Equal(40m, (await db.Set<InventSum>().SingleAsync()).Ordered);
    }

    [Fact]
    public async Task Purchase_line_rejects_warehouse_from_another_site_and_blocked_vendor()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedPurchaseMasterData(db);
        db.Set<InventLocation>().Add(new InventLocation { RecId = 2, DataAreaId = "dat", InventLocationId = "WH-03", InventSiteId = "S02" });
        db.Set<VendTable>().Add(new VendTable { RecId = 2, DataAreaId = "dat", AccountNum = "V-BLOCK", Currency = "SAR", Blocked = CustVendBlocked.All });
        await db.SaveChangesAsync();
        var (orders, _, _, _) = PurchaseServices(db);

        Assert.Equal(422, (await orders.CreateAsync(new PurchOrderCreateDto("V-BLOCK", null, "SAR", null, "S01", "WH-01", null), default)).StatusCode);
        Assert.Equal(422, (await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "SAR", null, "S01", "WH-03", null), default)).StatusCode);
        Assert.Empty(await db.Set<PurchTable>().ToListAsync());
    }

    [Fact]
    public async Task A_foreign_currency_purchase_invoice_stores_inventory_cost_in_the_accounting_currency()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        SeedRate(db, "USD", "SAR", 3.75m);
        await db.SaveChangesAsync();
        var numbers = new TestInventNumbers();
        var (orders, _, receipts, vendorInvoices) = PurchaseServices(db, numbers);

        // Buy 100 @ 60 USD against a ledger that keeps its accounts in SAR.
        await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "USD", new DateTime(2026, 10, 1), "S01", "WH-01", null), default);
        var po = await db.Set<PurchTable>().SingleAsync();
        await orders.AddLineAsync(po.RecId, new PurchLineInputDto("ITEM-001", 100, "PCS", 60, 1, null, null, null, null), default);
        var pl = await db.Set<PurchLine>().SingleAsync();
        Assert.Equal(200, (await receipts.PostAsync(po.RecId,
            new ProductReceiptPostDto("PR-1", new DateTime(2026, 10, 2), [new(pl.RecId, 100)]), default)).StatusCode);
        Assert.Equal(22500m, (await db.Set<InventSum>().SingleAsync()).PhysicalValue);
        Assert.Equal(22500m, (await db.Set<InventTrans>().SingleAsync(x => x.StatusReceipt == StatusReceipt.Received)).CostAmountPhysical);
        Assert.Equal(22500m, (await db.Set<VendPackingSlipTrans>().SingleAsync()).ValueMst);
        var invoice = await vendorInvoices.PostAsync(po.RecId,
            new VendorInvoicePostDto("VI-1", new DateTime(2026, 10, 3), [new(pl.RecId, 100)]), default);
        Assert.True(invoice.StatusCode == 200, invoice.Message);

        // The point of the test: inventory value is held in the accounting currency. 6,000 USD at
        // 3.75 is 22,500 SAR, so the cost per unit that every later settlement reads is 225, not 60.
        var purchased = await db.Set<InventTrans>().AsNoTracking()
            .SingleAsync(x => x.StatusReceipt == StatusReceipt.Purchased);
        Assert.Equal(22500m, purchased.CostAmountPosted);
        var sum = await db.Set<InventSum>().AsNoTracking().SingleAsync();
        Assert.Equal(100m, sum.PostedQty);
        Assert.Equal(22500m, sum.PostedValue);
        Assert.Equal(0m, sum.PhysicalValue);

        // The vendor is owed in the order currency, and the same debt translated.
        var vendTrans = await db.Set<VendTrans>().AsNoTracking().SingleAsync();
        Assert.Equal("USD", vendTrans.CurrencyCode);
        Assert.Equal(-6000m, vendTrans.AmountCur);
        Assert.Equal(-22500m, vendTrans.AmountMst);

        // The invoice voucher balances in the accounting currency.
        var journal = await db.Set<VendInvoiceJour>().AsNoTracking().SingleAsync();
        var entryIds = await db.Set<GeneralJournalEntry>().AsNoTracking()
            .Where(x => x.SubledgerVoucher == journal.LedgerVoucher).Select(x => x.RecId).ToListAsync();
        var lines = await db.Set<GeneralJournalAccountEntry>().AsNoTracking()
            .Where(x => entryIds.Contains(x.GeneralJournalEntry)).ToListAsync();
        Assert.Equal(0m, lines.Sum(x => x.AccountingCurrencyAmount));
        var inventory = lines.Single(x => x.PostingType == LedgerPostingType.PurchReceipt
            && x.AccountingCurrencyAmount > 0);
        Assert.Equal("USD", inventory.TransactionCurrencyCode);
        Assert.Equal(6000m, inventory.TransactionCurrencyAmount);
        Assert.Equal(22500m, inventory.AccountingCurrencyAmount);
    }

    [Fact]
    public async Task Foreign_currency_product_receipt_without_rate_rolls_back()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        await db.SaveChangesAsync();
        var (orders, _, receipts, _) = PurchaseServices(db);
        Assert.Equal(200, (await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "USD", new DateTime(2026, 10, 1), "S01", "WH-01", null), default)).StatusCode);
        var order = await db.Set<PurchTable>().SingleAsync();
        Assert.Equal(200, (await orders.AddLineAsync(order.RecId, new PurchLineInputDto("ITEM-001", 10, "PCS", 60, 1, null, null, null, null), default)).StatusCode);
        var line = await db.Set<PurchLine>().SingleAsync();

        var result = await receipts.PostAsync(order.RecId, new ProductReceiptPostDto("NO-RATE", new DateTime(2026, 10, 2), [new(line.RecId, 10)]), default);

        Assert.Equal(422, result.StatusCode);
        Assert.Contains("No exchange rate is available", result.Message);
        Assert.Empty(await db.Set<VendPackingSlipJour>().ToListAsync());
        Assert.Equal(0m, (await db.Set<InventSum>().SingleAsync()).PhysicalInvent);
        Assert.Equal(0m, (await db.Set<InventSum>().SingleAsync()).PhysicalValue);
    }

    [Fact]
    public async Task Vendor_payments_settle_partially_then_fully_preserve_overpayment_and_reject_conflicting_retries()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        db.Add(new MainAccount { RecId = 10, DataAreaId = "dat", MainAccountId = "BANK" });
        db.Add(new DimensionAttributeValueCombination { RecId = 10, DataAreaId = "dat", MainAccount = 10, DisplayValue = "BANK" });
        db.Add(new BankAccountTable { DataAreaId = "dat", AccountId = "BANK-1", LedgerDimension = 10, CurrencyCode = "SAR" });
        var invoice = new VendTrans { DataAreaId = "dat", AccountNum = "V-0001", Invoice = "INV-1", CurrencyCode = "SAR",
            AmountCur = -100, AmountMst = -100, TransDate = new DateTime(2026, 10, 1), DueDate = new DateTime(2026, 10, 31), TransType = LedgerTransType.Purch };
        db.Add(invoice);
        await db.SaveChangesAsync();
        VendorOpenBalance.Add(db, invoice);
        // Vendor payment validates the method of payment against its setup table.
        db.Set<VendPaymModeTable>().Add(new VendPaymModeTable { RecId = 1, DataAreaId = "dat", PaymMode = "BANK" });
        await db.SaveChangesAsync();
        var payments = new VendPaymentService(new TestUnitOfWork(db), new TestUser(), new TestCompany(), Sequence());
        var request = new VendPaymentPostDto("V-0001", "BANK-1", 40, "VP1", new DateTime(2026, 10, 15), "BANK", "INV-1");
        Assert.Equal(200, (await payments.PostAsync(request, default)).StatusCode);
        Assert.Equal(-60m, (await db.Set<VendTransOpen>().SingleAsync()).AmountCur);
        Assert.Equal(200, (await payments.PostAsync(request, default)).StatusCode);
        Assert.Equal(422, (await payments.PostAsync(request with { Amount = 41 }, default)).StatusCode);
        Assert.Equal(422, (await payments.PostAsync(request with { VendAccount = "OTHER" }, default)).StatusCode);
        Assert.Equal(422, (await payments.PostAsync(request with { RequestId = "BAD", SettleInvoiceId = "MISSING" }, default)).StatusCode);
        (await db.Set<BankAccountTable>().SingleAsync()).CurrencyCode = "USD";
        await db.SaveChangesAsync();
        Assert.Equal(422, (await payments.PostAsync(request with { RequestId = "FX" }, default)).StatusCode);
        (await db.Set<BankAccountTable>().SingleAsync()).CurrencyCode = "SAR";
        (await db.Set<VendTable>().SingleAsync()).Blocked = CustVendBlocked.Payment;
        await db.SaveChangesAsync();
        Assert.Equal(422, (await payments.PostAsync(request with { RequestId = "BLOCKED" }, default)).StatusCode);
        (await db.Set<VendTable>().SingleAsync()).Blocked = CustVendBlocked.No;
        await db.SaveChangesAsync();
        Assert.Equal(200, (await payments.PostAsync(request with { RequestId = "VP2", Amount = 70 }, default)).StatusCode);
        var outstanding = await db.Set<VendTransOpen>().SingleAsync();
        Assert.Equal(10m, outstanding.AmountCur);
        Assert.Equal(LedgerTransType.Payment, (await db.Set<VendTrans>().SingleAsync(x => x.RecId == outstanding.RefRecId)).TransType);
        Assert.Equal(-100m, (await db.Set<VendTrans>().SingleAsync(x => x.Invoice == "INV-1")).SettleAmountCur);
        Assert.Equal(2, await db.Set<VendSettlement>().CountAsync());
        Assert.Equal(2, await db.Set<LedgerJournalTable>().CountAsync());
        Assert.Equal(2, await db.Set<BankAccountTrans>().CountAsync());
        Assert.Equal(-110m, (await db.Set<BankAccountTrans>().ToListAsync()).Sum(x => x.AmountCur));
        Assert.Equal(0m, (await db.Set<GeneralJournalAccountEntry>().ToListAsync()).Sum(x => x.AccountingCurrencyAmount));
        Assert.Empty(await db.Set<InventTrans>().ToListAsync());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Vendor_return_ships_at_original_cost_and_vendor_credit_reduces_payable(bool taxable)
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        if (taxable) SeedPurchaseTax(db);
        await db.SaveChangesAsync();
        var numbers = new TestInventNumbers();
        var (orders, _, receipts, invoices) = PurchaseServices(db, numbers);
        await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 10, 1), "S01", "WH-01", null), default);
        var po = await db.Set<PurchTable>().SingleAsync();
        await orders.AddLineAsync(po.RecId, new PurchLineInputDto("ITEM-001", 100, "PCS", 60, 1, null, null, null, null), default);
        var pl = await db.Set<PurchLine>().SingleAsync();
        await receipts.PostAsync(po.RecId, new ProductReceiptPostDto("PR-1", new DateTime(2026, 10, 2), [new(pl.RecId, 100)]), default);
        await invoices.PostAsync(po.RecId, new VendorInvoicePostDto("VI-1", new DateTime(2026, 10, 3), [new(pl.RecId, 100)]), default);
        var invoiced = await db.Set<VendInvoiceTrans>().SingleAsync();
        if (taxable)
        {
            // A later rate change must not change the tax reversed from the original invoice.
            (await db.Set<TaxData>().SingleAsync()).TaxValue = 20m;
            await db.SaveChangesAsync();
        }
        var returns = new PurchReturnService(new TestUnitOfWork(db), new TestCompany(), Sequence(), numbers);

        Assert.Equal(422, (await returns.CreateAsync(new PurchReturnCreateDto("VI-1", null, [new(invoiced.RecId, 101)]), default)).StatusCode);
        Assert.Equal(200, (await returns.CreateAsync(new PurchReturnCreateDto("VI-1", "RMA-V-7", [new(invoiced.RecId, 10)]), default)).StatusCode);
        var ret = await db.Set<PurchTable>().SingleAsync(x => x.PurchaseType == PurchaseType.ReturnItem);
        var retLine = await db.Set<PurchLine>().SingleAsync(x => x.PurchId == ret.PurchId);
        Assert.Equal(-10m, retLine.PurchQty);
        Assert.Equal(10m, (await db.Set<InventSum>().SingleAsync()).OnOrder);
        Assert.Equal(422, (await invoices.PostAsync(ret.RecId, new VendorInvoicePostDto("VI-X", null, [new(retLine.RecId, 1)]), default)).StatusCode);

        // Ship back 10 -> physical 90 (issue); then the vendor credit note finalizes it at the original 60.
        Assert.Equal(200, (await returns.ShipAsync(ret.RecId, new PurchReturnShipDto("RS-1", new DateTime(2026, 10, 5), [new(retLine.RecId, 10)]), default)).StatusCode);
        var sum = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(90m, sum.PhysicalInvent);
        Assert.Equal(10m, sum.Deducted);
        Assert.Equal(0m, sum.OnOrder);
        var vendorCreditLedger = await db.Set<Ledger>().SingleAsync();
        vendorCreditLedger.ReportingCurrency = "USD";
        await db.SaveChangesAsync();
        Assert.Equal(422, (await returns.CreditAsync(ret.RecId,
            new VendCreditNoteDto("VCN-FX", new DateTime(2026, 10, 6)), default)).StatusCode);
        Assert.Empty(await db.Set<VendInvoiceJour>().Where(x => x.PurchId == ret.PurchId).ToListAsync());
        vendorCreditLedger = await db.Set<Ledger>().SingleAsync();
        vendorCreditLedger.ReportingCurrency = "SAR";
        ret = await db.Set<PurchTable>().SingleAsync(x => x.RecId == ret.RecId);
        ret.CurrencyCode = "USD";
        await db.SaveChangesAsync();
        Assert.Equal(422, (await returns.CreditAsync(ret.RecId,
            new VendCreditNoteDto("VCN-FOREIGN", new DateTime(2026, 10, 6)), default)).StatusCode);
        Assert.Empty(await db.Set<VendInvoiceJour>().Where(x => x.PurchId == ret.PurchId).ToListAsync());
        ret = await db.Set<PurchTable>().SingleAsync(x => x.RecId == ret.RecId);
        ret.CurrencyCode = "SAR";
        await db.SaveChangesAsync();
        Assert.Equal(200, (await returns.CreditAsync(ret.RecId, new VendCreditNoteDto("VCN-1", new DateTime(2026, 10, 6)), default)).StatusCode);

        sum = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(90m, sum.PhysicalInvent);
        Assert.Equal(90m, sum.PostedQty);
        Assert.Equal(0m, sum.Deducted);
        Assert.Equal(5400m, sum.PostedValue);
        Assert.Equal(taxable ? -6210m : -5400m, (await db.Set<VendTrans>().ToListAsync()).Sum(x => x.AmountCur));
        Assert.Equal(taxable ? 810m : 0m, (await db.Set<TaxTrans>().ToListAsync()).Sum(x => x.TaxAmount));
        Assert.Equal(PurchStatus.Invoiced, (await db.Set<PurchTable>().SingleAsync(x => x.RecId == ret.RecId)).PurchStatus);
        Assert.Equal(0m, (await db.Set<GeneralJournalAccountEntry>().ToListAsync()).Sum(x => x.AccountingCurrencyAmount));
        Assert.Equal(422, (await returns.CreditAsync(ret.RecId, new VendCreditNoteDto("VCN-1"), default)).StatusCode);
    }

    [Fact]
    public async Task Vendor_return_uses_selected_invoice_even_when_number_is_reused_and_costs_differ()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        db.Add(new VendInvoiceJour { DataAreaId = "dat", InvoiceId = "VI-2", InternalInvoiceId = "OTHER-ID",
            PurchId = "OTHER-PO", OrderAccount = "OTHER", InvoiceAccount = "OTHER", CurrencyCode = "SAR" });
        await db.SaveChangesAsync();
        var numbers = new TestInventNumbers();
        var (orders, _, receipts, invoices) = PurchaseServices(db, numbers);
        Assert.Equal(200, (await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 10, 1), "S01", "WH-01", null), default)).StatusCode);
        var order = await db.Set<PurchTable>().SingleAsync();
        Assert.Equal(200, (await orders.AddLineAsync(order.RecId, new PurchLineInputDto("ITEM-001", 20, "PCS", 60, 1, null, null, null, null), default)).StatusCode);
        var line = await db.Set<PurchLine>().SingleAsync();
        Assert.Equal(200, (await receipts.PostAsync(order.RecId, new ProductReceiptPostDto("PR-1", new DateTime(2026, 10, 2), [new(line.RecId, 20)]), default)).StatusCode);
        Assert.Equal(200, (await invoices.PostAsync(order.RecId, new VendorInvoicePostDto("VI-1", new DateTime(2026, 10, 3), [new(line.RecId, 10, 60)]), default)).StatusCode);
        Assert.Equal(200, (await invoices.PostAsync(order.RecId, new VendorInvoicePostDto("VI-2", new DateTime(2026, 10, 4), [new(line.RecId, 10, 70)]), default)).StatusCode);
        var selected = await db.Set<VendInvoiceTrans>().SingleAsync(x => x.InvoiceId == "VI-2");
        var returnSequence = Sequence();
        await returnSequence.NextAsync("PurchTable", "dat", default);
        var returns = new PurchReturnService(new TestUnitOfWork(db), new TestCompany(), returnSequence, numbers);
        var create = await returns.CreateAsync(new PurchReturnCreateDto("VI-2", null, [new(selected.RecId, 2)]), default);
        Assert.Equal(200, create.StatusCode);
        var returnOrder = await db.Set<PurchTable>().SingleAsync(x => x.PurchaseType == PurchaseType.ReturnItem);
        Assert.Equal("V-0001", returnOrder.InvoiceAccount);
        var returnLine = await db.Set<PurchLine>().SingleAsync(x => x.PurchId == returnOrder.PurchId);
        Assert.Equal(selected.RecId, returnLine.OriginalInvoiceTransRecId);
        Assert.Equal(200, (await returns.ShipAsync(returnOrder.RecId,
            new PurchReturnShipDto("RETURN-1", new DateTime(2026, 10, 5), [new(returnLine.RecId, 2)]), default)).StatusCode);
        var origin = await db.Set<InventTransOrigin>().SingleAsync(x => x.InventTransId == returnLine.InventTransId);
        Assert.Equal(-140m, (await db.Set<InventTrans>().SingleAsync(x => x.InventTransOrigin == origin.RecId)).CostAmountPhysical);
        Assert.Equal(200, (await returns.CreditAsync(returnOrder.RecId, new VendCreditNoteDto("CREDIT-1", new DateTime(2026, 10, 6)), default)).StatusCode);
        Assert.Equal(-140m, (await db.Set<InventTrans>().SingleAsync(x => x.InventTransOrigin == origin.RecId)).CostAmountPosted);
        var persistedReturn = await db.Set<PurchLine>().AsNoTracking().SingleAsync(x => x.RecId == returnLine.RecId);
        Assert.Equal(selected.RecId, persistedReturn.OriginalInvoiceTransRecId);
        Assert.Equal(selected.InventTransId, persistedReturn.InventTransIdReturn);
        var selectedReceipt = await db.Set<InventTrans>().AsNoTracking().SingleAsync(x => x.InvoiceId == selected.InternalInvoiceId && x.StatusReceipt == StatusReceipt.Purchased);
        Assert.Equal(10m, selectedReceipt.Qty);
        var close = await new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence())
            .CloseItemAsync("ITEM-001", new DateTime(2026, 10, 31));
        Assert.Null(close.Error);
        var settlement = Assert.Single(await db.Set<InventSettlement>().Where(x => x.TransRecId != 0).ToListAsync());
        var settledReceipt = await db.Set<InventTrans>().SingleAsync(x => x.RecId == settlement.ReceiptTransRecId);
        Assert.Equal(selected.InternalInvoiceId, settledReceipt.InvoiceId);
        Assert.Equal(-140m, settlement.CostAmountSettled);
    }

    [Fact]
    public async Task Purchase_order_prepayment_definition_respects_the_order_limit_and_is_repeatable()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedPurchaseMasterData(db);
        var category = new EcoResCategory { Name = "Purchase prepayment", IsActive = true };
        db.Add(category);
        await db.SaveChangesAsync();
        var (orders, _, _, _) = PurchaseServices(db);
        var date = new DateTime(2026, 10, 1);
        Assert.Equal(200, (await orders.CreateAsync(new("V-0001", null, "SAR", date, "S01", "WH-01", null), default)).StatusCode);
        var order = await db.Set<PurchTable>().SingleAsync();
        Assert.Equal(200, (await orders.AddLineAsync(order.RecId, new("ITEM-001", 10m, "PCS", 100m, 1m, null, null, null, null), default)).StatusCode);
        Assert.Equal(422, (await orders.DefinePrepaymentAsync(order.RecId, new(1100m, category.RecId, "Deposit"), default)).StatusCode);
        Assert.Empty(await db.Set<PurchPrepayTable>().ToListAsync());
        var input = new PurchPrepaymentDefinitionDto(300m, category.RecId, "Deposit");
        Assert.Equal(200, (await orders.DefinePrepaymentAsync(order.RecId, input, default)).StatusCode);
        Assert.Equal(200, (await orders.DefinePrepaymentAsync(order.RecId, input, default)).StatusCode);
        var definition = await db.Set<PurchPrepayTable>().AsNoTracking().SingleAsync();
        Assert.Equal(order.PurchId, definition.PurchTable);
        Assert.Equal("SAR", definition.Currency);
        Assert.Equal(1, definition.Type);
        Assert.Equal(300m, definition.PrepayAvailable);
        Assert.Equal(1000m, definition.PrepayLimit);
        Assert.Equal(0m, definition.PrepayApplicationRemaining);
        Assert.Empty(await db.Set<VendTrans>().ToListAsync());
        Assert.Empty(await db.Set<GeneralJournalEntry>().ToListAsync());
    }

    [Fact]
    public async Task Purchase_order_prepayment_invoice_uses_the_configured_advance_accounts()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        var category = new EcoResCategory { Name = "Purchase prepayment", IsActive = true };
        db.Add(category);
        await db.SaveChangesAsync();
        db.Add(new VendParameters { DataAreaId = "dat", VendPostingProfile = "GEN", AdvancePostingProfile = "ADV" });
        db.Add(new VendLedgerAccounts { DataAreaId = "dat", PostingProfile = "ADV", AccountCode = AccountCode.All, SummaryLedgerDimension = 2 });
        db.Add(new InventPosting { DataAreaId = "dat", InventAccountType = InventAccountType.PurchAdvance,
            ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 3, CategoryRelation = category.RecId + 1 });
        db.Add(new InventPosting { DataAreaId = "dat", InventAccountType = InventAccountType.PurchAdvance,
            ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 1, CategoryRelation = category.RecId });
        await db.SaveChangesAsync();
        var (orders, _, _, _) = PurchaseServices(db);
        var paymentNumbers = Sequence();
        var invoices = new VendorInvoiceService(new TestUnitOfWork(db), new TestUser(), new TestCompany(), paymentNumbers);
        var date = new DateTime(2026, 10, 1);
        Assert.Equal(200, (await orders.CreateAsync(new("V-0001", null, "SAR", date, "S01", "WH-01", null), default)).StatusCode);
        var order = await db.Set<PurchTable>().SingleAsync();
        Assert.Equal(200, (await orders.AddLineAsync(order.RecId, new("ITEM-001", 10m, "PCS", 100m, 1m, null, null, null, null), default)).StatusCode);
        Assert.Equal(200, (await orders.DefinePrepaymentAsync(order.RecId, new(300m, category.RecId, "Deposit"), default)).StatusCode);
        Assert.Equal(422, (await invoices.PostPrepaymentAsync(order.RecId, "ADV-1", date, 350m, default)).StatusCode);
        Assert.Empty(await db.Set<VendInvoiceJour>().ToListAsync());
        var posted = await invoices.PostPrepaymentAsync(order.RecId, "ADV-1", date, 300m, default);
        Assert.True(posted.StatusCode == 200, posted.Message);
        Assert.Equal(200, (await invoices.PostPrepaymentAsync(order.RecId, "ADV-1", date, 300m, default)).StatusCode);
        Assert.Equal(422, (await invoices.PostPrepaymentAsync(order.RecId, "ADV-1", date, 299m, default)).StatusCode);
        Assert.Equal(422, (await invoices.PostPrepaymentAsync(order.RecId, "ADV-2", date, 1m, default)).StatusCode);

        var invoice = await db.Set<VendInvoiceJour>().AsNoTracking().SingleAsync();
        Assert.Equal(NoYes.Yes, invoice.Prepayment);
        Assert.Equal(order.PurchId, invoice.PurchId);
        Assert.Equal(-300m, (await db.Set<VendTransOpen>().SingleAsync()).AmountCur);
        var entries = await db.Set<GeneralJournalAccountEntry>().ToListAsync();
        Assert.Equal(2, entries.Count);
        Assert.Equal(300m, Assert.Single(entries, x => x.LedgerDimension == 1).AccountingCurrencyAmount);
        Assert.Equal(-300m, Assert.Single(entries, x => x.LedgerDimension == 2).AccountingCurrencyAmount);
        Assert.Equal(0m, (await db.Set<PurchPrepayTable>().AsNoTracking().SingleAsync()).PrepayAvailable);
        db.Add(new BankAccountTable { DataAreaId = "dat", AccountId = "PREPAY-BANK", LedgerDimension = 90, CurrencyCode = "SAR" });
        var profile = await db.Set<VendLedgerAccounts>().SingleAsync(x => x.PostingProfile == "ADV");
        profile.SummaryLedgerDimension = 3;
        await db.SaveChangesAsync();
        var payments = new VendPaymentService(new TestUnitOfWork(db), new TestUser(), new TestCompany(), paymentNumbers);
        var payment = new VendPaymentPostDto("V-0001", "PREPAY-BANK", 300m, "ADV-PAY", date.AddDays(1), SettleInvoiceId: "ADV-1");
        Assert.Equal(422, (await payments.PostAsync(payment with { Amount = 301m }, default)).StatusCode);
        var paid = await payments.PostAsync(payment, default);
        Assert.True(paid.StatusCode == 200, paid.Message);
        Assert.Equal(200, (await payments.PostAsync(payment, default)).StatusCode);
        Assert.Empty(await db.Set<VendTransOpen>().ToListAsync());
        Assert.Equal(300m, (await db.Set<VendSettlement>().SingleAsync()).AmountCur);
        var paymentLine = await db.Set<LedgerJournalTrans>().SingleAsync(x => x.PaymReference == "ADV-PAY");
        Assert.Equal(2, paymentLine.LedgerDimension);
        Assert.Equal(300m, paymentLine.AmountCurDebit);
        var reversalService = new VendPaymentReversalService(new TestUnitOfWork(db), new TestUser(), new TestCompany(), paymentNumbers);
        var reversed = await reversalService.ReverseAsync(paymentLine.JournalNum, date.AddDays(2), default);
        Assert.True(reversed.StatusCode == 200, reversed.Message);
        var reopened = await db.Set<VendTransOpen>().AsNoTracking().SingleAsync();
        Assert.Equal(-300m, reopened.AmountCur);
        var reversalJournal = await db.Set<LedgerJournalTable>().SingleAsync(x => x.OriginalJournalNum == paymentLine.JournalNum);
        var reversalLine = await db.Set<LedgerJournalTrans>().SingleAsync(x => x.JournalNum == reversalJournal.JournalNum);
        Assert.Equal(2, reversalLine.LedgerDimension);
        Assert.Equal(300m, reversalLine.AmountCurCredit);
        Assert.Equal(10m, (await db.Set<PurchLine>().AsNoTracking().SingleAsync()).RemainPurchFinancial);
        Assert.Equal(0m, (await db.Set<InventSum>().AsNoTracking().SingleAsync()).PhysicalInvent);
        Assert.Equal(0m, (await db.Set<PurchPrepayTable>().AsNoTracking().SingleAsync()).PrepayApplicationRemaining);

        var (_, _, receipts, _) = PurchaseServices(db);
        var purchaseLine = await db.Set<PurchLine>().SingleAsync();
        Assert.Equal(200, (await receipts.PostAsync(order.RecId, new("ADV-R", date.AddDays(2), [new(purchaseLine.RecId, 10m)]), default)).StatusCode);
        var finalPosted = await invoices.PostAsync(order.RecId, new("ADV-FINAL", date.AddDays(3), [new(purchaseLine.RecId, 10m)]), default);
        Assert.True(finalPosted.StatusCode == 200, finalPosted.Message);
        Assert.Equal(200, (await invoices.PostPrepaymentAsync(order.RecId, "ADV-1", date, 300m, default)).StatusCode);
        var final = await db.Set<VendInvoiceJour>().AsNoTracking().SingleAsync(x => x.InvoiceId == "ADV-FINAL");
        Assert.Equal(422, (await invoices.ApplyPrepaymentAsync(order.RecId, invoice.RecId, final.RecId, date.AddDays(4), 200m, default)).StatusCode);
        var repaid = await payments.PostAsync(payment with { RequestId = "ADV-PAY-2", PaymentDate = date.AddDays(4) }, default);
        Assert.True(repaid.StatusCode == 200, repaid.Message);
        Assert.Equal(300m, (await db.Set<PurchPrepayTable>().AsNoTracking().SingleAsync()).PrepayApplicationRemaining);
        Assert.Equal(422, (await invoices.ApplyPrepaymentAsync(order.RecId, invoice.RecId, final.RecId, date.AddDays(5), 301m, default)).StatusCode);
        var applied = await invoices.ApplyPrepaymentAsync(order.RecId, invoice.RecId, final.RecId, date.AddDays(5), 200m, default);
        Assert.True(applied.StatusCode == 200, applied.Message);
        Assert.Equal(200, (await invoices.ApplyPrepaymentAsync(order.RecId, invoice.RecId, final.RecId, date.AddDays(5), 200m, default)).StatusCode);
        var appliedMatch = Assert.IsType<VendSettlement>(applied.Data);
        var appliedEntry = await db.Set<GeneralJournalEntry>().SingleAsync(x => x.SubledgerVoucher == appliedMatch.Voucher);
        var transfer = await db.Set<GeneralJournalAccountEntry>().Where(x => x.GeneralJournalEntry == appliedEntry.RecId).ToListAsync();
        Assert.Equal(200m, Assert.Single(transfer, x => x.LedgerDimension == 3).AccountingCurrencyAmount);
        Assert.Equal(-200m, Assert.Single(transfer, x => x.LedgerDimension == 1).AccountingCurrencyAmount);
        Assert.Equal(-800m, (await db.Set<VendTransOpen>().AsNoTracking().SingleAsync()).AmountCur);
        Assert.Equal(100m, (await db.Set<PurchPrepayTable>().AsNoTracking().SingleAsync()).PrepayApplicationRemaining);
        var secondPayment = await db.Set<LedgerJournalTrans>().SingleAsync(x => x.PaymReference == "ADV-PAY-2");
        Assert.Equal(422, (await reversalService.ReverseAsync(secondPayment.JournalNum, date.AddDays(6), default)).StatusCode);

        var undo = await invoices.ReversePrepaymentApplicationAsync(order.RecId, appliedMatch.RecId, date.AddDays(6), default);
        Assert.True(undo.StatusCode == 200, undo.Message);
        Assert.Equal(200, (await invoices.ReversePrepaymentApplicationAsync(order.RecId, appliedMatch.RecId, date.AddDays(6), default)).StatusCode);
        Assert.Equal(300m, (await db.Set<PurchPrepayTable>().AsNoTracking().SingleAsync()).PrepayApplicationRemaining);
        Assert.Equal(-1000m, (await db.Set<VendTransOpen>().AsNoTracking().SingleAsync()).AmountCur);
        var undoMatch = Assert.IsType<VendSettlement>(undo.Data);
        var undoEntry = await db.Set<GeneralJournalEntry>().SingleAsync(x => x.SubledgerVoucher == undoMatch.Voucher);
        var undoLines = await db.Set<GeneralJournalAccountEntry>().Where(x => x.GeneralJournalEntry == undoEntry.RecId).ToListAsync();
        Assert.Equal(-200m, Assert.Single(undoLines, x => x.LedgerDimension == 3).AccountingCurrencyAmount);
        Assert.Equal(200m, Assert.Single(undoLines, x => x.LedgerDimension == 1).AccountingCurrencyAmount);
        var retryReversePayment = await reversalService.ReverseAsync(secondPayment.JournalNum, date.AddDays(7), default);
        Assert.True(retryReversePayment.StatusCode == 200, retryReversePayment.Message);
        Assert.Equal(0m, (await db.Set<PurchPrepayTable>().AsNoTracking().SingleAsync()).PrepayApplicationRemaining);
    }

    [Fact]
    public async Task Receipt_accrual_reverses_only_matched_quantities_using_original_accounts()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        var policy = db.Set<InventModelGroup>().Local.Single();
        policy.PostPhysicalPurchase = NoYes.Yes;
        policy.PostOnHandPhysical = NoYes.Yes;
        db.Set<InventPosting>().AddRange(
            new InventPosting { RecId = 4, DataAreaId = "dat", InventAccountType = InventAccountType.PurchPackingSlip, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 1 },
            new InventPosting { RecId = 5, DataAreaId = "dat", InventAccountType = InventAccountType.PurchPackingSlipOffsetAccount, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 3 });
        await db.SaveChangesAsync();
        var (orders, _, receipts, invoices) = PurchaseServices(db);
        Assert.Equal(200, (await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 10, 1), "S01", "WH-01", null), default)).StatusCode);
        var order = await db.Set<PurchTable>().SingleAsync();
        Assert.Equal(200, (await orders.AddLineAsync(order.RecId, new PurchLineInputDto("ITEM-001", 60, "PCS", 50, 1, null, null, null, null), default)).StatusCode);
        var line = await db.Set<PurchLine>().SingleAsync();
        Assert.Equal(200, (await receipts.PostAsync(order.RecId, new ProductReceiptPostDto("ACCRUAL-1", new DateTime(2026, 10, 2), [new(line.RecId, 60)]), default)).StatusCode);
        var accrual = await db.Set<PurchReceiptAccrual>().SingleAsync();
        Assert.Equal(3000m, accrual.AmountMst);
        Assert.Equal(0m, accrual.ReversedAmountMst);
        Assert.Equal(0m, (await db.Set<GeneralJournalAccountEntry>().ToListAsync()).Sum(x => x.AccountingCurrencyAmount));
        Assert.Equal(200, (await invoices.PostAsync(order.RecId, new VendorInvoicePostDto("ACCRUAL-I1", new DateTime(2026, 10, 3), [new(line.RecId, 40)]), default)).StatusCode);
        Assert.Equal(2000m, accrual.ReversedAmountMst);
        Assert.Equal(40m, accrual.ReversedInventQuantity);
        // Later setup edits must not redirect the reversal of the original liability.
        (await db.Set<InventPosting>().SingleAsync(x => x.InventAccountType == InventAccountType.PurchPackingSlipOffsetAccount)).LedgerDimension = 2;
        await db.SaveChangesAsync();
        Assert.Equal(200, (await invoices.PostAsync(order.RecId, new VendorInvoicePostDto("ACCRUAL-I2", new DateTime(2026, 10, 4), [new(line.RecId, 20)]), default)).StatusCode);
        Assert.Equal(3000m, accrual.ReversedAmountMst);
        Assert.Equal(60m, accrual.ReversedInventQuantity);
        var matches = await db.Set<VendInvoicePackingSlipQuantityMatch>().ToListAsync();
        Assert.Equal(3000m, matches.Sum(x => x.AccrualReversalAmountMst));
        Assert.All(matches, x => Assert.Equal(accrual.RecId, x.ReceiptAccrualRecId));
        var entries = await db.Set<GeneralJournalAccountEntry>().ToListAsync();
        Assert.Equal(0m, entries.Sum(x => x.AccountingCurrencyAmount));
        Assert.DoesNotContain(entries, x => x.LedgerDimension == 2);
        Assert.Equal(3000m, entries.Where(x => x.LedgerDimension == 1).Sum(x => x.AccountingCurrencyAmount));
        Assert.Equal(-3000m, (await db.Set<VendTransOpen>().ToListAsync()).Sum(x => x.AmountCur));
        // The receipt's posting record drove the reversal accounts and is retired once fully matched.
        var posting = await db.Set<InventTransPosting>().SingleAsync(x => x.InventTransPostingType == InventTransPostingType.Physical);
        Assert.Equal(1, posting.LedgerDimension);
        Assert.Equal(3, posting.OffsetLedgerDimension);
        Assert.Equal(NoYes.No, posting.IsPosted);
        Assert.Equal(422, (await invoices.PostAsync(order.RecId, new VendorInvoicePostDto("ACCRUAL-I2", null, [new(line.RecId, 20)]), default)).StatusCode);
        Assert.Equal(2, await db.Set<VendInvoicePackingSlipQuantityMatch>().CountAsync());
    }

    [Fact]
    public async Task Partial_receipt_accrual_reversals_do_not_exceed_the_original_amount()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        var policy = db.Set<InventModelGroup>().Local.Single();
        policy.PostPhysicalPurchase = NoYes.Yes;
        policy.PostOnHandPhysical = NoYes.Yes;
        db.Set<InventPosting>().AddRange(
            new InventPosting { RecId = 4, DataAreaId = "dat", InventAccountType = InventAccountType.PurchPackingSlip, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 1 },
            new InventPosting { RecId = 5, DataAreaId = "dat", InventAccountType = InventAccountType.PurchPackingSlipOffsetAccount, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 3 });
        await db.SaveChangesAsync();
        var (orders, _, receipts, _) = PurchaseServices(db);
        Assert.Equal(200, (await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 10, 1), "S01", "WH-01", null), default)).StatusCode);
        var order = await db.Set<PurchTable>().SingleAsync();
        Assert.Equal(200, (await orders.AddLineAsync(order.RecId, new PurchLineInputDto("ITEM-001", 4, "PCS", 50, 1, null, null, null, null), default)).StatusCode);
        var line = await db.Set<PurchLine>().SingleAsync();
        Assert.Equal(200, (await receipts.PostAsync(order.RecId, new ProductReceiptPostDto("ACCRUAL-ROUND", new DateTime(2026, 10, 2), [new(line.RecId, 4)]), default)).StatusCode);
        var accrual = await db.Set<PurchReceiptAccrual>().SingleAsync();
        accrual.AmountMst = 0.0002m;

        var posting = new PurchaseReceiptAccrualPosting(db);
        foreach (var expected in new[] { 0.0001m, 0m, 0.0001m, 0m })
        {
            var match = new VendInvoicePackingSlipQuantityMatch { PackingSlipTransRecId = accrual.PackingSlipTransRecId, InventQuantity = 1m };
            await posting.ReverseAsync("dat", match, accrual.GeneralJournalEntryRecId, "SAR", default);
            Assert.Equal(expected, match.AccrualReversalAmountMst);
        }
        Assert.Equal(accrual.AmountMst, accrual.ReversedAmountMst);
        Assert.Equal(accrual.InventQuantity, accrual.ReversedInventQuantity);
    }

    [Fact]
    public async Task Correcting_a_receipt_preserves_history_and_reverses_only_the_removed_accrual()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        var policy = db.Set<InventModelGroup>().Local.Single();
        policy.PostPhysicalPurchase = NoYes.Yes;
        policy.PostOnHandPhysical = NoYes.Yes;
        db.Set<InventPosting>().AddRange(
            new InventPosting { RecId = 4, DataAreaId = "dat", InventAccountType = InventAccountType.PurchPackingSlip, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 1 },
            new InventPosting { RecId = 5, DataAreaId = "dat", InventAccountType = InventAccountType.PurchPackingSlipOffsetAccount, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 3 });
        await db.SaveChangesAsync();
        var (orders, _, receipts, invoices) = PurchaseServices(db);
        var date = new DateTime(2026, 10, 1);
        Assert.Equal(200, (await orders.CreateAsync(new("V-0001", null, "SAR", date, "S01", "WH-01", null), default)).StatusCode);
        var order = await db.Set<PurchTable>().SingleAsync();
        Assert.Equal(200, (await orders.AddLineAsync(order.RecId, new("ITEM-001", 100m, "PCS", 100m, 1m, null, null, null, null), default)).StatusCode);
        var line = await db.Set<PurchLine>().SingleAsync();
        Assert.Equal(200, (await receipts.PostAsync(order.RecId, new("RCPT-CORRECT", date.AddDays(1), [new(line.RecId, 60m)]), default)).StatusCode);
        var receipt = await db.Set<VendPackingSlipJour>().AsNoTracking().SingleAsync();
        var source = await db.Set<VendPackingSlipTrans>().AsNoTracking().SingleAsync();

        var bad = await receipts.CorrectAsync(receipt.RecId, new(source.RecId, 70m, date.AddDays(2)), default);
        Assert.Equal(422, bad.StatusCode);
        Assert.Equal(60m, (await db.Set<InventSum>().AsNoTracking().SingleAsync()).PhysicalInvent);
        Assert.Single(await db.Set<VendPackingSlipVersion>().ToListAsync());
        var correction = new ProductReceiptCorrectionDto(source.RecId, 50m, date.AddDays(2));
        // A stock rejection occurs after the reversal voucher is staged. The transaction must undo it.
        var reserved = await db.Set<InventSum>().SingleAsync();
        reserved.ReservPhysical = 55m;
        await db.SaveChangesAsync();
        Assert.Equal(422, (await receipts.CorrectAsync(receipt.RecId, correction, default)).StatusCode);
        Assert.Single(await db.Set<GeneralJournalEntry>().ToListAsync());
        Assert.Single(await db.Set<VendPackingSlipVersion>().ToListAsync());
        Assert.Equal(0m, (await db.Set<PurchReceiptAccrual>().AsNoTracking().SingleAsync()).ReversedAmountMst);
        Assert.Equal(60m, (await db.Set<VendPackingSlipTrans>().AsNoTracking().SingleAsync()).Qty);
        reserved = await db.Set<InventSum>().SingleAsync();
        reserved.ReservPhysical = 0m;
        await db.SaveChangesAsync();
        var result = await receipts.CorrectAsync(receipt.RecId, correction, default);
        Assert.True(result.StatusCode == 200, result.Message);
        Assert.Equal(200, (await receipts.CorrectAsync(receipt.RecId, correction, default)).StatusCode);

        var versions = await db.Set<VendPackingSlipVersion>().OrderBy(x => x.VersionDateTime).ToListAsync();
        Assert.Equal(new[] { 60m, 50m }, versions.Select(x => x.Qty));
        Assert.NotEqual(versions[0].LedgerVoucher, versions[1].LedgerVoucher);
        var history = await db.Set<VendPackingSlipTransHistory>().OrderBy(x => x.ValidFrom).ToListAsync();
        Assert.Equal(new[] { 6000m, 5000m }, history.Select(x => x.ValueMst));
        Assert.Equal(history[0].ValidTo, history[1].ValidFrom);
        var current = await db.Set<VendPackingSlipTrans>().AsNoTracking().SingleAsync();
        Assert.Equal(50m, current.Qty);
        Assert.Equal(50m, (await db.Set<PurchLine>().AsNoTracking().SingleAsync()).RemainPurchPhysical);
        var stock = await db.Set<InventSum>().AsNoTracking().SingleAsync();
        Assert.Equal(50m, stock.PhysicalInvent);
        Assert.Equal(50m, stock.Ordered);
        Assert.Equal(5000m, stock.PhysicalValue);
        Assert.Empty(await db.Set<VendTrans>().ToListAsync());
        var ledger = await db.Set<GeneralJournalAccountEntry>().ToListAsync();
        Assert.Equal(5000m, ledger.Where(x => x.LedgerDimension == 1).Sum(x => x.AccountingCurrencyAmount));
        Assert.Equal(-5000m, ledger.Where(x => x.LedgerDimension == 3).Sum(x => x.AccountingCurrencyAmount));
        Assert.Equal(200, (await invoices.PostAsync(order.RecId, new("CORRECT-I", date.AddDays(3), [new(line.RecId, 50m)]), default)).StatusCode);
        Assert.Equal(6000m, (await db.Set<PurchReceiptAccrual>().AsNoTracking().SingleAsync()).ReversedAmountMst);
    }

    [Fact]
    public async Task Receipt_cancellation_reverses_every_line_in_one_transaction()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        var policy = db.Set<InventModelGroup>().Local.Single();
        policy.PostPhysicalPurchase = NoYes.Yes;
        policy.PostOnHandPhysical = NoYes.Yes;
        db.Set<InventPosting>().AddRange(
            new InventPosting { RecId = 4, DataAreaId = "dat", InventAccountType = InventAccountType.PurchPackingSlip, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 1 },
            new InventPosting { RecId = 5, DataAreaId = "dat", InventAccountType = InventAccountType.PurchPackingSlipOffsetAccount, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 3 });
        await db.SaveChangesAsync();
        var (orders, _, receipts, _) = PurchaseServices(db);
        var date = new DateTime(2026, 10, 1);
        Assert.Equal(200, (await orders.CreateAsync(new("V-0001", null, "SAR", date, "S01", "WH-01", null), default)).StatusCode);
        var order = await db.Set<PurchTable>().SingleAsync();
        foreach (var qty in new[] { 20m, 30m })
            Assert.Equal(200, (await orders.AddLineAsync(order.RecId, new("ITEM-001", qty, "PCS", 100m, 1m, null, null, null, null), default)).StatusCode);
        var lines = await db.Set<PurchLine>().OrderBy(x => x.RecId).ToListAsync();
        Assert.Equal(200, (await receipts.PostAsync(order.RecId, new("CANCEL-TWO", date.AddDays(1), lines.Select(x => new PurchPostLineDto(x.RecId, x.PurchQty)).ToList()), default)).StatusCode);
        var receipt = await db.Set<VendPackingSlipJour>().AsNoTracking().SingleAsync();

        var stock = await db.Set<InventSum>().SingleAsync();
        stock.ReservPhysical = 25m;
        await db.SaveChangesAsync();
        Assert.Equal(422, (await receipts.CancelAsync(receipt.RecId, date.AddDays(2), default)).StatusCode);
        Assert.Equal(50m, (await db.Set<InventSum>().AsNoTracking().SingleAsync()).PhysicalInvent);
        Assert.Equal(2, await db.Set<GeneralJournalEntry>().CountAsync());
        Assert.Single(await db.Set<VendPackingSlipVersion>().ToListAsync());
        Assert.All(await db.Set<PurchReceiptAccrual>().AsNoTracking().ToListAsync(), x => Assert.Equal(0m, x.ReversedAmountMst));

        stock = await db.Set<InventSum>().SingleAsync();
        stock.ReservPhysical = 0m;
        await db.SaveChangesAsync();
        var result = await receipts.CancelAsync(receipt.RecId, date.AddDays(2), default);
        Assert.True(result.StatusCode == 200, result.Message);
        Assert.Equal(200, (await receipts.CancelAsync(receipt.RecId, date.AddDays(2), default)).StatusCode);
        stock = await db.Set<InventSum>().AsNoTracking().SingleAsync();
        Assert.Equal(50m, stock.Ordered);
        Assert.Equal(0m, stock.PhysicalInvent);
        Assert.Equal(0m, stock.PhysicalValue);
        Assert.Equal(3, await db.Set<GeneralJournalEntry>().CountAsync());
        var versions = await db.Set<VendPackingSlipVersion>().OrderBy(x => x.VersionDateTime).ToListAsync();
        Assert.Equal(new[] { 50m, 0m }, versions.Select(x => x.Qty));
        Assert.All(await db.Set<VendPackingSlipTrans>().AsNoTracking().ToListAsync(), x => Assert.Equal(0m, x.Qty));
        Assert.All(await db.Set<PurchReceiptAccrual>().AsNoTracking().ToListAsync(), x => Assert.Equal(x.AmountMst, x.ReversedAmountMst));
        Assert.Empty(await db.Set<VendTrans>().ToListAsync());
    }

    [Fact]
    public async Task Invoice_spanning_receipts_reverses_each_receipts_original_accrual_accounts()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        var policy = db.Set<InventModelGroup>().Local.Single();
        policy.PostPhysicalPurchase = NoYes.Yes;
        policy.PostOnHandPhysical = NoYes.Yes;
        db.Set<InventPosting>().AddRange(
            new InventPosting { RecId = 4, DataAreaId = "dat", InventAccountType = InventAccountType.PurchPackingSlip, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 1 },
            new InventPosting { RecId = 5, DataAreaId = "dat", InventAccountType = InventAccountType.PurchPackingSlipOffsetAccount, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 3 });
        await db.SaveChangesAsync();
        var (orders, _, receipts, invoices) = PurchaseServices(db);
        Assert.Equal(200, (await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 10, 1), "S01", "WH-01", null), default)).StatusCode);
        var order = await db.Set<PurchTable>().SingleAsync();
        Assert.Equal(200, (await orders.AddLineAsync(order.RecId, new PurchLineInputDto("ITEM-001", 60, "PCS", 50, 1, null, null, null, null), default)).StatusCode);
        var line = await db.Set<PurchLine>().SingleAsync();
        Assert.Equal(200, (await receipts.PostAsync(order.RecId, new ProductReceiptPostDto("RCPT-1", new DateTime(2026, 10, 2), [new(line.RecId, 40)]), default)).StatusCode);
        (await db.Set<InventPosting>().SingleAsync(x => x.InventAccountType == InventAccountType.PurchPackingSlipOffsetAccount)).LedgerDimension = 2;
        await db.SaveChangesAsync();
        Assert.Equal(200, (await receipts.PostAsync(order.RecId, new ProductReceiptPostDto("RCPT-2", new DateTime(2026, 10, 3), [new(line.RecId, 20)]), default)).StatusCode);
        Assert.Equal(200, (await invoices.PostAsync(order.RecId, new VendorInvoicePostDto("INV-ALL", new DateTime(2026, 10, 4), [new(line.RecId, 60)]), default)).StatusCode);

        var accruals = await db.Set<PurchReceiptAccrual>().OrderBy(x => x.RecId).ToListAsync();
        Assert.Equal(new long[] { 3, 2 }, accruals.Select(x => x.AccrualLedgerDimension));
        Assert.All(accruals, x => Assert.Equal(x.InventQuantity, x.ReversedInventQuantity));
        var postings = await db.Set<InventTransPosting>().Where(x => x.InventTransPostingType == InventTransPostingType.Physical).ToListAsync();
        Assert.Equal(2, postings.Count);
        Assert.All(postings, x => Assert.Equal(NoYes.No, x.IsPosted));
        var liabilityLines = (await db.Set<GeneralJournalAccountEntry>().ToListAsync()).Where(x =>
            x.PostingType == LedgerPostingType.LedgerJournal && (x.LedgerDimension == 2 || x.LedgerDimension == 3));
        Assert.All(liabilityLines.GroupBy(x => x.LedgerDimension), x => Assert.Equal(0m, x.Sum(y => y.AccountingCurrencyAmount)));
    }
}
