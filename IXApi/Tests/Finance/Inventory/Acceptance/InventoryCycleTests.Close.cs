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
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Close_excludes_balanced_transfer_order_movements_and_rejects_unbalanced_ones(bool balanced)
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        db.Set<InventDim>().AddRange(
            new InventDim { RecId = 1, DataAreaId = "dat", InventDimId = "SOURCE", InventSiteId = "S1", InventLocationId = "WH1" },
            new InventDim { RecId = 2, DataAreaId = "dat", InventDimId = "TRANSIT", InventSiteId = "S1", InventLocationId = "TRANSIT" });
        db.Set<InventModelGroup>().Add(new InventModelGroup { RecId = 1, DataAreaId = "dat", ModelGroupId = "FIFO", InventModel = InventModel.FIFO });
        db.Set<InventModelGroupItem>().Add(new InventModelGroupItem { RecId = 1, ItemDataAreaId = "dat", ModelGroupDataAreaId = "dat", ModelGroupId = "FIFO", ItemId = "ITEM" });
        db.Set<InventItemGroupItem>().Add(new InventItemGroupItem { RecId = 1, ItemDataAreaId = "dat", ItemId = "ITEM", ItemGroupId = "TRADE" });
        db.Set<InventTransOrigin>().AddRange(
            new InventTransOrigin { RecId = 1, DataAreaId = "dat", InventTransId = "R1", ItemId = "ITEM", ReferenceId = "IJ1", ReferenceCategory = InventRefType.InventoryJournal },
            new InventTransOrigin { RecId = 2, DataAreaId = "dat", InventTransId = "T1", ItemId = "ITEM", ReferenceId = "TO1", ReferenceCategory = InventRefType.TransferOrder },
            new InventTransOrigin { RecId = 3, DataAreaId = "dat", InventTransId = "T2", ItemId = "ITEM", ReferenceId = "TO1", ReferenceCategory = InventRefType.TransferOrder });
        db.Set<InventTrans>().AddRange(
            new InventTrans { RecId = 1, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "SOURCE", InventTransOrigin = 1, Qty = 100, StatusReceipt = StatusReceipt.Purchased, CostAmountPosted = 1000, DateFinancial = new DateTime(2026, 6, 1), ValueOpen = InventTransOpen.Yes },
            new InventTrans { RecId = 2, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "SOURCE", InventTransOrigin = 2, Qty = -10, StatusIssue = StatusIssue.Sold, CostAmountPosted = -100, Voucher = "TO1:SHIP", DateFinancial = new DateTime(2026, 6, 2), ValueOpen = InventTransOpen.Yes });
        if (balanced)
            db.Set<InventTrans>().Add(new InventTrans { RecId = 3, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "TRANSIT", InventTransOrigin = 3, Qty = 10, StatusReceipt = StatusReceipt.Purchased, CostAmountPosted = 100, Voucher = "TO1:SHIP", DateFinancial = new DateTime(2026, 6, 2), ValueOpen = InventTransOpen.Yes });
        await db.SaveChangesAsync();

        var (result, error) = await new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence())
            .CloseItemAsync("ITEM", new DateTime(2026, 6, 30), default);
        if (balanced)
        {
            Assert.Null(error);
            Assert.NotNull(result);
            Assert.Equal(0, result.IssuesSettled);
            Assert.Empty(await db.Set<InventSettlement>().ToListAsync());
        }
        else
            Assert.Contains("transfer order must have balanced", error);
    }

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
            new InventTrans { RecId = 3, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 3, Qty = -150, StatusIssue = StatusIssue.Sold, CostAmountPosted = -1650, Voucher = "ISSUE", DateFinancial = new DateTime(2026, 6, 3), ValueOpen = InventTransOpen.Yes });
        db.Set<InventSum>().Add(new InventSum { RecId = 1, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", PhysicalInvent = 50, PostedValue = 550 });
        db.Add(new InventTransPosting { DataAreaId = "dat", InventTransOrigin = 3, ItemId = "ITEM", Voucher = "ISSUE",
            InventTransPostingType = InventTransPostingType.Financial, IsPosted = NoYes.Yes,
            LedgerDimension = 1, OffsetLedgerDimension = 2, PostingType = LedgerPostingType.InventIssue,
            PostingTypeOffset = LedgerPostingType.InventLoss, ProjId = "" });
        await db.SaveChangesAsync();

        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(),
            new TestUser(), Sequence());
        var (result, error) = await service.CloseItemAsync("ITEM", new DateTime(2026, 6, 30), default);

        Assert.Null(error);
        Assert.Equal(1, result!.IssuesSettled);
        Assert.Equal(2, result.SettlementsCreated);
        Assert.Equal(50m, result.TotalAdjustment);
        Assert.Equal("ITEM", (await db.Set<InventClosing>().SingleAsync()).ScopeItemId);
        Assert.NotNull(await InventoryFinancialBoundary.CheckAsync(db, "dat", "ITEM", new DateTime(2026, 6, 30), default));
        Assert.Null(await InventoryFinancialBoundary.CheckAsync(db, "dat", "OTHER", new DateTime(2026, 6, 30), default));
        Assert.All(await db.Set<InventSettlement>().ToListAsync(), x => Assert.NotNull(x.ReceiptTransRecId));

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
        var settlementCount = await db.Set<InventSettlement>().CountAsync();
        var companyClose = await service.CloseAsync(new DateTime(2026, 6, 30), default);
        Assert.Null(companyClose.Error); // A scoped example must not block subsequent company-wide close.
        Assert.Equal(0, companyClose.Result!.IssuesSettled);
        Assert.Equal(settlementCount, await db.Set<InventSettlement>().CountAsync());
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

    [Fact]
    public async Task Weighted_average_mixed_pool_allows_remaining_stock()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        db.Set<InventModelGroup>().Local.Single().InventModel = InventModel.WeightedAvg;
        db.Set<InventTrans>().Local.Single(x => x.RecId == 3).MarkingRefInventTransOrigin = 2;
        db.Add(new InventTransOrigin { RecId = 4, DataAreaId = "dat", InventTransId = "OTHER-ISSUE", ItemId = "ITEM", ReferenceId = "IJ4", ReferenceCategory = InventRefType.InventoryJournal });
        db.Add(new InventTrans { RecId = 4, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 4,
            Qty = -1m, StatusIssue = StatusIssue.Sold, CostAmountPosted = -11m, Voucher = "OTHER-ISSUE",
            DateFinancial = new DateTime(2026, 6, 2), ValueOpen = InventTransOpen.Yes });
        db.Add(new InventTransPosting { DataAreaId = "dat", InventTransOrigin = 4, ItemId = "ITEM", Voucher = "OTHER-ISSUE",
            InventTransPostingType = InventTransPostingType.Financial, IsPosted = NoYes.Yes, LedgerDimension = 1,
            OffsetLedgerDimension = 2, PostingType = LedgerPostingType.InventIssue, PostingTypeOffset = LedgerPostingType.InventLoss, ProjId = "" });
        await db.SaveChangesAsync();
        var result = await new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence())
            .CloseItemAsync("ITEM", new DateTime(2026, 6, 30));
        Assert.Null(result.Error);
        var settlements = await db.Set<InventSettlement>().ToListAsync();
        Assert.Equal(-600m, settlements.Where(x => x.TransRecId == 3).Sum(x => x.CostAmountSettled));
        Assert.Equal(-10.67m, settlements.Where(x => x.TransRecId == 4).Sum(x => x.CostAmountSettled));
    }

    [Fact]
    public async Task CST05_weighted_average_close_excludes_a_physically_received_uninvoiced_layer()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        var model = db.Set<InventModelGroup>().Local.Single();
        model.InventModel = InventModel.WeightedAvg;
        var receipt = db.Set<InventTrans>().Local.Single(x => x.RecId == 2);
        receipt.StatusReceipt = StatusReceipt.Received;
        receipt.DatePhysical = receipt.DateFinancial;
        receipt.DateFinancial = default;
        receipt.CostAmountPhysical = receipt.CostAmountPosted;
        receipt.CostAmountPosted = 0;
        db.Set<InventSum>().Local.Single().PostedValue = 450m;
        await db.SaveChangesAsync();

        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        var result = await service.CloseItemAsync("ITEM", new DateTime(2026, 6, 30));

        Assert.Null(result.Error);
        var issue = await db.Set<InventTrans>().SingleAsync(x => x.RecId == 3);
        Assert.Equal(-500m, issue.CostAmountSettled);
        Assert.Equal(50m, issue.CostAmountAdjustment);
        Assert.Equal(0m, receipt.QtySettled);
        Assert.Equal(1200m, receipt.CostAmountPhysical);
        Assert.Equal(150m, (await db.Set<InventSum>().SingleAsync()).PhysicalInvent);
    }

    [Fact]
    public async Task Weighted_average_carries_remaining_value_to_next_period_and_reverses()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        db.Set<InventModelGroup>().Local.Single().InventModel = InventModel.WeightedAvg;
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        var first = await service.CloseItemAsync("ITEM", new DateTime(2026, 6, 30));
        Assert.Null(first.Error);

        db.AddRange(
            new InventTransOrigin { RecId = 4, DataAreaId = "dat", InventTransId = "R3", ItemId = "ITEM", ReferenceId = "IJ4", ReferenceCategory = InventRefType.InventoryJournal },
            new InventTransOrigin { RecId = 5, DataAreaId = "dat", InventTransId = "I2", ItemId = "ITEM", ReferenceId = "IJ5", ReferenceCategory = InventRefType.InventoryJournal },
            new InventTrans { RecId = 4, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 4,
                Qty = 50m, StatusReceipt = StatusReceipt.Purchased, CostAmountPosted = 1000m, DateFinancial = new DateTime(2026, 7, 1), ValueOpen = InventTransOpen.Yes },
            new InventTrans { RecId = 5, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 5,
                Qty = -100m, StatusIssue = StatusIssue.Sold, CostAmountPosted = -1100m, Voucher = "I2-VOUCHER", DateFinancial = new DateTime(2026, 7, 2), ValueOpen = InventTransOpen.Yes },
            new InventTransPosting { DataAreaId = "dat", InventTransOrigin = 5, ItemId = "ITEM", Voucher = "I2-VOUCHER",
                InventTransPostingType = InventTransPostingType.Financial, IsPosted = NoYes.Yes, LedgerDimension = 1,
                OffsetLedgerDimension = 2, PostingType = LedgerPostingType.InventIssue, PostingTypeOffset = LedgerPostingType.InventLoss, ProjId = "" });
        await db.SaveChangesAsync();
        var second = await service.CloseItemAsync("ITEM", new DateTime(2026, 7, 31));
        Assert.Null(second.Error);
        Assert.Equal(-1325m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 5)).CostAmountSettled);
        Assert.Null((await service.CancelAsync(second.Result!.ClosingRecId)).Error);
        Assert.Equal(0m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 5)).CostAmountSettled);
        var repeated = await service.CloseItemAsync("ITEM", new DateTime(2026, 7, 31));
        Assert.Null(repeated.Error);
        Assert.Equal(-1325m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 5)).CostAmountSettled);
    }

    [Fact]
    public async Task Weighted_average_purchase_return_settles_at_its_linked_receipt_cost()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        db.Set<InventModelGroup>().Local.Single().InventModel = InventModel.WeightedAvg;
        db.Set<InventTransOrigin>().Local.Single(x => x.RecId == 3).ReferenceCategory = InventRefType.PurchaseOrder;
        db.Set<InventTrans>().Local.Single(x => x.RecId == 3).CostAmountPosted = -600m;
        db.Set<InventSum>().Local.Single().PostedValue = 1600m;
        db.Add(new PurchLine { DataAreaId = "dat", ItemId = "ITEM", PurchId = "RETURN", InventTransId = "I1", InventTransIdReturn = "R2" });
        await db.SaveChangesAsync();

        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        var result = await service.CloseItemAsync("ITEM", new DateTime(2026, 6, 30));
        Assert.Null(result.Error);
        Assert.Equal(0m, result.Result!.TotalAdjustment);
        var settlement = Assert.Single(await db.Set<InventSettlement>().ToListAsync());
        Assert.Equal(2L, settlement.ReceiptTransRecId);
        Assert.Equal(-600m, settlement.CostAmountSettled);
        Assert.Equal(1600m, (await db.Set<InventSum>().SingleAsync()).PostedValue);
        Assert.Empty(await db.Set<GeneralJournalAccountEntry>().ToListAsync());
    }

    [Fact]
    public async Task Multi_receipt_adjustment_allocates_rounding_remainder_and_reverses_exactly()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        var first = db.Set<InventTrans>().Local.Single(x => x.RecId == 1);
        var second = db.Set<InventTrans>().Local.Single(x => x.RecId == 2);
        var issue = db.Set<InventTrans>().Local.Single(x => x.RecId == 3);
        first.Qty = second.Qty = 1m;
        first.CostAmountPosted = second.CostAmountPosted = 1m;
        issue.Qty = -3m;
        issue.CostAmountPosted = -3.01m;
        var summary = db.Set<InventSum>().Local.Single();
        summary.PhysicalInvent = 0m;
        summary.PostedValue = -0.01m;
        db.Add(new InventTransOrigin { RecId = 4, DataAreaId = "dat", InventTransId = "R3", ItemId = "ITEM",
            ReferenceId = "IJ4", ReferenceCategory = InventRefType.InventoryJournal });
        db.Add(new InventTrans { RecId = 4, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 4,
            Qty = 1m, StatusReceipt = StatusReceipt.Purchased, CostAmountPosted = 1m,
            DateFinancial = new DateTime(2026, 6, 2), ValueOpen = InventTransOpen.Yes });
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        var close = await service.CloseItemAsync("ITEM", new DateTime(2026, 6, 30));
        Assert.Null(close.Error);
        var settlements = await db.Set<InventSettlement>().ToListAsync();
        Assert.Equal(new[] { 0.0033m, 0.0033m, 0.0034m }, settlements.Select(x => x.CostAmountAdjustment).OrderBy(x => x));
        Assert.Equal(0.01m, settlements.Sum(x => x.CostAmountAdjustment));
        Assert.Equal(0m, summary.PostedValue);
        Assert.Null((await service.CancelAsync(close.Result!.ClosingRecId)).Error);
        Assert.Equal(0m, issue.CostAmountAdjustment);
        Assert.Equal(-0.01m, summary.PostedValue);
        Assert.Equal(0m, (await db.Set<GeneralJournalAccountEntry>().ToListAsync()).Sum(x => x.AccountingCurrencyAmount));
    }

    [Fact]
    public async Task Cost_adjustment_requires_inventory_summary_and_rolls_back_when_missing()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        db.Remove(db.Set<InventSum>().Local.Single());
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        var result = await service.CloseItemAsync("ITEM", new DateTime(2026, 6, 30));
        Assert.Contains("Inventory summary is missing", result.Error);
        Assert.Equal(0m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 3)).CostAmountAdjustment);
        Assert.Equal(0m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 1)).QtySettled);
        Assert.Empty(await db.Set<InventSettlement>().ToListAsync());
        Assert.Empty(await db.Set<GeneralJournalAccountEntry>().ToListAsync());
        Assert.Empty(await db.Set<InventClosing>().ToListAsync());
    }

    [Fact]
    public async Task Reversal_requires_inventory_summary_and_keeps_original_run_when_missing()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        var recalculation = await service.RecalculateItemAsync("ITEM", new DateTime(2026, 6, 30));
        Assert.Null(recalculation.Error);
        db.Remove(await db.Set<InventSum>().SingleAsync());
        await db.SaveChangesAsync();
        var reversal = await service.CancelAsync(recalculation.Result!.ClosingRecId);
        Assert.Contains("Inventory summary is missing", reversal.Error);
        Assert.Equal(50m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 3)).CostAmountAdjustment);
        Assert.Equal(NoYes.No, (await db.Set<InventClosing>().SingleAsync()).Cancellation);
        Assert.All(await db.Set<InventSettlement>().ToListAsync(), x => Assert.Equal(NoYes.No, x.Cancelled));
        Assert.Equal(2, await db.Set<GeneralJournalAccountEntry>().CountAsync());
    }

    [Fact]
    public async Task Purchase_return_cost_adjustment_uses_the_original_posting_accounts()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        db.Set<InventTransOrigin>().Local.Single(x => x.RecId == 3).ReferenceCategory = InventRefType.PurchaseOrder;
        db.Add(new PurchLine { DataAreaId = "dat", ItemId = "ITEM", PurchId = "RETURN", InventTransId = "I1", InventTransIdReturn = "R2" });
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        var result = await service.CloseItemAsync("ITEM", new DateTime(2026, 6, 30));
        Assert.Null(result.Error);

        // A vendor return settles against its original receipt, and the close adjusts the accounts
        // the original transaction used. This path was refused outright before.
        Assert.NotEmpty(await db.Set<InventClosing>().ToListAsync());
        var entries = await db.Set<GeneralJournalAccountEntry>().ToListAsync();
        Assert.NotEmpty(entries);
        // The pair recorded when the return was posted, not whatever profile is current now.
        Assert.Contains(entries, x => x.PostingType == LedgerPostingType.InventIssue && x.LedgerDimension == 1);
        Assert.Contains(entries, x => x.PostingType == LedgerPostingType.InventLoss && x.LedgerDimension == 2);
        // The pair balances, so the correction leaves the voucher balanced.
        Assert.Equal(0m, entries.Sum(x => x.AccountingCurrencyAmount));
        // The issue carried -550 provisionally and settles against the 10.00 FIFO layer at -500.
        Assert.Equal(-50m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 3)).CostAmountAdjustment);
    }

    [Fact]
    public async Task Inventory_close_refuses_ambiguous_sales_return_invoice_cost()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        var firstIssue = db.Set<InventTrans>().Local.Single(x => x.RecId == 3);
        firstIssue.InvoiceId = "FIRST";
        firstIssue.ValueOpen = InventTransOpen.No;
        firstIssue.QtySettled = firstIssue.Qty;
        db.Add(new InventTrans { RecId = 5, DataAreaId = "dat", InventTransOrigin = 3, ItemId = "ITEM", InventDimId = "DIM",
            Qty = -10m, StatusIssue = StatusIssue.Sold, CostAmountPosted = -150m, InvoiceId = "SECOND",
            Voucher = "SECOND", DateFinancial = new DateTime(2026, 6, 3), ValueOpen = InventTransOpen.No, QtySettled = -10m });
        db.Add(new InventTransOrigin { RecId = 4, DataAreaId = "dat", InventTransId = "RETURN", ItemId = "ITEM",
            ReferenceId = "RET", ReferenceCategory = InventRefType.SalesTable });
        db.Add(new SalesLine { DataAreaId = "dat", SalesId = "RET", ItemId = "ITEM", InventTransId = "RETURN", InventTransIdReturn = "I1" });
        db.Add(new InventTrans { RecId = 4, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 4,
            Qty = 10m, StatusReceipt = StatusReceipt.Purchased, CostAmountPosted = 100m, Voucher = "CREDIT",
            DateFinancial = new DateTime(2026, 6, 4), ValueOpen = InventTransOpen.Yes });
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        var result = await service.CloseItemAsync("ITEM", new DateTime(2026, 6, 30));
        Assert.Contains("different inventory costs", result.Error);
        Assert.Equal(0m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 4)).CostAmountAdjustment);
        Assert.Empty(await db.Set<InventSettlement>().ToListAsync());
        Assert.Empty(await db.Set<GeneralJournalAccountEntry>().ToListAsync());
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task Sales_return_close_requires_only_the_selected_invoice_to_be_settled(bool selectedSettled, bool recalculate)
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        var selected = db.Set<InventTrans>().Local.Single(x => x.RecId == 3);
        selected.InvoiceId = "FIRST";
        selected.DateFinancial = new DateTime(2026, 6, 3);
        selected.ValueOpen = selectedSettled ? InventTransOpen.No : InventTransOpen.Yes;
        selected.QtySettled = selectedSettled ? selected.Qty : 0m;
        var otherOpen = selectedSettled || !recalculate;
        db.Set<InventTrans>().Local.Single(x => x.RecId == 1).QtySettled = selectedSettled ? 50m : otherOpen ? 0m : 10m;
        db.Add(new InventTrans { RecId = 5, DataAreaId = "dat", InventTransOrigin = 3, ItemId = "ITEM", InventDimId = "DIM",
            Qty = -10m, StatusIssue = StatusIssue.Sold, CostAmountPosted = -150m, InvoiceId = "SECOND", Voucher = "SECOND",
            DateFinancial = new DateTime(2026, 6, 5), ValueOpen = otherOpen ? InventTransOpen.Yes : InventTransOpen.No,
            QtySettled = otherOpen ? 0m : -10m });
        db.Add(new InventTransPosting { DataAreaId = "dat", InventTransOrigin = 3, ItemId = "ITEM", Voucher = "SECOND",
            InventTransPostingType = InventTransPostingType.Financial, IsPosted = NoYes.Yes, LedgerDimension = 1,
            OffsetLedgerDimension = 2, PostingType = LedgerPostingType.InventIssue, PostingTypeOffset = LedgerPostingType.InventLoss, ProjId = "" });
        db.Add(new CustInvoiceTrans { RecId = 10, DataAreaId = "dat", InventTransId = "I1", InvoiceId = "FIRST" });
        db.Add(new InventTransOrigin { RecId = 4, DataAreaId = "dat", InventTransId = "RETURN", ItemId = "ITEM", ReferenceId = "RET", ReferenceCategory = InventRefType.SalesTable });
        db.Add(new SalesLine { DataAreaId = "dat", SalesId = "RET", ItemId = "ITEM", InventTransId = "RETURN", InventTransIdReturn = "I1", OriginalInvoiceTransRecId = 10 });
        db.Add(new InventTrans { RecId = 4, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 4,
            Qty = 10m, StatusReceipt = StatusReceipt.Purchased, CostAmountPosted = 100m, Voucher = "CREDIT",
            DateFinancial = new DateTime(2026, 6, 4), ValueOpen = InventTransOpen.Yes });
        db.Add(new InventTransPosting { DataAreaId = "dat", InventTransOrigin = 4, ItemId = "ITEM", Voucher = "CREDIT",
            InventTransPostingType = InventTransPostingType.Financial, IsPosted = NoYes.Yes, LedgerDimension = 1,
            OffsetLedgerDimension = 2, PostingType = LedgerPostingType.SalesIssue, PostingTypeOffset = LedgerPostingType.SalesConsump, ProjId = "" });
        await db.SaveChangesAsync();

        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        var close = recalculate
            ? await service.RecalculateItemAsync("ITEM", new DateTime(2026, 6, 30))
            : await service.CloseItemAsync("ITEM", new DateTime(2026, 6, 30));
        if (recalculate)
        {
            Assert.Contains("original financially settled sales issue", close.Error);
            Assert.Empty(await db.Set<InventSettlement>().ToListAsync());
            return;
        }
        Assert.Null(close.Error);
        Assert.Equal(selectedSettled ? 10m : 0m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 4)).CostAmountAdjustment);
        Assert.Null((await service.CancelAsync(close.Result!.ClosingRecId)).Error);
        Assert.Equal(0m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 4)).CostAmountAdjustment);
        Assert.Equal(selectedSettled ? InventTransOpen.No : InventTransOpen.Yes, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 3)).ValueOpen);
        Assert.Equal(InventTransOpen.Yes, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 5)).ValueOpen);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Fifo_close_settles_same_period_sale_before_costing_its_return_and_reverses_both(bool laterReceipt)
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        db.Set<InventTransOrigin>().Local.Single(x => x.RecId == 3).ReferenceCategory = InventRefType.SalesTable;
        db.Set<InventSum>().Local.Single().PostedValue = 1760m;
        db.Add(new InventTransOrigin { RecId = 4, DataAreaId = "dat", InventTransId = "RETURN", ItemId = "ITEM", ReferenceId = "RET", ReferenceCategory = InventRefType.SalesTable });
        db.Add(new SalesLine { DataAreaId = "dat", SalesId = "RET", ItemId = "ITEM", InventTransId = "RETURN", InventTransIdReturn = "I1" });
        db.Add(new InventTrans { RecId = 4, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 4,
            Qty = 10m, StatusReceipt = StatusReceipt.Purchased, CostAmountPosted = 110m, Voucher = "CREDIT",
            DateFinancial = new DateTime(2026, 6, 4), ValueOpen = InventTransOpen.Yes });
        db.Add(new InventTransPosting { DataAreaId = "dat", InventTransOrigin = 4, ItemId = "ITEM", Voucher = "CREDIT",
            InventTransPostingType = InventTransPostingType.Financial, IsPosted = NoYes.Yes, LedgerDimension = 1,
            OffsetLedgerDimension = 2, PostingType = LedgerPostingType.SalesIssue, PostingTypeOffset = LedgerPostingType.SalesConsump, ProjId = "" });
        if (laterReceipt)
        {
            db.Add(new InventTransOrigin { RecId = 5, DataAreaId = "dat", InventTransId = "LATER", ItemId = "ITEM", ReferenceId = "IJ5", ReferenceCategory = InventRefType.InventoryJournal });
            db.Add(new InventTrans { RecId = 5, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 5,
                Qty = 50m, StatusReceipt = StatusReceipt.Purchased, CostAmountPosted = 1000m,
                DateFinancial = new DateTime(2026, 6, 5), ValueOpen = InventTransOpen.Yes });
        }
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());

        var close = await service.CloseItemAsync("ITEM", new DateTime(2026, 6, 30));

        Assert.Null(close.Error);
        var issue = await db.Set<InventTrans>().SingleAsync(x => x.RecId == 3);
        var receipt = await db.Set<InventTrans>().SingleAsync(x => x.RecId == 4);
        Assert.Equal(-50m, issue.QtySettled);
        Assert.Equal(50m, issue.CostAmountAdjustment);
        Assert.Equal(-10m, receipt.CostAmountAdjustment);
        Assert.Equal(1800m, (await db.Set<InventSum>().SingleAsync()).PostedValue);
        Assert.Null((await service.CancelAsync(close.Result!.ClosingRecId)).Error);
        Assert.Equal(0m, issue.QtySettled);
        Assert.Equal(0m, issue.CostAmountAdjustment);
        Assert.Equal(0m, receipt.CostAmountAdjustment);
        Assert.Equal(1760m, (await db.Set<InventSum>().SingleAsync()).PostedValue);
        Assert.Equal(0m, (await db.Set<GeneralJournalAccountEntry>().ToListAsync()).Sum(x => x.AccountingCurrencyAmount));
    }

    [Fact]
    public async Task Same_period_sales_return_rolls_back_when_original_issue_cannot_be_fully_settled()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        var issue = db.Set<InventTrans>().Local.Single(x => x.RecId == 3);
        issue.Qty = -250m;
        issue.CostAmountPosted = -2750m;
        db.Set<InventTransOrigin>().Local.Single(x => x.RecId == 3).ReferenceCategory = InventRefType.SalesTable;
        db.Add(new InventTransOrigin { RecId = 4, DataAreaId = "dat", InventTransId = "RETURN", ItemId = "ITEM", ReferenceId = "RET", ReferenceCategory = InventRefType.SalesTable });
        db.Add(new SalesLine { DataAreaId = "dat", SalesId = "RET", ItemId = "ITEM", InventTransId = "RETURN", InventTransIdReturn = "I1" });
        db.Add(new InventTrans { RecId = 4, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 4,
            Qty = 10m, StatusReceipt = StatusReceipt.Purchased, CostAmountPosted = 110m, Voucher = "CREDIT",
            DateFinancial = new DateTime(2026, 6, 4), ValueOpen = InventTransOpen.Yes });
        db.Add(new InventTransOrigin { RecId = 5, DataAreaId = "dat", InventTransId = "LATER", ItemId = "ITEM", ReferenceId = "IJ5", ReferenceCategory = InventRefType.InventoryJournal });
        db.Add(new InventTrans { RecId = 5, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 5,
            Qty = 50m, StatusReceipt = StatusReceipt.Purchased, CostAmountPosted = 1000m,
            DateFinancial = new DateTime(2026, 6, 5), ValueOpen = InventTransOpen.Yes });
        await db.SaveChangesAsync();

        var result = await new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence())
            .CloseItemAsync("ITEM", new DateTime(2026, 6, 30));

        Assert.Contains("original financially settled sales issue", result.Error);
        Assert.Equal(0m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 3)).QtySettled);
        Assert.Empty(await db.Set<InventSettlement>().ToListAsync());
        Assert.Empty(await db.Set<InventClosing>().ToListAsync());
    }

    [Fact]
    public async Task Sales_return_cost_adjustment_uses_the_credit_note_posting_accounts()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        var originalIssue = db.Set<InventTrans>().Local.Single(x => x.RecId == 3);
        originalIssue.QtySettled = -50m;
        originalIssue.ValueOpen = InventTransOpen.No;
        db.Set<InventTrans>().Local.Single(x => x.RecId == 1).QtySettled = 50m;
        db.Add(new InventTransOrigin { RecId = 4, DataAreaId = "dat", InventTransId = "RETURN", ItemId = "ITEM",
            ReferenceId = "RET", ReferenceCategory = InventRefType.SalesTable });
        db.Add(new SalesLine { DataAreaId = "dat", SalesId = "RET", ItemId = "ITEM", InventTransId = "RETURN", InventTransIdReturn = "I1" });
        db.Add(new InventTrans { RecId = 4, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 4,
            Qty = 10, StatusReceipt = StatusReceipt.Purchased, CostAmountPosted = 100, Voucher = "CREDIT",
            DateFinancial = new DateTime(2026, 6, 4), ValueOpen = InventTransOpen.Yes });
        db.Add(new InventTransPosting { DataAreaId = "dat", InventTransOrigin = 4, ItemId = "ITEM", Voucher = "CREDIT",
            InventTransPostingType = InventTransPostingType.Financial, IsPosted = NoYes.Yes,
            LedgerDimension = 1, OffsetLedgerDimension = 2, PostingType = LedgerPostingType.SalesIssue,
            PostingTypeOffset = LedgerPostingType.SalesConsump, ProjId = "" });
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        var close = await service.CloseItemAsync("ITEM", new DateTime(2026, 6, 30));
        Assert.Null(close.Error);
        Assert.Equal(10m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 4)).CostAmountAdjustment);
        Assert.Null((await service.CancelAsync(close.Result!.ClosingRecId)).Error);
        Assert.Equal(0m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 4)).CostAmountAdjustment);
        var entries = await db.Set<GeneralJournalAccountEntry>().ToListAsync();
        Assert.Equal(4, entries.Count);
        Assert.All(entries, x => Assert.Contains(x.LedgerDimension, new long[] { 1, 2 }));
        Assert.Equal(0m, entries.Sum(x => x.AccountingCurrencyAmount));
    }

    [Fact]
    public async Task Sales_cost_adjustments_use_accounts_from_the_original_invoice_voucher()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        db.Set<InventTransOrigin>().Local.Single(x => x.RecId == 3).ReferenceCategory = InventRefType.SalesTable;
        var posting = db.Set<InventTransPosting>().Local.Single(x => x.InventTransOrigin == 3);
        posting.PostingType = LedgerPostingType.SalesIssue;
        posting.PostingTypeOffset = LedgerPostingType.SalesConsump;
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        var recalculation = await service.RecalculateItemAsync("ITEM", new DateTime(2026, 6, 30));
        Assert.Null(recalculation.Error);
        Assert.Null((await service.CancelAsync(recalculation.Result!.ClosingRecId)).Error);
        var entries = await db.Set<GeneralJournalAccountEntry>().ToListAsync();
        Assert.Equal(4, entries.Count);
        Assert.All(entries, x => Assert.Contains(x.LedgerDimension, new long[] { 1, 2 }));
        Assert.Equal(0m, entries.Sum(x => x.AccountingCurrencyAmount));
    }

    [Fact]
    public async Task Journal_recalculation_refuses_missing_original_financial_posting()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        db.Remove(db.Set<InventTransPosting>().Local.Single());
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        var result = await service.RecalculateItemAsync("ITEM", new DateTime(2026, 6, 30));
        Assert.Contains("original financial inventory posting", result.Error);
        Assert.Equal(0m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 3)).CostAmountAdjustment);
        Assert.Equal(1650m, (await db.Set<InventSum>().SingleAsync()).PostedValue);
        Assert.Empty(await db.Set<GeneralJournalAccountEntry>().ToListAsync());
    }

    [Fact]
    public async Task Journal_recalculation_and_reversal_use_original_posting_accounts()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        var issue = db.Set<InventTrans>().Local.Single(x => x.RecId == 3);
        issue.Voucher = "ISSUE-VOUCHER";
        db.Add(new InventJournalTrans { DataAreaId = "dat", JournalId = "IJ3", InventTransId = "I1", JournalType = InventJournalType.Adjustment });
        db.Set<InventTransPosting>().Local.Single(x => x.InventTransOrigin == 3).Voucher = "ISSUE-VOUCHER";
        db.Set<MainAccount>().AddRange(
            new MainAccount { RecId = 3, DataAreaId = "dat", MainAccountId = "NEWINV" },
            new MainAccount { RecId = 4, DataAreaId = "dat", MainAccountId = "NEWLOSS" });
        db.Set<DimensionAttributeValueCombination>().AddRange(
            new DimensionAttributeValueCombination { RecId = 3, DataAreaId = "dat", MainAccount = 3, DisplayValue = "NEWINV" },
            new DimensionAttributeValueCombination { RecId = 4, DataAreaId = "dat", MainAccount = 4, DisplayValue = "NEWLOSS" });
        db.Set<InventPosting>().Local.Single(x => x.InventAccountType == InventAccountType.InventIssue).LedgerDimension = 3;
        db.Add(new InventPosting { RecId = 3, DataAreaId = "dat", InventAccountType = InventAccountType.InventLoss,
            ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 4 });
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        var recalculation = await service.RecalculateItemAsync("ITEM", new DateTime(2026, 6, 30));
        Assert.Null(recalculation.Error);
        Assert.Null((await service.CancelAsync(recalculation.Result!.ClosingRecId)).Error);
        var entries = await db.Set<GeneralJournalAccountEntry>().ToListAsync();
        Assert.Equal(4, entries.Count);
        Assert.All(entries, x => Assert.Contains(x.LedgerDimension, new long[] { 1, 2 }));
        Assert.Equal(0m, entries.Sum(x => x.AccountingCurrencyAmount));
    }

    [Fact]
    public async Task Earlier_close_automatically_reverses_later_recalculation()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        var recalculation = await service.RecalculateItemAsync("ITEM", new DateTime(2026, 6, 30));
        Assert.Null(recalculation.Error);
        var close = await service.CloseItemAsync("ITEM", new DateTime(2026, 6, 3));
        Assert.Null(close.Error);
        Assert.Equal(NoYes.Yes, (await db.Set<InventClosing>().SingleAsync(x => x.RecId == recalculation.Result!.ClosingRecId)).Cancellation);
        Assert.Single(await db.Set<InventClosing>().Where(x => x.CancelClosing == NoYes.Yes).ToListAsync());
        Assert.Equal(50m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 3)).CostAmountAdjustment);
        Assert.NotNull(await InventoryFinancialBoundary.CheckAsync(db, "dat", "ITEM", new DateTime(2026, 6, 3), default));
        Assert.Equal(3, close.Result!.RunNum);
        Assert.Equal(0m, (await db.Set<GeneralJournalAccountEntry>().ToListAsync()).Sum(x => x.AccountingCurrencyAmount));
    }

    [Fact]
    public async Task Item_close_reverses_its_recalculation_without_reversing_a_later_independent_item()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        await db.SaveChangesAsync();
        db.Add(new InventModelGroupItem { ItemDataAreaId = "dat", ModelGroupDataAreaId = "dat", ModelGroupId = "FIFO", ItemId = "OTHER" });
        db.Add(new InventItemGroupItem { ItemDataAreaId = "dat", ItemId = "OTHER", ItemGroupId = "TRADE" });
        db.AddRange(
            new InventTransOrigin { RecId = 10, DataAreaId = "dat", InventTransId = "OR", ItemId = "OTHER", ReferenceId = "O1", ReferenceCategory = InventRefType.InventoryJournal },
            new InventTransOrigin { RecId = 11, DataAreaId = "dat", InventTransId = "OI", ItemId = "OTHER", ReferenceId = "O2", ReferenceCategory = InventRefType.InventoryJournal },
            new InventTrans { RecId = 10, DataAreaId = "dat", ItemId = "OTHER", InventDimId = "DIM", InventTransOrigin = 10, Qty = 10, StatusReceipt = StatusReceipt.Purchased, CostAmountPosted = 100, DateFinancial = new DateTime(2026, 6, 1), ValueOpen = InventTransOpen.Yes },
            new InventTrans { RecId = 11, DataAreaId = "dat", ItemId = "OTHER", InventDimId = "DIM", InventTransOrigin = 11, Qty = -5, StatusIssue = StatusIssue.Sold, CostAmountPosted = -60, Voucher = "OTHER-ISSUE", DateFinancial = new DateTime(2026, 6, 3), ValueOpen = InventTransOpen.Yes },
            new InventSum { DataAreaId = "dat", ItemId = "OTHER", InventDimId = "DIM", PhysicalInvent = 5, PostedValue = 40 },
            new InventTransPosting { DataAreaId = "dat", InventTransOrigin = 11, ItemId = "OTHER", Voucher = "OTHER-ISSUE", InventTransPostingType = InventTransPostingType.Financial, IsPosted = NoYes.Yes, LedgerDimension = 1, OffsetLedgerDimension = 2, PostingType = LedgerPostingType.InventIssue, PostingTypeOffset = LedgerPostingType.InventLoss, ProjId = "" });
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        var first = await service.RecalculateItemAsync("ITEM", new DateTime(2026, 6, 30));
        var second = await service.RecalculateItemAsync("OTHER", new DateTime(2026, 6, 30));
        Assert.Null(first.Error);
        Assert.Null(second.Error);
        Assert.Equal(10m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 11)).CostAmountAdjustment);

        var close = await service.CloseItemAsync("ITEM", new DateTime(2026, 6, 3));

        Assert.Null(close.Error);
        Assert.Equal(NoYes.Yes, (await db.Set<InventClosing>().SingleAsync(x => x.RecId == first.Result!.ClosingRecId)).Cancellation);
        Assert.Equal(NoYes.No, (await db.Set<InventClosing>().SingleAsync(x => x.RecId == second.Result!.ClosingRecId)).Cancellation);
        Assert.Equal(10m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 11)).CostAmountAdjustment);
        Assert.Equal(50m, (await db.Set<InventSum>().SingleAsync(x => x.ItemId == "OTHER")).PostedValue);
    }

    [Fact]
    public async Task Item_close_does_not_reverse_a_company_wide_recalculation()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        var recalculation = await service.RecalculateAsync(new DateTime(2026, 6, 30));
        Assert.Null(recalculation.Error);
        var close = await service.CloseItemAsync("ITEM", new DateTime(2026, 6, 3));
        Assert.Contains("cannot automatically reverse a company-wide recalculation", close.Error);
        Assert.Equal(NoYes.No, (await db.Set<InventClosing>().SingleAsync(x => x.RecId == recalculation.Result!.ClosingRecId)).Cancellation);
        Assert.Equal(50m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 3)).CostAmountAdjustment);
    }

    [Fact]
    public async Task Failed_close_rolls_back_automatic_recalculation_reversal()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        var recalculation = await service.RecalculateItemAsync("ITEM", new DateTime(2026, 6, 30));
        Assert.Null(recalculation.Error);
        var close = await service.CloseItemAsync("ITEM", new DateTime(2026, 1, 31));
        Assert.NotNull(close.Error);
        Assert.Equal(NoYes.No, (await db.Set<InventClosing>().SingleAsync(x => x.RecId == recalculation.Result!.ClosingRecId)).Cancellation);
        Assert.Empty(await db.Set<InventClosing>().Where(x => x.CancelClosing == NoYes.Yes).ToListAsync());
        Assert.Equal(50m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 3)).CostAmountAdjustment);
        Assert.Equal(1700m, (await db.Set<InventSum>().SingleAsync()).PostedValue);
    }

    [Fact]
    public async Task Legacy_recalculation_settlement_must_be_reversed_before_close()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        var cutoff = new DateTime(2026, 6, 30);
        var recalculation = await service.RecalculateItemAsync("ITEM", cutoff);
        Assert.Null(recalculation.Error);

        // Reproduce the records written by the earlier recalculation implementation.
        var settlement = await db.Set<InventSettlement>().SingleAsync();
        settlement.ReceiptTransRecId = 1;
        settlement.QtySettled = -50m;
        settlement.CostAmountSettled = -500m;
        var issue = await db.Set<InventTrans>().SingleAsync(x => x.RecId == 3);
        var receipt = await db.Set<InventTrans>().SingleAsync(x => x.RecId == 1);
        issue.QtySettled = -50m;
        issue.CostAmountSettled = -500m;
        receipt.QtySettled = 50m;
        await db.SaveChangesAsync();

        var refused = await service.CloseItemAsync("ITEM", cutoff);
        Assert.Contains("earlier inventory recalculation contains settled", refused.Error);
        Assert.Equal(-50m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 3)).QtySettled);
        Assert.Null((await service.CancelAsync(recalculation.Result!.ClosingRecId)).Error);
        Assert.Equal(0m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 3)).QtySettled);
        Assert.Equal(0m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 1)).QtySettled);
        Assert.Null((await service.CloseItemAsync("ITEM", cutoff)).Error);
    }

    [Fact]
    public async Task Cost_run_numbers_remain_unique_after_a_reversal_and_reclose()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        var cutoff = new DateTime(2026, 6, 30);

        var first = await service.CloseItemAsync("ITEM", cutoff);
        Assert.Null(first.Error);
        var reversal = await service.CancelAsync(first.Result!.ClosingRecId);
        Assert.Null(reversal.Error);
        Assert.Null(await InventoryFinancialBoundary.CheckAsync(db, "dat", "ITEM", cutoff, default));
        var second = await service.CloseItemAsync("ITEM", cutoff);
        Assert.Null(second.Error);

        Assert.Equal(1, first.Result.RunNum);
        Assert.Equal(2, reversal.Result!.RunNum);
        Assert.Equal(3, second.Result!.RunNum);
        Assert.Equal(new[] { 1, 2, 3 }, (await db.Set<InventClosing>().ToListAsync()).Select(x => x.RunNum).OrderBy(x => x));
        Assert.NotNull(await InventoryFinancialBoundary.CheckAsync(db, "dat", "ITEM", cutoff, default));
    }

    [Fact]
    public async Task Cancelling_recalculation_reverses_its_value_without_settling_inventory()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        var result = await service.RecalculateItemAsync("ITEM", new DateTime(2026, 6, 30));
        Assert.Null(result.Error);
        Assert.Null((await service.CancelAsync(result.Result!.ClosingRecId)).Error);
        var issue = await db.Set<InventTrans>().SingleAsync(x => x.RecId == 3);
        Assert.Equal(0m, issue.QtySettled);
        Assert.Equal(0m, issue.CostAmountAdjustment);
        Assert.Equal(0m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 1)).QtySettled);
        Assert.Equal(1650m, (await db.Set<InventSum>().SingleAsync()).PostedValue);
    }

    [Fact]
    public async Task Recalculation_can_repeat_and_be_reversed_before_final_close()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        var cutoff = new DateTime(2026, 6, 30);

        var first = await service.RecalculateItemAsync("ITEM", cutoff);
        Assert.Null(first.Error);
        var issue = await db.Set<InventTrans>().SingleAsync(x => x.RecId == 3);
        var receipt = await db.Set<InventTrans>().SingleAsync(x => x.RecId == 1);
        Assert.Equal(50m, issue.CostAmountAdjustment);
        Assert.Equal(0m, issue.QtySettled);
        Assert.Equal(0m, receipt.QtySettled);
        Assert.Equal(InventTransOpen.Yes, issue.ValueOpen);
        Assert.Equal(1700m, (await db.Set<InventSum>().SingleAsync()).PostedValue);

        var second = await service.RecalculateItemAsync("ITEM", cutoff);
        Assert.Null(second.Error);
        Assert.Equal(0m, second.Result!.TotalAdjustment);
        Assert.Null((await service.CancelAsync(second.Result.ClosingRecId)).Error);
        Assert.Equal(50m, issue.CostAmountAdjustment);

        var close = await service.CloseItemAsync("ITEM", cutoff);
        Assert.Null(close.Error);
        Assert.Equal(0m, close.Result!.TotalAdjustment);
        Assert.Equal(-50m, issue.QtySettled);
        Assert.Equal(50m, receipt.QtySettled);
        Assert.Equal(InventTransOpen.No, issue.ValueOpen);
        Assert.Equal(1700m, (await db.Set<InventSum>().SingleAsync()).PostedValue);
        Assert.Equal(0m, (await db.Set<GeneralJournalAccountEntry>().ToListAsync()).Sum(x => x.AccountingCurrencyAmount));
    }

    [Fact]
    public async Task Recalculation_leaves_a_partially_coverable_issue_unchanged()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        var issue = db.Set<InventTrans>().Local.Single(x => x.RecId == 3);
        issue.Qty = -150m;
        issue.CostAmountPosted = -1650m;
        db.Set<InventTrans>().Local.Single(x => x.RecId == 2).DateFinancial = new DateTime(2026, 6, 20);
        await db.SaveChangesAsync();
        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());

        var run = await service.RecalculateItemAsync("ITEM", new DateTime(2026, 6, 10));
        Assert.Null(run.Error);
        Assert.Equal(150m, Assert.Single(run.Result!.UnsettledIssues).RemainingQuantity);
        Assert.Equal(0m, issue.QtySettled);
        Assert.Equal(0m, issue.CostAmountAdjustment);
        Assert.Empty(await db.Set<InventSettlement>().ToListAsync());
    }

    [Fact]
    public async Task Close_preserves_unsettled_quantity_for_a_later_receipt_and_reverses_both_runs()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        var issue = db.Set<InventTrans>().Local.Single(x => x.RecId == 3);
        issue.Qty = -150m;
        issue.CostAmountPosted = -1650m;
        db.Set<InventTrans>().Local.Single(x => x.RecId == 2).DateFinancial = new DateTime(2026, 6, 20);
        await db.SaveChangesAsync();

        var service = new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence());
        var first = await service.CloseItemAsync("ITEM", new DateTime(2026, 6, 10));
        Assert.Null(first.Error);
        Assert.Equal(50m, Assert.Single(first.Result!.UnsettledIssues).RemainingQuantity);
        Assert.Equal(-100m, issue.QtySettled);
        Assert.Equal(-1000m, issue.CostAmountSettled);
        Assert.Equal(100m, issue.CostAmountAdjustment);
        Assert.Equal(InventTransOpen.Yes, issue.ValueOpen);
        var retry = await service.CloseItemAsync("ITEM", new DateTime(2026, 6, 10));
        Assert.Equal(50m, Assert.Single(retry.Result!.UnsettledIssues).RemainingQuantity);

        var second = await service.CloseItemAsync("ITEM", new DateTime(2026, 6, 30));
        Assert.Null(second.Error);
        Assert.Empty(second.Result!.UnsettledIssues);
        Assert.Equal(-150m, issue.QtySettled);
        Assert.Equal(-1600m, issue.CostAmountSettled);
        Assert.Equal(50m, issue.CostAmountAdjustment);
        Assert.Equal(InventTransOpen.No, issue.ValueOpen);
        Assert.Null((await service.CancelAsync(second.Result.ClosingRecId)).Error);
        Assert.Equal(-100m, issue.QtySettled);
        Assert.Null((await service.CancelAsync(first.Result.ClosingRecId)).Error);
        Assert.Equal(0m, issue.QtySettled);
        Assert.Equal(0m, issue.CostAmountAdjustment);
    }
}
