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
        var dimensions = new InventDimensionResolver(financeData, numbers);
        var service = new SalesInventoryDemandService(financeData, numbers, dimensions);
        var order = new SalesTable
        {
            SalesId = "SO-1", CurrencyCode = "USD", DataAreaId = "DAT",
            InventSiteId = "1", InventLocationId = "11"
        };
        var line = new SalesLine
        {
            SalesId = order.SalesId, ItemId = "A0001", SalesQty = 10,
            RemainSalesPhysical = 10, RemainSalesFinancial = 10,
            ReceiptDateRequested = new DateTime(2026, 9, 30), DataAreaId = order.DataAreaId
        };

        await service.CreateAsync(order, line, "1", "11");
        await financeData.SaveChangesAsync();

        var dimension = await db.Set<InventDim>().SingleAsync();
        var origin = await db.Set<InventTransOrigin>().SingleAsync();
        var transaction = await db.Set<InventTrans>().SingleAsync();
        var inventorySummary = await db.Set<InventSum>().SingleAsync();
        Assert.Equal("DIM-000001", dimension.InventDimId);
        Assert.Equal("1", dimension.InventSiteId);
        Assert.Equal("11", dimension.InventLocationId);
        Assert.Equal(dimension.InventDimId, line.InventDimId);
        Assert.Equal(origin.InventTransId, line.InventTransId);
        Assert.Equal(origin.RecId, transaction.InventTransOrigin);
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

        var sameDimension = await dimensions.ResolveAsync("DAT", "1", "11");
        Assert.Equal(dimension.RecId, sameDimension.RecId);
        Assert.Equal(1, numbers.DimensionCalls);

        line.SalesQty = 12;
        await service.UpdateAsync(line);
        await financeData.SaveChangesAsync();
        Assert.Equal(-12, (await db.Set<InventTrans>().SingleAsync()).Qty);
        Assert.Equal(12, (await db.Set<InventSum>().SingleAsync()).OnOrder);

        await service.DeleteAsync(line);
        await financeData.SaveChangesAsync();
        Assert.Empty(await db.Set<InventTrans>().ToListAsync());
        Assert.Empty(await db.Set<InventTransOrigin>().ToListAsync());
        Assert.Single(await db.Set<InventDim>().ToListAsync());
        Assert.Equal(0, (await db.Set<InventSum>().SingleAsync()).OnOrder);
    }

    private sealed class InventoryContext(DbContextOptions<InventoryContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            Configure<InventDim>(builder);
            Configure<InventTransOrigin>(builder);
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

    private sealed class TestNumbers : ISalesInventoryNumberService
    {
        public int DimensionCalls { get; private set; }
        public Task<string> NextInventDimIdAsync(CancellationToken cancellationToken = default)
        {
            DimensionCalls++;
            return Task.FromResult("DIM-000001");
        }
        public Task<string> NextInventTransIdAsync(CancellationToken cancellationToken = default)
            => Task.FromResult("LOT-000001");
    }
}
