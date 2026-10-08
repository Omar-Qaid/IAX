using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Services;

public sealed class SalesInventoryDemandService : ISalesInventoryDemandService
{
    private readonly IFinanceDataContext _dbContext;
    private readonly ISalesInventoryNumberService _numbers;
    private readonly IInventDimensionResolver _dimensions;

    public SalesInventoryDemandService(IFinanceDataContext dbContext,
        ISalesInventoryNumberService numbers, IInventDimensionResolver dimensions)
    {
        _dbContext = dbContext;
        _numbers = numbers;
        _dimensions = dimensions;
    }

    public async Task CreateAsync(SalesTable order, SalesLine line, string inventSiteId,
        string inventLocationId, CancellationToken cancellationToken = default, InventDim? dimensionTemplate = null)
    {
        var dimension = await _dimensions.ResolveAsync(order.DataAreaId, inventSiteId,
            inventLocationId, cancellationToken, dimensionTemplate);
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
        _dbContext.Set<InventTransOrigin>().Add(origin);
        await _dbContext.SaveChangesAsync(cancellationToken);

        line.InventDimId = dimension.InventDimId;
        line.InventTransId = transactionId;
        line.InventRefId = order.SalesId;
        line.InventRefTransId = transactionId;
        line.InventRefType = InventRefType.SalesTable;
        line.RemainInventPhysical = line.QtyOrdered;
        line.RemainInventFinancial = line.QtyOrdered;

        var inventorySummary = await GetOrCreateInventorySummaryAsync(
            order.DataAreaId, line.ItemId, dimension, cancellationToken);
        inventorySummary.OnOrder += line.QtyOrdered;
        inventorySummary.LastUpdDateExpected = line.ReceiptDateRequested;

        _dbContext.Set<InventTrans>().Add(new InventTrans
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
        // Preserve update support for sales lines created before inventory-demand integration.
        if (string.IsNullOrWhiteSpace(line.InventTransId))
        {
            var existingLegacyDimension = await _dbContext.Set<InventDim>().AsNoTracking()
                .FirstOrDefaultAsync(item => item.DataAreaId == line.DataAreaId
                    && item.InventDimId == line.InventDimId, cancellationToken);
            var legacyDimension = CopyDimension(existingLegacyDimension);
            if (dimensionTemplate != null) CopyDimensionValues(dimensionTemplate, legacyDimension);
            if (batchNumber != null) legacyDimension.InventBatchId = batchNumber;
            if (serialNumber != null) legacyDimension.InventSerialId = serialNumber;
            var resolvedLegacyDimension = await _dimensions.ResolveAsync(line.DataAreaId,
                inventSiteId, inventLocationId, cancellationToken, legacyDimension);
            line.InventDimId = resolvedLegacyDimension.InventDimId;
            return;
        }
        var origin = await _dbContext.Set<InventTransOrigin>().SingleOrDefaultAsync(item =>
            item.DataAreaId == line.DataAreaId && item.InventTransId == line.InventTransId,
            cancellationToken);
        if (origin == null) throw new InvalidOperationException("The sales line inventory origin was not found.");
        var transaction = await _dbContext.Set<InventTrans>().SingleOrDefaultAsync(item =>
            item.DataAreaId == line.DataAreaId && item.InventTransOrigin == origin.RecId, cancellationToken);
        if (transaction == null) throw new InvalidOperationException("The sales line inventory transaction was not found.");
        if (transaction.StatusIssue != StatusIssue.Ordered)
            throw new InvalidOperationException("Processed inventory demand cannot be changed through sales line editing.");
        var previousDemand = -transaction.Qty;
        var previousDimensionId = line.InventDimId;
        var previousDimension = await _dbContext.Set<InventDim>().AsNoTracking()
            .FirstOrDefaultAsync(item => item.DataAreaId == line.DataAreaId
                && item.InventDimId == previousDimensionId, cancellationToken);
        var nextDimensionTemplate = CopyDimension(previousDimension);
        if (dimensionTemplate != null) CopyDimensionValues(dimensionTemplate, nextDimensionTemplate);
        if (batchNumber != null) nextDimensionTemplate.InventBatchId = batchNumber;
        if (serialNumber != null) nextDimensionTemplate.InventSerialId = serialNumber;
        var dimension = await _dimensions.ResolveAsync(line.DataAreaId, inventSiteId,
            inventLocationId, cancellationToken, nextDimensionTemplate);
        var nextOpenDemand = Math.Max(0m, line.RemainInventPhysical);
        transaction.Qty = -nextOpenDemand;
        transaction.DateExpected = line.ReceiptDateRequested;
        var previousSummary = await _dbContext.Set<InventSum>().SingleOrDefaultAsync(item =>
            item.DataAreaId == line.DataAreaId && item.ItemId == line.ItemId
            && item.InventDimId == previousDimensionId, cancellationToken)
            ?? throw new InvalidOperationException("The sales line inventory summary was not found.");
        if (previousDimensionId == dimension.InventDimId)
        {
            previousSummary.OnOrder += nextOpenDemand - previousDemand;
            previousSummary.LastUpdDateExpected = line.ReceiptDateRequested;
            origin.ItemInventDimId = dimension.InventDimId;
            transaction.InventDimId = dimension.InventDimId;
            return;
        }

        previousSummary.OnOrder -= previousDemand;
        var nextSummary = await GetOrCreateInventorySummaryAsync(
            line.DataAreaId, line.ItemId, dimension, cancellationToken);
        nextSummary.OnOrder += nextOpenDemand;
        nextSummary.LastUpdDateExpected = line.ReceiptDateRequested;
        line.InventDimId = dimension.InventDimId;
        origin.ItemInventDimId = dimension.InventDimId;
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
        var origin = await _dbContext.Set<InventTransOrigin>().SingleOrDefaultAsync(item =>
            item.DataAreaId == line.DataAreaId && item.InventTransId == line.InventTransId, cancellationToken);
        if (origin == null) throw new InvalidOperationException("The sales line inventory origin was not found.");
        var transactions = await _dbContext.Set<InventTrans>()
            .Where(item => item.DataAreaId == line.DataAreaId && item.InventTransOrigin == origin.RecId)
            .ToListAsync(cancellationToken);
        var demandQuantity = -transactions.Sum(item => item.Qty);
        var inventorySummary = await _dbContext.Set<InventSum>().SingleOrDefaultAsync(item =>
            item.DataAreaId == line.DataAreaId && item.ItemId == line.ItemId
            && item.InventDimId == line.InventDimId, cancellationToken)
            ?? throw new InvalidOperationException("The sales line inventory summary was not found.");
        inventorySummary.OnOrder -= demandQuantity;
        _dbContext.Set<InventTrans>().RemoveRange(transactions);
        _dbContext.Set<InventTransOrigin>().Remove(origin);
    }

    public async Task CancelRemainingAsync(SalesLine line, CancellationToken cancellationToken = default)
    {
        var remainingDemand = Math.Max(0m, line.RemainInventPhysical);
        if (!string.IsNullOrWhiteSpace(line.InventTransId))
        {
            var origin = await _dbContext.Set<InventTransOrigin>().SingleOrDefaultAsync(item =>
                item.DataAreaId == line.DataAreaId && item.InventTransId == line.InventTransId,
                cancellationToken);
            if (origin == null) throw new InvalidOperationException("The sales line inventory origin was not found.");
            var transactions = await _dbContext.Set<InventTrans>()
                .Where(item => item.DataAreaId == line.DataAreaId
                    && item.InventTransOrigin == origin.RecId
                    && item.StatusIssue == StatusIssue.Ordered)
                .ToListAsync(cancellationToken);
            if (transactions.Count == 0 && remainingDemand > 0)
                throw new InvalidOperationException(
                    "The open sales line has no On order inventory demand to cancel.");
            var openTransactionDemand = -transactions.Sum(item => item.Qty);
            if (openTransactionDemand != remainingDemand)
                throw new InvalidOperationException(
                    "The inventory demand no longer matches the completely open sales quantity.");
            var demandToCancel = Math.Min(remainingDemand, Math.Max(0m, openTransactionDemand));
            var inventorySummary = await _dbContext.Set<InventSum>().SingleOrDefaultAsync(item =>
                item.DataAreaId == line.DataAreaId && item.ItemId == line.ItemId
                && item.InventDimId == line.InventDimId, cancellationToken);
            if (inventorySummary != null)
                inventorySummary.OnOrder -= demandToCancel;
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

    private async Task<InventSum> GetOrCreateInventorySummaryAsync(
        string dataAreaId,
        string itemId,
        InventDim dimension,
        CancellationToken cancellationToken)
    {
        var inventorySummary = await _dbContext.Set<InventSum>().SingleOrDefaultAsync(item =>
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
        _dbContext.Set<InventSum>().Add(inventorySummary);
        return inventorySummary;
    }
}
