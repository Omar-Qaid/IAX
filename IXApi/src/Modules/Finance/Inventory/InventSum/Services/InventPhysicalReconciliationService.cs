using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Inventory;

/// <summary>Diagnostic for the physical states implemented by the current journal and delivery writers.</summary>
public sealed class InventPhysicalReconciliationService(
    IUnitOfWork unitOfWork, ICompanyExecutionContext company) : IInventPhysicalReconciliationService
{
    public async Task<InventPhysicalReconciliationDto?> CheckAsync(string? itemId, string? siteId,
        string? warehouseId, CancellationToken ct = default)
    {
        var companyId = company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(companyId)) return null;
        itemId = itemId?.Trim();
        siteId = siteId?.Trim();
        warehouseId = warehouseId?.Trim();

        var dimensions = await unitOfWork.Context.Set<InventDim>().AsNoTracking()
            .Where(x => x.DataAreaId == companyId
                && (siteId == null || siteId == "" || x.InventSiteId == siteId)
                && (warehouseId == null || warehouseId == "" || x.InventLocationId == warehouseId))
            .Select(x => new { x.InventDimId, x.InventSiteId, x.InventLocationId })
            .ToListAsync(ct);
        var dimensionById = dimensions.ToDictionary(x => x.InventDimId);
        var dimensionIds = dimensions.Select(x => x.InventDimId).ToList();
        var limitByDimension = !string.IsNullOrEmpty(siteId) || !string.IsNullOrEmpty(warehouseId);

        var transactionGroups = await unitOfWork.Context.Set<InventTrans>().AsNoTracking()
            .Where(x => x.DataAreaId == companyId && (!limitByDimension || dimensionIds.Contains(x.InventDimId))
                && (itemId == null || itemId == "" || x.ItemId == itemId))
            .GroupBy(x => new { x.ItemId, x.InventDimId })
            .Select(g => new
            {
                g.Key.ItemId, g.Key.InventDimId,
                Physical = g.Sum(x => x.StatusReceipt == StatusReceipt.Received
                    || x.StatusReceipt == StatusReceipt.Purchased
                    || x.StatusIssue == StatusIssue.Deducted
                    || x.StatusIssue == StatusIssue.Sold ? x.Qty : 0m),
                Unclassified = g.Count(x =>
                    (x.StatusReceipt != StatusReceipt.None && x.StatusReceipt != StatusReceipt.Ordered
                        && x.StatusReceipt != StatusReceipt.Received && x.StatusReceipt != StatusReceipt.Purchased)
                    || (x.StatusIssue != StatusIssue.None && x.StatusIssue != StatusIssue.Quotation
                        && x.StatusIssue != StatusIssue.Ordered && x.StatusIssue != StatusIssue.Deducted
                        && x.StatusIssue != StatusIssue.Sold)
                    || (x.Qty > 0 && x.StatusReceipt == StatusReceipt.None)
                    || (x.Qty < 0 && x.StatusIssue == StatusIssue.None))
            }).ToListAsync(ct);

        var summaryGroups = await unitOfWork.Context.Set<InventSum>().AsNoTracking()
            .Where(x => x.DataAreaId == companyId && (!limitByDimension || dimensionIds.Contains(x.InventDimId))
                && (itemId == null || itemId == "" || x.ItemId == itemId))
            .GroupBy(x => new { x.ItemId, x.InventDimId })
            .Select(g => new { g.Key.ItemId, g.Key.InventDimId, Physical = g.Sum(x => x.PhysicalInvent) })
            .ToListAsync(ct);

        var transactionByKey = transactionGroups.ToDictionary(x => (x.ItemId, x.InventDimId));
        var summaryByKey = summaryGroups.ToDictionary(x => (x.ItemId, x.InventDimId));
        var keys = transactionByKey.Keys.Union(summaryByKey.Keys)
            .OrderBy(x => x.ItemId).ThenBy(x => x.InventDimId).ToList();
        var rows = new List<InventPhysicalReconciliationRowDto>(keys.Count);
        foreach (var key in keys)
        {
            transactionByKey.TryGetValue(key, out var transaction);
            summaryByKey.TryGetValue(key, out var summary);
            dimensionById.TryGetValue(key.InventDimId, out var dimension);
            var transactionPhysical = transaction?.Physical ?? 0m;
            var summaryPhysical = summary?.Physical ?? 0m;
            rows.Add(new InventPhysicalReconciliationRowDto(key.ItemId, key.InventDimId,
                dimension?.InventSiteId ?? "", dimension?.InventLocationId ?? "",
                transactionPhysical, summaryPhysical, summaryPhysical - transactionPhysical,
                transaction?.Unclassified ?? 0));
        }
        var unclassifiedCount = rows.Sum(x => x.UnclassifiedTransactionCount);
        var differenceCount = rows.Count(x => x.Difference != 0);
        var missingDimensionCount = rows.Count(x => !dimensionById.ContainsKey(x.InventDimId));
        return new InventPhysicalReconciliationDto(companyId, rows.Count,
            differenceCount, unclassifiedCount, missingDimensionCount,
            differenceCount == 0 && unclassifiedCount == 0 && missingDimensionCount == 0,
            rows.Where(x => x.Difference != 0 || x.UnclassifiedTransactionCount != 0
                || !dimensionById.ContainsKey(x.InventDimId)).ToList());
    }
}
