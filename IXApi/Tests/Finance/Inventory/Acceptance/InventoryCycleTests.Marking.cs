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
    public async Task Marking_links_an_issue_to_a_receipt_on_both_sides_and_rejects_remarking()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedMarkablePool(db);
        await db.SaveChangesAsync();
        var service = new InventoryMarkingService(new TestUnitOfWork(db), new TestCompany());

        var (marked, error) = await service.MarkAsync("I1", "R2", default);
        Assert.Null(error);
        Assert.True(marked!.IsMarked);

        // D365 holds the link on both transactions, each pointing at the other's origin.
        var issue = await db.Set<InventTrans>().SingleAsync(x => x.RecId == 3);
        var receipt = await db.Set<InventTrans>().SingleAsync(x => x.RecId == 2);
        Assert.Equal(2L, issue.MarkingRefInventTransOrigin);   // origin RecId of R2
        Assert.Equal(3L, receipt.MarkingRefInventTransOrigin); // origin RecId of I1

        // The same pair again is a no-operation, a different pair is refused.
        Assert.Null((await service.MarkAsync("I1", "R2", default)).Error);
        Assert.Contains("already marked", (await service.MarkAsync("I1", "R1", default)).Error);
        // Direction and item are enforced.
        Assert.Contains("not an issue", (await service.MarkAsync("R1", "R2", default)).Error);
        Assert.Contains("cannot be marked to itself", (await service.MarkAsync("I1", "I1", default)).Error);

        var listed = Assert.Single(await service.ListAsync(null, default) ?? []);
        Assert.Equal(("I1", "R2"), (listed.IssueInventTransId, listed.ReceiptInventTransId));

        // Removing the marking clears both sides.
        Assert.Null((await service.UnmarkAsync("I1", default)).Error);
        Assert.Equal(0L, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 3)).MarkingRefInventTransOrigin);
        Assert.Equal(0L, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 2)).MarkingRefInventTransOrigin);
        Assert.Contains("not marked", (await service.UnmarkAsync("I1", default)).Error);
    }

    [Fact]
    public async Task Split_inventory_movement_can_be_unmarked_by_its_record_id()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedMarkablePool(db);
        var first = db.Set<InventTrans>().Local.Single(x => x.RecId == 3);
        first.Qty = -25m;
        first.CostAmountPosted = -275m;
        db.Add(new InventTrans { RecId = 4, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 3,
            Qty = -25m, StatusIssue = StatusIssue.Sold, CostAmountPosted = -275m,
            DateFinancial = new DateTime(2026, 6, 3), ValueOpen = InventTransOpen.Yes });
        await db.SaveChangesAsync();
        var service = new InventoryMarkingService(new TestUnitOfWork(db), new TestCompany());
        Assert.Null((await service.MarkAsync("I1", "R2", 3, null, default)).Error);
        Assert.Contains("split across", (await service.UnmarkAsync("I1", default)).Error);
        Assert.Contains("does not belong", (await service.UnmarkAsync("I1", 999, default)).Error);
        Assert.Null((await service.UnmarkAsync("I1", 3, default)).Error);
        var movements = await db.Set<InventTrans>().Where(x => x.RecId == 2 || x.RecId == 3 || x.RecId == 4).ToListAsync();
        Assert.All(movements, x => Assert.Equal(0L, x.MarkingRefInventTransOrigin));
    }

    [Fact]
    public async Task Split_marking_rejects_indistinguishable_pairs_and_refuses_ambiguous_removal()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedMarkablePool(db);
        db.Add(new InventTrans { RecId = 4, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 3,
            Qty = -10m, StatusIssue = StatusIssue.Sold, CostAmountPosted = -110m,
            DateFinancial = new DateTime(2026, 6, 3), ValueOpen = InventTransOpen.Yes });
        db.Add(new InventTrans { RecId = 5, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 2,
            Qty = 10m, StatusReceipt = StatusReceipt.Purchased, CostAmountPosted = 120m,
            DateFinancial = new DateTime(2026, 6, 2), ValueOpen = InventTransOpen.Yes });
        await db.SaveChangesAsync();
        var service = new InventoryMarkingService(new TestUnitOfWork(db), new TestCompany());
        Assert.Null((await service.MarkAsync("I1", "R2", 3, 2, default)).Error);
        Assert.Contains("cannot be identified safely", (await service.MarkAsync("I1", "R2", 4, 5, default)).Error);
        var issue = await db.Set<InventTrans>().SingleAsync(x => x.RecId == 4);
        var receipt = await db.Set<InventTrans>().SingleAsync(x => x.RecId == 5);
        Assert.Equal(0L, issue.MarkingRefInventTransOrigin);
        Assert.Equal(0L, receipt.MarkingRefInventTransOrigin);
        // Existing ambiguous records must not cause removal of several counterpart markings.
        issue.MarkingRefInventTransOrigin = 2;
        receipt.MarkingRefInventTransOrigin = 3;
        await db.SaveChangesAsync();
        Assert.Contains("cannot be uniquely identified", (await service.UnmarkAsync("I1", 3, default)).Error);
        var marked = await db.Set<InventTrans>().Where(x => x.RecId == 2 || x.RecId == 3 || x.RecId == 4 || x.RecId == 5).ToListAsync();
        Assert.All(marked, x => Assert.NotEqual(0L, x.MarkingRefInventTransOrigin));
    }

    [Fact]
    public async Task Marking_removal_refuses_a_settled_or_missing_counterpart()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedMarkablePool(db);
        await db.SaveChangesAsync();
        var service = new InventoryMarkingService(new TestUnitOfWork(db), new TestCompany());
        Assert.Null((await service.MarkAsync("I1", "R2", default)).Error);
        var receipt = await db.Set<InventTrans>().SingleAsync(x => x.RecId == 2);
        receipt.ValueOpen = InventTransOpen.No;
        await db.SaveChangesAsync();
        Assert.Contains("financially settled", (await service.UnmarkAsync("I1", default)).Error);
        receipt = await db.Set<InventTrans>().SingleAsync(x => x.RecId == 2);
        receipt.ValueOpen = InventTransOpen.Yes;
        receipt.MarkingRefInventTransOrigin = 0;
        await db.SaveChangesAsync();
        Assert.Contains("cannot be uniquely identified", (await service.UnmarkAsync("I1", default)).Error);
        Assert.Equal(2L, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 3)).MarkingRefInventTransOrigin);
    }

    [Fact]
    public async Task Marked_issue_settles_against_its_marked_receipt_instead_of_the_model_order()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        await db.SaveChangesAsync();
        var work = new TestUnitOfWork(db);
        // Marked to the dearer second receipt, so FIFO order is deliberately overridden.
        Assert.Null((await new InventoryMarkingService(work, new TestCompany()).MarkAsync("I1", "R2", default)).Error);

        var (result, error) = await new InventCloseService(work, new TestCompany(), new TestUser(), Sequence())
            .CloseItemAsync("ITEM", new DateTime(2026, 6, 30), default);

        Assert.Null(error);
        Assert.Equal(1, result!.IssuesSettled);
        var settlement = Assert.Single(await db.Set<InventSettlement>().ToListAsync());
        Assert.Equal(2L, settlement.ReceiptTransRecId);          // the marked receipt, not the oldest
        Assert.Equal(-600m, settlement.CostAmountSettled);       // 50 units at the marked receipt's 12
        var issue = await db.Set<InventTrans>().SingleAsync(x => x.RecId == 3);
        Assert.Equal(-600m, issue.CostAmountSettled);
        Assert.Equal(-50m, issue.CostAmountAdjustment);          // provisional was 11 per unit
        // The oldest receipt is untouched, which is the whole point of marking.
        Assert.Equal(0m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 1)).QtySettled);
    }

    [Theory]
    [InlineData(InventModel.FIFO, 100, -50, -50)]
    [InlineData(InventModel.LIFO, 150, -50, -100)]
    public async Task Close_reserves_only_marked_quantity_for_the_later_marked_issue(
        InventModel model, int earlyQuantity, int firstQuantity, int secondQuantity)
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        db.Set<InventModelGroup>().Local.Single().InventModel = model;
        await db.SaveChangesAsync();
        db.Add(new InventTransOrigin { RecId = 4, DataAreaId = "dat", InventTransId = "EARLY", ItemId = "ITEM", ReferenceId = "IJ4", ReferenceCategory = InventRefType.InventoryJournal });
        db.Add(new InventTrans { RecId = 4, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 4,
            Qty = -earlyQuantity, StatusIssue = StatusIssue.Sold, CostAmountPosted = -10m * earlyQuantity, Voucher = "EARLY-VOUCHER",
            DateFinancial = new DateTime(2026, 6, 2), ValueOpen = InventTransOpen.Yes });
        db.Add(new InventTransPosting { DataAreaId = "dat", InventTransOrigin = 4, ItemId = "ITEM", Voucher = "EARLY-VOUCHER",
            InventTransPostingType = InventTransPostingType.Financial, IsPosted = NoYes.Yes, LedgerDimension = 1,
            OffsetLedgerDimension = 2, PostingType = LedgerPostingType.InventIssue, PostingTypeOffset = LedgerPostingType.InventLoss, ProjId = "" });
        await db.SaveChangesAsync();
        var work = new TestUnitOfWork(db);
        Assert.Null((await new InventoryMarkingService(work, new TestCompany()).MarkAsync("I1", "R1", default)).Error);
        var result = await new InventCloseService(work, new TestCompany(), new TestUser(), Sequence())
            .CloseItemAsync("ITEM", new DateTime(2026, 6, 30));
        Assert.Null(result.Error);
        var settlements = await db.Set<InventSettlement>().ToListAsync();
        var early = settlements.Where(x => x.TransRecId == 4).ToList();
        Assert.Equal(2, early.Count);
        Assert.Equal(firstQuantity, Assert.Single(early, x => x.ReceiptTransRecId == 1).QtySettled);
        Assert.Equal(secondQuantity, Assert.Single(early, x => x.ReceiptTransRecId == 2).QtySettled);
        var marked = Assert.Single(settlements, x => x.TransRecId == 3);
        Assert.Equal(1L, marked.ReceiptTransRecId);
        Assert.Equal(-50m, marked.QtySettled);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    public async Task Weighted_average_close_excludes_marked_quantity_from_unmarked_issue_cost(int issueDay)
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        db.Set<InventModelGroup>().Local.Single().InventModel = InventModel.WeightedAvg;
        db.Set<InventTrans>().Local.Single(x => x.RecId == 3).MarkingRefInventTransOrigin = 2;
        db.Add(new InventTransOrigin { RecId = 4, DataAreaId = "dat", InventTransId = "OTHER-ISSUE", ItemId = "ITEM", ReferenceId = "IJ4", ReferenceCategory = InventRefType.InventoryJournal });
        db.Add(new InventTrans { RecId = 4, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 4,
            Qty = -150m, StatusIssue = StatusIssue.Sold, CostAmountPosted = -1650m, Voucher = "OTHER-ISSUE",
            DateFinancial = new DateTime(2026, 6, issueDay), ValueOpen = InventTransOpen.Yes });
        db.Add(new InventTransPosting { DataAreaId = "dat", InventTransOrigin = 4, ItemId = "ITEM", Voucher = "OTHER-ISSUE",
            InventTransPostingType = InventTransPostingType.Financial, IsPosted = NoYes.Yes,
            LedgerDimension = 1, OffsetLedgerDimension = 2, PostingType = LedgerPostingType.InventIssue,
            PostingTypeOffset = LedgerPostingType.InventLoss, ProjId = "" });
        await db.SaveChangesAsync();
        var result = await new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence())
            .CloseItemAsync("ITEM", new DateTime(2026, 6, 30));
        Assert.Null(result.Error);
        var settlements = await db.Set<InventSettlement>().ToListAsync();
        var marked = Assert.Single(settlements, x => x.TransRecId == 3);
        Assert.Equal(2L, marked.ReceiptTransRecId);
        Assert.Equal(-600m, marked.CostAmountSettled);
        Assert.Equal(-1600m, settlements.Where(x => x.TransRecId == 4).Sum(x => x.CostAmountSettled));
    }

    [Fact]
    public async Task Weighted_average_refuses_marking_an_original_layer_after_average_carry_forward()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        db.Set<InventModelGroup>().Local.Single().InventModel = InventModel.WeightedAvg;
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        Assert.Null((await service.CloseItemAsync("ITEM", new DateTime(2026, 6, 30))).Error);
        db.Add(new InventTransOrigin { RecId = 4, DataAreaId = "dat", InventTransId = "I2", ItemId = "ITEM", ReferenceId = "IJ4", ReferenceCategory = InventRefType.InventoryJournal });
        db.Add(new InventTrans { RecId = 4, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 4,
            Qty = -1m, StatusIssue = StatusIssue.Sold, CostAmountPosted = -11m, Voucher = "I2", MarkingRefInventTransOrigin = 2,
            DateFinancial = new DateTime(2026, 7, 2), ValueOpen = InventTransOpen.Yes });
        await db.SaveChangesAsync();
        var second = await service.CloseItemAsync("ITEM", new DateTime(2026, 7, 31));
        Assert.Contains("linked receipt value is unavailable", second.Error);
        Assert.Equal(0m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 4)).QtySettled);
    }

    [Fact]
    public async Task Fifo_close_refuses_a_partially_available_marked_receipt_without_substituting_cost()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        var issue = db.Set<InventTrans>().Local.Single(x => x.RecId == 3);
        issue.MarkingRefInventTransOrigin = 2;
        db.Set<InventTrans>().Local.Single(x => x.RecId == 2).QtySettled = 75m;
        await db.SaveChangesAsync();
        var result = await new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence())
            .CloseItemAsync("ITEM", new DateTime(2026, 6, 30));
        Assert.Contains("partially marked issue", result.Error);
        Assert.Empty(await db.Set<InventSettlement>().ToListAsync());
    }

    [Fact]
    public async Task Marking_rejects_receipts_from_another_financial_site()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedMarkablePool(db);
        db.Add(new InventDim { RecId = 2, DataAreaId = "dat", InventDimId = "DIM-OTHER", InventSiteId = "S2", InventLocationId = "WH2" });
        db.Set<InventTrans>().Local.Single(x => x.RecId == 2).InventDimId = "DIM-OTHER";
        await db.SaveChangesAsync();
        var result = await new InventoryMarkingService(new TestUnitOfWork(db), new TestCompany()).MarkAsync("I1", "R2", default);
        Assert.Contains("same financial site", result.Error);
        Assert.All(await db.Set<InventTrans>().ToListAsync(), x => Assert.Equal(0, x.MarkingRefInventTransOrigin));
    }

    [Fact]
    public async Task Close_does_not_substitute_an_unmarked_receipt_when_the_marked_receipt_is_unavailable()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        var issue = db.Set<InventTrans>().Local.Single(x => x.RecId == 3);
        issue.MarkingRefInventTransOrigin = 2;
        var markedReceipt = db.Set<InventTrans>().Local.Single(x => x.RecId == 2);
        markedReceipt.QtySettled = markedReceipt.Qty;
        markedReceipt.ValueOpen = InventTransOpen.No;
        await db.SaveChangesAsync();
        var result = await new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence())
            .CloseItemAsync("ITEM", new DateTime(2026, 6, 30));
        Assert.Contains("Marked receipt", result.Error);
        Assert.Empty(await db.Set<InventSettlement>().ToListAsync());
        Assert.Equal(0m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 1)).QtySettled);
    }

    [Fact]
    public async Task Weighted_average_issue_with_insufficient_marked_receipt_is_not_approximated()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        db.Set<InventModelGroup>().Local.Single().InventModel = InventModel.WeightedAvg;
        var issue = db.Set<InventTrans>().Local.Single(x => x.RecId == 3);
        issue.Qty = -150m;
        issue.CostAmountPosted = -1650m;
        issue.MarkingRefInventTransOrigin = 2;
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        var result = await service.CloseItemAsync("ITEM", new DateTime(2026, 6, 30));
        Assert.Contains("partially marked weighted-average issue", result.Error);
        Assert.Equal(0m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 2)).QtySettled);
        Assert.Empty(await db.Set<InventSettlement>().ToListAsync());
        Assert.Empty(await db.Set<GeneralJournalAccountEntry>().ToListAsync());
    }

    [Fact]
    public async Task Weighted_average_marked_issue_uses_marked_receipt_cost()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        db.Set<InventModelGroup>().Local.Single().InventModel = InventModel.WeightedAvg;
        await db.SaveChangesAsync();
        var work = new TestUnitOfWork(db);
        Assert.Null((await new InventoryMarkingService(work, new TestCompany()).MarkAsync("I1", "R2", default)).Error);
        var result = await new InventCloseService(work, new TestCompany(), new TestUser(), Sequence())
            .CloseItemAsync("ITEM", new DateTime(2026, 6, 30));
        Assert.Null(result.Error);
        var settlement = Assert.Single(await db.Set<InventSettlement>().ToListAsync());
        Assert.Equal(2L, settlement.ReceiptTransRecId);
        Assert.Equal(-600m, settlement.CostAmountSettled);
        Assert.Equal(-50m, result.Result!.TotalAdjustment);
    }
}
