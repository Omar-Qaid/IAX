using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip.Dtos;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Services;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IAX.IXApi.Tests;

/// <summary>
/// MD-04 of the inventory acceptance reference: a batch dimension is preserved from receipt
/// through reservation and issue, a required batch cannot be left blank, and an undefined batch
/// is refused. Batch tracking is in scope by explicit authorization; serial tracking shares the
/// same policy and is exercised only through its blank-and-undefined checks here.
/// </summary>
public sealed partial class InventoryCycleTests
{
    /// <summary>Activates batch tracking for ITEM-001 and defines the batches the tests post into.</summary>
    private static void SeedBatchTracking(CycleContext db, params string[] batchIds)
    {
        // The group row itself is not seeded: the policy resolves a group only through the item
        // assignment and its field setup, which is all this context carries.
        // Blank is allowed on neither side, which is what makes a missing batch a refusal rather
        // than a silently blank dimension.
        db.Set<EcoResTrackingDimensionGroupFldSetup>().AddRange(
            new EcoResTrackingDimensionGroupFldSetup
            {
                RecId = 702, TrackingDimensionGroup = 701, DimensionFieldId = InventDimensionFields.BatchNumber,
                LineNumber = 1, IsActive = true, IsPhysicalInventoryEnabled = true,
                IsFinancialInventoryEnabled = false, IsAllowBlankReceiptEnabled = false, IsAllowBlankIssueEnabled = false
            },
            new EcoResTrackingDimensionGroupFldSetup
            {
                RecId = 703, TrackingDimensionGroup = 701, DimensionFieldId = InventDimensionFields.SerialNumber,
                LineNumber = 2, IsActive = false, IsPhysicalInventoryEnabled = false,
                IsFinancialInventoryEnabled = false, IsAllowBlankReceiptEnabled = true, IsAllowBlankIssueEnabled = true
            });
        db.Set<EcoResTrackingDimensionGroupItem>().Add(new EcoResTrackingDimensionGroupItem
        {
            RecId = 704, ItemId = "ITEM-001", ItemDataAreaId = "dat", TrackingDimensionGroup = 701
        });
        var recId = 710;
        foreach (var batchId in batchIds)
            db.Set<InventBatch>().Add(new InventBatch
            {
                RecId = recId++, DataAreaId = "dat", ItemId = "ITEM-001", InventBatchId = batchId
            });
    }

    /// <summary>
    /// Receives <paramref name="qty"/> of ITEM-001 under <paramref name="trackingId"/>, read as a
    /// serial number when <paramref name="serial"/> is set and as a batch otherwise. The services
    /// are passed in rather than built here: each PurchaseServices call starts its own number
    /// sequence, so building them per receipt would hand both orders the same PurchId.
    /// </summary>
    private static async Task<PurchLine> ReceiveIntoBatchAsync(CycleContext db,
        (PurchOrderService Orders, PurchConfirmationService Confirmations, ProductReceiptService Receipts, VendorInvoiceService Invoices) services,
        string trackingId, decimal qty, int day, bool serial = false)
    {
        var (orders, confirmations, receipts, invoices) = services;
        var created = await orders.CreateAsync(
            new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 11, day), "S01", "WH-01", null), default);
        Assert.Equal(200, created.StatusCode);
        var order = await db.Set<PurchTable>().OrderByDescending(x => x.RecId).FirstAsync();
        var added = await orders.AddLineAsync(order.RecId,
            new PurchLineInputDto("ITEM-001", qty, "PCS", 10, 1, null, null, null, null,
                InventBatchId: serial ? null : trackingId, InventSerialId: serial ? trackingId : null), default);
        Assert.Equal(200, added.StatusCode);
        Assert.Equal(200, (await confirmations.ConfirmAsync(order.RecId, default)).StatusCode);
        var line = await db.Set<PurchLine>().SingleAsync(x => x.PurchId == order.PurchId);
        Assert.Equal(200, (await receipts.PostAsync(order.RecId,
            new ProductReceiptPostDto($"RCV-{trackingId}", new DateTime(2026, 11, day), [new(line.RecId, qty)]), default)).StatusCode);
        Assert.Equal(200, (await invoices.PostAsync(order.RecId,
            new VendorInvoicePostDto($"INV-{trackingId}", new DateTime(2026, 11, day), [new(line.RecId, qty)]), default)).StatusCode);
        return line;
    }

    [Fact]
    public async Task MD04_receipt_keeps_each_batch_on_its_own_dimension_and_on_hand_row()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        SeedBatchTracking(db, "B-001", "B-002");
        await db.SaveChangesAsync();
        var numbers = new TestInventNumbers();
        var services = PurchaseServices(db, numbers);

        var first = await ReceiveIntoBatchAsync(db, services, "B-001", 10, 1);
        var second = await ReceiveIntoBatchAsync(db, services, "B-002", 6, 2);

        // Each batch resolves to its own InventDim, so the two receipts cannot collapse into one
        // on-hand row even though item, site and warehouse are identical.
        var firstDim = await db.Set<InventDim>().SingleAsync(x => x.InventDimId == first.InventDimId);
        var secondDim = await db.Set<InventDim>().SingleAsync(x => x.InventDimId == second.InventDimId);
        Assert.Equal("B-001", firstDim.InventBatchId);
        Assert.Equal("B-002", secondDim.InventBatchId);
        Assert.NotEqual(first.InventDimId, second.InventDimId);
        Assert.Equal("S01", secondDim.InventSiteId);
        Assert.Equal("WH-01", secondDim.InventLocationId);

        var sums = await db.Set<InventSum>().Where(x => x.ItemId == "ITEM-001").ToListAsync();
        Assert.Equal(2, sums.Count);
        Assert.Equal(10m, sums.Single(x => x.InventDimId == first.InventDimId).PhysicalInvent);
        Assert.Equal(6m, sums.Single(x => x.InventDimId == second.InventDimId).PhysicalInvent);
    }

    [Fact]
    public async Task MD04_issue_deducts_only_the_batch_it_names()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        SeedSalesMasterData(db);
        SeedBatchTracking(db, "B-001", "B-002");
        db.AddRange(
            new InventPosting { RecId = 13, DataAreaId = "dat", InventAccountType = InventAccountType.SalesPackingSlip, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 1 },
            new InventPosting { RecId = 14, DataAreaId = "dat", InventAccountType = InventAccountType.SalesPackingSlipOffsetAccount, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 2 });
        await db.SaveChangesAsync();
        var numbers = new TestInventNumbers();
        var services = PurchaseServices(db, numbers);

        var first = await ReceiveIntoBatchAsync(db, services, "B-001", 10, 1);
        var second = await ReceiveIntoBatchAsync(db, services, "B-002", 6, 2);

        var sales = new SalesTable
        {
            DataAreaId = "dat", SalesId = "SO-BATCH", SalesStatus = SalesStatus.Backorder, CustAccount = "C-001",
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
        // The demand carries the batch, so the issue resolves to the same dimension the receipt used.
        await new SalesInventoryDemandService(work, numbers, new InventDimensionResolver(work, numbers))
            .CreateAsync(sales, salesLine, "S01", "WH-01", dimensionTemplate: new InventDim { InventBatchId = "B-001" });
        db.Add(salesLine);
        await db.SaveChangesAsync();

        var result = await new PackingSlipJournalService(work, new TestCompany(), Sequence()).PostAsync(sales.RecId,
            new PostPackingSlipRequest(new DateTime(2026, 11, 3), "All", true, "PS-BATCH",
                [new PackingSlipPostingLineRequest(salesLine.RecId.ToString(), 4)]), default);
        Assert.True(result.StatusCode == 200, result.Message);

        var issue = await db.Set<InventTrans>().SingleAsync(x => x.StatusIssue == StatusIssue.Deducted);
        Assert.Equal(first.InventDimId, issue.InventDimId);
        var sums = await db.Set<InventSum>().Where(x => x.ItemId == "ITEM-001").ToListAsync();
        Assert.Equal(6m, sums.Single(x => x.InventDimId == first.InventDimId).PhysicalInvent);
        Assert.Equal(6m, sums.Single(x => x.InventDimId == second.InventDimId).PhysicalInvent);
    }

    [Fact]
    public async Task MD04_a_tracked_item_refuses_a_blank_batch()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        SeedBatchTracking(db, "B-001");
        await db.SaveChangesAsync();
        var (orders, _, _, _) = PurchaseServices(db);
        Assert.Equal(200, (await orders.CreateAsync(
            new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 11, 1), "S01", "WH-01", null), default)).StatusCode);
        var order = await db.Set<PurchTable>().OrderByDescending(x => x.RecId).FirstAsync();

        var blank = await orders.AddLineAsync(order.RecId,
            new PurchLineInputDto("ITEM-001", 10, "PCS", 10, 1, null, null, null, null), default);
        Assert.Equal(422, blank.StatusCode);
        Assert.Equal("A batch number is required for item 'ITEM-001'.", blank.Message);
        Assert.Empty(await db.Set<PurchLine>().Where(x => x.PurchId == order.PurchId).ToListAsync());
    }

    [Fact]
    public async Task MD04_a_tracked_item_refuses_a_batch_that_is_not_defined()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        SeedBatchTracking(db, "B-001");
        await db.SaveChangesAsync();
        var (orders, _, _, _) = PurchaseServices(db);
        Assert.Equal(200, (await orders.CreateAsync(
            new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 11, 1), "S01", "WH-01", null), default)).StatusCode);
        var order = await db.Set<PurchTable>().OrderByDescending(x => x.RecId).FirstAsync();

        var unknown = await orders.AddLineAsync(order.RecId,
            new PurchLineInputDto("ITEM-001", 10, "PCS", 10, 1, null, null, null, null, InventBatchId: "B-404"), default);
        Assert.Equal(422, unknown.StatusCode);
        Assert.Equal("Batch 'B-404' is not defined for item 'ITEM-001'.", unknown.Message);
        Assert.Empty(await db.Set<PurchLine>().Where(x => x.PurchId == order.PurchId).ToListAsync());
    }

    [Fact]
    public async Task MD04_an_untracked_item_refuses_a_batch_it_cannot_record()
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

        var batched = await orders.AddLineAsync(order.RecId,
            new PurchLineInputDto("ITEM-001", 10, "PCS", 10, 1, null, null, null, null, InventBatchId: "B-001"), default);
        Assert.Equal(422, batched.StatusCode);
        Assert.Equal("Item 'ITEM-001' has no tracking dimension group, so a batch or serial number cannot be recorded.", batched.Message);
    }

    [Fact]
    public async Task MD04_dimensions_outside_the_basic_scope_are_still_refused()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedPurchaseMasterData(db);
        await db.SaveChangesAsync();
        var work = new TestUnitOfWork(db);
        var resolver = new InventDimensionResolver(work, new TestInventNumbers());

        // Relaxing the guard admitted batch and serial only; the product and WMS dimensions stay out.
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => resolver.ResolveAsync(
            "dat", "S01", "WH-01", default, new InventDim { ConfigId = "CFG-1" }));
        Assert.Equal("Only site, warehouse, batch, and serial dimensions are supported in the basic inventory scope.", error.Message);

        var batched = await resolver.ResolveAsync("dat", "S01", "WH-01", default, new InventDim { InventBatchId = "B-001" });
        Assert.Equal("B-001", batched.InventBatchId);
    }
    /// <summary>Activates serial tracking for ITEM-001 and defines the serial numbers used.</summary>
    private static void SeedSerialTracking(CycleContext db, params string[] serialIds)
    {
        db.Set<EcoResTrackingDimensionGroupFldSetup>().AddRange(
            new EcoResTrackingDimensionGroupFldSetup
            {
                RecId = 752, TrackingDimensionGroup = 751, DimensionFieldId = InventDimensionFields.BatchNumber,
                LineNumber = 1, IsActive = false, IsPhysicalInventoryEnabled = false,
                IsFinancialInventoryEnabled = false, IsAllowBlankReceiptEnabled = true, IsAllowBlankIssueEnabled = true
            },
            new EcoResTrackingDimensionGroupFldSetup
            {
                RecId = 753, TrackingDimensionGroup = 751, DimensionFieldId = InventDimensionFields.SerialNumber,
                LineNumber = 2, IsActive = true, IsPhysicalInventoryEnabled = true,
                IsFinancialInventoryEnabled = false, IsAllowBlankReceiptEnabled = false, IsAllowBlankIssueEnabled = false
            });
        db.Set<EcoResTrackingDimensionGroupItem>().Add(new EcoResTrackingDimensionGroupItem
        {
            RecId = 754, ItemId = "ITEM-001", ItemDataAreaId = "dat", TrackingDimensionGroup = 751
        });
        var recId = 760;
        foreach (var serialId in serialIds)
            db.Set<InventSerial>().Add(new InventSerial
            {
                RecId = recId++, DataAreaId = "dat", ItemId = "ITEM-001", InventSerialId = serialId
            });
    }

    [Fact]
    public async Task MD04_serial_receipt_keeps_one_unit_on_its_own_dimension()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        SeedSerialTracking(db, "SN-1", "SN-2");
        await db.SaveChangesAsync();
        var numbers = new TestInventNumbers();
        var services = PurchaseServices(db, numbers);

        // A serial-tracked line carries exactly one unit, so two units are two receipts.
        var first = await ReceiveIntoBatchAsync(db, services, "SN-1", 1, 1, serial: true);
        var second = await ReceiveIntoBatchAsync(db, services, "SN-2", 1, 2, serial: true);

        var firstDim = await db.Set<InventDim>().SingleAsync(x => x.InventDimId == first.InventDimId);
        var secondDim = await db.Set<InventDim>().SingleAsync(x => x.InventDimId == second.InventDimId);
        Assert.Equal("SN-1", firstDim.InventSerialId);
        Assert.Equal("SN-2", secondDim.InventSerialId);
        Assert.Equal(string.Empty, firstDim.InventBatchId);
        Assert.NotEqual(first.InventDimId, second.InventDimId);

        var sums = await db.Set<InventSum>().Where(x => x.ItemId == "ITEM-001").ToListAsync();
        Assert.Equal(2, sums.Count);
        Assert.All(sums, sum => Assert.Equal(1m, sum.PhysicalInvent));
    }

    [Fact]
    public async Task MD04_a_serial_tracked_line_must_identify_one_unit()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        SeedSerialTracking(db, "SN-1");
        await db.SaveChangesAsync();
        var (orders, _, _, _) = PurchaseServices(db);
        Assert.Equal(200, (await orders.CreateAsync(
            new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 11, 1), "S01", "WH-01", null), default)).StatusCode);
        var order = await db.Set<PurchTable>().OrderByDescending(x => x.RecId).FirstAsync();

        var tooMany = await orders.AddLineAsync(order.RecId,
            new PurchLineInputDto("ITEM-001", 3, "PCS", 10, 1, null, null, null, null, InventSerialId: "SN-1"), default);
        Assert.Equal(422, tooMany.StatusCode);
        Assert.Equal("Each line for serial-tracked item 'ITEM-001' must identify one serial-numbered unit.", tooMany.Message);

        var blank = await orders.AddLineAsync(order.RecId,
            new PurchLineInputDto("ITEM-001", 1, "PCS", 10, 1, null, null, null, null), default);
        Assert.Equal(422, blank.StatusCode);
        Assert.Equal("A serial number is required for item 'ITEM-001'.", blank.Message);

        var unknown = await orders.AddLineAsync(order.RecId,
            new PurchLineInputDto("ITEM-001", 1, "PCS", 10, 1, null, null, null, null, InventSerialId: "SN-404"), default);
        Assert.Equal(422, unknown.StatusCode);
        Assert.Equal("Serial number 'SN-404' is not defined for item 'ITEM-001'.", unknown.Message);

        Assert.Empty(await db.Set<PurchLine>().Where(x => x.PurchId == order.PurchId).ToListAsync());
    }
}
