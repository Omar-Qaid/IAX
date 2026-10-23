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

        var transactions = await BuildQuery(dataAreaId, itemId, sourceInventTransId, siteId, warehouseId)
            .OrderByDescending(transaction => transaction.DateStatus)
            .ThenByDescending(transaction => transaction.RecId)
            .ToListAsync(cancellationToken);
        return await MapAsync(dataAreaId, transactions, cancellationToken);
    }

    public async Task<InventTransPageDto?> SearchPageAsync(string? itemId, string? sourceInventTransId,
        string? siteId, string? warehouseId, DateTime? physicalFrom, DateTime? physicalTo,
        DateTime? financialFrom, DateTime? financialTo, int page, int pageSize,
        CancellationToken cancellationToken = default)
    {
        var dataAreaId = company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(dataAreaId)) return null;
        var query = BuildQuery(dataAreaId, itemId, sourceInventTransId, siteId, warehouseId);
        if (physicalFrom.HasValue)
            query = query.Where(transaction => transaction.DatePhysical >= physicalFrom.Value.Date);
        if (physicalTo.HasValue)
        {
            if (physicalTo.Value.Date < DateTime.MaxValue.Date)
            {
                var end = physicalTo.Value.Date.AddDays(1);
                query = query.Where(transaction => transaction.DatePhysical < end && transaction.DatePhysical != default);
            }
            else query = query.Where(transaction => transaction.DatePhysical != default);
        }
        if (financialFrom.HasValue)
            query = query.Where(transaction => transaction.DateFinancial >= financialFrom.Value.Date);
        if (financialTo.HasValue)
        {
            if (financialTo.Value.Date < DateTime.MaxValue.Date)
            {
                var end = financialTo.Value.Date.AddDays(1);
                query = query.Where(transaction => transaction.DateFinancial < end && transaction.DateFinancial != default);
            }
            else query = query.Where(transaction => transaction.DateFinancial != default);
        }
        var totalCount = await query.CountAsync(cancellationToken);
        var transactions = await query.OrderByDescending(transaction => transaction.DateStatus)
            .ThenByDescending(transaction => transaction.RecId)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        var rows = await MapAsync(dataAreaId, transactions, cancellationToken);
        return new InventTransPageDto(page, pageSize, totalCount, rows);
    }

    private IQueryable<InventTrans> BuildQuery(string dataAreaId, string? itemId,
        string? sourceInventTransId, string? siteId, string? warehouseId)
    {

        var query = unitOfWork.Context.Set<InventTrans>()
            .AsNoTracking()
            .Where(transaction => transaction.DataAreaId == dataAreaId);
        if (!string.IsNullOrWhiteSpace(itemId))
            query = query.Where(transaction => transaction.ItemId == itemId.Trim());
        if (!string.IsNullOrWhiteSpace(sourceInventTransId))
        {
            var sourceId = sourceInventTransId.Trim();
            var matchingOrigins = unitOfWork.Context.Set<InventTransOrigin>().AsNoTracking()
                .Where(origin => origin.DataAreaId == dataAreaId && origin.InventTransId == sourceId)
                .Select(origin => origin.RecId);
            query = query.Where(transaction => matchingOrigins.Contains(transaction.InventTransOrigin));
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
        return query;
    }

    private async Task<List<InventTransListDto>> MapAsync(string dataAreaId,
        List<InventTrans> transactions, CancellationToken cancellationToken)
    {
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
