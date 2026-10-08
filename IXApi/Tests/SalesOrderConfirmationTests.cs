using System.Reflection;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Confirm;
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
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Shared.Application.Identity;
using IAX.IXApi.Shared.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Mapster;
using Xunit;

namespace IAX.IXApi.Tests;

public sealed class SalesOrderConfirmationTests
{
    static SalesOrderConfirmationTests()
    {
        new SalesOrderConfirmationMapping().Register(TypeAdapterConfig.GlobalSettings);
    }

    [Fact]
    public async Task Sales_unit_conversion_uses_product_rule_and_rejects_missing_unit()
    {
        await using var fixture = await Fixture.CreateAsync();
        fixture.Db.Set<InventTable>().Add(new InventTable {
            RecId = 10, DataAreaId = "dat", ItemId = "ITEM-A", Product = 42
        });
        fixture.Db.Set<InventTableModule>().Add(new InventTableModule {
            RecId = 10, DataAreaId = "dat", ItemId = "ITEM-A", ModuleType = ModuleInventPurchSales.Inventory,
            UnitId = "ea"
        });
        fixture.Db.Set<UnitOfMeasure>().AddRange(
            new UnitOfMeasure { RecId = 1, Symbol = "ea", DecimalPrecision = 0 },
            new UnitOfMeasure { RecId = 2, Symbol = "box", DecimalPrecision = 0 });
        fixture.Db.Set<UnitOfMeasureConversion>().Add(new UnitOfMeasureConversion {
            RecId = 1, Product = 42, FromUnitOfMeasure = 2, ToUnitOfMeasure = 1, Factor = 12
        });
        await fixture.Db.SaveChangesAsync();
        var service = new SalesUnitConversionService(new FinanceContextAdapter(fixture.Db));
        var item = await fixture.Db.Set<InventTable>().SingleAsync();
        Assert.Equal(24m, await service.ToInventoryQuantityAsync(item, "box", 2m, default));
        Assert.Equal(2m, await service.ToInventoryQuantityAsync(item, "ea", 2m, default));
        Assert.Null(await service.ToInventoryQuantityAsync(item, "pallet", 2m, default));
        var inventoryModule = await fixture.Db.Set<InventTableModule>().SingleAsync();
        inventoryModule.UnitId = "box";
        (await fixture.Db.Set<UnitOfMeasure>().SingleAsync(unit => unit.Symbol == "box")).DecimalPrecision = 3;
        await fixture.Db.SaveChangesAsync();
        Assert.Equal(0.167m, await service.ToInventoryQuantityAsync(item, "ea", 2m, default));
    }

    [Fact]
    public async Task Confirmation_and_reconfirmation_keep_separate_snapshots_without_consuming_inventory()
    {
        await using var fixture = await Fixture.CreateAsync();
        var first = await fixture.Controller.Post(1, new(DateTime.UtcNow.Date), default);
        Assert.IsType<OkObjectResult>(first);
        var journal = await fixture.Db.Set<CustConfirmJour>().SingleAsync();
        var firstLine = await fixture.Db.Set<CustConfirmTrans>().SingleAsync();
        Assert.Equal("CONF-000001", journal.ConfirmId);
        Assert.Equal(900m, firstLine.LineAmount);
        Assert.Equal(10m, firstLine.Qty);
        Assert.Equal(900m, journal.ConfirmAmount);
        Assert.Equal(DocumentStatus.Confirmation, (await fixture.Db.Set<SalesTable>().SingleAsync()).DocumentStatus);
        Assert.Equal(10m, (await fixture.Db.Set<SalesLine>().SingleAsync()).RemainSalesPhysical);
        Assert.Equal(-10m, (await fixture.Db.Set<InventTrans>().SingleAsync()).Qty);

        var line = await fixture.Db.Set<SalesLine>().SingleAsync();
        line.SalesPrice = 120m;
        line.LineAmount = 1200m;
        await fixture.Db.SaveChangesAsync();
        Assert.IsType<OkObjectResult>(await fixture.Controller.Post(1, new(DateTime.UtcNow.Date), default));
        var journals = await fixture.Db.Set<CustConfirmJour>().OrderBy(x => x.ConfirmId).ToListAsync();
        var snapshots = await fixture.Db.Set<CustConfirmTrans>().OrderBy(x => x.ConfirmId).ToListAsync();
        Assert.Equal(2, journals.Count);
        Assert.Equal(new[] { 900m, 1100m }, snapshots.Select(x => x.LineAmount));
        Assert.Equal(new[] { 900m, 1100m }, journals.Select(x => x.ConfirmAmount));
        Assert.Equal(10m, (await fixture.Db.Set<SalesLine>().SingleAsync()).RemainSalesPhysical);
        Assert.Equal(-10m, (await fixture.Db.Set<InventTrans>().SingleAsync()).Qty);
    }

    [Fact]
    public async Task Posting_disabled_or_mandatory_credit_check_creates_no_history()
    {
        await using var fixture = await Fixture.CreateAsync();
        Assert.IsType<BadRequestObjectResult>(await fixture.Controller.Post(1, new(Posting: false), default));
        var customer = await fixture.Db.Set<CustTable>().SingleAsync();
        customer.MandatoryCreditLimit = NoYes.Yes;
        await fixture.Db.SaveChangesAsync();
        Assert.IsType<UnprocessableEntityObjectResult>(await fixture.Controller.Post(1, new(), default));
        Assert.Empty(await fixture.Db.Set<CustConfirmJour>().ToListAsync());
        Assert.Empty(await fixture.Db.Set<CustConfirmTrans>().ToListAsync());
        Assert.Equal(DocumentStatus.None, (await fixture.Db.Set<SalesTable>().SingleAsync()).DocumentStatus);
    }

    [Fact]
    public async Task Confirmation_reads_keep_list_and_detail_contracts()
    {
        await using var fixture = await Fixture.CreateAsync();
        Assert.IsType<OkObjectResult>(await fixture.Controller.Post(1, new(), default));

        var listed = Assert.IsType<OkObjectResult>(await fixture.Controller.List(1, default));
        var listResponse = Assert.IsType<APIResponse<object>>(listed.Value);
        var row = Assert.Single(Assert.IsType<List<ConfirmationListItemDto>>(listResponse.Data));
        Assert.Equal("1", row.Id);
        Assert.Equal("CONF-000001", row.ConfirmId);

        var detail = Assert.IsType<OkObjectResult>(await fixture.Controller.Get(1, 1, default));
        Assert.NotNull(Assert.IsType<APIResponse<object>>(detail.Value).Data);
        Assert.IsType<NotFoundObjectResult>(await fixture.Controller.Get(1, 2, default));
        Assert.IsType<NotFoundObjectResult>(await fixture.Controller.List(2, default));
    }

    private sealed class Fixture : IAsyncDisposable
    {
        private readonly SqliteConnection connection = new("Data Source=:memory:");
        public ConfirmationContext Db { get; private set; } = null!;
        public SalesOrderConfirmationController Controller { get; private set; } = null!;

        public static async Task<Fixture> CreateAsync()
        {
            var fixture = new Fixture();
            await fixture.connection.OpenAsync();
            fixture.Db = new ConfirmationContext(new DbContextOptionsBuilder<ConfirmationContext>()
                .UseSqlite(fixture.connection).Options);
            await fixture.Db.Database.EnsureCreatedAsync();
            fixture.Db.Set<CustTable>().Add(new CustTable { RecId = 1, DataAreaId = "dat", AccountNum = "C1" });
            fixture.Db.Set<SalesTable>().Add(new SalesTable {
                RecId = 1, DataAreaId = "dat", SalesId = "SO1", CustAccount = "C1",
                InvoiceAccount = "C1", CurrencyCode = "SAR", SalesStatus = SalesStatus.Backorder,
                DocumentStatus = DocumentStatus.None, OrderDate = DateTime.UtcNow.Date
            });
            fixture.Db.Set<SalesLine>().Add(new SalesLine {
                RecId = 1, DataAreaId = "dat", SalesId = "SO1", LineNum = 1, ItemId = "ITEM-A",
                SalesQty = 10, QtyOrdered = 10, RemainSalesPhysical = 10, RemainSalesFinancial = 10,
                SalesUnit = "EA", SalesPrice = 100, PriceUnit = 1, LineAmount = 1000,
                LineDisc = 100, SalesStatus = SalesStatus.Backorder, CurrencyCode = "SAR"
            });
            fixture.Db.Set<InventTrans>().Add(new InventTrans { RecId = 1, DataAreaId = "dat", ItemId = "ITEM-A", Qty = -10 });
            await fixture.Db.SaveChangesAsync();
            var sequence = DispatchProxy.Create<ISysNumberSequenceService, TestSequenceProxy>();
            var context = new FinanceContextAdapter(fixture.Db);
            var company = new TestCompany();
            fixture.Controller = new SalesOrderConfirmationController(
                new SalesOrderConfirmationService(new TestUnitOfWork(fixture.Db), sequence, company,
                    new SalesOrderConfirmationValidator()));
            return fixture;
        }

        public async ValueTask DisposeAsync() { await Db.DisposeAsync(); await connection.DisposeAsync(); }
    }

    private sealed class ConfirmationContext(DbContextOptions<ConfirmationContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            var included = new HashSet<Type>
            {
                typeof(SalesTable), typeof(SalesLine), typeof(CustTable), typeof(CustParameters),
                typeof(CustConfirmJour), typeof(CustConfirmTrans), typeof(InventTrans),
                typeof(MarkupTrans), typeof(TaxGroupData), typeof(TaxOnItem),
                typeof(TaxData), typeof(InventTableModule), typeof(InventTable),
                typeof(UnitOfMeasure), typeof(UnitOfMeasureConversion)
            };
            foreach (var type in typeof(SalesTable).Assembly.GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract
                    && typeof(Entity<long>).IsAssignableFrom(type) && !included.Contains(type)))
                builder.Ignore(type);
            Configure<SalesTable>(builder);
            Configure<SalesLine>(builder);
            Configure<CustTable>(builder);
            Configure<CustParameters>(builder);
            Configure<CustConfirmJour>(builder);
            Configure<CustConfirmTrans>(builder);
            Configure<InventTrans>(builder);
            Configure<MarkupTrans>(builder);
            Configure<TaxGroupData>(builder);
            Configure<TaxOnItem>(builder);
            Configure<TaxData>(builder);
            Configure<InventTableModule>(builder);
            Configure<InventTable>(builder);
            Configure<UnitOfMeasure>(builder);
            Configure<UnitOfMeasureConversion>(builder);
        }

        private static void Configure<TEntity>(ModelBuilder builder) where TEntity : Entity<long>
        {
            builder.Entity<TEntity>().HasKey(x => x.RecId);
            builder.Entity<TEntity>().Property(x => x.RecId).ValueGeneratedOnAdd();
            builder.Entity<TEntity>().Ignore(x => x.RowVersion);
            foreach (var property in typeof(TEntity).GetProperties())
            {
                var type = property.PropertyType;
                if (type != typeof(string) && type != typeof(byte[]) && !type.IsValueType)
                    builder.Entity<TEntity>().Ignore(property.Name);
            }
        }
    }

    public class TestSequenceProxy : DispatchProxy
    {
        private int next;
        protected override object? Invoke(MethodInfo? method, object?[]? args)
        {
            if (method?.Name == nameof(ISysNumberSequenceService.NextAsync))
                return Task.FromResult(new NextSequenceResultDto {
                    EntityName = "CustConfirmJour", Value = ++next, Code = $"CONF-{next:D6}"
                });
            throw new NotSupportedException(method?.Name);
        }
    }

    private sealed class TestCompany : ICompanyExecutionContext
    {
        public string GetDataAreaId() => "dat";
        public bool IsRequestedCompanyAuthorized() => true;
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

    private sealed class FinanceContextAdapter(DbContext context) : IFinanceDataContext
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
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => context.SaveChangesAsync(cancellationToken);
    }
}
