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

public sealed class InventoryCycleTests
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

    // ---- Reservation engine (Phase 3 §7–8) --------------------------------------------------

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

        // Release 5 → reserved 15, available 85.
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

    // ---- FIFO inventory close (Phase 7 worked example: 1,600 COGS / 600 remaining) -----------

    [Fact]
    public async Task Fifo_close_settles_issue_to_actual_fifo_cost_and_posts_adjustment()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        db.Set<InventDim>().Add(new InventDim { RecId = 1, DataAreaId = "dat", InventDimId = "DIM", InventSiteId = "S1", InventLocationId = "WH" });
        db.Set<InventModelGroup>().Add(new InventModelGroup { RecId = 1, DataAreaId = "dat", ModelGroupId = "FIFO", InventModel = InventModel.FIFO });
        db.Set<InventModelGroupItem>().Add(new InventModelGroupItem { RecId = 1, ItemDataAreaId = "dat", ModelGroupDataAreaId = "dat", ModelGroupId = "FIFO", ItemId = "ITEM" });
        db.Set<InventItemGroupItem>().Add(new InventItemGroupItem { RecId = 1, ItemDataAreaId = "dat", ItemId = "ITEM", ItemGroupId = "TRADE" });
        // Two financially posted receipts and one provisional-cost issue (running average 2200/200 = 11).
        db.Set<InventTransOrigin>().AddRange(
            new InventTransOrigin { RecId = 1, DataAreaId = "dat", InventTransId = "R1", ItemId = "ITEM", ReferenceId = "IJ1", ReferenceCategory = InventRefType.InventoryJournal },
            new InventTransOrigin { RecId = 2, DataAreaId = "dat", InventTransId = "R2", ItemId = "ITEM", ReferenceId = "IJ2", ReferenceCategory = InventRefType.InventoryJournal },
            new InventTransOrigin { RecId = 3, DataAreaId = "dat", InventTransId = "I1", ItemId = "ITEM", ReferenceId = "IJ3", ReferenceCategory = InventRefType.InventoryJournal });
        db.Set<InventTrans>().AddRange(
            new InventTrans { RecId = 1, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 1, Qty = 100, StatusReceipt = StatusReceipt.Purchased, CostAmountPosted = 1000, DateFinancial = new DateTime(2026, 6, 1), ValueOpen = InventTransOpen.Yes },
            new InventTrans { RecId = 2, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 2, Qty = 100, StatusReceipt = StatusReceipt.Purchased, CostAmountPosted = 1200, DateFinancial = new DateTime(2026, 6, 2), ValueOpen = InventTransOpen.Yes },
            new InventTrans { RecId = 3, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 3, Qty = -150, StatusIssue = StatusIssue.Sold, CostAmountPosted = -1650, DateFinancial = new DateTime(2026, 6, 3), ValueOpen = InventTransOpen.Yes });
        db.Set<InventSum>().Add(new InventSum { RecId = 1, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", PhysicalInvent = 50, PostedValue = 550 });
        await db.SaveChangesAsync();

        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(),
            new TestUser(), Sequence());
        var (result, error) = await service.CloseAsync(new DateTime(2026, 6, 30), default);

        Assert.Null(error);
        Assert.Equal(1, result!.IssuesSettled);
        Assert.Equal(2, result.SettlementsCreated);
        Assert.Equal(50m, result.TotalAdjustment);

        var issue = await db.Set<InventTrans>().SingleAsync(x => x.RecId == 3);
        Assert.Equal(50m, issue.CostAmountAdjustment);
        Assert.Equal(-1600m, issue.CostAmountPosted + issue.CostAmountAdjustment); // final FIFO COGS
        Assert.Equal(600m, (await db.Set<InventSum>().SingleAsync()).PostedValue); // remaining inventory value

        var settlements = await db.Set<InventSettlement>().ToListAsync();
        Assert.Equal(2, settlements.Count);
        Assert.Equal(-1600m, settlements.Sum(x => x.CostAmountSettled));
        Assert.Equal(-150m, settlements.Sum(x => x.QtySettled));

        var closing = await db.Set<InventClosing>().SingleAsync();
        Assert.Equal(InventCostStatus.Finished, closing.InventCostStatus);

        var ledgerLines = await db.Set<GeneralJournalAccountEntry>().ToListAsync();
        Assert.Equal(2, ledgerLines.Count);                              // one balanced Dr/Cr pair
        Assert.Equal(0m, ledgerLines.Sum(x => x.AccountingCurrencyAmount)); // debits and credits balance
        Assert.Contains(ledgerLines, x => x.AccountingCurrencyAmount == 50m);  // Dr Inventory +50
        Assert.Contains(ledgerLines, x => x.AccountingCurrencyAmount == -50m); // Cr COGS -50
    }

    [Fact]
    public async Task Fifo_close_is_idempotent_and_blocks_backdated_runs()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        db.Set<InventDim>().Add(new InventDim { RecId = 1, DataAreaId = "dat", InventDimId = "DIM", InventSiteId = "S1", InventLocationId = "WH" });
        db.Set<InventModelGroup>().Add(new InventModelGroup { RecId = 1, DataAreaId = "dat", ModelGroupId = "FIFO", InventModel = InventModel.FIFO });
        db.Set<InventModelGroupItem>().Add(new InventModelGroupItem { RecId = 1, ItemDataAreaId = "dat", ModelGroupDataAreaId = "dat", ModelGroupId = "FIFO", ItemId = "ITEM" });
        db.Set<InventItemGroupItem>().Add(new InventItemGroupItem { RecId = 1, ItemDataAreaId = "dat", ItemId = "ITEM", ItemGroupId = "TRADE" });
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());

        var first = await service.CloseAsync(new DateTime(2026, 6, 30), default);
        Assert.Null(first.Error);
        var second = await service.CloseAsync(new DateTime(2026, 6, 30), default); // same cutoff -> idempotent
        Assert.Null(second.Error);
        Assert.Equal(first.Result!.RunNum, second.Result!.RunNum);
        Assert.Single(await db.Set<InventClosing>().ToListAsync());

        var backdated = await service.CloseAsync(new DateTime(2026, 5, 31), default); // before last close
        Assert.NotNull(backdated.Error);
    }

    // ---- Purchase order Milestone 1 (Purchorder.md §12.2): PO 100 @ 50, receive/invoice 60 + 40 ----

    [Fact]
    public async Task Purchase_order_partial_receipts_and_invoices_complete_the_cycle()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
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
        Assert.Equal(-5000m, (await db.Set<VendTrans>().ToListAsync()).Sum(x => x.AmountCur));
        var ledger = await db.Set<GeneralJournalAccountEntry>().ToListAsync();
        Assert.Equal(0m, ledger.Sum(x => x.AccountingCurrencyAmount));                     // balanced
        Assert.Equal(5000m, ledger.Where(x => x.AccountingCurrencyAmount > 0).Sum(x => x.AccountingCurrencyAmount));
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

    // ---- Sales order cycle (CycleSalesorder.md / ARTables.md §14 state table) -------------------

    [Fact]
    public async Task Sales_order_partial_delivery_partial_invoice_and_payment_reconcile()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        SeedSalesMasterData(db);
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
            SalesStatus = SalesStatus.Backorder, InventSiteId = "S01", InventLocationId = "WH-01", OrderDate = new DateTime(2026, 10, 5)
        };
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

        var packingSlips = new PackingSlipJournalService(work, company, Sequence());
        var invoices = new SalesInvoiceService(work, company, Sequence());

        // Packing slip 6 -> remain physical 4, remain financial 10; reservation consumed.
        var ps1 = await packingSlips.PostAsync(order.RecId, new PostPackingSlipRequest(new DateTime(2026, 10, 6), "All", true, "ps-1",
            [new PackingSlipPostingLineRequest(line.RecId.ToString(), 6)]), default);
        Assert.Equal(200, ps1.StatusCode);
        line = await db.Set<SalesLine>().SingleAsync();
        Assert.Equal(4m, line.RemainSalesPhysical);
        Assert.Equal(10m, line.RemainSalesFinancial);
        var sum = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(94m, sum.PhysicalInvent);
        Assert.Equal(4m, sum.ReservPhysical);

        // Invoice 4 of the 6 delivered -> remain financial 6; invoicing beyond delivered is rejected.
        var inv1 = await invoices.PostAsync(order.RecId, new SalesInvoicePostDto(new DateTime(2026, 10, 7), "inv-1", [new(line.RecId, 4)]), default);
        Assert.Equal(200, inv1.StatusCode);
        line = await db.Set<SalesLine>().SingleAsync();
        Assert.Equal(4m, line.RemainSalesPhysical);
        Assert.Equal(6m, line.RemainSalesFinancial);
        Assert.Equal(422, (await invoices.PostAsync(order.RecId, new SalesInvoicePostDto(null, null, [new(line.RecId, 3)]), default)).StatusCode);
        Assert.Equal(200, (await invoices.PostAsync(order.RecId, new SalesInvoicePostDto(null, "inv-1", [new(line.RecId, 4)]), default)).StatusCode);
        Assert.Single(await db.Set<CustInvoiceJour>().ToListAsync()); // idempotent retry

        // Packing slip 4, then invoice the remaining 6 delivered.
        Assert.Equal(200, (await packingSlips.PostAsync(order.RecId, new PostPackingSlipRequest(new DateTime(2026, 10, 8), "All", true, "ps-2",
            [new PackingSlipPostingLineRequest(line.RecId.ToString(), 4)]), default)).StatusCode);
        Assert.Equal(200, (await invoices.PostAsync(order.RecId, new SalesInvoicePostDto(new DateTime(2026, 10, 9), "inv-2"), default)).StatusCode);

        line = await db.Set<SalesLine>().SingleAsync();
        Assert.Equal(0m, line.RemainSalesPhysical);
        Assert.Equal(0m, line.RemainSalesFinancial);
        Assert.Equal(SalesStatus.Invoiced, line.SalesStatus);
        Assert.Equal(SalesStatus.Invoiced, (await db.Set<SalesTable>().SingleAsync()).SalesStatus);

        var journals = await db.Set<CustInvoiceJour>().ToListAsync();
        Assert.Equal(1000m, journals.Sum(x => x.SalesBalance));
        Assert.Equal(150m, journals.Sum(x => x.SumTax));
        Assert.Equal(1150m, journals.Sum(x => x.InvoiceAmount));
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
    }

    // ---- Returns (CycleSalesorder.md 10.3, Purchorder.md 8) ---------------------------------------

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
        var credit = await returns.CreditAsync(rma.RecId, new CustCreditNoteDto(new DateTime(2026, 10, 21), "cn-1"), default);
        Assert.Equal(200, credit.StatusCode);
        var note = await db.Set<CustInvoiceJour>().SingleAsync(x => x.SalesId == rma.SalesId);
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

    [Fact]
    public async Task Vendor_return_ships_at_original_cost_and_vendor_credit_reduces_payable()
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
        Assert.Equal(200, (await returns.CreditAsync(ret.RecId, new VendCreditNoteDto("VCN-1", new DateTime(2026, 10, 6)), default)).StatusCode);

        sum = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(90m, sum.PhysicalInvent);
        Assert.Equal(90m, sum.PostedQty);
        Assert.Equal(0m, sum.Deducted);
        Assert.Equal(5400m, sum.PostedValue);
        Assert.Equal(-5400m, (await db.Set<VendTrans>().ToListAsync()).Sum(x => x.AmountCur)); // payable 6,000 - 600 credit
        Assert.Equal(PurchStatus.Invoiced, (await db.Set<PurchTable>().SingleAsync(x => x.RecId == ret.RecId)).PurchStatus);
        Assert.Equal(0m, (await db.Set<GeneralJournalAccountEntry>().ToListAsync()).Sum(x => x.AccountingCurrencyAmount));
        Assert.Equal(422, (await returns.CreditAsync(ret.RecId, new VendCreditNoteDto("VCN-1"), default)).StatusCode);
    }

    // Stocks 100 @ 60 through the purchase cycle and sells/ships/invoices 10 @ 100 (+15% VAT); returns the invoice id.
    private static async Task<string> StockAndSellTenAsync(CycleContext db, TestInventNumbers numbers)
    {
        var work = new TestUnitOfWork(db);
        var company = new TestCompany();
        var (orders, _, receipts, vendorInvoices) = PurchaseServices(db, numbers);
        await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 10, 1), "S01", "WH-01", null), default);
        var po = await db.Set<PurchTable>().SingleAsync();
        await orders.AddLineAsync(po.RecId, new PurchLineInputDto("ITEM-001", 100, "PCS", 60, 1, null, null, null, null), default);
        var pl = await db.Set<PurchLine>().SingleAsync();
        await receipts.PostAsync(po.RecId, new ProductReceiptPostDto("PR-1", new DateTime(2026, 10, 2), [new(pl.RecId, 100)]), default);
        await vendorInvoices.PostAsync(po.RecId, new VendorInvoicePostDto("VI-1", new DateTime(2026, 10, 3), [new(pl.RecId, 100)]), default);

        var order = new SalesTable
        {
            DataAreaId = "dat", SalesId = "SO-0001", CustAccount = "C-001", InvoiceAccount = "C-001", CurrencyCode = "SAR",
            SalesStatus = SalesStatus.Backorder, SalesType = SalesType.Sales, InventSiteId = "S01", InventLocationId = "WH-01",
            OrderDate = new DateTime(2026, 10, 5)
        };
        db.Set<SalesTable>().Add(order);
        await db.SaveChangesAsync();
        var line = new SalesLine
        {
            DataAreaId = "dat", SalesId = order.SalesId, LineNum = 1, ItemId = "ITEM-001", SalesQty = 10, QtyOrdered = 10,
            RemainSalesPhysical = 10, RemainSalesFinancial = 10, RemainInventPhysical = 10, RemainInventFinancial = 10,
            SalesUnit = "PCS", SalesPrice = 100, PriceUnit = 1, LineAmount = 1000, SalesStatus = SalesStatus.Backorder,
            SalesType = SalesType.Sales, StockedProduct = NoYes.Yes, CurrencyCode = "SAR", TaxGroup = "VAT15", TaxItemGroup = "VAT15",
            ReceiptDateRequested = new DateTime(2026, 10, 10)
        };
        await new SalesInventoryDemandService(work, numbers, new InventDimensionResolver(work, numbers)).CreateAsync(order, line, "S01", "WH-01");
        db.Set<SalesLine>().Add(line);
        await db.SaveChangesAsync();
        Assert.Equal(200, (await new PackingSlipJournalService(work, company, Sequence()).PostAsync(order.RecId,
            new PostPackingSlipRequest(new DateTime(2026, 10, 6), "All", true, "ps-1",
                [new PackingSlipPostingLineRequest(line.RecId.ToString(), 10)]), default)).StatusCode);
        Assert.Equal(200, (await new SalesInvoiceService(work, company, Sequence()).PostAsync(order.RecId,
            new SalesInvoicePostDto(new DateTime(2026, 10, 7), "inv-1"), default)).StatusCode);
        return (await db.Set<CustInvoiceJour>().SingleAsync()).InvoiceId;
    }

    private static void SeedSalesMasterData(CycleContext db)
    {
        db.Set<CustTable>().Add(new CustTable { RecId = 1, DataAreaId = "dat", AccountNum = "C-001", CustGroupId = "DOM", CurrencyCode = "SAR" });
        db.Set<MainAccount>().AddRange(
            new MainAccount { RecId = 4, DataAreaId = "dat", MainAccountId = "220100" },  // output VAT
            new MainAccount { RecId = 5, DataAreaId = "dat", MainAccountId = "410100" },  // sales revenue
            new MainAccount { RecId = 6, DataAreaId = "dat", MainAccountId = "120100" },  // accounts receivable
            new MainAccount { RecId = 7, DataAreaId = "dat", MainAccountId = "110100" }); // bank
        db.Set<DimensionAttributeValueCombination>().AddRange(
            new DimensionAttributeValueCombination { RecId = 4, DataAreaId = "dat", MainAccount = 4, DisplayValue = "220100" },
            new DimensionAttributeValueCombination { RecId = 5, DataAreaId = "dat", MainAccount = 5, DisplayValue = "410100" },
            new DimensionAttributeValueCombination { RecId = 6, DataAreaId = "dat", MainAccount = 6, DisplayValue = "120100" },
            new DimensionAttributeValueCombination { RecId = 7, DataAreaId = "dat", MainAccount = 7, DisplayValue = "110100" });
        db.Set<InventPosting>().AddRange(
            new InventPosting { RecId = 10, DataAreaId = "dat", InventAccountType = InventAccountType.SalesRevenue, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 5 },
            new InventPosting { RecId = 11, DataAreaId = "dat", InventAccountType = InventAccountType.SalesConsump, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 2 },
            new InventPosting { RecId = 12, DataAreaId = "dat", InventAccountType = InventAccountType.SalesIssue, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 1 });
        db.Set<CustLedgerAccounts>().Add(new CustLedgerAccounts { RecId = 1, DataAreaId = "dat", PostingProfile = "GEN", AccountCode = AccountCode.All, SummaryLedgerDimension = 6 });
        db.Set<TaxGroupData>().Add(new TaxGroupData { RecId = 1, DataAreaId = "dat", TaxGroup = "VAT15", TaxCode = "VAT15C" });
        db.Set<TaxOnItem>().Add(new TaxOnItem { RecId = 1, DataAreaId = "dat", TaxItemGroup = "VAT15", TaxCode = "VAT15C" });
        db.Set<TaxData>().Add(new TaxData { RecId = 1, DataAreaId = "dat", TaxCode = "VAT15C", TaxValue = 15m });
        db.Set<TaxTable>().Add(new TaxTable { RecId = 1, DataAreaId = "dat", TaxCode = "VAT15C", TaxAccountGroup = "VATG" });
        db.Set<TaxLedgerAccountGroup>().Add(new TaxLedgerAccountGroup { RecId = 1, DataAreaId = "dat", TaxAccountGroup = "VATG", TaxOutgoingLedgerDimension = 4 });
        db.Set<BankAccountTable>().Add(new BankAccountTable { RecId = 1, DataAreaId = "dat", AccountId = "BANK-1", LedgerDimension = 7, CurrencyCode = "SAR" });
    }

    private static void SeedPurchaseMasterData(CycleContext db)
    {
        db.Set<VendTable>().Add(new VendTable { RecId = 1, DataAreaId = "dat", AccountNum = "V-0001", Currency = "SAR", VendGroup = "LOCAL" });
        db.Set<InventSite>().AddRange(
            new InventSite { RecId = 1, DataAreaId = "dat", SiteId = "S01", Name = "Jeddah Main" },
            new InventSite { RecId = 2, DataAreaId = "dat", SiteId = "S02", Name = "Riyadh Branch" });
        db.Set<InventLocation>().Add(new InventLocation { RecId = 1, DataAreaId = "dat", InventLocationId = "WH-01", InventSiteId = "S01" });
        db.Set<InventTable>().Add(new InventTable { RecId = 1, DataAreaId = "dat", ItemId = "ITEM-001", NameAlias = "Cotton Uniform" });
        db.Set<InventTableModule>().AddRange(
            new InventTableModule { RecId = 1, DataAreaId = "dat", ItemId = "ITEM-001", ModuleType = ModuleInventPurchSales.Inventory, UnitId = "PCS" },
            new InventTableModule { RecId = 2, DataAreaId = "dat", ItemId = "ITEM-001", ModuleType = ModuleInventPurchSales.Purchase, UnitId = "PCS" });
        db.Set<UnitOfMeasure>().Add(new UnitOfMeasure { RecId = 1, Symbol = "PCS", DecimalPrecision = 0 });
        if (!db.Set<InventModelGroup>().Local.Any())
            db.Set<InventModelGroup>().Add(new InventModelGroup { RecId = 1, DataAreaId = "dat", ModelGroupId = "FIFO", InventModel = InventModel.FIFO, StockedProduct = NoYes.Yes });
        db.Set<InventModelGroupItem>().Add(new InventModelGroupItem { RecId = 1, ItemDataAreaId = "dat", ModelGroupDataAreaId = "dat", ModelGroupId = "FIFO", ItemId = "ITEM-001" });
        db.Set<InventItemGroupItem>().Add(new InventItemGroupItem { RecId = 1, ItemDataAreaId = "dat", ItemId = "ITEM-001", ItemGroupId = "FINISHED" });
        // Purchase inventory receipt -> inventory asset 130100; vendor summary -> AP 210100.
        db.Set<MainAccount>().Add(new MainAccount { RecId = 3, DataAreaId = "dat", MainAccountId = "210100" });
        db.Set<DimensionAttributeValueCombination>().Add(new DimensionAttributeValueCombination { RecId = 3, DataAreaId = "dat", MainAccount = 3, DisplayValue = "210100" });
        db.Set<InventPosting>().Add(new InventPosting { RecId = 3, DataAreaId = "dat", InventAccountType = InventAccountType.PurchReceipt, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 1 });
        db.Set<VendLedgerAccounts>().Add(new VendLedgerAccounts { RecId = 1, DataAreaId = "dat", PostingProfile = "GEN", AccountCode = AccountCode.All, SummaryLedgerDimension = 3 });
    }

    private static (PurchOrderService, PurchConfirmationService, ProductReceiptService, VendorInvoiceService) PurchaseServices(CycleContext db, TestInventNumbers? sharedNumbers = null)
    {
        var work = new TestUnitOfWork(db);
        var company = new TestCompany();
        var numbers = sharedNumbers ?? new TestInventNumbers();
        var supply = new PurchInventorySupplyService(work, numbers, new InventDimensionResolver(work, numbers));
        return (new PurchOrderService(work, company, Sequence(), supply, new SalesUnitConversionService(work)),
            new PurchConfirmationService(work, company, Sequence()),
            new ProductReceiptService(work, company, Sequence()),
            new VendorInvoiceService(work, company, Sequence()));
    }

    internal sealed class TestInventNumbers : ISalesInventoryNumberService
    {
        private int _dim, _trans;
        public Task<string> NextInventDimIdAsync(CancellationToken cancellationToken = default) => Task.FromResult($"DIM-{++_dim:000000}");
        public Task<string> NextInventTransIdAsync(CancellationToken cancellationToken = default) => Task.FromResult($"LOT-{++_trans:000000}");
    }

    private static void SeedLedgerAndPosting(CycleContext db)
    {
        db.Set<Ledger>().Add(new Ledger { RecId = 1, DataAreaId = "dat", AccountingCurrency = "SAR", FiscalCalendar = 1 });
        db.Set<FiscalCalendarPeriod>().Add(new FiscalCalendarPeriod { RecId = 1, FiscalCalendar = 1, FiscalCalendarYear = 1, Type = FiscalPeriodType.Operating, StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 12, 31) });
        db.Set<LedgerFiscalCalendarPeriod>().Add(new LedgerFiscalCalendarPeriod { RecId = 1, Ledger = 1, FiscalCalendarPeriod = 1, Status = FiscalPeriodStatus.Open });
        db.Set<MainAccount>().AddRange(
            new MainAccount { RecId = 1, DataAreaId = "dat", MainAccountId = "130100" },
            new MainAccount { RecId = 2, DataAreaId = "dat", MainAccountId = "510100" });
        db.Set<DimensionAttributeValueCombination>().AddRange(
            new DimensionAttributeValueCombination { RecId = 1, DataAreaId = "dat", MainAccount = 1, DisplayValue = "130100" },
            new DimensionAttributeValueCombination { RecId = 2, DataAreaId = "dat", MainAccount = 2, DisplayValue = "510100" });
        db.Set<InventPosting>().AddRange(
            new InventPosting { RecId = 1, DataAreaId = "dat", InventAccountType = InventAccountType.InventReceipt, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 1 },
            new InventPosting { RecId = 2, DataAreaId = "dat", InventAccountType = InventAccountType.InventIssue, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 2 });
    }

    private static ISysNumberSequenceService Sequence() =>
        DispatchProxy.Create<ISysNumberSequenceService, TestSequenceProxy>();

    // ---- Harness ----------------------------------------------------------------------------

    private sealed class Fixture : IAsyncDisposable
    {
        private readonly SqliteConnection _connection = new("Data Source=:memory:");
        public CycleContext Db { get; private set; } = null!;

        public static async Task<Fixture> CreateAsync()
        {
            var fixture = new Fixture();
            await fixture._connection.OpenAsync();
            fixture.Db = new CycleContext(new DbContextOptionsBuilder<CycleContext>().UseSqlite(fixture._connection).Options);
            await fixture.Db.Database.EnsureCreatedAsync();
            return fixture;
        }

        public async ValueTask DisposeAsync() { await Db.DisposeAsync(); await _connection.DisposeAsync(); }
    }

    private sealed class CycleContext(DbContextOptions<CycleContext> options) : DbContext(options)
    {
        private static readonly Type[] Included =
        [
            typeof(InventDim), typeof(InventTrans), typeof(InventTransOrigin), typeof(InventSum),
            typeof(InventModelGroup), typeof(InventModelGroupItem), typeof(InventItemGroupItem),
            typeof(InventPosting), typeof(DimensionAttributeValueCombination), typeof(MainAccount),
            typeof(Ledger), typeof(FiscalCalendarPeriod), typeof(LedgerFiscalCalendarPeriod),
            typeof(InventSettlement), typeof(InventClosing), typeof(InventJournalTrans),
            typeof(GeneralJournalEntry), typeof(GeneralJournalAccountEntry), typeof(SubledgerVoucherGeneralJournalEntry),
            typeof(PurchTable), typeof(PurchLine), typeof(VendPurchOrderJour), typeof(VendPurchOrderTrans),
            typeof(VendPackingSlipJour), typeof(VendPackingSlipTrans), typeof(VendInvoiceJour), typeof(VendInvoiceTrans),
            typeof(VendTrans), typeof(VendLedgerAccounts), typeof(InventTransOriginPurchLine), typeof(VendTable),
            typeof(InventSite), typeof(InventLocation), typeof(InventTable), typeof(InventTableModule), typeof(UnitOfMeasure), typeof(UnitOfMeasureConversion),
            typeof(SalesTable), typeof(SalesLine), typeof(CustTable), typeof(CustParameters), typeof(CustPackingSlipJour), typeof(CustPackingSlipTrans),
            typeof(CustInvoiceJour), typeof(CustInvoiceTrans), typeof(CustTrans), typeof(CustTransOpen), typeof(CustSettlement), typeof(CustLedgerAccounts),
            typeof(TaxGroupData), typeof(TaxOnItem), typeof(TaxData), typeof(TaxTable), typeof(TaxLedgerAccountGroup), typeof(TaxTrans),
            typeof(InventTransOriginSalesLine), typeof(BankAccountTable), typeof(LedgerJournalTable), typeof(LedgerJournalTrans),
            typeof(ReturnDispositionCode), typeof(ReturnReasonCode)
        ];

        protected override void OnModelCreating(ModelBuilder builder)
        {
            var included = new HashSet<Type>(Included);
            foreach (var type in typeof(InventTrans).Assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && typeof(Entity<long>).IsAssignableFrom(t) && !included.Contains(t)))
                builder.Ignore(type);
            foreach (var type in Included) Configure(builder, type);
        }

        private static void Configure(ModelBuilder builder, Type entity)
        {
            var e = builder.Entity(entity);
            e.HasKey(nameof(Entity<long>.RecId));
            e.Property(nameof(Entity<long>.RecId)).ValueGeneratedOnAdd();
            e.Ignore(nameof(Entity<long>.RowVersion));
            foreach (var property in entity.GetProperties())
            {
                var t = property.PropertyType;
                if (t != typeof(string) && t != typeof(byte[]) && !t.IsValueType)
                    e.Ignore(property.Name);
            }
        }
    }

    public class TestSequenceProxy : DispatchProxy
    {
        private int _n;
        protected override object? Invoke(MethodInfo? method, object?[]? args)
        {
            if (method?.Name == nameof(ISysNumberSequenceService.NextAsync))
                return Task.FromResult(new NextSequenceResultDto { EntityName = "X", Value = ++_n, Code = $"X-{_n:D6}" });
            throw new NotSupportedException(method?.Name);
        }
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
            var repository = DispatchProxy.Create<IGenericRepository<T>, CycleRepositoryProxy<T>>();
            ((CycleRepositoryProxy<T>)(object)repository).Context = context;
            return repository;
        }
        public async Task BeginTransactionAsync(CancellationToken ct = default) => _tx = await context.Database.BeginTransactionAsync(ct);
        public async Task CommitTransactionAsync(CancellationToken ct = default) { if (_tx != null) await _tx.CommitAsync(ct); }
        public async Task RollbackTransactionAsync(CancellationToken ct = default) { if (_tx != null) await _tx.RollbackAsync(ct); }
        public void Dispose() { }
    }

    public class CycleRepositoryProxy<T> : DispatchProxy where T : class
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
}
