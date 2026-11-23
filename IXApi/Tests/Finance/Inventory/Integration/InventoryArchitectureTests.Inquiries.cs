using System.Reflection;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip.Dtos;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Services;
using IAX.IXApi.Modules.Finance.Common;
using System.Text.Json;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory;
using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using IAX.IXApi.Modules.Finance.Inventory.ProductCategories;
using IAX.IXApi.Modules.Finance.Inventory.ProductCategories.Entities;
using IAX.IXApi.Modules.Finance.Inventory.InventoryStatus;
using IAX.IXApi.Modules.Finance.Inventory.InventoryStatus.Entities;
using IAX.IXApi.Modules.Finance.Inventory.ProductLifecycle;
using IAX.IXApi.Modules.Finance.Inventory.ProductLifecycle.Entities;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Shared.Application.Identity;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;
using ReleasedItem = IAX.IXApi.Modules.Finance.Entities.InventTable;
using SharedProduct = IAX.IXApi.Modules.Finance.Inventory.InventTable.EcoResProduct;

namespace IAX.IXApi.Tests;

public sealed partial class InventoryArchitectureTests
{
    [Fact]
    public async Task Inventory_closing_history_is_read_only_and_company_scoped()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new InventClosing { RecId = 1, DataAreaId = "dat", Voucher = "C-1", PeriodCode = "2026-10",
                TransDate = new DateTime(2026, 10, 1), RunNum = 1, InventCostStatus = InventCostStatus.Finished },
            new InventClosing { RecId = 2, DataAreaId = "other", Voucher = "C-2", PeriodCode = "2026-10",
                TransDate = new DateTime(2026, 10, 2), RunNum = 2, InventCostStatus = InventCostStatus.Finished });
        await db.SaveChangesAsync();
        var service = new InventClosingQueryService(new TestUnitOfWork(db), new TestIdentity());

        var listed = await service.ListAsync(default);

        Assert.Single(listed);
        Assert.Equal("C-1", listed[0].Voucher);
        Assert.Null(await service.GetAsync(2, default));
        Assert.Equal("2026-10", (await service.GetAsync(1, default))!.PeriodCode);
    }

    [Fact]
    public async Task Inventory_record_inquiries_are_company_scoped_and_do_not_write()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new InventDim { RecId = 1, DataAreaId = "dat", InventDimId = "D-1", InventSiteId = "JED", InventLocationId = "MAIN" },
            new InventDim { RecId = 2, DataAreaId = "other", InventDimId = "D-2", InventSiteId = "JED", InventLocationId = "MAIN" },
            new InventItemGroupItem { RecId = 3, DataAreaId = "dat", ItemDataAreaId = "dat", ItemGroupDataAreaId = "dat", ItemId = "PAPER", ItemGroupId = "OFFICE" },
            new InventItemGroupItem { RecId = 4, DataAreaId = "other", ItemDataAreaId = "other", ItemGroupDataAreaId = "other", ItemId = "PAPER", ItemGroupId = "OTHER" },
            new InventItemLocation { RecId = 5, DataAreaId = "dat", ItemId = "PAPER", InventDimId = "D-1", PickingLocationMaxQty = 10 },
            new InventItemLocation { RecId = 6, DataAreaId = "other", ItemId = "PAPER", InventDimId = "D-2" },
            new InventSettlement { RecId = 7, DataAreaId = "dat", ItemId = "PAPER", Voucher = "C-1", QtySettled = 2 },
            new InventSettlement { RecId = 8, DataAreaId = "other", ItemId = "PAPER", Voucher = "C-2", QtySettled = 3 });
        await db.SaveChangesAsync();
        var service = new InventoryRecordQueryService(new TestUnitOfWork(db), new TestIdentity());

        Assert.Equal("D-1", Assert.Single(await service.DimensionsAsync("JED", "MAIN", default)).InventDimId);
        Assert.Equal("OFFICE", Assert.Single(await service.ItemGroupsAsync("PAPER", default)).ItemGroupId);
        Assert.Equal(10, Assert.Single(await service.ItemLocationsAsync("PAPER", default)).PickingLocationMaxQty);
        Assert.Equal("C-1", Assert.Single(await service.SettlementsAsync("PAPER", "C-1", default)).Voucher);
        Assert.Empty(await service.SettlementsAsync("PAPER", "C-2", default));
    }

    [Fact]
    public async Task Physical_as_of_report_uses_dated_movements_and_excludes_draft_demand()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>()
            .UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new InventDim { RecId = 1001, DataAreaId = "dat", InventDimId = "D1", InventSiteId = "JED", InventLocationId = "MAIN" },
            new InventTrans { RecId = 1002, DataAreaId = "dat", ItemId = "PAPER", InventDimId = "D1", Qty = 100,
                StatusReceipt = StatusReceipt.Purchased, DatePhysical = new DateTime(2026, 10, 1, 10, 0, 0) },
            new InventTrans { RecId = 1003, DataAreaId = "dat", ItemId = "PAPER", InventDimId = "D1", Qty = -40,
                StatusIssue = StatusIssue.Deducted, DatePhysical = new DateTime(2026, 10, 2, 16, 0, 0) },
            new InventTrans { RecId = 1004, DataAreaId = "dat", ItemId = "PAPER", InventDimId = "D1", Qty = -10,
                StatusIssue = StatusIssue.Ordered },
            new InventTrans { RecId = 1005, DataAreaId = "other", ItemId = "PAPER", InventDimId = "D1", Qty = 999,
                StatusReceipt = StatusReceipt.Purchased, DatePhysical = new DateTime(2026, 10, 1) });
        await db.SaveChangesAsync();

        var service = new InventPhysicalAsOfService(new TestUnitOfWork(db), new TestIdentity());
        var beforeIssue = (await service.SearchAsync(new DateOnly(2026, 10, 1), "PAPER", "JED", "MAIN", 1, 50))!;
        Assert.Equal(100, Assert.Single(beforeIssue.Rows).PhysicalQuantity);
        var afterIssue = (await service.SearchAsync(new DateOnly(2026, 10, 2), "PAPER", "JED", "MAIN", 1, 50))!;
        Assert.Equal(60, Assert.Single(afterIssue.Rows).PhysicalQuantity);
        Assert.Equal(0, afterIssue.Rows[0].UnverifiedStateCount);
        Assert.True(afterIssue.Rows[0].DimensionFound);
    }

    [Fact]
    public async Task Physical_reconciliation_reports_differences_and_unclassified_states_without_writing_stock()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>()
            .UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new InventDim { RecId = 801, DataAreaId = "dat", InventDimId = "DIM-1", InventSiteId = "JED", InventLocationId = "MAIN" },
            new InventSum { RecId = 802, DataAreaId = "dat", ItemId = "PAPER", InventDimId = "DIM-1", PhysicalInvent = 12 },
            new InventTrans { RecId = 803, DataAreaId = "dat", ItemId = "PAPER", InventDimId = "DIM-1", Qty = 10, StatusReceipt = StatusReceipt.Purchased },
            new InventTrans { RecId = 804, DataAreaId = "dat", ItemId = "PAPER", InventDimId = "DIM-1", Qty = 3, StatusReceipt = StatusReceipt.Registered },
            new InventTrans { RecId = 805, DataAreaId = "other", ItemId = "PAPER", InventDimId = "DIM-1", Qty = 100, StatusReceipt = StatusReceipt.Purchased });
        await db.SaveChangesAsync();

        var result = (await new InventPhysicalReconciliationService(new TestUnitOfWork(db), new TestIdentity())
            .CheckAsync("PAPER", "JED", "MAIN"))!;

        Assert.Equal(1, result.ComparedKeys);
        Assert.Equal(1, result.DifferenceCount);
        Assert.Equal(1, result.UnclassifiedTransactionCount);
        Assert.False(result.NoReportedIssues);
        var row = Assert.Single(result.Rows);
        Assert.Equal(10, row.TransactionPhysical);
        Assert.Equal(12, row.SummaryPhysical);
        Assert.Equal(2, row.Difference);
        Assert.Equal(12, (await db.Set<InventSum>().SingleAsync(x => x.DataAreaId == "dat")).PhysicalInvent);
    }

    [Fact]
    public async Task Fifo_preview_allocates_financial_journal_receipts_by_site_without_writing_adjustments()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>()
            .UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var date = new DateTime(2026, 9, 30);
        db.AddRange(
            new InventModelGroup { RecId = 501, DataAreaId = "dat", ModelGroupId = "FIFO", InventModel = InventModel.FIFO },
            new InventModelGroupItem { RecId = 502, DataAreaId = "dat", ItemDataAreaId = "dat", ItemId = "PAPER", ModelGroupDataAreaId = "dat", ModelGroupId = "FIFO" },
            new InventDim { RecId = 503, DataAreaId = "dat", InventDimId = "DIM-A", InventSiteId = "JED", InventLocationId = "MAIN" },
            new InventDim { RecId = 504, DataAreaId = "dat", InventDimId = "DIM-B", InventSiteId = "JED", InventLocationId = "SHOP" },
            new InventTransOrigin { RecId = 505, DataAreaId = "dat", InventTransId = "R-A", ItemId = "PAPER", ReferenceCategory = InventRefType.InventoryJournal },
            new InventTransOrigin { RecId = 506, DataAreaId = "dat", InventTransId = "R-B", ItemId = "PAPER", ReferenceCategory = InventRefType.InventoryJournal },
            new InventTransOrigin { RecId = 507, DataAreaId = "dat", InventTransId = "ISS", ItemId = "PAPER", ReferenceCategory = InventRefType.InventoryJournal },
            new InventTrans { RecId = 508, DataAreaId = "dat", InventTransOrigin = 505, ItemId = "PAPER", InventDimId = "DIM-A", Qty = 100, CostAmountPosted = 1000, StatusReceipt = StatusReceipt.Purchased, DateFinancial = date },
            new InventTrans { RecId = 509, DataAreaId = "dat", InventTransOrigin = 506, ItemId = "PAPER", InventDimId = "DIM-B", Qty = 100, CostAmountPosted = 1400, StatusReceipt = StatusReceipt.Purchased, DateFinancial = date },
            new InventTrans { RecId = 510, DataAreaId = "dat", InventTransOrigin = 507, ItemId = "PAPER", InventDimId = "DIM-A", Qty = -150, CostAmountPosted = -1800, StatusIssue = StatusIssue.Sold, DateFinancial = date });
        await db.SaveChangesAsync();

        var result = await new FifoCostPreviewService(new TestUnitOfWork(db), new TestIdentity())
            .PreviewAsync(date);

        var issue = Assert.Single(result.Issues);
        Assert.Equal(-1700, issue.FifoCost);
        Assert.Equal(100, issue.RemainingAdjustment);
        Assert.Equal(new decimal[] { 100, 50 }, issue.Allocations.Select(x => x.Quantity));
        Assert.False(result.CanExecute);
        Assert.Equal(0, await db.Set<InventSettlement>().CountAsync());
    }

    [Fact]
    public async Task Transaction_query_preserves_costs_labels_order_and_company_scope()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new InventTrans { RecId = 1, DataAreaId = "dat", ItemId = "ITEM", InventTransOrigin = 10, InventDimId = "DIM", Qty = -2,
                CostAmountPosted = -20, CostAmountPhysical = -4, CostAmountAdjustment = 2, StatusIssue = StatusIssue.Ordered,
                DatePhysical = new DateTime(2026, 10, 23, 14, 0, 0), DateFinancial = new DateTime(2026, 10, 24, 9, 0, 0) },
            new InventTrans { RecId = 2, DataAreaId = "other", ItemId = "HIDDEN" },
            new InventTrans { RecId = 3, DataAreaId = "dat", ItemId = "EMPTY", Qty = 0 },
            new InventTransOrigin { RecId = 10, DataAreaId = "dat", InventTransId = "TX", ReferenceCategory = InventRefType.SalesTable, ReferenceId = "SO1" },
            new InventDim { RecId = 11, DataAreaId = "dat", InventDimId = "DIM", InventSiteId = "S1", InventLocationId = "W1" },
            new SalesLine { RecId = 12, DataAreaId = "dat", InventTransId = "TX", SalesPrice = 42 });
        await db.SaveChangesAsync();
        var identity = new TestIdentity();
        var service = new InventTransService(new TestUnitOfWork(db), identity);
        var rows = (await service.GetListAsync())!;
        Assert.Equal(new long[] { 3, 1 }, rows.Select(x => x.RecId));
        Assert.Equal(0, rows[0].UnitCost);
        Assert.Empty(rows[0].Reference);
        Assert.Null(rows[0].PhysicalDate);
        Assert.Equal(-22, rows[1].CostAmount);
        Assert.Equal(11, rows[1].UnitCost);
        Assert.Equal(42, rows[1].UnitPrice);
        Assert.Equal("On order", rows[1].IssueStatus);
        Assert.Equal("Sales order", rows[1].Reference);
        Assert.Equal("SO1", rows[1].ReferenceNumber);
        Assert.Equal("W1", rows[1].Warehouse);
        var dated = (await service.SearchPageAsync("ITEM", "TX", "S1", "W1",
            new DateTime(2026, 10, 23), new DateTime(2026, 10, 23),
            new DateTime(2026, 10, 24), new DateTime(2026, 10, 24), 1, 1))!;
        Assert.Equal(1, dated.TotalCount);
        Assert.Equal(1, Assert.Single(dated.Rows).RecId);
        Assert.Equal(-4, dated.Rows[0].PhysicalCostAmount);
        Assert.Equal(-20, dated.Rows[0].PostedCostAmount);
        Assert.Equal(2, dated.Rows[0].CostAdjustmentAmount);
        identity.DataAreaId = null;
        Assert.Null(await service.GetListAsync());
        Assert.IsType<BadRequestObjectResult>((await new InventTransController(service).GetList()).Result);
    }

    [Fact]
    public async Task On_hand_query_returns_basic_dimensions_and_standard_summary_buckets_for_selected_company()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new ReleasedItem { RecId = 1, DataAreaId = "dat", ItemId = "ITEM-001", Name = "Office Chair" },
            new ReleasedItem { RecId = 2, DataAreaId = "other", ItemId = "ITEM-001", Name = "Hidden Chair" },
            new InventDim { RecId = 3, DataAreaId = "dat", InventDimId = "DIM-1", InventSiteId = "JED", InventLocationId = "MAIN" },
            new InventDim { RecId = 4, DataAreaId = "other", InventDimId = "DIM-1", InventSiteId = "DXB", InventLocationId = "MAIN" },
            new InventSum
            {
                RecId = 5, DataAreaId = "dat", ItemId = "ITEM-001", InventDimId = "DIM-1",
                PhysicalInvent = 10, ReservPhysical = 2, Picked = 3, AvailPhysical = 8,
                Ordered = 4, OnOrder = 3, AvailOrdered = 9, PostedValue = 700, PhysicalValue = 100
            },
            new InventSum { RecId = 6, DataAreaId = "other", ItemId = "ITEM-001", InventDimId = "DIM-1", PhysicalInvent = 99 });
        await db.SaveChangesAsync();

        var identity = new TestIdentity();
        var rows = (await new InventOnHandService(new TestUnitOfWork(db), identity).ListAsync())!;

        var row = Assert.Single(rows);
        Assert.Equal("ITEM-001", row.ItemNumber);
        Assert.Equal("Office Chair", row.ProductName);
        Assert.Equal("JED", row.Site);
        Assert.Equal("MAIN", row.Warehouse);
        Assert.Equal(10, row.PhysicalInventory);
        Assert.Equal(2, row.PhysicalReserved);
        Assert.Equal(5, row.AvailablePhysical);
        Assert.Equal(4, row.OrderedReceipts);
        Assert.Equal(3, row.OrderedIssues);
        Assert.Equal(6, row.AvailableOrdered);
        Assert.Equal(700, row.PostedValue);
        Assert.Equal(100, row.PhysicalValue);

        db.Add(new InventDim { RecId = 7, DataAreaId = "dat", InventDimId = "DIM-VALUE", InventSiteId = "JED", InventLocationId = "MAIN" });
        db.Add(new InventDim { RecId = 8, DataAreaId = "dat", InventDimId = "DIM-ZERO", InventSiteId = "JED", InventLocationId = "MAIN" });
        db.Add(new InventSum { RecId = 9, DataAreaId = "dat", ItemId = "ITEM-001", InventDimId = "DIM-VALUE", PostedValue = 25 });
        db.Add(new InventSum { RecId = 10, DataAreaId = "dat", ItemId = "ITEM-001", InventDimId = "DIM-ZERO" });
        await db.SaveChangesAsync();
        var nonzeroPage = (await new InventOnHandService(new TestUnitOfWork(db), identity)
            .SearchPageAsync(null, null, null, null, false, 1, 50))!;
        Assert.Equal(2, nonzeroPage.TotalCount);
        Assert.Contains(nonzeroPage.Rows, x => x.InventDimId == "DIM-VALUE" && x.PostedValue == 25);
        Assert.DoesNotContain(nonzeroPage.Rows, x => x.InventDimId == "DIM-ZERO");

        identity.DataAreaId = null;
        Assert.Null(await new InventOnHandService(new TestUnitOfWork(db), identity).ListAsync());
    }

    [Fact]
    public async Task On_hand_page_filters_group_and_zero_rows_before_counting_and_paging()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new ReleasedItem { RecId = 901, DataAreaId = "dat", ItemId = "A", Name = "A" },
            new ReleasedItem { RecId = 902, DataAreaId = "dat", ItemId = "B", Name = "B" },
            new InventDim { RecId = 903, DataAreaId = "dat", InventDimId = "D1", InventSiteId = "JED", InventLocationId = "MAIN" },
            new InventItemGroupItem { RecId = 904, ItemDataAreaId = "dat", ItemGroupDataAreaId = "dat", ItemId = "A", ItemGroupId = "TRADE" },
            new InventItemGroupItem { RecId = 905, ItemDataAreaId = "dat", ItemGroupDataAreaId = "dat", ItemId = "B", ItemGroupId = "TRADE" },
            new InventTableModule { RecId = 906, DataAreaId = "dat", ItemId = "A", ModuleType = ModuleInventPurchSales.Inventory, UnitId = "EA" },
            new InventSum { RecId = 907, DataAreaId = "dat", ItemId = "A", InventDimId = "D1", PhysicalInvent = 4 },
            new InventSum { RecId = 908, DataAreaId = "dat", ItemId = "B", InventDimId = "D1", PhysicalInvent = 0 });
        await db.SaveChangesAsync();

        var service = new InventOnHandService(new TestUnitOfWork(db), new TestIdentity());
        var nonzero = (await service.SearchPageAsync(null, "TRADE", "JED", "MAIN", false, 1, 1))!;
        Assert.Equal(1, nonzero.TotalCount);
        Assert.Equal("A", Assert.Single(nonzero.Rows).ItemNumber);
        Assert.Equal("EA", nonzero.Rows[0].InventoryUnit);
        var all = (await service.SearchPageAsync(null, "TRADE", "JED", "MAIN", true, 2, 1))!;
        Assert.Equal(2, all.TotalCount);
        Assert.Equal("B", Assert.Single(all.Rows).ItemNumber);
        Assert.Equal(0, all.Rows[0].PhysicalInventory);
    }
}
