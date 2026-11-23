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

public sealed partial class InventoryCycleTests
{
    [Fact]
    public async Task CFG02_weighted_average_setup_accepts_include_physical_value()
    {
        await using var fixture = await Fixture.CreateAsync();
        var result = await new InventModelGroupService(new TestUnitOfWork(fixture.Db), new TestCompany())
            .CreateAsync(new InventModelGroupRecordDto
            {
                ModelGroupId = "WA-PHYSICAL", Name = "Weighted average with physical value",
                InventModel = (int)InventModel.WeightedAvg, StockedProduct = true,
                PostOnHandFinancial = true, InclPhysicalValueInCost = true
            }, default);
        Assert.Null(result.Error);
        Assert.True(result.Group!.InclPhysicalValueInCost);
    }

    [Theory]
    [InlineData(false, 10)]
    [InlineData(true, 15)]
    public async Task CST02_physical_receipt_changes_running_average_only_when_enabled(bool includePhysical, decimal expectedCost)
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        SeedLedgerAndPosting(db);
        SeedPurchaseMasterData(db);
        SeedSalesMasterData(db);
        var model = db.Set<InventModelGroup>().Local.Single();
        model.InventModel = InventModel.WeightedAvg;
        model.InclPhysicalValueInCost = includePhysical ? NoYes.Yes : NoYes.No;
        db.AddRange(
            new InventPosting { RecId = 13, DataAreaId = "dat", InventAccountType = InventAccountType.SalesPackingSlip, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 1 },
            new InventPosting { RecId = 14, DataAreaId = "dat", InventAccountType = InventAccountType.SalesPackingSlipOffsetAccount, ItemCode = TableGroupAll.All, CustVendCode = TableGroupAll.All, LedgerDimension = 2 });
        await db.SaveChangesAsync();
        var numbers = new TestInventNumbers();
        var (orders, confirmations, receipts, invoices) = PurchaseServices(db, numbers);
        for (var index = 1; index <= 2; index++)
        {
            Assert.Equal(200, (await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "SAR", new DateTime(2026, 11, index), "S01", "WH-01", null), default)).StatusCode);
            var order = await db.Set<PurchTable>().OrderByDescending(x => x.RecId).FirstAsync();
            Assert.Equal(200, (await orders.AddLineAsync(order.RecId, new PurchLineInputDto("ITEM-001", 10, "PCS", index * 10, 1, null, null, null, null), default)).StatusCode);
            Assert.Equal(200, (await confirmations.ConfirmAsync(order.RecId, default)).StatusCode);
            var line = await db.Set<PurchLine>().SingleAsync(x => x.PurchId == order.PurchId);
            Assert.Equal(200, (await receipts.PostAsync(order.RecId, new ProductReceiptPostDto($"RCV-{index}", new DateTime(2026, 11, index), [new(line.RecId, 10)]), default)).StatusCode);
            if (index == 1)
                Assert.Equal(200, (await invoices.PostAsync(order.RecId, new VendorInvoicePostDto("INV-1", new DateTime(2026, 11, 1), [new(line.RecId, 10)]), default)).StatusCode);
        }
        var sum = await db.Set<InventSum>().SingleAsync();
        Assert.Equal(20m, sum.PhysicalInvent);
        Assert.Equal(100m, sum.PostedValue);
        Assert.Equal(200m, sum.PhysicalValue);
        var sales = new SalesTable { DataAreaId = "dat", SalesId = "SO-PHYSICAL", SalesStatus = SalesStatus.Backorder, CustAccount = "C-001", CurrencyCode = "SAR", InventSiteId = "S01", InventLocationId = "WH-01" };
        db.Add(sales);
        await db.SaveChangesAsync();
        var salesLine = new SalesLine { DataAreaId = "dat", SalesId = sales.SalesId, SalesStatus = SalesStatus.Backorder, ItemId = "ITEM-001", SalesQty = 1, QtyOrdered = 1, SalesUnit = "PCS", SalesPrice = 25, PriceUnit = 1, LineAmount = 25, RemainSalesPhysical = 1, RemainSalesFinancial = 1, StockedProduct = NoYes.Yes };
        var work = new TestUnitOfWork(db);
        await new SalesInventoryDemandService(work, numbers, new InventDimensionResolver(work, numbers)).CreateAsync(sales, salesLine, "S01", "WH-01");
        db.Add(salesLine);
        await db.SaveChangesAsync();
        var result = await new PackingSlipJournalService(work, new TestCompany(), Sequence()).PostAsync(sales.RecId,
            new PostPackingSlipRequest(new DateTime(2026, 11, 3), "All", true, "PS-PHYSICAL", [new PackingSlipPostingLineRequest(salesLine.RecId.ToString(), 1)]), default);
        Assert.True(result.StatusCode == 200, result.Message);
        var issue = await db.Set<InventTrans>().SingleAsync(x => x.StatusIssue == StatusIssue.Deducted);
        Assert.Equal(-expectedCost, issue.CostAmountPhysical);
        Assert.Equal(19m, (await db.Set<InventSum>().SingleAsync()).PhysicalInvent);
    }
}
