using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IAX.IXApi.Tests;

/// <summary>
/// A customer return whose sale is in the same period as the close, under weighted average.
/// The close used to refuse this pool with <c>A customer return requires its original
/// financially settled sales issue in the same pool.</c>, because return receipts are costed
/// while the layers are built and the issue they depend on is only settled afterwards. FIFO had a
/// pre-settlement path for exactly this; weighted average did not, so the core cycle reference
/// (<c>Finance/FullExmaple.md</c>, AR-02 then INV-09) could not be closed at all.
/// </summary>
public sealed partial class InventoryCycleTests
{
    /// <summary>
    /// Receipts 100 at 10 and 100 at 12, a sale of 50 posted provisionally at 10, and a return of
    /// 10 of those posted at 10. The period average is 11, so the sale settles at 550 (-50 of
    /// adjustment) and the return re-enters at the sale's settled cost, 110 (+10). With a later
    /// receipt of 50 at 20 the average is 12.8 and both move further. Cancelling the run puts
    /// every figure back.
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Weighted_average_close_settles_same_period_sale_before_costing_its_return_and_reverses_both(bool laterReceipt)
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        db.Set<InventModelGroup>().Local.Single().InventModel = InventModel.WeightedAvg;
        db.Set<InventTransOrigin>().Local.Single(x => x.RecId == 3).ReferenceCategory = InventRefType.SalesTable;
        var issue = db.Set<InventTrans>().Local.Single(x => x.RecId == 3);
        issue.CostAmountPosted = -500m;                      // provisional 10 a piece
        db.Set<InventSum>().Local.Single().PostedValue = laterReceipt ? 2800m : 1800m;
        db.Add(new InventTransOrigin { RecId = 4, DataAreaId = "dat", InventTransId = "RETURN", ItemId = "ITEM", ReferenceId = "RET", ReferenceCategory = InventRefType.SalesTable });
        db.Add(new SalesLine { DataAreaId = "dat", SalesId = "RET", ItemId = "ITEM", InventTransId = "RETURN", InventTransIdReturn = "I1" });
        db.Add(new InventTrans { RecId = 4, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 4,
            Qty = 10m, StatusReceipt = StatusReceipt.Purchased, CostAmountPosted = 100m, Voucher = "CREDIT",
            DateFinancial = new DateTime(2026, 6, 4), ValueOpen = InventTransOpen.Yes });
        db.Add(new IAX.IXApi.Modules.Finance.Entities.InventTransPosting { DataAreaId = "dat", InventTransOrigin = 4, ItemId = "ITEM", Voucher = "CREDIT",
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
        var settledIssue = await db.Set<InventTrans>().SingleAsync(x => x.RecId == 3);
        var receipt = await db.Set<InventTrans>().SingleAsync(x => x.RecId == 4);
        // Average 11 over 200 pieces, or 12.8 over 250 once the later receipt joins the period.
        var expectedIssueAdjustment = laterReceipt ? -140m : -50m;
        var expectedReturnAdjustment = laterReceipt ? 28m : 10m;
        Assert.Equal(-50m, settledIssue.QtySettled);
        Assert.Equal(InventTransOpen.No, settledIssue.ValueOpen);
        Assert.Equal(expectedIssueAdjustment, settledIssue.CostAmountAdjustment);
        // The return carries the sale's settled unit cost, not the period average of a pool it
        // never belonged to as a free receipt.
        Assert.Equal(expectedReturnAdjustment, receipt.CostAmountAdjustment);
        Assert.Equal(-(settledIssue.CostAmountPosted + settledIssue.CostAmountAdjustment) / 50m * 10m,
            receipt.CostAmountPosted + receipt.CostAmountAdjustment);
        Assert.Equal((laterReceipt ? 2800m : 1800m) + expectedIssueAdjustment + expectedReturnAdjustment,
            (await db.Set<InventSum>().SingleAsync()).PostedValue);
        // Every receipt the pre-pass settled is explained by a settlement row of this run.
        var settlements = await db.Set<InventSettlement>().ToListAsync();
        Assert.Equal(-50m, settlements.Where(x => x.ReceiptTransRecId != null).Sum(x => x.QtySettled));
        Assert.Equal(0m, (await db.Set<GeneralJournalAccountEntry>().ToListAsync()).Sum(x => x.AccountingCurrencyAmount));

        Assert.Null((await service.CancelAsync(close.Result!.ClosingRecId)).Error);
        Assert.Equal(0m, settledIssue.QtySettled);
        Assert.Equal(0m, settledIssue.CostAmountAdjustment);
        Assert.Equal(0m, receipt.CostAmountAdjustment);
        Assert.Equal(laterReceipt ? 2800m : 1800m, (await db.Set<InventSum>().SingleAsync()).PostedValue);
        Assert.Equal(0m, (await db.Set<GeneralJournalAccountEntry>().ToListAsync()).Sum(x => x.AccountingCurrencyAmount));
    }

    /// <summary>
    /// The pre-pass is for close only. A recalculation leaves issues open, so it cannot establish
    /// the sale's cost and the return keeps the existing refusal, as it does under FIFO.
    /// </summary>
    [Fact]
    public async Task Weighted_average_recalculation_still_refuses_an_unsettled_same_period_return()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedMarkablePool(db);
        db.Set<InventModelGroup>().Local.Single().InventModel = InventModel.WeightedAvg;
        db.Set<InventTransOrigin>().Local.Single(x => x.RecId == 3).ReferenceCategory = InventRefType.SalesTable;
        db.Add(new InventTransOrigin { RecId = 4, DataAreaId = "dat", InventTransId = "RETURN", ItemId = "ITEM", ReferenceId = "RET", ReferenceCategory = InventRefType.SalesTable });
        db.Add(new SalesLine { DataAreaId = "dat", SalesId = "RET", ItemId = "ITEM", InventTransId = "RETURN", InventTransIdReturn = "I1" });
        db.Add(new InventTrans { RecId = 4, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "DIM", InventTransOrigin = 4,
            Qty = 10m, StatusReceipt = StatusReceipt.Purchased, CostAmountPosted = 110m, Voucher = "CREDIT",
            DateFinancial = new DateTime(2026, 6, 4), ValueOpen = InventTransOpen.Yes });
        await db.SaveChangesAsync();

        var result = await new InventCloseService(new TestUnitOfWork(db), new TestCompany(), new TestUser(), Sequence())
            .RecalculateItemAsync("ITEM", new DateTime(2026, 6, 30));

        Assert.Contains("original financially settled sales issue", result.Error);
        Assert.Equal(0m, (await db.Set<InventTrans>().SingleAsync(x => x.RecId == 3)).QtySettled);
        Assert.Empty(await db.Set<InventSettlement>().ToListAsync());
    }
}
