using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;
using Mapster;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventTransService(IUnitOfWork unitOfWork, ICompanyExecutionContext company) : IInventTransService
{
    public async Task<List<InventTransListDto>?> GetListAsync(CancellationToken cancellationToken = default)
        => await SearchAsync(null, null, null, null, cancellationToken);

    public async Task<List<InventTransListDto>?> SearchAsync(string? itemId, string? sourceInventTransId,
        string? siteId, string? warehouseId, CancellationToken cancellationToken = default)
    {
        var dataAreaId = company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(dataAreaId))
            return null;

        var query = unitOfWork.Context.Set<InventTrans>()
            .AsNoTracking()
            .Where(transaction => transaction.DataAreaId == dataAreaId);
        if (!string.IsNullOrWhiteSpace(itemId))
            query = query.Where(transaction => transaction.ItemId == itemId.Trim());
        if (!string.IsNullOrWhiteSpace(sourceInventTransId))
        {
            var sourceId = sourceInventTransId.Trim();
            var originId = await unitOfWork.Context.Set<InventTransOrigin>().AsNoTracking()
                .Where(origin => origin.DataAreaId == dataAreaId && origin.InventTransId == sourceId)
                .Select(origin => (long?)origin.RecId).FirstOrDefaultAsync(cancellationToken);
            if (originId is null) return [];
            query = query.Where(transaction => transaction.InventTransOrigin == originId.Value);
        }
        if (!string.IsNullOrWhiteSpace(siteId) || !string.IsNullOrWhiteSpace(warehouseId))
        {
            var site = siteId?.Trim();
            var warehouse = warehouseId?.Trim();
            var matchingDimensionIds = unitOfWork.Context.Set<InventDim>().AsNoTracking()
                .Where(dimension => dimension.DataAreaId == dataAreaId
                    && (site == null || site == "" || dimension.InventSiteId == site)
                    && (warehouse == null || warehouse == "" || dimension.InventLocationId == warehouse))
                .Select(dimension => dimension.InventDimId);
            query = query.Where(transaction => matchingDimensionIds.Contains(transaction.InventDimId));
        }
        var transactions = await query
            .OrderByDescending(transaction => transaction.DateStatus)
            .ThenByDescending(transaction => transaction.RecId)
            .ToListAsync(cancellationToken);

        var originIds = transactions.Select(transaction => transaction.InventTransOrigin).Distinct().ToList();
        var origins = await unitOfWork.Context.Set<InventTransOrigin>()
            .AsNoTracking()
            .Where(origin => origin.DataAreaId == dataAreaId && originIds.Contains(origin.RecId))
            .ToDictionaryAsync(origin => origin.RecId, cancellationToken);

        var dimensionIds = transactions
            .Select(transaction => transaction.InventDimId)
            .Where(id => id != string.Empty)
            .Distinct()
            .ToList();
        var dimensions = await unitOfWork.Context.Set<InventDim>()
            .AsNoTracking()
            .Where(dimension => dimension.DataAreaId == dataAreaId && dimensionIds.Contains(dimension.InventDimId))
            .ToDictionaryAsync(dimension => dimension.InventDimId, cancellationToken);

        var salesInventTransIds = origins.Values
            .Where(origin => origin.ReferenceCategory == Common.InventRefType.SalesTable)
            .Select(origin => origin.InventTransId)
            .Distinct()
            .ToList();
        var salesPrices = await unitOfWork.Context.Set<SalesLine>()
            .AsNoTracking()
            .Where(line => line.DataAreaId == dataAreaId && salesInventTransIds.Contains(line.InventTransId))
            .GroupBy(line => line.InventTransId)
            .Select(group => new { InventTransId = group.Key, UnitPrice = group.Select(line => line.SalesPrice).FirstOrDefault() })
            .ToDictionaryAsync(row => row.InventTransId, row => row.UnitPrice, cancellationToken);

        var result = transactions.Select(transaction =>
        {
            origins.TryGetValue(transaction.InventTransOrigin, out var origin);
            dimensions.TryGetValue(transaction.InventDimId, out var dimension);
            var costAmount = transaction.CostAmountPosted
                + transaction.CostAmountPhysical
                + transaction.CostAmountAdjustment;
            var unitPrice = origin != null && salesPrices.TryGetValue(origin.InventTransId, out var salesPrice) ? salesPrice : 0;
            return new InventTransMappingSource(transaction, origin, dimension, unitPrice, costAmount).Adapt<InventTransListDto>();
        }).ToList();

        return result;
    }

}
