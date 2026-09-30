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
        string inventLocationId, CancellationToken cancellationToken = default)
    {
        var dimension = await _dimensions.ResolveAsync(order.DataAreaId, inventSiteId,
            inventLocationId, cancellationToken);
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
        line.RemainInventPhysical = line.SalesQty;
        line.RemainInventFinancial = line.SalesQty;

        var inventorySummary = await GetOrCreateInventorySummaryAsync(
            order.DataAreaId, line.ItemId, dimension, cancellationToken);
        inventorySummary.OnOrder += line.SalesQty;
        inventorySummary.LastUpdDateExpected = line.ReceiptDateRequested;

        _dbContext.Set<InventTrans>().Add(new InventTrans
        {
            InventTransOrigin = origin.RecId,
            ItemId = line.ItemId,
            InventDimId = dimension.InventDimId,
            Qty = -line.SalesQty,
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
        string inventLocationId, CancellationToken cancellationToken = default)
    {
        // Preserve update support for sales lines created before inventory-demand integration.
        if (string.IsNullOrWhiteSpace(line.InventTransId)) return;
        var origin = await _dbContext.Set<InventTransOrigin>().SingleOrDefaultAsync(item =>
            item.DataAreaId == line.DataAreaId && item.InventTransId == line.InventTransId,
            cancellationToken);
        if (origin == null) throw new InvalidOperationException("The sales line inventory origin was not found.");
        var transaction = await _dbContext.Set<InventTrans>().SingleOrDefaultAsync(item =>
            item.DataAreaId == line.DataAreaId && item.InventTransOrigin == origin.RecId, cancellationToken);
        if (transaction == null) throw new InvalidOperationException("The sales line inventory transaction was not found.");
        var previousDemand = -transaction.Qty;
        var previousDimensionId = line.InventDimId;
        var dimension = await _dimensions.ResolveAsync(line.DataAreaId, inventSiteId,
            inventLocationId, cancellationToken);
        transaction.Qty = -line.SalesQty;
        transaction.DateExpected = line.ReceiptDateRequested;
        line.RemainInventPhysical = line.SalesQty;
        line.RemainInventFinancial = line.SalesQty;
        var previousSummary = await _dbContext.Set<InventSum>().SingleOrDefaultAsync(item =>
            item.DataAreaId == line.DataAreaId && item.ItemId == line.ItemId
            && item.InventDimId == previousDimensionId, cancellationToken)
            ?? throw new InvalidOperationException("The sales line inventory summary was not found.");
        if (previousDimensionId == dimension.InventDimId)
        {
            previousSummary.OnOrder += line.SalesQty - previousDemand;
            previousSummary.LastUpdDateExpected = line.ReceiptDateRequested;
            return;
        }

        previousSummary.OnOrder -= previousDemand;
        var nextSummary = await GetOrCreateInventorySummaryAsync(
            line.DataAreaId, line.ItemId, dimension, cancellationToken);
        nextSummary.OnOrder += line.SalesQty;
        nextSummary.LastUpdDateExpected = line.ReceiptDateRequested;
        line.InventDimId = dimension.InventDimId;
        origin.ItemInventDimId = dimension.InventDimId;
        transaction.InventDimId = dimension.InventDimId;
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
