using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;
using ReleasedItem = IAX.IXApi.Modules.Finance.Entities.InventTable;

namespace IAX.IXApi.Modules.Finance.Inventory;

/// <summary>
/// The inventory value report: dated quantity and value per item and inventory dimension, built
/// from inventory transactions rather than from the on-hand summary, with the summary reported
/// alongside so drift between the two is visible. Value is reported on both sides: the financial
/// value of invoiced stock and the physical value of stock that is received or delivered but not
/// yet invoiced, each compared against its own on-hand bucket so drift is attributable.
///
/// It also totals the ledger amounts posted under each inventory posting type up to the same date.
/// Those buckets are reported individually, because what belongs in "inventory on the balance
/// sheet" depends on which accounts the posting profiles point at - the report shows the
/// composition and the difference, and leaves the judgement to the reader.
/// </summary>
public sealed class InventValueReportService(
    IUnitOfWork unitOfWork, ICompanyExecutionContext company) : IInventValueReportService
{
    private DbContext Db => unitOfWork.Context;

    /// <summary>The posting types this repository's inventory writers use for the inventory side of a posting.</summary>
    private static readonly LedgerPostingType[] InventoryPostingTypes =
    [
        LedgerPostingType.InventReceipt,
        LedgerPostingType.InventIssue,
        LedgerPostingType.SalesIssue,
        LedgerPostingType.SalesPackingSlip,
        LedgerPostingType.PurchReceipt
    ];

    public async Task<InventValueReportDto?> RunAsync(DateOnly asOfDate, string? itemId, string? itemGroupId,
        string? siteId, string? warehouseId, bool includeZero, int page, int pageSize, CancellationToken ct = default)
    {
        var companyId = company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(companyId)) return null;
        itemId = itemId?.Trim();
        itemGroupId = itemGroupId?.Trim();
        siteId = siteId?.Trim();
        warehouseId = warehouseId?.Trim();
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 50;
        var exclusiveEnd = asOfDate.ToDateTime(TimeOnly.MinValue).AddDays(1);

        var itemGroups = (await Db.Set<InventItemGroupItem>().AsNoTracking()
            .Where(x => x.ItemDataAreaId == companyId).Select(x => new { x.ItemId, x.ItemGroupId })
            .ToListAsync(ct))
            .GroupBy(x => x.ItemId).ToDictionary(x => x.Key, x => x.First().ItemGroupId);
        var scopedItems = itemGroupId is null or ""
            ? null
            : itemGroups.Where(x => x.Value == itemGroupId).Select(x => x.Key).ToHashSet(StringComparer.Ordinal);

        var dimensions = await Db.Set<InventDim>().AsNoTracking()
            .Where(x => x.DataAreaId == companyId
                && (siteId == null || siteId == "" || x.InventSiteId == siteId)
                && (warehouseId == null || warehouseId == "" || x.InventLocationId == warehouseId))
            .Select(x => new { x.InventDimId, x.InventSiteId, x.InventLocationId })
            .ToListAsync(ct);
        var dimensionById = dimensions.ToDictionary(x => x.InventDimId);
        var limitByDimension = !string.IsNullOrEmpty(siteId) || !string.IsNullOrEmpty(warehouseId);
        var dimensionIds = dimensions.Select(x => x.InventDimId).ToList();

        // Physical quantity follows the physical date; financial quantity and value follow the
        // financial date, because a delivery and its invoice need not fall on the same day.
        var transactions = await Db.Set<InventTrans>().AsNoTracking()
            .Where(x => x.DataAreaId == companyId
                && (itemId == null || itemId == "" || x.ItemId == itemId)
                && (!limitByDimension || dimensionIds.Contains(x.InventDimId)))
            .GroupBy(x => new { x.ItemId, x.InventDimId })
            .Select(g => new
            {
                g.Key.ItemId,
                g.Key.InventDimId,
                PhysicalQuantity = g.Sum(x => x.DatePhysical != default && x.DatePhysical < exclusiveEnd
                    && (x.StatusReceipt == StatusReceipt.Received || x.StatusReceipt == StatusReceipt.Purchased
                        || x.StatusIssue == StatusIssue.Deducted || x.StatusIssue == StatusIssue.Sold)
                    ? x.Qty : 0m),
                FinancialQuantity = g.Sum(x => x.DateFinancial != default && x.DateFinancial < exclusiveEnd
                    && (x.StatusReceipt == StatusReceipt.Purchased || x.StatusIssue == StatusIssue.Sold)
                    ? x.Qty : 0m),
                FinancialValue = g.Sum(x => x.DateFinancial != default && x.DateFinancial < exclusiveEnd
                    && (x.StatusReceipt == StatusReceipt.Purchased || x.StatusIssue == StatusIssue.Sold)
                    ? x.CostAmountPosted + x.CostAmountAdjustment : 0m),
                // The value of stock that is physically but not financially updated: a product
                // receipt not yet invoiced, or a packing slip not yet invoiced. The invoice moves
                // the transaction to Purchased/Sold and clears its physical cost, so a transaction
                // is only ever counted on one of the two sides.
                PhysicalValue = g.Sum(x => x.DatePhysical != default && x.DatePhysical < exclusiveEnd
                    && (x.StatusReceipt == StatusReceipt.Received || x.StatusIssue == StatusIssue.Deducted)
                    ? x.CostAmountPhysical : 0m)
            }).ToListAsync(ct);

        var summaries = await Db.Set<InventSum>().AsNoTracking()
            .Where(x => x.DataAreaId == companyId
                && (itemId == null || itemId == "" || x.ItemId == itemId)
                && (!limitByDimension || dimensionIds.Contains(x.InventDimId)))
            .Select(x => new { x.ItemId, x.InventDimId, x.PhysicalInvent, x.PostedValue, x.PhysicalValue })
            .ToListAsync(ct);

        var items = await Db.Set<ReleasedItem>().AsNoTracking()
            .Where(x => x.DataAreaId == companyId).Select(x => new { x.ItemId, x.Name })
            .ToDictionaryAsync(x => x.ItemId, x => x.Name, ct);
        var units = await Db.Set<InventTableModule>().AsNoTracking()
            .Where(x => x.DataAreaId == companyId && x.ModuleType == ModuleInventPurchSales.Inventory)
            .Select(x => new { x.ItemId, x.UnitId }).ToDictionaryAsync(x => x.ItemId, x => x.UnitId, ct);

        var transactionByKey = transactions.ToDictionary(x => (x.ItemId, x.InventDimId));
        var summaryByKey = summaries.GroupBy(x => (x.ItemId, x.InventDimId))
            .ToDictionary(x => x.Key, x => (
                Quantity: x.Sum(y => y.PhysicalInvent),
                PostedValue: x.Sum(y => y.PostedValue),
                PhysicalValue: x.Sum(y => y.PhysicalValue)));

        var rows = new List<InventValueReportRowDto>();
        foreach (var key in transactionByKey.Keys.Union(summaryByKey.Keys)
            .OrderBy(x => x.ItemId, StringComparer.Ordinal).ThenBy(x => x.InventDimId, StringComparer.Ordinal))
        {
            if (scopedItems != null && !scopedItems.Contains(key.ItemId)) continue;
            transactionByKey.TryGetValue(key, out var transaction);
            summaryByKey.TryGetValue(key, out var summary);
            dimensionById.TryGetValue(key.InventDimId, out var dimension);

            var physicalQuantity = transaction?.PhysicalQuantity ?? 0m;
            var financialQuantity = transaction?.FinancialQuantity ?? 0m;
            var financialValue = transaction?.FinancialValue ?? 0m;
            var physicalValue = transaction?.PhysicalValue ?? 0m;
            if (!includeZero && physicalQuantity == 0m && financialQuantity == 0m && financialValue == 0m
                && physicalValue == 0m && summary.Quantity == 0m
                && summary.PostedValue == 0m && summary.PhysicalValue == 0m)
                continue;

            rows.Add(new InventValueReportRowDto(
                key.ItemId,
                items.GetValueOrDefault(key.ItemId, string.Empty),
                itemGroups.GetValueOrDefault(key.ItemId, string.Empty),
                key.InventDimId,
                dimension?.InventSiteId ?? string.Empty,
                dimension?.InventLocationId ?? string.Empty,
                units.GetValueOrDefault(key.ItemId, string.Empty),
                physicalQuantity,
                physicalValue,
                financialQuantity,
                financialValue,
                financialQuantity == 0m ? 0m : decimal.Round(financialValue / financialQuantity, 4, MidpointRounding.AwayFromZero),
                summary.Quantity,
                summary.PostedValue,
                summary.PhysicalValue,
                summary.PostedValue + summary.PhysicalValue,
                summary.Quantity - physicalQuantity,
                summary.PostedValue - financialValue,
                summary.PhysicalValue - physicalValue));
        }

        var buckets = await (
            from line in Db.Set<GeneralJournalAccountEntry>().AsNoTracking()
            join entry in Db.Set<GeneralJournalEntry>().AsNoTracking()
                on line.GeneralJournalEntry equals entry.RecId
            where line.DataAreaId == companyId && entry.DataAreaId == companyId
                && entry.AccountingDate < exclusiveEnd
                && InventoryPostingTypes.Contains(line.PostingType)
            group line by line.PostingType into g
            select new InventValueLedgerBucketDto(g.Key, g.Sum(x => x.AccountingCurrencyAmount)))
            .ToListAsync(ct);

        var totalFinancialValue = rows.Sum(x => x.FinancialValue);
        var ledgerAmount = buckets.Sum(x => x.AccountingCurrencyAmount);
        return new InventValueReportDto(asOfDate, page, pageSize, rows.Count,
            rows.Sum(x => x.PhysicalValue),
            totalFinancialValue, rows.Sum(x => x.SummaryValue), ledgerAmount,
            ledgerAmount - totalFinancialValue,
            buckets.OrderBy(x => x.PostingType).ToList(),
            rows.Skip((page - 1) * pageSize).Take(pageSize).ToList());
    }
}
