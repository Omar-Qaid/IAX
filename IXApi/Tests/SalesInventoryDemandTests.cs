using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Services;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.Departments;
using IAX.IXApi.Modules.Finance.Foundation.HcmShowrooms;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;
using IAX.IXApi.Modules.Finance.Foundation.OrganizationUnits;
using IAX.IXApi.Modules.Finance.Foundation.Structure;
using IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments;
using IAX.IXApi.Modules.Finance.Foundation.WorkerShowroomAssignments;
using IAX.IXApi.Modules.Finance.Persistence;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Xunit;

namespace IAX.IXApi.Tests;

public sealed class SalesInventoryDemandTests
{
    [Fact]
    public async Task Creation_reuses_dimensions_and_keeps_demand_in_sync()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>()
            .UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var financeData = new TestFinanceDataContext(db);
        var numbers = new TestNumbers();
        var unitOfWork = new TestUnitOfWork(db);
        var dimensions = new InventDimensionResolver(unitOfWork, numbers);
        var service = new SalesInventoryDemandService(unitOfWork, numbers, dimensions);
        var order = new SalesTable
        {
            SalesId = "SO-1", CurrencyCode = "USD", DataAreaId = "DAT",
            InventSiteId = "1", InventLocationId = "11"
        };
        var line = new SalesLine
        {
            SalesId = order.SalesId, ItemId = "A0001", SalesQty = 10, QtyOrdered = 10,
            RemainSalesPhysical = 10, RemainSalesFinancial = 10,
            ReceiptDateRequested = new DateTime(2026, 9, 30), DataAreaId = order.DataAreaId
        };

        await service.CreateAsync(order, line, "1", "11");
        await financeData.SaveChangesAsync();

        var dimension = await db.Set<InventDim>().SingleAsync();
        var origin = await db.Set<InventTransOrigin>().SingleAsync();
        var salesLineLink = await db.Set<InventTransOriginSalesLine>().SingleAsync();
        var transaction = await db.Set<InventTrans>().SingleAsync();
        var inventorySummary = await db.Set<InventSum>().SingleAsync();
        Assert.Equal("DIM-000001", dimension.InventDimId);
        Assert.Equal("1", dimension.InventSiteId);
        Assert.Equal("11", dimension.InventLocationId);
        Assert.Equal(dimension.InventDimId, line.InventDimId);
        Assert.Equal(origin.InventTransId, line.InventTransId);
        Assert.Equal(origin.RecId, transaction.InventTransOrigin);
        Assert.Equal(origin.RecId, salesLineLink.InventTransOrigin);
        Assert.Equal(line.InventTransId, salesLineLink.SalesLineInventTransId);
        Assert.Equal(line.DataAreaId, salesLineLink.SalesLineDataAreaId);
        Assert.Equal(-10, transaction.Qty);
        Assert.Equal(10, line.RemainSalesPhysical);
        Assert.Equal(10, line.RemainSalesFinancial);
        Assert.Equal(10, line.RemainInventPhysical);
        Assert.Equal(10, line.RemainInventFinancial);
        Assert.Equal(StatusIssue.Ordered, transaction.StatusIssue);
        Assert.Equal(StatusReceipt.None, transaction.StatusReceipt);
        Assert.Equal(InventTransOpen.Yes, transaction.ValueOpen);
        Assert.Equal(0, transaction.CostAmountPhysical);
        Assert.Equal(0, transaction.CostAmountPosted);
        Assert.Equal(default, transaction.DatePhysical);
        Assert.Equal(default, transaction.DateFinancial);
        Assert.Empty(transaction.VoucherPhysical);
        Assert.Empty(transaction.Voucher);
        Assert.Equal(10, inventorySummary.OnOrder);
        Assert.Equal(0, inventorySummary.PostedQty);
        Assert.Equal(0, inventorySummary.Deducted);
        Assert.Equal(0, inventorySummary.PostedValue);
        Assert.Equal(0, inventorySummary.PhysicalValue);

        // Editing a line created before the link was populated repairs its origin relationship.
        db.Set<InventTransOriginSalesLine>().Remove(salesLineLink);
        await financeData.SaveChangesAsync();

        var sameDimension = await dimensions.ResolveAsync("DAT", "1", "11");
        Assert.Equal(dimension.RecId, sameDimension.RecId);
        Assert.Equal(1, numbers.DimensionCalls);

        line.SalesQty = 12;
        line.QtyOrdered = 12;
        line.RemainInventPhysical = 12;
        await service.UpdateAsync(line, "1", "11");
        await financeData.SaveChangesAsync();
        Assert.Equal(origin.RecId, (await db.Set<InventTransOriginSalesLine>().SingleAsync()).InventTransOrigin);
        Assert.Equal(-12, (await db.Set<InventTrans>().SingleAsync()).Qty);
        Assert.Equal(12, (await db.Set<InventSum>().SingleAsync()).OnOrder);

        // A legacy link with the right origin but wrong sales-line identity is repaired.
        var wrongLink = await db.Set<InventTransOriginSalesLine>().SingleAsync();
        wrongLink.SalesLineInventTransId = "WRONG-TRANS";
        await financeData.SaveChangesAsync();
        await service.UpdateAsync(line, "1", "11");
        await financeData.SaveChangesAsync();
        Assert.Equal(line.InventTransId,
            (await db.Set<InventTransOriginSalesLine>().SingleAsync()).SalesLineInventTransId);

        line.RemainInventPhysical = 11;
        inventorySummary.OnOrder = 0;
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAsync(line, "1", "11"));
        Assert.Equal(-12, transaction.Qty);
        Assert.Equal(0, inventorySummary.OnOrder);
        line.RemainInventPhysical = 12;
        inventorySummary.OnOrder = 12;

        // A stale summary or a processed transaction must not be removed with the line.
        transaction.StatusIssue = StatusIssue.Sold;
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(line));
        Assert.Equal(12, inventorySummary.OnOrder);
        Assert.Single(await db.Set<InventTransOrigin>().ToListAsync());
        transaction.StatusIssue = StatusIssue.Ordered;
        inventorySummary.OnOrder = 11;
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(line));
        Assert.Equal(11, inventorySummary.OnOrder);
        Assert.Single(await db.Set<InventTrans>().ToListAsync());
        inventorySummary.OnOrder = 12;

        await service.DeleteAsync(line);
        await financeData.SaveChangesAsync();
        Assert.Empty(await db.Set<InventTrans>().ToListAsync());
        Assert.Empty(await db.Set<InventTransOrigin>().ToListAsync());
        Assert.Empty(await db.Set<InventTransOriginSalesLine>().ToListAsync());
        Assert.Single(await db.Set<InventDim>().ToListAsync());
        Assert.Equal(0, (await db.Set<InventSum>().SingleAsync()).OnOrder);
    }

    [Fact]
    public async Task Cancelling_one_line_preserves_other_orders_inventory_demand()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>()
            .UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var financeData = new TestFinanceDataContext(db);
        var numbers = new TestNumbers();
        var unitOfWork = new TestUnitOfWork(db);
        var dimensions = new InventDimensionResolver(unitOfWork, numbers);
        var service = new SalesInventoryDemandService(unitOfWork, numbers, dimensions);
        var firstOrder = new SalesTable { SalesId = "SO-1", CurrencyCode = "USD", DataAreaId = "DAT" };
        var secondOrder = new SalesTable { SalesId = "SO-2", CurrencyCode = "USD", DataAreaId = "DAT" };
        var firstLine = new SalesLine
        {
            SalesId = firstOrder.SalesId, ItemId = "A0001", SalesQty = 10, QtyOrdered = 10,
            RemainInventPhysical = 10, RemainSalesPhysical = 10,
            RemainSalesFinancial = 10, DataAreaId = "DAT"
        };
        var secondLine = new SalesLine
        {
            SalesId = secondOrder.SalesId, ItemId = "A0001", SalesQty = 7, QtyOrdered = 7,
            RemainInventPhysical = 7, RemainSalesPhysical = 7,
            RemainSalesFinancial = 7, DataAreaId = "DAT"
        };

        await service.CreateAsync(firstOrder, firstLine, "1", "11");
        await financeData.SaveChangesAsync();
        await service.CreateAsync(secondOrder, secondLine, "1", "11");
        await financeData.SaveChangesAsync();
        Assert.Equal(17, (await db.Set<InventSum>().SingleAsync()).OnOrder);

        await service.CancelRemainingAsync(firstLine);
        await financeData.SaveChangesAsync();

        Assert.Equal(7, (await db.Set<InventSum>().SingleAsync()).OnOrder);
        Assert.Equal(-7, (await db.Set<InventTrans>().SingleAsync(
            item => item.InventTransOrigin == db.Set<InventTransOrigin>()
                .Where(origin => origin.InventTransId == secondLine.InventTransId)
                .Select(origin => origin.RecId).Single())).Qty);

        // A stale aggregate must not be reduced below zero while cancelling the other order.
        (await db.Set<InventSum>().SingleAsync()).OnOrder = 6;
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CancelRemainingAsync(secondLine));
        Assert.Equal(-7, (await db.Set<InventTrans>().SingleAsync(
            item => item.InventTransOrigin == db.Set<InventTransOrigin>()
                .Where(origin => origin.InventTransId == secondLine.InventTransId)
                .Select(origin => origin.RecId).Single())).Qty);
    }

    [Fact]
    public async Task Mismatched_origin_and_duplicate_links_cannot_change_another_sales_lines_demand()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>()
            .UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var work = new TestUnitOfWork(db);
        var numbers = new TestNumbers();
        var service = new SalesInventoryDemandService(work, numbers, new InventDimensionResolver(work, numbers));
        var order = new SalesTable { SalesId = "SO-1", CurrencyCode = "USD", DataAreaId = "DAT" };
        var line = new SalesLine {
            SalesId = order.SalesId, ItemId = "A0001", SalesQty = 10, QtyOrdered = 10,
            RemainInventPhysical = 10, RemainSalesPhysical = 10, RemainSalesFinancial = 10,
            DataAreaId = order.DataAreaId
        };
        await service.CreateAsync(order, line, "1", "11");
        await db.SaveChangesAsync();
        var origin = await db.Set<InventTransOrigin>().SingleAsync();
        var summary = await db.Set<InventSum>().SingleAsync();

        origin.ReferenceId = "SO-OTHER";
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAsync(line, "1", "11"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(line));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CancelRemainingAsync(line));
        Assert.Equal(10m, summary.OnOrder);
        Assert.Equal(-10m, (await db.Set<InventTrans>().SingleAsync()).Qty);

        origin.ReferenceId = order.SalesId;
        var transaction = await db.Set<InventTrans>().SingleAsync();
        transaction.ItemId = "OTHER-ITEM";
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAsync(line, "1", "11"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(line));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CancelRemainingAsync(line));
        Assert.Equal(10m, summary.OnOrder);
        transaction.ItemId = line.ItemId;
        db.Set<InventTransOriginSalesLine>().Add(new InventTransOriginSalesLine {
            InventTransOrigin = origin.RecId, DataAreaId = order.DataAreaId,
            SalesLineDataAreaId = order.DataAreaId, SalesLineInventTransId = "OTHER-TRANS"
        });
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAsync(line, "1", "11"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(line));
        Assert.Equal(2, await db.Set<InventTransOriginSalesLine>().CountAsync());
        Assert.Equal(10m, summary.OnOrder);
    }

    private sealed class InventoryContext(DbContextOptions<InventoryContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            Configure<InventDim>(builder);
            Configure<InventTransOrigin>(builder);
            Configure<InventTransOriginSalesLine>(builder);
            Configure<InventTrans>(builder);
            Configure<InventSum>(builder);
        }

        private static void Configure<TEntity>(ModelBuilder builder) where TEntity : IAX.IXApi.Shared.Domain.Entities.Entity<long>
        {
            builder.Entity<TEntity>().HasKey(item => item.RecId);
            builder.Entity<TEntity>().Property(item => item.RecId).ValueGeneratedOnAdd();
            builder.Entity<TEntity>().Ignore(item => item.RowVersion);
        }
    }

    private sealed class TestFinanceDataContext(DbContext context) : IFinanceDataContext
    {
        public DatabaseFacade Database => context.Database;
        public ChangeTracker ChangeTracker => context.ChangeTracker;
        public DbSet<HcmWorker> HcmWorkers => context.Set<HcmWorker>();
        public DbSet<HcmDepartment> HcmDepartments => context.Set<HcmDepartment>();
        public DbSet<HcmShowroom> HcmShowrooms => context.Set<HcmShowroom>();
        public DbSet<OrganizationUnit> OrganizationUnits => context.Set<OrganizationUnit>();
        public DbSet<HcmWorkerOrganizationAssignment> HcmWorkerOrganizationAssignments => context.Set<HcmWorkerOrganizationAssignment>();
        public DbSet<HcmWorkerOrganizationAssignmentV1> HcmWorkerOrganizationAssignmentsV1 => context.Set<HcmWorkerOrganizationAssignmentV1>();
        public DbSet<HcmWorkerShowroomAssignment> HcmWorkerShowroomAssignments => context.Set<HcmWorkerShowroomAssignment>();
        public DbSet<OrganizationRole> OrganizationRoles => context.Set<OrganizationRole>();
        public DbSet<OrganizationHierarchy> OrganizationHierarchies => context.Set<OrganizationHierarchy>();
        public DbSet<OrganizationHierarchyNode> OrganizationHierarchyNodes => context.Set<OrganizationHierarchyNode>();
        public DbSet<HcmPosition> HcmPositions => context.Set<HcmPosition>();
        public DbSet<HcmReportingHierarchy> HcmReportingHierarchies => context.Set<HcmReportingHierarchy>();
        public DbSet<HcmPositionReportingLine> HcmPositionReportingLines => context.Set<HcmPositionReportingLine>();
        public DbSet<TaxData> TaxData => context.Set<TaxData>();
        public DbSet<TaxGroupHeading> TaxGroupHeadings => context.Set<TaxGroupHeading>();
        public DbSet<TaxGroupData> TaxGroupDatas => context.Set<TaxGroupData>();
        public DbSet<TaxOnItem> TaxOnItems => context.Set<TaxOnItem>();
        public DbSet<TEntity> Set<TEntity>() where TEntity : class => context.Set<TEntity>();
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            context.SaveChangesAsync(cancellationToken);
    }

    private sealed class TestUnitOfWork(DbContext context) : IUnitOfWork
    {
        public DbContext Context => context;
        public Task<int> CompleteAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
        public IGenericRepository<T> Repository<T>() where T : class => throw new NotSupportedException();
        public Task BeginTransactionAsync(CancellationToken ct = default) => throw new NotSupportedException();
        public Task CommitTransactionAsync(CancellationToken ct = default) => throw new NotSupportedException();
        public Task RollbackTransactionAsync(CancellationToken ct = default) => throw new NotSupportedException();
        public void Dispose() { }
    }

    private sealed class TestNumbers : ISalesInventoryNumberService
    {
        public int DimensionCalls { get; private set; }
        private int _transactionCalls;
        public Task<string> NextInventDimIdAsync(CancellationToken cancellationToken = default)
        {
            DimensionCalls++;
            return Task.FromResult("DIM-000001");
        }
        public Task<string> NextInventTransIdAsync(CancellationToken cancellationToken = default)
            => Task.FromResult($"LOT-{++_transactionCalls:000001}");
    }
}
