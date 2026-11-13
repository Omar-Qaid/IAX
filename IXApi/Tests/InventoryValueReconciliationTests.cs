using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IAX.IXApi.Tests;

/// <summary>
/// The two value-reconciliation contracts that <c>InventValueReportService</c> depends on:
/// a transaction is counted on exactly one of the two sides, and each side's transaction rows
/// sum back to the matching <see cref="InventSum"/> bucket. Both were corrected on 2026-11-11
/// and had no test of their own.
/// </summary>
public sealed partial class InventoryCycleTests
{
    /// <summary>
    /// A vendor return that has been shipped but not yet credited is a physical issue with no
    /// financial posting, so its cost belongs to <see cref="InventSum.PhysicalValue"/> until the
    /// credit note arrives. The shipment used to leave that bucket untouched while writing
    /// <c>CostAmountPhysical</c> onto the issue, so the value report showed a physical drift of
    /// the whole returned cost for every return in flight. The credit note then releases it.
    /// </summary>
    [Fact]
    public async Task A_shipped_but_uncredited_vendor_return_keeps_the_physical_value_reconciled()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
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

        // Everything invoiced sits on the financial side, nothing on the physical side.
        var sum = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(6000m, sum.PostedValue);
        Assert.Equal(0m, sum.PhysicalValue);

        var returns = new PurchReturnService(new TestUnitOfWork(db), new TestCompany(), Sequence(), numbers);
        Assert.Equal(200, (await returns.CreateAsync(new PurchReturnCreateDto("VI-1", "RMA-V-1", [new(invoiced.RecId, 10)]), default)).StatusCode);
        var ret = await db.Set<PurchTable>().SingleAsync(x => x.PurchaseType == PurchaseType.ReturnItem);
        var retLine = await db.Set<PurchLine>().SingleAsync(x => x.PurchId == ret.PurchId);

        // Ship 10 back at the original 60. The goods have left; no credit note has posted yet.
        Assert.Equal(200, (await returns.ShipAsync(ret.RecId, new PurchReturnShipDto("RS-1", new DateTime(2026, 10, 5), [new(retLine.RecId, 10)]), default)).StatusCode);

        sum = await db.Set<InventSum>().SingleAsync();
        var shipped = await db.Set<InventTrans>().SingleAsync(x => x.StatusIssue == StatusIssue.Deducted);
        Assert.Equal(-600m, shipped.CostAmountPhysical);
        Assert.Equal(0m, shipped.CostAmountPosted);
        Assert.Equal(-600m, sum.PhysicalValue);          // the bucket follows the transaction
        Assert.Equal(6000m, sum.PostedValue);            // the financial side is untouched

        var report = new InventValueReportService(new TestUnitOfWork(db), new TestCompany());
        var row = Assert.Single((await report.RunAsync(new DateOnly(2026, 10, 31), null, null, null, null, false, 1, 50, default))!.Rows);
        Assert.Equal(-600m, row.PhysicalValue);
        Assert.Equal(0m, row.PhysicalValueDifference);   // the drift this fix removes
        Assert.Equal(0m, row.FinancialValueDifference);

        // The credit note posts the return financially and releases the provisional cost again.
        Assert.Equal(200, (await returns.CreditAsync(ret.RecId, new VendCreditNoteDto("VCN-1", new DateTime(2026, 10, 6)), default)).StatusCode);

        sum = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(0m, sum.PhysicalValue);
        Assert.Equal(5400m, sum.PostedValue);
        row = Assert.Single((await report.RunAsync(new DateOnly(2026, 10, 31), null, null, null, null, false, 1, 50, default))!.Rows);
        Assert.Equal(0m, row.PhysicalValueDifference);
        Assert.Equal(0m, row.FinancialValueDifference);
        Assert.Equal(5400m, row.FinancialValue);
        Assert.Equal(0m, (await db.Set<InventTrans>().ToListAsync()).Sum(x => x.CostAmountPhysical));
    }

    /// <summary>
    /// One invoice settling several product receipts allocates its accounting amount over the
    /// receipt transactions. A per-unit cost multiplied out row by row does not add back to the
    /// amount when the division does not terminate, so the allocation is cumulative with the
    /// last row carrying the remainder. Three receipts of one piece at 33.335 invoice for
    /// <b>100.01</b>, which three equal shares cannot reproduce.
    /// </summary>
    [Fact]
    public async Task A_vendor_invoice_across_receipts_allocates_a_cost_that_sums_back_to_the_posted_value()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        await db.SaveChangesAsync();
        var (orders, _, receipts, invoices) = PurchaseServices(db);
        await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 10, 1), "S01", "WH-01", null), default);
        var po = await db.Set<PurchTable>().SingleAsync();
        await orders.AddLineAsync(po.RecId, new PurchLineInputDto("ITEM-001", 3, "PCS", 33.335m, 1, null, null, null, null), default);
        var pl = await db.Set<PurchLine>().SingleAsync();

        // Three separate receipts, so the invoice has three transactions to allocate over.
        foreach (var (receipt, day) in new[] { ("PR-1", 2), ("PR-2", 3), ("PR-3", 4) })
            Assert.Equal(200, (await receipts.PostAsync(po.RecId,
                new ProductReceiptPostDto(receipt, new DateTime(2026, 10, day), [new(pl.RecId, 1)]), default)).StatusCode);
        Assert.Equal(3, await db.Set<InventTrans>().CountAsync(x => x.StatusReceipt == StatusReceipt.Received));

        Assert.Equal(200, (await invoices.PostAsync(po.RecId,
            new VendorInvoicePostDto("VI-1", new DateTime(2026, 10, 5), [new(pl.RecId, 3)]), default)).StatusCode);

        var sum = await db.Set<InventSum>().SingleAsync();
        var trans = await db.Set<InventTrans>().ToListAsync();
        Assert.Equal(100.01m, sum.PostedValue);
        Assert.Equal(100.01m, trans.Sum(x => x.CostAmountPosted));   // allocation adds back exactly
        Assert.Equal(0m, sum.PhysicalValue);
        Assert.Equal(0m, trans.Sum(x => x.CostAmountPhysical));      // the physical side is fully released
        Assert.All(trans, x => Assert.Equal(StatusReceipt.Purchased, x.StatusReceipt));

        var report = new InventValueReportService(new TestUnitOfWork(db), new TestCompany());
        var row = Assert.Single((await report.RunAsync(new DateOnly(2026, 10, 31), null, null, null, null, false, 1, 50, default))!.Rows);
        Assert.Equal(0m, row.FinancialValueDifference);
        Assert.Equal(0m, row.PhysicalValueDifference);

        var ledger = await db.Set<GeneralJournalAccountEntry>().ToListAsync();
        Assert.Equal(0m, ledger.Sum(x => x.AccountingCurrencyAmount));
        Assert.Equal(100.01m, ledger.Where(x => x.AccountingCurrencyAmount > 0).Sum(x => x.AccountingCurrencyAmount));
    }
}
