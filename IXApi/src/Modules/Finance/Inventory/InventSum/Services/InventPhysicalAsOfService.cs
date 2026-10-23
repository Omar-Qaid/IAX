using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Inventory;

/// <summary>
/// Reconstructs dated physical quantity from the signed transaction portions written by
/// the supported journal and sales-delivery posting paths. It does not read InventSum.
/// </summary>
public sealed class InventPhysicalAsOfService(
    IUnitOfWork unitOfWork, ICompanyExecutionContext company) : IInventPhysicalAsOfService
{
    public async Task<InventPhysicalAsOfPageDto?> SearchAsync(DateOnly asOfDate, string? itemId,
        string? siteId, string? warehouseId, int page, int pageSize, CancellationToken ct = default)
    {
        var companyId = company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(companyId)) return null;
        var cutoff = asOfDate.ToDateTime(TimeOnly.MinValue);
        var hasUpperBound = cutoff.Date < DateTime.MaxValue.Date;
        var exclusiveEnd = hasUpperBound ? cutoff.AddDays(1) : DateTime.MaxValue;
        itemId = itemId?.Trim();
        siteId = siteId?.Trim();
        warehouseId = warehouseId?.Trim();

        var transactions = unitOfWork.Context.Set<InventTrans>().AsNoTracking()
            .Where(x => x.DataAreaId == companyId && x.DatePhysical != default
                && (!hasUpperBound || x.DatePhysical < exclusiveEnd)
                && (itemId == null || itemId == "" || x.ItemId == itemId));
        if (!string.IsNullOrEmpty(siteId) || !string.IsNullOrEmpty(warehouseId))
        {
            var matchingDimensions = unitOfWork.Context.Set<InventDim>().AsNoTracking()
                .Where(x => x.DataAreaId == companyId
                    && (siteId == null || siteId == "" || x.InventSiteId == siteId)
                    && (warehouseId == null || warehouseId == "" || x.InventLocationId == warehouseId))
                .Select(x => x.InventDimId);
            transactions = transactions.Where(x => matchingDimensions.Contains(x.InventDimId));
        }

        var grouped = transactions.GroupBy(x => new { x.ItemId, x.InventDimId })
            .Select(g => new
            {
                g.Key.ItemId, g.Key.InventDimId,
                PhysicalQuantity = g.Sum(x => x.Qty),
                UnverifiedStateCount = g.Count(x =>
                    x.StatusReceipt != StatusReceipt.Received && x.StatusReceipt != StatusReceipt.Purchased
                    && x.StatusIssue != StatusIssue.Deducted && x.StatusIssue != StatusIssue.Sold)
            });
        var totalCount = await grouped.CountAsync(ct);
        var pageRows = await grouped.OrderBy(x => x.ItemId).ThenBy(x => x.InventDimId)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        var dimensionIds = pageRows.Select(x => x.InventDimId).Distinct().ToList();
        var dimensions = await unitOfWork.Context.Set<InventDim>().AsNoTracking()
            .Where(x => x.DataAreaId == companyId && dimensionIds.Contains(x.InventDimId))
            .Select(x => new { x.InventDimId, x.InventSiteId, x.InventLocationId })
            .ToDictionaryAsync(x => x.InventDimId, ct);
        var rows = pageRows.Select(x =>
        {
            dimensions.TryGetValue(x.InventDimId, out var dimension);
            return new InventPhysicalAsOfRowDto(x.ItemId, x.InventDimId,
                dimension?.InventSiteId ?? string.Empty, dimension?.InventLocationId ?? string.Empty,
                x.PhysicalQuantity, x.UnverifiedStateCount, dimension is not null);
        }).ToList();
        return new InventPhysicalAsOfPageDto(asOfDate, page, pageSize, totalCount, rows);
    }
}
