using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip.Dtos;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Services;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IAX.IXApi.Tests;

/// <summary>
/// CFG-01 of the inventory acceptance reference: a correct site and warehouse combination is
/// accepted, and a warehouse that belongs to another site is refused. The fixture seeds S01 with
/// warehouse WH-01 and S02 with none, so S02/WH-01 is the mismatch the case asks about.
/// </summary>
public sealed partial class InventoryCycleTests
{
    [Fact]
    public async Task CFG01_a_valid_site_and_warehouse_combination_resolves_one_dimension()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedPurchaseMasterData(db);
        await db.SaveChangesAsync();
        var work = new TestUnitOfWork(db);
        var resolver = new InventDimensionResolver(work, new TestInventNumbers());

        var dimension = await resolver.ResolveAsync("dat", "S01", "WH-01", default);
        Assert.Equal("S01", dimension.InventSiteId);
        Assert.Equal("WH-01", dimension.InventLocationId);

        // Resolving the same combination again must reuse the row rather than create a second one,
        // because on-hand is keyed by the dimension id.
        var again = await resolver.ResolveAsync("dat", "S01", "WH-01", default);
        Assert.Equal(dimension.InventDimId, again.InventDimId);
        Assert.Single(await db.Set<InventDim>().Where(x => x.InventSiteId == "S01").ToListAsync());
    }

    [Fact]
    public async Task CFG01_a_warehouse_outside_the_site_is_refused()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedPurchaseMasterData(db);
        await db.SaveChangesAsync();
        var work = new TestUnitOfWork(db);
        var resolver = new InventDimensionResolver(work, new TestInventNumbers());

        // WH-01 belongs to S01, so asking for it under S02 is the combination the case rejects.
        var mismatch = await Assert.ThrowsAsync<InvalidOperationException>(
            () => resolver.ResolveAsync("dat", "S02", "WH-01", default));
        Assert.Equal("The warehouse does not belong to the selected site and company.", mismatch.Message);

        var unknownSite = await Assert.ThrowsAsync<InvalidOperationException>(
            () => resolver.ResolveAsync("dat", "S99", "WH-01", default));
        Assert.Equal("The warehouse does not belong to the selected site and company.", unknownSite.Message);

        var blank = await Assert.ThrowsAsync<InvalidOperationException>(
            () => resolver.ResolveAsync("dat", "S01", "   ", default));
        Assert.Equal("Company, site, and warehouse are required for basic inventory dimensions.", blank.Message);

        // A refused combination must leave no dimension behind for a later lookup to match.
        Assert.Empty(await db.Set<InventDim>().ToListAsync());
    }

    [Fact]
    public async Task CFG01_a_purchase_line_refuses_a_warehouse_outside_its_site()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        await db.SaveChangesAsync();
        var (orders, _, _, _) = PurchaseServices(db);
        Assert.Equal(200, (await orders.CreateAsync(
            new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 11, 1), "S01", "WH-01", null), default)).StatusCode);
        var order = await db.Set<PurchTable>().OrderByDescending(x => x.RecId).FirstAsync();

        // The owning service refuses the combination before a line exists, rather than leaving the
        // resolver to throw mid-posting.
        var mismatch = await orders.AddLineAsync(order.RecId,
            new PurchLineInputDto("ITEM-001", 10, "PCS", 10, 1, "S02", "WH-01", null, null), default);
        Assert.Equal(422, mismatch.StatusCode);
        Assert.Empty(await db.Set<PurchLine>().Where(x => x.PurchId == order.PurchId).ToListAsync());
    }
    /// <summary>
    /// CFG-03 of the reference: every operation posts a balanced voucher to the configured accounts,
    /// and a missing required account must refuse the post without a partial one. The existing atomic
    /// rollback test covers a refusal for insufficient availability; this covers the missing-account
    /// path, which is what the case names.
    /// </summary>
    [Fact]
    public async Task CFG03_a_missing_posting_account_refuses_the_post_and_leaves_no_partial_voucher()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        SeedSalesMasterData(db);
        // Physical revenue posting is opted into, so the packing slip must resolve an inventory
        // account. Without it the posting is physical-only, needs no voucher, and a missing
        // account would be correctly irrelevant.
        var model = db.Set<InventModelGroup>().Local.Single();
        model.PostOnHandPhysical = NoYes.Yes;
        model.PostPhysicalRevenue = NoYes.Yes;
        // The packing-slip offset account is configured; the packing-slip account itself is left out,
        // so resolution fails on a required account rather than on an absent profile set.
        db.Add(new InventPosting
        {
            RecId = 14, DataAreaId = "dat", InventAccountType = InventAccountType.SalesPackingSlipOffsetAccount,
            ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 2
        });
        await db.SaveChangesAsync();
        var numbers = new TestInventNumbers();
        var services = PurchaseServices(db, numbers);
        await ReceiveIntoBatchAsync(db, services, string.Empty, 10, 1);

        var sales = new SalesTable
        {
            DataAreaId = "dat", SalesId = "SO-CFG03", SalesStatus = SalesStatus.Backorder, CustAccount = "C-001",
            CurrencyCode = "SAR", InventSiteId = "S01", InventLocationId = "WH-01"
        };
        db.Add(sales);
        await db.SaveChangesAsync();
        var salesLine = new SalesLine
        {
            DataAreaId = "dat", SalesId = sales.SalesId, SalesStatus = SalesStatus.Backorder, ItemId = "ITEM-001",
            SalesQty = 4, QtyOrdered = 4, SalesUnit = "PCS", SalesPrice = 25, PriceUnit = 1, LineAmount = 100,
            RemainSalesPhysical = 4, RemainSalesFinancial = 4, StockedProduct = NoYes.Yes
        };
        var work = new TestUnitOfWork(db);
        await new SalesInventoryDemandService(work, numbers, new InventDimensionResolver(work, numbers))
            .CreateAsync(sales, salesLine, "S01", "WH-01");
        db.Add(salesLine);
        await db.SaveChangesAsync();

        var onHandBefore = await db.Set<InventSum>().Where(x => x.ItemId == "ITEM-001").SumAsync(x => x.PhysicalInvent);
        var movementsBefore = await db.Set<InventTrans>().CountAsync();
        var vouchersBefore = await db.Set<GeneralJournalEntry>().CountAsync();

        // The exception type is internal to the Finance assembly, so the message is the contract
        // asserted here; whether it surfaces as a 422 result or an exception, the text is the same.
        string? message = null;
        int? status = null;
        try
        {
            var result = await new PackingSlipJournalService(work, new TestCompany(), Sequence()).PostAsync(sales.RecId,
                new PostPackingSlipRequest(new DateTime(2026, 11, 3), "All", true, "PS-CFG03",
                    [new PackingSlipPostingLineRequest(salesLine.RecId.ToString(), 4)]), default);
            status = result.StatusCode;
            message = result.Message;
        }
        catch (Exception exception)
        {
            message = exception.Message;
        }

        Assert.NotEqual(200, status);
        Assert.Equal("Inventory posting setup is missing for SalesPackingSlip and item 'ITEM-001'.", message);

        // Nothing partial: no packing slip, no movement, no voucher, and on-hand untouched.
        db.ChangeTracker.Clear();
        Assert.Empty(await db.Set<CustPackingSlipJour>().Where(x => x.SalesId == sales.SalesId).ToListAsync());
        Assert.Equal(movementsBefore, await db.Set<InventTrans>().CountAsync());
        Assert.Equal(vouchersBefore, await db.Set<GeneralJournalEntry>().CountAsync());
        Assert.Equal(onHandBefore, await db.Set<InventSum>().Where(x => x.ItemId == "ITEM-001").SumAsync(x => x.PhysicalInvent));
        Assert.Equal(0m, (await db.Set<InventTrans>().Where(x => x.StatusIssue == StatusIssue.Deducted).ToListAsync()).Sum(x => x.Qty));
    }
    /// <summary>
    /// Builds a delivery ready to post, with physical revenue posting opted in so the delivery
    /// reaches the ledger and therefore the fiscal-period guard. Returns the order and its line.
    /// </summary>
    private static async Task<(SalesTable Order, SalesLine Line)> SeedDeliverableSalesOrderAsync(CycleContext db)
    {
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        SeedSalesMasterData(db);
        var model = db.Set<InventModelGroup>().Local.Single();
        model.PostOnHandPhysical = NoYes.Yes;
        model.PostPhysicalRevenue = NoYes.Yes;
        db.AddRange(
            new InventPosting { RecId = 13, DataAreaId = "dat", InventAccountType = InventAccountType.SalesPackingSlip, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 1 },
            new InventPosting { RecId = 14, DataAreaId = "dat", InventAccountType = InventAccountType.SalesPackingSlipOffsetAccount, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 2 });
        await db.SaveChangesAsync();
        var numbers = new TestInventNumbers();
        await ReceiveIntoBatchAsync(db, PurchaseServices(db, numbers), string.Empty, 10, 1);

        var order = new SalesTable
        {
            DataAreaId = "dat", SalesId = "SO-CFG04", SalesStatus = SalesStatus.Backorder, CustAccount = "C-001",
            CurrencyCode = "SAR", InventSiteId = "S01", InventLocationId = "WH-01"
        };
        db.Add(order);
        await db.SaveChangesAsync();
        var line = new SalesLine
        {
            DataAreaId = "dat", SalesId = order.SalesId, SalesStatus = SalesStatus.Backorder, ItemId = "ITEM-001",
            SalesQty = 4, QtyOrdered = 4, SalesUnit = "PCS", SalesPrice = 25, PriceUnit = 1, LineAmount = 100,
            RemainSalesPhysical = 4, RemainSalesFinancial = 4, StockedProduct = NoYes.Yes
        };
        var work = new TestUnitOfWork(db);
        await new SalesInventoryDemandService(work, numbers, new InventDimensionResolver(work, numbers))
            .CreateAsync(order, line, "S01", "WH-01");
        db.Add(line);
        await db.SaveChangesAsync();
        return (order, line);
    }

    /// <summary>Posts the delivery and returns the refusal text, however it surfaces.</summary>
    private static async Task<(int? Status, string? Message)> TryDeliverAsync(CycleContext db,
        SalesTable order, SalesLine line, DateTime date, string parmId)
    {
        try
        {
            var result = await new PackingSlipJournalService(new TestUnitOfWork(db), new TestCompany(), Sequence())
                .PostAsync(order.RecId, new PostPackingSlipRequest(date, "All", true, parmId,
                    [new PackingSlipPostingLineRequest(line.RecId.ToString(), 4)]), default);
            return (result.StatusCode, result.Message);
        }
        catch (Exception exception)
        {
            return (null, exception.Message);
        }
    }

    [Fact]
    public async Task CFG04_posting_into_a_closed_fiscal_period_is_refused_with_no_partial_posting()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (order, line) = await SeedDeliverableSalesOrderAsync(db);

        // The seeded 2026 period is closed rather than removed, so the refusal comes from the
        // period status and not from an absent period.
        db.Set<LedgerFiscalCalendarPeriod>().Local.Single().Status = FiscalPeriodStatus.Closed;
        await db.SaveChangesAsync();

        var onHandBefore = await db.Set<InventSum>().Where(x => x.ItemId == "ITEM-001").SumAsync(x => x.PhysicalInvent);
        var movementsBefore = await db.Set<InventTrans>().CountAsync();
        var vouchersBefore = await db.Set<GeneralJournalEntry>().CountAsync();

        var date = new DateTime(2026, 11, 3);
        var (status, message) = await TryDeliverAsync(db, order, line, date, "PS-CLOSED");

        Assert.NotEqual(200, status);
        // Built with the same interpolation the product uses, so the date format cannot drift apart.
        Assert.Equal($"The fiscal period for {date:d} is not open.", message);

        db.ChangeTracker.Clear();
        Assert.Empty(await db.Set<CustPackingSlipJour>().Where(x => x.SalesId == order.SalesId).ToListAsync());
        Assert.Equal(movementsBefore, await db.Set<InventTrans>().CountAsync());
        Assert.Equal(vouchersBefore, await db.Set<GeneralJournalEntry>().CountAsync());
        Assert.Equal(onHandBefore, await db.Set<InventSum>().Where(x => x.ItemId == "ITEM-001").SumAsync(x => x.PhysicalInvent));
    }

    [Fact]
    public async Task CFG04_a_date_outside_every_operating_period_is_refused()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (order, line) = await SeedDeliverableSalesOrderAsync(db);

        // 2027 lies outside the seeded 2026 operating period.
        var date = new DateTime(2027, 2, 1);
        var (status, message) = await TryDeliverAsync(db, order, line, date, "PS-NOPERIOD");

        Assert.NotEqual(200, status);
        Assert.Equal($"No operating fiscal period contains {date:d}.", message);
        Assert.Empty(await db.Set<CustPackingSlipJour>().Where(x => x.SalesId == order.SalesId).ToListAsync());
    }

    [Fact]
    public async Task CFG04_a_module_closed_for_the_period_is_refused()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (order, line) = await SeedDeliverableSalesOrderAsync(db);

        // The period stays open; only the posting module is closed. This guard is wrapped in a
        // FindEntityType check, so it is enforced only because CycleContext now maps the entity.
        var ledgerPeriod = db.Set<LedgerFiscalCalendarPeriod>().Local.Single();
        db.Add(new LedgerPeriodModuleAccessControl
        {
            RecId = 801, DataAreaId = "dat", LedgerFiscalCalendarPeriod = ledgerPeriod.RecId,
            CustAccessLevel = LedgerPeriodAccessLevel.None
        });
        await db.SaveChangesAsync();

        var date = new DateTime(2026, 11, 3);
        var (status, message) = await TryDeliverAsync(db, order, line, date, "PS-MODULECLOSED");

        Assert.NotEqual(200, status);
        Assert.Equal($"The Customer module is closed for the fiscal period containing {date:d}.", message);
        Assert.Empty(await db.Set<CustPackingSlipJour>().Where(x => x.SalesId == order.SalesId).ToListAsync());
    }

    [Fact]
    public async Task CFG04_an_open_period_posts_the_same_delivery()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (order, line) = await SeedDeliverableSalesOrderAsync(db);

        // The control: with the period open and no module restriction, the identical call posts.
        // Without this the three refusals above could pass for an unrelated reason.
        var (status, message) = await TryDeliverAsync(db, order, line, new DateTime(2026, 11, 3), "PS-OPEN");

        Assert.True(status == 200, message);
        Assert.Single(await db.Set<CustPackingSlipJour>().Where(x => x.SalesId == order.SalesId).ToListAsync());
    }
    /// <summary>
    /// RES-02 of the reference: a reservation or issue beyond what is available is refused by the
    /// control that owns that stage. The concurrent-request half of the case needs SQL Server and is
    /// not covered here; the register records that.
    /// </summary>
    [Fact]
    public async Task RES02_reserving_beyond_demand_or_beyond_available_stock_is_refused()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        SeedSalesMasterData(db);
        await db.SaveChangesAsync();
        var numbers = new TestInventNumbers();
        // Three on hand against a demand of ten, so the two limits are distinguishable: demand is the
        // larger bound and physical availability the smaller one.
        await ReceiveIntoBatchAsync(db, PurchaseServices(db, numbers), string.Empty, 3, 1);

        var order = new SalesTable
        {
            DataAreaId = "dat", SalesId = "SO-RES02", SalesStatus = SalesStatus.Backorder, CustAccount = "C-001",
            CurrencyCode = "SAR", InventSiteId = "S01", InventLocationId = "WH-01"
        };
        db.Add(order);
        await db.SaveChangesAsync();
        var line = new SalesLine
        {
            DataAreaId = "dat", SalesId = order.SalesId, SalesStatus = SalesStatus.Backorder, ItemId = "ITEM-001",
            SalesQty = 10, QtyOrdered = 10, SalesUnit = "PCS", SalesPrice = 25, PriceUnit = 1, LineAmount = 250,
            RemainSalesPhysical = 10, RemainSalesFinancial = 10, StockedProduct = NoYes.Yes
        };
        var work = new TestUnitOfWork(db);
        await new SalesInventoryDemandService(work, numbers, new InventDimensionResolver(work, numbers))
            .CreateAsync(order, line, "S01", "WH-01");
        db.Add(line);
        await db.SaveChangesAsync();
        var reservations = new InventoryReservationService(work, new TestCompany());

        // Above the demand itself: refused by the demand bound, before availability is consulted.
        var beyondDemand = await reservations.ReserveAsync(line.InventTransId, 11m, false, default);
        Assert.Equal("The reservation cannot exceed the source's remaining demand.", beyondDemand.Error);

        // Within demand but above physical stock, with partial disallowed.
        var beyondStock = await reservations.ReserveAsync(line.InventTransId, 10m, false, default);
        Assert.Equal("There is not enough available physical stock to reserve the requested quantity.", beyondStock.Error);

        db.ChangeTracker.Clear();
        var untouched = await db.Set<InventSum>().SingleAsync(x => x.ItemId == "ITEM-001");
        Assert.Equal(3m, untouched.PhysicalInvent);
        Assert.Equal(0m, untouched.ReservPhysical);

        // The same request with partial allowed takes exactly what is available and no more.
        var partial = await reservations.ReserveAsync(line.InventTransId, 10m, true, default);
        Assert.Null(partial.Error);
        Assert.Equal(3m, partial.Result!.Reserved);
        db.ChangeTracker.Clear();
        var reserved = await db.Set<InventSum>().SingleAsync(x => x.ItemId == "ITEM-001");
        Assert.Equal(3m, reserved.PhysicalInvent);
        Assert.Equal(3m, reserved.ReservPhysical);
        Assert.Equal(0m, reserved.AvailPhysical);

        // Nothing is left to reserve once availability is exhausted.
        var exhausted = await reservations.ReserveAsync(line.InventTransId, 10m, true, default);
        Assert.Equal("There is no available physical stock to reserve.", exhausted.Error);
    }
    /// <summary>
    /// MD-03 of the reference: buying 2 BOX at a factor of 10 must raise inventory by 20 PCS. The
    /// conversion service and the barcode lookup are covered elsewhere; what is asserted here is that
    /// the purchase path actually applies the conversion through to the posted movement and on-hand.
    /// </summary>
    [Fact]
    public async Task MD03_a_purchase_in_a_larger_unit_posts_the_converted_inventory_quantity()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        // PCS is the item's inventory unit and carries zero decimals; BOX converts to it at 10 to 1.
        // Product 0 scopes the rule globally, so no product row is needed.
        db.Set<UnitOfMeasure>().Add(new UnitOfMeasure { RecId = 2, Symbol = "BOX", DecimalPrecision = 0 });
        db.Set<UnitOfMeasureConversion>().Add(new UnitOfMeasureConversion
        {
            RecId = 3, Product = 0, FromUnitOfMeasure = 2, ToUnitOfMeasure = 1, Numerator = 10, Denominator = 1
        });
        await db.SaveChangesAsync();

        var numbers = new TestInventNumbers();
        var (orders, confirmations, receipts, invoices) = PurchaseServices(db, numbers);
        Assert.Equal(200, (await orders.CreateAsync(
            new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 11, 1), "S01", "WH-01", null), default)).StatusCode);
        var order = await db.Set<PurchTable>().OrderByDescending(x => x.RecId).FirstAsync();

        // Two boxes at 100 a box; the line keeps the purchase unit while inventory counts pieces.
        Assert.Equal(200, (await orders.AddLineAsync(order.RecId,
            new PurchLineInputDto("ITEM-001", 2, "BOX", 100, 1, null, null, null, null), default)).StatusCode);
        var line = await db.Set<PurchLine>().SingleAsync(x => x.PurchId == order.PurchId);

        Assert.Equal(2m, line.PurchQty);
        Assert.Equal("BOX", line.PurchUnit);
        Assert.Equal(20m, line.QtyOrdered);
        Assert.Equal(20m, line.RemainInventPhysical);

        // The ordered supply is the converted quantity, before any receipt exists.
        db.ChangeTracker.Clear();
        Assert.Equal(20m, await db.Set<InventSum>().Where(x => x.ItemId == "ITEM-001").SumAsync(x => x.Ordered));
        Assert.Equal(0m, await db.Set<InventSum>().Where(x => x.ItemId == "ITEM-001").SumAsync(x => x.PhysicalInvent));

        Assert.Equal(200, (await confirmations.ConfirmAsync(order.RecId, default)).StatusCode);
        // The receipt is stated in the purchase unit, as the document is.
        Assert.Equal(200, (await receipts.PostAsync(order.RecId,
            new ProductReceiptPostDto("RCV-BOX", new DateTime(2026, 11, 2), [new(line.RecId, 2)]), default)).StatusCode);

        db.ChangeTracker.Clear();
        Assert.Equal(20m, await db.Set<InventSum>().Where(x => x.ItemId == "ITEM-001").SumAsync(x => x.PhysicalInvent));
        var movement = await db.Set<InventTrans>().SingleAsync(x => x.StatusReceipt == StatusReceipt.Received);
        Assert.Equal(20m, movement.Qty);

        Assert.Equal(200, (await invoices.PostAsync(order.RecId,
            new VendorInvoicePostDto("INV-BOX", new DateTime(2026, 11, 2), [new(line.RecId, 2)]), default)).StatusCode);
        db.ChangeTracker.Clear();
        // Two boxes at 100 is 200 of value over 20 pieces, so the unit cost is 10 a piece.
        var summary = await db.Set<InventSum>().SingleAsync(x => x.ItemId == "ITEM-001");
        Assert.Equal(20m, summary.PhysicalInvent);
        Assert.Equal(200m, summary.PostedValue);
    }
}
