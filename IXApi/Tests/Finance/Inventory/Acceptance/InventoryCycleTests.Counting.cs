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
    public async Task Inventory_journal_creation_rejects_reused_sequence_without_changing_old_header()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        db.Add(new InventJournalName { DataAreaId = "dat", JournalNameId = "ADJ", JournalType = InventJournalType.Adjustment });
        db.Add(new InventJournalTable { DataAreaId = "dat", JournalId = "X-000001", Description = "Original", Posted = NoYes.Yes });
        await db.SaveChangesAsync();
        var work = new TestUnitOfWork(db);
        var service = new InventJournalService(work, new TestCompany(), Sequence(),
            new InventDimensionResolver(work, new TestInventNumbers()));
        var result = await service.CreateAsync(new InventJournalCreateDto { JournalNameId = "ADJ", Description = "New example" }, default);
        Assert.Null(result.Journal);
        Assert.Contains("already exists", result.Error);
        var retained = await db.Set<InventJournalTable>().SingleAsync();
        Assert.Equal("Original", retained.Description);
        Assert.Equal(NoYes.Yes, retained.Posted);
    }

    [Fact]
    public async Task Inventory_summary_key_prevents_duplicate_on_hand_rows()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        db.Set<InventSum>().Add(new InventSum { DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM" });
        await db.SaveChangesAsync();
        db.Set<InventSum>().Add(new InventSum { DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM" });
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        db.ChangeTracker.Clear();
        Assert.Single(await db.Set<InventSum>().ToListAsync());
        db.Set<InventSum>().Add(new InventSum { DataAreaId = "usmf", ItemId = "ITEM", InventDimId = "DIM" });
        await db.SaveChangesAsync();
        Assert.Equal(2, await db.Set<InventSum>().CountAsync());
    }

    [Theory]
    [InlineData(0, "1.5", false)]
    [InlineData(0, "2", true)]
    [InlineData(2, "1.23", true)]
    [InlineData(2, "1.234", false)]
    [InlineData(6, "1.00001", false)]
    [InlineData(6, "1.0001", true)]
    public async Task Count_confirmation_respects_inventory_unit_and_storage_precision(int precision, string quantityText, bool accepted)
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedCountableItem(db);
        db.Set<UnitOfMeasure>().Local.Single().DecimalPrecision = precision;
        db.Add(new InventJournalTable { DataAreaId = "dat", JournalId = "CNT-1", JournalType = InventJournalType.Counting, CountingFrozen = true });
        db.Add(new InventJournalTrans { RecId = 1, DataAreaId = "dat", JournalId = "CNT-1", JournalType = InventJournalType.Counting,
            ItemId = "ITEM", InventDimId = "DIM", Unit = "EA" });
        await db.SaveChangesAsync();
        var service = new InventJournalPostingService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence(), new TestInventNumbers());
        var quantity = decimal.Parse(quantityText, System.Globalization.CultureInfo.InvariantCulture);

        var error = await service.ConfirmCountAsync("CNT-1", new Dictionary<long, decimal> { [1] = quantity }, default);

        Assert.Equal(accepted, error is null);
        Assert.Equal(accepted, (await db.Set<InventJournalTable>().SingleAsync()).CountingConfirmed);
        if (accepted) Assert.Equal(quantity, (await db.Set<InventJournalTrans>().SingleAsync()).Counted);
    }

    [Fact]
    public async Task Counting_journal_generates_lines_from_on_hand_and_skips_what_it_cannot_count()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedCountableItem(db);
        db.Add(new InventJournalName { DataAreaId = "dat", JournalNameId = "CNT", JournalType = InventJournalType.Counting });
        db.Add(new InventJournalTable
        {
            DataAreaId = "dat", JournalId = "CNT-1", JournalNameId = "CNT",
            JournalType = InventJournalType.Counting, Posted = NoYes.No
        });
        // Countable stock, batch-dimension stock the basic journal cannot count, and an empty bin.
        db.Set<InventDim>().AddRange(
            new InventDim { RecId = 1, DataAreaId = "dat", InventDimId = "DIM1", InventSiteId = "S1", InventLocationId = "WH1" },
            new InventDim { RecId = 2, DataAreaId = "dat", InventDimId = "DIM2", InventSiteId = "S1", InventLocationId = "WH1", InventBatchId = "B1" },
            new InventDim { RecId = 3, DataAreaId = "dat", InventDimId = "DIM3", InventSiteId = "S1", InventLocationId = "WH1" });
        db.Set<InventSum>().AddRange(
            new InventSum { RecId = 1, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM1", PhysicalInvent = 10m },
            new InventSum { RecId = 2, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM2", PhysicalInvent = 4m },
            new InventSum { RecId = 3, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM3", PhysicalInvent = 0m });
        await db.SaveChangesAsync();

        var work = new TestUnitOfWork(db);
        var service = new InventJournalService(work, new TestCompany(), Sequence(),
            new InventDimensionResolver(work, new TestInventNumbers()));

        var (result, error) = await service.CreateCountingLinesAsync("CNT-1",
            new InventCountingLinesInputDto { InventSiteId = "S1", InventLocationId = "WH1" }, default);

        Assert.Null(error);
        Assert.Equal(1, result!.LinesCreated);           // only the countable basic dimension
        Assert.Equal(1, result.Skipped);                 // the batch dimension
        Assert.Contains(result.Problems, x => x.Contains("batch", StringComparison.OrdinalIgnoreCase));

        var line = await db.Set<InventJournalTrans>().SingleAsync();
        Assert.Equal("ITEM", line.ItemId);
        Assert.Equal("DIM1", line.InventDimId);
        Assert.Equal(0m, line.Counted);                  // the counter still enters the count
        Assert.Equal(InventRefType.PhysicalinventoryCount, line.InventRefType);
        Assert.Equal(1, (await db.Set<InventJournalTable>().SingleAsync()).NumOfLines);

        // A second run must not duplicate a line that is already in the journal.
        var (second, secondError) = await service.CreateCountingLinesAsync("CNT-1",
            new InventCountingLinesInputDto { InventSiteId = "S1", InventLocationId = "WH1" }, default);
        Assert.Null(secondError);
        Assert.Equal(0, second!.LinesCreated);
        Assert.Single(await db.Set<InventJournalTrans>().ToListAsync());
    }

    [Fact]
    public async Task Counting_line_generation_is_refused_for_the_wrong_journal_state()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        db.Add(new InventJournalName { DataAreaId = "dat", JournalNameId = "ADJ", JournalType = InventJournalType.Adjustment });
        db.Add(new InventJournalTable
        {
            DataAreaId = "dat", JournalId = "ADJ-1", JournalNameId = "ADJ",
            JournalType = InventJournalType.Adjustment, Posted = NoYes.No
        });
        db.Add(new InventJournalTable
        {
            DataAreaId = "dat", JournalId = "CNT-2", JournalNameId = "CNT",
            JournalType = InventJournalType.Counting, Posted = NoYes.Yes
        });
        await db.SaveChangesAsync();
        var work = new TestUnitOfWork(db);
        var service = new InventJournalService(work, new TestCompany(), Sequence(),
            new InventDimensionResolver(work, new TestInventNumbers()));

        var wrongType = await service.CreateCountingLinesAsync("ADJ-1", new InventCountingLinesInputDto(), default);
        Assert.Contains("counting journal", wrongType.Error);
        var posted = await service.CreateCountingLinesAsync("CNT-2", new InventCountingLinesInputDto(), default);
        Assert.Contains("already been posted", posted.Error);
        Assert.Empty(await db.Set<InventJournalTrans>().ToListAsync());
    }

    [Fact]
    public async Task Inventory_value_report_values_stock_from_transactions_and_shows_summary_drift()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        db.Set<InventTable>().Add(new InventTable { RecId = 1, DataAreaId = "dat", ItemId = "ITEM", Name = "Example item" });
        db.Set<InventTableModule>().Add(new InventTableModule
        {
            RecId = 1, DataAreaId = "dat", ItemId = "ITEM",
            ModuleType = ModuleInventPurchSales.Inventory, UnitId = "EA"
        });
        db.Set<InventItemGroupItem>().Add(new InventItemGroupItem
        {
            RecId = 1, ItemDataAreaId = "dat", ItemId = "ITEM", ItemGroupId = "TRADE"
        });
        db.Set<InventDim>().Add(new InventDim { RecId = 1, DataAreaId = "dat", InventDimId = "DIM1", InventSiteId = "S1", InventLocationId = "WH1" });
        db.Set<InventTrans>().AddRange(
            new InventTrans
            {
                RecId = 1, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM1", Qty = 100m,
                CostAmountPosted = 1000m, StatusReceipt = StatusReceipt.Purchased,
                DatePhysical = new DateTime(2026, 6, 1), DateFinancial = new DateTime(2026, 6, 1)
            },
            new InventTrans
            {
                RecId = 2, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM1", Qty = -40m,
                CostAmountPosted = -400m, StatusIssue = StatusIssue.Sold,
                DatePhysical = new DateTime(2026, 6, 20), DateFinancial = new DateTime(2026, 6, 20)
            });
        // The summary carries 50 more than the transactions, which the report must surface.
        db.Set<InventSum>().Add(new InventSum
        {
            RecId = 1, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM1",
            PhysicalInvent = 60m, PostedValue = 650m
        });
        await db.SaveChangesAsync();

        var service = new InventValueReportService(new TestUnitOfWork(db), new TestCompany());

        var report = await service.RunAsync(new DateOnly(2026, 6, 30), null, null, null, null, false, 1, 50, default);
        Assert.NotNull(report);
        var row = Assert.Single(report!.Rows);
        Assert.Equal("ITEM", row.ItemId);
        Assert.Equal("TRADE", row.ItemGroupId);
        Assert.Equal("S1", row.InventSiteId);
        Assert.Equal("EA", row.InventoryUnit);
        Assert.Equal(60m, row.PhysicalQuantity);
        Assert.Equal(60m, row.FinancialQuantity);
        Assert.Equal(600m, row.FinancialValue);
        Assert.Equal(10m, row.AverageUnitCost);
        Assert.Equal(50m, row.FinancialValueDifference); // summary minus transactions
        Assert.Equal(0m, row.QuantityDifference);
        Assert.Equal(600m, report.TotalFinancialValue);

        // Before the issue, only the receipt counts.
        var earlier = await service.RunAsync(new DateOnly(2026, 6, 10), null, null, null, null, false, 1, 50, default);
        var earlierRow = Assert.Single(earlier!.Rows);
        Assert.Equal(100m, earlierRow.FinancialQuantity);
        Assert.Equal(1000m, earlierRow.FinancialValue);

        // An item group that nothing belongs to returns no rows.
        var filtered = await service.RunAsync(new DateOnly(2026, 6, 30), null, "OTHER", null, null, false, 1, 50, default);
        Assert.Empty(filtered!.Rows);
    }

    [Fact]
    public async Task Inventory_value_report_values_received_and_delivered_stock_on_the_physical_side()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        db.Set<InventTable>().Add(new InventTable { RecId = 1, DataAreaId = "dat", ItemId = "ITEM", Name = "Example item" });
        db.Set<InventTableModule>().Add(new InventTableModule
        {
            RecId = 1, DataAreaId = "dat", ItemId = "ITEM",
            ModuleType = ModuleInventPurchSales.Inventory, UnitId = "EA"
        });
        db.Set<InventDim>().Add(new InventDim { RecId = 1, DataAreaId = "dat", InventDimId = "DIM1", InventSiteId = "S1", InventLocationId = "WH1" });
        db.Set<InventTrans>().AddRange(
            // Invoiced: counts on the financial side only.
            new InventTrans
            {
                RecId = 1, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM1", Qty = 100m,
                CostAmountPosted = 1000m, CostAmountPhysical = 0m, StatusReceipt = StatusReceipt.Purchased,
                DatePhysical = new DateTime(2026, 6, 1), DateFinancial = new DateTime(2026, 6, 1)
            },
            // Received, not invoiced: counts on the physical side only.
            new InventTrans
            {
                RecId = 2, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM1", Qty = 30m,
                CostAmountPhysical = 330m, StatusReceipt = StatusReceipt.Received,
                DatePhysical = new DateTime(2026, 6, 10)
            },
            // Delivered on a packing slip, not invoiced: physical side, issue direction.
            new InventTrans
            {
                RecId = 3, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM1", Qty = -20m,
                CostAmountPhysical = -200m, StatusIssue = StatusIssue.Deducted,
                DatePhysical = new DateTime(2026, 6, 15)
            });
        db.Set<InventSum>().Add(new InventSum
        {
            RecId = 1, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM1",
            PhysicalInvent = 110m, PostedValue = 1000m, PhysicalValue = 130m
        });
        await db.SaveChangesAsync();

        var service = new InventValueReportService(new TestUnitOfWork(db), new TestCompany());
        var report = await service.RunAsync(new DateOnly(2026, 6, 30), null, null, null, null, false, 1, 50, default);
        var row = Assert.Single(report!.Rows);

        // The financial side carries only the invoiced receipt; the physical side carries the
        // receipt and the delivery that have no invoice yet.
        Assert.Equal(100m, row.FinancialQuantity);
        Assert.Equal(1000m, row.FinancialValue);
        Assert.Equal(110m, row.PhysicalQuantity);
        Assert.Equal(130m, row.PhysicalValue);
        Assert.Equal(130m, report.TotalPhysicalValue);

        // Each side reconciles against its own on-hand bucket, so drift is attributable.
        Assert.Equal(1000m, row.SummaryPostedValue);
        Assert.Equal(130m, row.SummaryPhysicalValue);
        Assert.Equal(1130m, row.SummaryValue);
        Assert.Equal(0m, row.FinancialValueDifference);
        Assert.Equal(0m, row.PhysicalValueDifference);

        // Before the delivery, only the receipt's physical value counts.
        var earlier = await service.RunAsync(new DateOnly(2026, 6, 12), null, null, null, null, false, 1, 50, default);
        Assert.Equal(330m, Assert.Single(earlier!.Rows).PhysicalValue);
    }
}
