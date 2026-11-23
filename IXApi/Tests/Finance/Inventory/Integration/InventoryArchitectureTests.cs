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
    static InventoryArchitectureTests()
    {
        new InventLocationMapping().Register(TypeAdapterConfig.GlobalSettings);
        new InventTransMapping().Register(TypeAdapterConfig.GlobalSettings);
    }

    private sealed class InventoryContext(DbContextOptions<InventoryContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            Configure<InventSite>(builder);
            Configure<InventLocation>(builder);
            Configure<InventModelGroup>(builder);
            Configure<InventBatch>(builder);
            Configure<InventSerial>(builder);
            Configure<InventItemLocation>(builder);
            Configure<InventCountGroup>(builder);
            Configure<InventPackagingGroup>(builder);
            Configure<InventPackagingUnit>(builder);
            Configure<WMSLocation>(builder);
            Configure<InventTrans>(builder);
            Configure<IAX.IXApi.Modules.Finance.Entities.InventTransPosting>(builder);
            Configure<InventTransOrigin>(builder);
            Configure<InventSettlement>(builder);
            Configure<InventClosing>(builder);
            Configure<InventDim>(builder);
            Configure<SalesLine>(builder);
            Configure<SalesTable>(builder);
            Configure<CustPackingSlipJour>(builder);
            Configure<ReleasedItem>(builder);
            Configure<InventSum>(builder);
            Configure<InventItemBarcode>(builder);
            Configure<UnitOfMeasure>(builder);
            Configure<UnitOfMeasureConversion>(builder);
            Configure<InventTableModule>(builder);
            Configure<InventItemPurchSetup>(builder);
            Configure<InventItemSalesSetup>(builder);
            Configure<InventItemInventSetup>(builder);
            Configure<InventItemSetupSupplyType>(builder);
            Configure<InventItemPrice>(builder);
            Configure<SharedProduct>(builder);
            builder.Entity<SharedProduct>().HasQueryFilter(x => !x.IsDeleted);
            Configure<IAX.IXApi.Modules.Finance.Inventory.InventTable.EcoResDistinctProduct>(builder);
            Configure<IAX.IXApi.Modules.Finance.Inventory.InventTable.EcoResProductTranslation>(builder);
            Configure<EcoResCategory>(builder);
            Configure<EcoResCategoryHierarchy>(builder);
            Configure<EcoResProductCategory>(builder);
            Configure<EcoResProductLifecycleState>(builder);
            Configure<WHSInventStatus>(builder);
            Configure<InventParameters>(builder);
            Configure<InventJournalName>(builder);
            Configure<InventJournalTable>(builder);
            Configure<InventTransferTable>(builder);
            Configure<InventTransferLine>(builder);
            Configure<InventTransferJour>(builder);
            Configure<InventTransOriginTransferTransitTo>(builder);
            Configure<InventTransOriginTransferTransitFrom>(builder);
            Configure<InventTransOriginTransferReceive>(builder);
            Configure<InventTransOriginTransferShip>(builder);
            builder.ApplyConfiguration(new IAX.IXApi.Modules.Finance.Inventory.Transfer.InventTransferTableConfiguration());
            builder.ApplyConfiguration(new IAX.IXApi.Modules.Finance.Inventory.Transfer.InventTransferLineConfiguration());
            builder.ApplyConfiguration(new IAX.IXApi.Modules.Finance.Inventory.Transfer.InventTransferJourConfiguration());
            builder.ApplyConfiguration(new IAX.IXApi.Modules.Finance.Inventory.Transfer.InventTransOriginTransferTransitToConfiguration());
            builder.ApplyConfiguration(new IAX.IXApi.Modules.Finance.Inventory.Transfer.InventTransOriginTransferTransitFromConfiguration());
            builder.ApplyConfiguration(new IAX.IXApi.Modules.Finance.Inventory.Transfer.InventTransOriginTransferReceiveConfiguration());
            builder.ApplyConfiguration(new IAX.IXApi.Modules.Finance.Inventory.Transfer.InventTransOriginTransferShipConfiguration());
            Configure<InventJournalTrans>(builder);
            Configure<InventModelGroup>(builder);
            Configure<InventModelGroupItem>(builder);
            Configure<EcoResTrackingDimensionGroupItem>(builder);
            Configure<EcoResTrackingDimensionGroup>(builder);
            Configure<EcoResTrackingDimensionGroupFldSetup>(builder);
            Configure<EcoResStorageDimensionGroup>(builder);
            Configure<EcoResStorageDimensionGroupItem>(builder);
            Configure<EcoResStorageDimensionGroupFldSetup>(builder);
            Configure<InventItemGroupItem>(builder);
            Configure<InventItemGroup>(builder);
            Configure<InventPosting>(builder);
            Configure<TaxGroupHeading>(builder);
            Configure<TaxItemGroupHeading>(builder);
            Configure<MainAccount>(builder);
            Configure<DimensionAttributeValueCombination>(builder);
            // Posting validates a combination against the structures assigned to the ledger.
            Configure<LedgerStructure>(builder);
            Configure<DimensionHierarchy>(builder);
            Configure<Ledger>(builder);
            Configure<FiscalCalendarYear>(builder);
            Configure<FiscalCalendarPeriod>(builder);
            Configure<LedgerFiscalCalendarPeriod>(builder);
            Configure<LedgerPeriodModuleAccessControl>(builder);
            Configure<GeneralJournalEntry>(builder);
            Configure<GeneralJournalAccountEntry>(builder);
            Configure<SubledgerVoucherGeneralJournalEntry>(builder);
            Configure<SysNumberSequence>(builder);
            builder.Entity<InventSite>().HasQueryFilter(x => x.DataAreaId == "dat");
            builder.Entity<InventLocation>().HasQueryFilter(x => x.DataAreaId == "dat");
        }
        private static void Configure<T>(ModelBuilder builder) where T : class
        {
            var entity = builder.Entity<T>();
            entity.HasKey("RecId");
            entity.Property<long>("RecId").ValueGeneratedOnAdd();
            entity.Ignore("RowVersion");
            foreach (var property in typeof(T).GetProperties())
                if (!property.PropertyType.IsValueType && property.PropertyType != typeof(string))
                    entity.Ignore(property.Name);
        }
    }

    private sealed class TestIdentity : ICurrentUserService, ICompanyExecutionContext
    {
        public string GetCurrentUserId() => "test";
        public string GetOwnerAccountId() => "test";
        public string? DataAreaId { get; set; } = "dat";
        public string? GetDataAreaId() => DataAreaId;
        public bool IsRequestedCompanyAuthorized() => true;
    }

    private sealed class TestSalesInventoryNumbers : ISalesInventoryNumberService
    {
        private int _dimension;
        private int _transaction;
        public Task<string> NextInventDimIdAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult($"DIM-{++_dimension:000}");
        public Task<string> NextInventTransIdAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult($"TRANS-{++_transaction:000}");
    }

    private sealed class TestUnitOfWork : IUnitOfWork
    {
        private readonly DbContext context;
        private readonly int failOnComplete;
        private int completeCount;
        private IDbContextTransaction? transaction;
        public TestUnitOfWork(DbContext context, int failOnComplete = 0)
        { this.context = context; this.failOnComplete = failOnComplete; }
        public DbContext Context => context;
        public IGenericRepository<T> Repository<T>() where T : class
        {
            var repository = DispatchProxy.Create<IGenericRepository<T>, RepositoryProxy<T>>();
            ((RepositoryProxy<T>)(object)repository).Context = context;
            return repository;
        }
        public Task<int> CompleteAsync(CancellationToken ct = default)
        {
            if (++completeCount == failOnComplete) throw new InvalidOperationException("Injected save failure.");
            return context.SaveChangesAsync(ct);
        }
        public async Task BeginTransactionAsync(CancellationToken ct = default)
        {
            if (context.Database.CurrentTransaction == null) transaction = await context.Database.BeginTransactionAsync(ct);
        }
        public async Task CommitTransactionAsync(CancellationToken ct = default)
        { if (transaction != null) { await transaction.CommitAsync(ct); await transaction.DisposeAsync(); transaction = null; } }
        public async Task RollbackTransactionAsync(CancellationToken ct = default)
        { if (transaction != null) { await transaction.RollbackAsync(ct); await transaction.DisposeAsync(); transaction = null; } }
        public void Dispose() { }
    }

    public class RepositoryProxy<T> : DispatchProxy where T : class
    {
        public DbContext Context { get; set; } = null!;
        protected override object? Invoke(MethodInfo? method, object?[]? args)
        {
            switch (method?.Name)
            {
                case "GetQueryable": return Context.Set<T>().AsQueryable();
                case "AddAsync": Context.Add((T)args![0]!); return Task.FromResult((T)args[0]!);
                case "UpdateAsync": return Task.FromResult((T)args![0]!);
                case "RemoveAsync": Context.Remove((T)args![0]!); return Task.CompletedTask;
                default: throw new NotSupportedException(method?.Name);
            }
        }
    }
}
