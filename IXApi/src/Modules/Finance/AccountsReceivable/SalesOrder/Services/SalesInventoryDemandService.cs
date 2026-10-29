using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Inventory.InventoryStatus.Entities;
using IAX.IXApi.Modules.Finance.Inventory.ProductLifecycle;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Services;

public sealed class SalesInventoryDemandService : ISalesInventoryDemandService
{
    private readonly IUnitOfWork _dbContext;
    private readonly ISalesInventoryNumberService _numbers;
    private readonly IInventDimensionResolver _dimensions;

    public SalesInventoryDemandService(IUnitOfWork dbContext,
        ISalesInventoryNumberService numbers, IInventDimensionResolver dimensions)
    {
        _dbContext = dbContext;
        _numbers = numbers;
        _dimensions = dimensions;
    }

    public async Task CreateAsync(SalesTable order, SalesLine line, string inventSiteId,
        string inventLocationId, CancellationToken cancellationToken = default, InventDim? dimensionTemplate = null)
    {
        if (order.SalesType == SalesType.ReturnItem || line.SalesType == SalesType.ReturnItem
            || !string.IsNullOrWhiteSpace(line.InventTransIdReturn))
            throw new InvalidOperationException(
                "Customer returns need a verified original-invoice and cost workflow; ordinary sales demand cannot represent a return receipt.");
        var lifecycleError = await ProductLifecyclePolicy.ValidateAsync(_dbContext.Context,
            order.DataAreaId, line.ItemId, ProductLifecycleProcess.Sales, cancellationToken);
        if (lifecycleError is not null) throw new InvalidOperationException(lifecycleError);
        if (!await IsStockedProductAsync(order.DataAreaId, line.ItemId, cancellationToken))
        {
            // D365 uses the item model group's Stocked product setting to decide whether
            // sales demand participates in inventory transactions, independently of Item/Service type.
            line.StockedProduct = NoYes.No;
            line.InventDimId = string.Empty;
            line.InventTransId = string.Empty;
            line.InventRefId = string.Empty;
            line.InventRefTransId = string.Empty;
            line.InventRefType = InventRefType.None;
            line.RemainInventPhysical = 0;
            line.RemainInventFinancial = 0;
            return;
        }

        var dimension = await _dimensions.ResolveAsync(order.DataAreaId, inventSiteId,
            inventLocationId, cancellationToken, dimensionTemplate);
        await EnsureStatusAvailableAsync(order.DataAreaId, dimension.InventStatusId, cancellationToken);
        var transactionId = await _numbers.NextInventTransIdAsync(cancellationToken);
        var origin = new InventTransOrigin
        {
            InventTransId = transactionId,
            ItemId = line.ItemId,
            ItemInventDimId = dimension.InventDimId,
            ReferenceCategory = InventRefType.SalesTable,
            ReferenceId = order.SalesId,
            DataAreaId = order.DataAreaId
        };
        _dbContext.Context.Set<InventTransOrigin>().Add(origin);
        await _dbContext.CompleteAsync(cancellationToken);
        _dbContext.Context.Set<InventTransOriginSalesLine>().Add(new InventTransOriginSalesLine
        {
            InventTransOrigin = origin.RecId,
            SalesLineDataAreaId = line.DataAreaId,
            SalesLineInventTransId = transactionId,
            DataAreaId = order.DataAreaId
        });

        line.InventDimId = dimension.InventDimId;
        line.InventTransId = transactionId;
        line.InventRefId = order.SalesId;
        line.InventRefTransId = transactionId;
        line.InventRefType = InventRefType.SalesTable;
        line.StockedProduct = NoYes.Yes;
        line.RemainInventPhysical = line.QtyOrdered;
        line.RemainInventFinancial = line.QtyOrdered;

        var inventorySummary = await GetOrCreateInventorySummaryAsync(
            order.DataAreaId, line.ItemId, dimension, cancellationToken);
        inventorySummary.OnOrder += line.QtyOrdered;
        inventorySummary.LastUpdDateExpected = line.ReceiptDateRequested;

        _dbContext.Context.Set<InventTrans>().Add(new InventTrans
        {
            InventTransOrigin = origin.RecId,
            ItemId = line.ItemId,
            InventDimId = dimension.InventDimId,
            Qty = -line.QtyOrdered,
            StatusIssue = StatusIssue.Ordered,
            StatusReceipt = StatusReceipt.None,
            ValueOpen = InventTransOpen.Yes,
            CurrencyCode = order.CurrencyCode,
            DateExpected = line.ReceiptDateRequested,
            ShippingDateRequested = line.ShippingDateRequested,
            DataAreaId = order.DataAreaId
        });
    }

    public async Task UpdateAsync(SalesLine line, string inventSiteId,
        string inventLocationId, CancellationToken cancellationToken = default,
        string? batchNumber = null, string? serialNumber = null, InventDim? dimensionTemplate = null)
    {
        var lifecycleError = await ProductLifecyclePolicy.ValidateAsync(_dbContext.Context,
            line.DataAreaId, line.ItemId, ProductLifecycleProcess.Sales, cancellationToken);
        if (lifecycleError is not null) throw new InvalidOperationException(lifecycleError);
        if (!await IsStockedProductAsync(line.DataAreaId, line.ItemId, cancellationToken))
        {
            if (!string.IsNullOrWhiteSpace(line.InventTransId))
                throw new InvalidOperationException("A sales line with existing inventory demand cannot be changed to a non-stocked model group.");
            line.StockedProduct = NoYes.No;
            line.InventDimId = string.Empty;
            line.RemainInventPhysical = 0;
            line.RemainInventFinancial = 0;
            return;
        }
        line.StockedProduct = NoYes.Yes;

        // Preserve update support for sales lines created before inventory-demand integration.
        if (string.IsNullOrWhiteSpace(line.InventTransId))
        {
            var existingLegacyDimension = await _dbContext.Context.Set<InventDim>().AsNoTracking()
                .FirstOrDefaultAsync(item => item.DataAreaId == line.DataAreaId
                    && item.InventDimId == line.InventDimId, cancellationToken);
            var legacyDimension = CopyDimension(existingLegacyDimension);
            if (dimensionTemplate != null) CopyDimensionValues(dimensionTemplate, legacyDimension);
            if (batchNumber != null) legacyDimension.InventBatchId = batchNumber;
            if (serialNumber != null) legacyDimension.InventSerialId = serialNumber;
            var resolvedLegacyDimension = await _dimensions.ResolveAsync(line.DataAreaId,
                inventSiteId, inventLocationId, cancellationToken, legacyDimension);
            await EnsureStatusAvailableAsync(line.DataAreaId, resolvedLegacyDimension.InventStatusId, cancellationToken);
            line.InventDimId = resolvedLegacyDimension.InventDimId;
            return;
        }
        var origin = await _dbContext.Context.Set<InventTransOrigin>().SingleOrDefaultAsync(item =>
            item.DataAreaId == line.DataAreaId && item.InventTransId == line.InventTransId,
            cancellationToken);
        if (origin == null) throw new InvalidOperationException("The sales line inventory origin was not found.");
        ValidateOrigin(line, origin);
        var salesLineLinks = await _dbContext.Context.Set<InventTransOriginSalesLine>().Where(item =>
                item.DataAreaId == line.DataAreaId && item.InventTransOrigin == origin.RecId)
            .ToListAsync(cancellationToken);
        if (salesLineLinks.Count > 1)
            throw new InvalidOperationException("The sales line inventory origin has multiple sales-line links.");
        var salesLineLink = salesLineLinks.SingleOrDefault();
        if (salesLineLink == null)
        {
            _dbContext.Context.Set<InventTransOriginSalesLine>().Add(new InventTransOriginSalesLine
            {
                InventTransOrigin = origin.RecId,
                SalesLineDataAreaId = line.DataAreaId,
                SalesLineInventTransId = line.InventTransId,
                DataAreaId = line.DataAreaId
            });
        }
        else
        {
            salesLineLink.SalesLineDataAreaId = line.DataAreaId;
            salesLineLink.SalesLineInventTransId = line.InventTransId;
        }
        var transaction = await _dbContext.Context.Set<InventTrans>().SingleOrDefaultAsync(item =>
            item.DataAreaId == line.DataAreaId && item.InventTransOrigin == origin.RecId, cancellationToken);
        if (transaction == null) throw new InvalidOperationException("The sales line inventory transaction was not found.");
        if (transaction.ItemId != line.ItemId)
            throw new InvalidOperationException("The inventory transaction item does not match this sales line.");
        if (transaction.StatusIssue != StatusIssue.Ordered)
            throw new InvalidOperationException("Processed inventory demand cannot be changed through sales line editing.");
        var previousDemand = -transaction.Qty;
        if (previousDemand < 0)
            throw new InvalidOperationException("The sales line inventory demand quantity is invalid.");
        var previousDimensionId = line.InventDimId;
        var previousDimension = await _dbContext.Context.Set<InventDim>().AsNoTracking()
            .FirstOrDefaultAsync(item => item.DataAreaId == line.DataAreaId
                && item.InventDimId == previousDimensionId, cancellationToken);
        var nextDimensionTemplate = CopyDimension(previousDimension);
        if (dimensionTemplate != null) CopyDimensionValues(dimensionTemplate, nextDimensionTemplate);
        if (batchNumber != null) nextDimensionTemplate.InventBatchId = batchNumber;
        if (serialNumber != null) nextDimensionTemplate.InventSerialId = serialNumber;
        var dimension = await _dimensions.ResolveAsync(line.DataAreaId, inventSiteId,
            inventLocationId, cancellationToken, nextDimensionTemplate);
        await EnsureStatusAvailableAsync(line.DataAreaId, dimension.InventStatusId, cancellationToken);
        var nextOpenDemand = Math.Max(0m, line.RemainInventPhysical);
        var previousSummary = await _dbContext.Context.Set<InventSum>().SingleOrDefaultAsync(item =>
            item.DataAreaId == line.DataAreaId && item.ItemId == line.ItemId
            && item.InventDimId == previousDimensionId, cancellationToken)
            ?? throw new InvalidOperationException("The sales line inventory summary was not found.");
        if (previousDimensionId == dimension.InventDimId)
        {
            if (previousSummary.OnOrder < previousDemand)
                throw new InvalidOperationException("The inventory summary has less open demand than this sales line.");
            previousSummary.OnOrder += nextOpenDemand - previousDemand;
            previousSummary.LastUpdDateExpected = line.ReceiptDateRequested;
            origin.ItemInventDimId = dimension.InventDimId;
            transaction.Qty = -nextOpenDemand;
            transaction.DateExpected = line.ReceiptDateRequested;
            transaction.InventDimId = dimension.InventDimId;
            return;
        }

        if (previousSummary.OnOrder < previousDemand)
            throw new InvalidOperationException("The inventory summary has less open demand than this sales line.");
        previousSummary.OnOrder -= previousDemand;
        var nextSummary = await GetOrCreateInventorySummaryAsync(
            line.DataAreaId, line.ItemId, dimension, cancellationToken);
        nextSummary.OnOrder += nextOpenDemand;
        nextSummary.LastUpdDateExpected = line.ReceiptDateRequested;
        line.InventDimId = dimension.InventDimId;
        origin.ItemInventDimId = dimension.InventDimId;
        transaction.Qty = -nextOpenDemand;
        transaction.DateExpected = line.ReceiptDateRequested;
        transaction.InventDimId = dimension.InventDimId;
    }

    private static InventDim CopyDimension(InventDim? source)
    {
        return new InventDim
        {
            ConfigId = source?.ConfigId ?? string.Empty,
            InventSizeId = source?.InventSizeId ?? string.Empty,
            InventColorId = source?.InventColorId ?? string.Empty,
            InventStyleId = source?.InventStyleId ?? string.Empty,
            InventVersionId = source?.InventVersionId ?? string.Empty,
            InventBatchId = source?.InventBatchId ?? string.Empty,
            InventSerialId = source?.InventSerialId ?? string.Empty,
            InventStatusId = source?.InventStatusId ?? string.Empty,
            WmsLocationId = source?.WmsLocationId ?? string.Empty,
            LicensePlateId = source?.LicensePlateId ?? string.Empty,
            InventDimension10 = source?.InventDimension10 ?? 0m,
            InventDimension9 = source?.InventDimension9 ?? default,
            InventDimension9TzId = source?.InventDimension9TzId ?? 0
        };
    }

    private static void CopyDimensionValues(InventDim source, InventDim target)
    {
        target.ConfigId = source.ConfigId;
        target.InventSizeId = source.InventSizeId;
        target.InventColorId = source.InventColorId;
        target.InventStyleId = source.InventStyleId;
        target.InventVersionId = source.InventVersionId;
    }

    public async Task DeleteAsync(SalesLine line, CancellationToken cancellationToken = default)
    {
        // Preserve delete support for sales lines created before inventory-demand integration.
        if (string.IsNullOrWhiteSpace(line.InventTransId)) return;
        var origin = await _dbContext.Context.Set<InventTransOrigin>().SingleOrDefaultAsync(item =>
            item.DataAreaId == line.DataAreaId && item.InventTransId == line.InventTransId, cancellationToken);
        if (origin == null) throw new InvalidOperationException("The sales line inventory origin was not found.");
        ValidateOrigin(line, origin);
        var transactions = await _dbContext.Context.Set<InventTrans>()
            .Where(item => item.DataAreaId == line.DataAreaId && item.InventTransOrigin == origin.RecId)
            .ToListAsync(cancellationToken);
        if (transactions.Count == 0 || transactions.Any(item => item.StatusIssue != StatusIssue.Ordered))
            throw new InvalidOperationException("Processed inventory transactions cannot be deleted with an open sales line.");
        if (transactions.Any(item => item.ItemId != line.ItemId))
            throw new InvalidOperationException("The inventory transaction item does not match this sales line.");
        var demandQuantity = -transactions.Sum(item => item.Qty);
        if (demandQuantity < 0)
            throw new InvalidOperationException("The sales line inventory demand quantity is invalid.");
        var salesLineLinks = await _dbContext.Context.Set<InventTransOriginSalesLine>()
            .Where(item => item.DataAreaId == line.DataAreaId && item.InventTransOrigin == origin.RecId)
            .ToListAsync(cancellationToken);
        if (salesLineLinks.Count > 1)
            throw new InvalidOperationException("The sales line inventory origin has multiple sales-line links.");
        var inventorySummary = await _dbContext.Context.Set<InventSum>().SingleOrDefaultAsync(item =>
            item.DataAreaId == line.DataAreaId && item.ItemId == line.ItemId
            && item.InventDimId == line.InventDimId, cancellationToken)
            ?? throw new InvalidOperationException("The sales line inventory summary was not found.");
        if (inventorySummary.OnOrder < demandQuantity)
            throw new InvalidOperationException("The inventory summary has less open demand than this sales line.");
        inventorySummary.OnOrder -= demandQuantity;
        _dbContext.Context.Set<InventTransOriginSalesLine>().RemoveRange(salesLineLinks);
        _dbContext.Context.Set<InventTrans>().RemoveRange(transactions);
        _dbContext.Context.Set<InventTransOrigin>().Remove(origin);
    }

    public async Task CancelRemainingAsync(SalesLine line, CancellationToken cancellationToken = default)
    {
        var remainingDemand = Math.Max(0m, line.RemainInventPhysical);
        if (!string.IsNullOrWhiteSpace(line.InventTransId))
        {
            var origin = await _dbContext.Context.Set<InventTransOrigin>().SingleOrDefaultAsync(item =>
                item.DataAreaId == line.DataAreaId && item.InventTransId == line.InventTransId,
                cancellationToken);
            if (origin == null) throw new InvalidOperationException("The sales line inventory origin was not found.");
            ValidateOrigin(line, origin);
            var transactions = await _dbContext.Context.Set<InventTrans>()
                .Where(item => item.DataAreaId == line.DataAreaId
                    && item.InventTransOrigin == origin.RecId
                    && (item.StatusIssue == StatusIssue.Ordered || item.StatusIssue == StatusIssue.ReservPhysical))
                .ToListAsync(cancellationToken);
            if (transactions.Any(item => item.ItemId != line.ItemId))
                throw new InvalidOperationException("The inventory transaction item does not match this sales line.");
            if (transactions.Count == 0 && remainingDemand > 0)
                throw new InvalidOperationException(
                    "The open sales line has no On order inventory demand to cancel.");
            var openTransactionDemand = -transactions.Sum(item => item.Qty);
            if (openTransactionDemand != remainingDemand)
                throw new InvalidOperationException(
                    "The inventory demand no longer matches the completely open sales quantity.");
            var demandToCancel = Math.Min(remainingDemand, Math.Max(0m, openTransactionDemand));
            if (transactions.Any(item => item.InventDimId != line.InventDimId || item.Qty > 0))
                throw new InvalidOperationException("Cancellation requires matching sales-line inventory dimensions and issue quantities.");
            var reservedToCancel = -transactions.Where(item => item.StatusIssue == StatusIssue.ReservPhysical).Sum(item => item.Qty);
            var orderedToCancel = demandToCancel - reservedToCancel;
            var inventorySummary = await _dbContext.Context.Set<InventSum>().SingleOrDefaultAsync(item =>
                item.DataAreaId == line.DataAreaId && item.ItemId == line.ItemId
                && item.InventDimId == line.InventDimId, cancellationToken)
                ?? throw new InvalidOperationException("The sales line inventory summary was not found.");
            if (inventorySummary.OnOrder < orderedToCancel || inventorySummary.ReservPhysical < reservedToCancel)
                throw new InvalidOperationException("The inventory summary has less open demand than this sales line.");
            inventorySummary.OnOrder -= orderedToCancel;
            inventorySummary.ReservPhysical -= reservedToCancel;
            inventorySummary.AvailPhysical = inventorySummary.PhysicalInvent - inventorySummary.ReservPhysical - inventorySummary.Picked;
            inventorySummary.AvailOrdered = inventorySummary.AvailPhysical + inventorySummary.Ordered - inventorySummary.OnOrder - inventorySummary.ReservOrdered;
            foreach (var transaction in transactions)
            {
                transaction.Qty = 0;
                transaction.StatusIssue = StatusIssue.None;
                transaction.ValueOpen = InventTransOpen.No;
            }
        }

        line.RemainSalesPhysical = 0;
        line.RemainSalesFinancial = 0;
        line.RemainInventPhysical = 0;
        line.RemainInventFinancial = 0;
        line.SalesDeliverNow = 0;
        line.InventDeliverNow = 0;
        line.SalesStatus = SalesStatus.Canceled;
    }

    private static void ValidateOrigin(SalesLine line, InventTransOrigin origin)
    {
        if (origin.ReferenceCategory != InventRefType.SalesTable
            || origin.ReferenceId != line.SalesId || origin.ItemId != line.ItemId)
            throw new InvalidOperationException("The inventory origin does not belong to this sales line.");
    }

    private async Task EnsureStatusAvailableAsync(string companyId, string statusId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(statusId)) return;
        var status = await _dbContext.Context.Set<WHSInventStatus>().AsNoTracking().FirstOrDefaultAsync(
            x => x.DataAreaId == companyId && x.InventStatusId == statusId, ct);
        if (status is null) throw new InvalidOperationException($"Inventory status '{statusId}' does not exist in the selected company.");
        if (status.InventStatusBlocking == NoYes.Yes)
            throw new InvalidOperationException($"Inventory status '{statusId}' is blocked for sales demand.");
    }

    private async Task<bool> IsStockedProductAsync(
        string dataAreaId, string itemId, CancellationToken cancellationToken)
    {
        var assignment = await _dbContext.Context.Set<InventModelGroupItem>().AsNoTracking()
            .FirstOrDefaultAsync(item => item.ItemDataAreaId == dataAreaId && item.ItemId == itemId,
                cancellationToken);
        // Existing unconfigured items retain legacy demand behavior; services default to
        // non-stocked until a model group explicitly opts them into inventory.
        if (assignment is null)
        {
            var itemType = await _dbContext.Context.Set<InventTable>().AsNoTracking()
                .Where(item => item.DataAreaId == dataAreaId && item.ItemId == itemId)
                .Select(item => (ItemType?)item.ItemType).FirstOrDefaultAsync(cancellationToken);
            return itemType != ItemType.Service;
        }

        var modelGroup = await _dbContext.Context.Set<InventModelGroup>().AsNoTracking()
            .FirstOrDefaultAsync(group => group.DataAreaId == assignment.ModelGroupDataAreaId
                && group.ModelGroupId == assignment.ModelGroupId, cancellationToken);
        if (modelGroup is null)
            throw new InvalidOperationException("The assigned item model group was not found in its company.");
        return modelGroup.StockedProduct == NoYes.Yes;
    }

    private async Task<InventSum> GetOrCreateInventorySummaryAsync(
        string dataAreaId,
        string itemId,
        InventDim dimension,
        CancellationToken cancellationToken)
    {
        var inventorySummary = await _dbContext.Context.Set<InventSum>().SingleOrDefaultAsync(item =>
            item.DataAreaId == dataAreaId && item.ItemId == itemId
            && item.InventDimId == dimension.InventDimId, cancellationToken);
        if (inventorySummary != null) return inventorySummary;

        inventorySummary = new InventSum
        {
            ItemId = itemId,
            InventDimId = dimension.InventDimId,
            InventSiteId = dimension.InventSiteId,
            InventLocationId = dimension.InventLocationId,
            WmsLocationId = dimension.WmsLocationId,
            LicensePlateId = dimension.LicensePlateId,
            ConfigId = dimension.ConfigId,
            InventSizeId = dimension.InventSizeId,
            InventColorId = dimension.InventColorId,
            InventStyleId = dimension.InventStyleId,
            InventVersionId = dimension.InventVersionId,
            InventBatchId = dimension.InventBatchId,
            InventSerialId = dimension.InventSerialId,
            InventStatusId = dimension.InventStatusId,
            DataAreaId = dataAreaId
        };
        _dbContext.Context.Set<InventSum>().Add(inventorySummary);
        return inventorySummary;
    }
}
