using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Persistence;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public enum SalesDiscountRecalculationStatus { NotFound, NotOpen, Recalculated }
public sealed record SalesDiscountRecalculationResult(SalesDiscountRecalculationStatus Status,
    int UpdatedLines = 0, decimal LineDiscount = 0, decimal MultiLineDiscount = 0, decimal TotalDiscountPercent = 0);

public sealed class SalesOrderDiscountService
{
    private readonly IFinanceDataContext _dbContext;
    private readonly ICompanyExecutionContext _company;

    public SalesOrderDiscountService(IFinanceDataContext dbContext, ICompanyExecutionContext company)
    {
        _dbContext = dbContext;
        _company = company;
    }

    public async Task<SalesDiscountRecalculationResult> RecalculateAsync(long recId, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Set<SalesTable>()
            .FirstOrDefaultAsync(row => row.RecId == recId, cancellationToken);
        if (order == null || !string.Equals(order.DataAreaId, _company.GetDataAreaId(), StringComparison.OrdinalIgnoreCase))
            return new SalesDiscountRecalculationResult(SalesDiscountRecalculationStatus.NotFound);
        if (order.SalesStatus != SalesStatus.Backorder)
            return new SalesDiscountRecalculationResult(SalesDiscountRecalculationStatus.NotOpen);

        var parameters = await _dbContext.Set<CustParameters>().AsNoTracking()
            .Where(row => row.DataAreaId == order.DataAreaId)
            .OrderBy(row => row.Key)
            .FirstOrDefaultAsync(cancellationToken);
        var lines = await _dbContext.Set<SalesLine>()
            .Where(line => line.SalesId == order.SalesId && line.DataAreaId == order.DataAreaId
                && line.SalesStatus == SalesStatus.Backorder)
            .ToListAsync(cancellationToken);
        var modules = await _dbContext.Set<InventTableModule>().AsNoTracking()
            .Where(module => module.DataAreaId == order.DataAreaId && module.ModuleType == ModuleInventPurchSales.Sales
                && lines.Select(line => line.ItemId).Contains(module.ItemId))
            .ToListAsync(cancellationToken);
        if (parameters?.PriceDiscSearchLineDisc != NoYes.No)
        {
            foreach (var line in lines)
            {
                var module = modules.FirstOrDefault(row => row.ItemId == line.ItemId);
                if (module == null) continue;
                var agreement = await FindSalesDiscountAsync(order, line.ItemId, module.LineDisc,
                    line.SalesQty, line.SalesUnit, line.InventDimId,
                    PriceType.LineDiscSales, order.LineDisc, cancellationToken);
                var gross = Math.Max(0m, line.LineAmount);
                if (agreement == null)
                {
                    line.LineDisc = 0m;
                    line.LinePercent = 0m;
                    continue;
                }

                var discountPercent = Math.Clamp(agreement.Percent1, 0m, 100m);
                var discount = discountPercent > 0m
                    ? gross * discountPercent / 100m
                    : Math.Min(gross, Math.Max(0m, agreement.Amount * line.SalesQty));
                line.LineDisc = decimal.Round(discount, 2, MidpointRounding.AwayFromZero);
                line.LinePercent = gross > 0m
                    ? decimal.Round(line.LineDisc / gross * 100m, 4, MidpointRounding.AwayFromZero)
                    : 0m;
            }
        }

        if (parameters?.PriceDiscSearchTotalDisc != NoYes.No)
        {
            var eligibleNet = lines.Where(line => modules.Any(module => module.ItemId == line.ItemId && module.EndDisc == NoYes.Yes))
                .Sum(line => Math.Max(0m, line.LineAmount - line.LineDisc - line.MultiLnDisc));
            var agreement = eligibleNet > 0m
                ? await FindSalesDiscountAsync(order, string.Empty, string.Empty, eligibleNet, string.Empty,
                    null, PriceType.EndDiscSales, order.EndDisc, cancellationToken)
                : null;
            order.DiscPercent = Math.Clamp(agreement?.Percent1 ?? 0m, 0m, 100m);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return new SalesDiscountRecalculationResult(SalesDiscountRecalculationStatus.Recalculated,
            lines.Count, lines.Sum(line => line.LineDisc), lines.Sum(line => line.MultiLnDisc), order.DiscPercent);
    }

    private async Task<PriceDiscTable?> FindSalesDiscountAsync(SalesTable order,
        string itemId, string itemGroup, decimal qualifier, string unitId, string? inventDimId,
        PriceType relation, string customerGroup, CancellationToken cancellationToken)
    {
        var pricingDate = order.OrderDate == default ? DateTime.UtcNow.Date : order.OrderDate.Date;
        var rows = await _dbContext.Set<PriceDiscTable>().AsNoTracking()
            .Where(row => row.DataAreaId == order.DataAreaId
                && row.Module == ModuleInventCustVend.Cust
                && row.Relation == relation
                && (row.Currency == order.CurrencyCode || row.GenericCurrency != 0)
                && (string.IsNullOrEmpty(unitId) || row.UnitId == unitId || row.UnitAppliesToAll != 0)
                && (string.IsNullOrEmpty(row.InventDimId) || row.InventDimId == inventDimId)
                && (row.FromDate == default || row.FromDate.Date <= pricingDate)
                && (row.ToDate == default || row.ToDate.Date >= pricingDate)
                && (row.QuantityAmountFrom <= 0 || row.QuantityAmountFrom <= qualifier)
                && (row.QuantityAmountTo <= 0 || row.QuantityAmountTo >= qualifier))
            .ToListAsync(cancellationToken);

        return rows
            .Where(row => row.AccountCode == PriceDiscPartyCodeType.All
                || (row.AccountCode == PriceDiscPartyCodeType.Table && row.AccountRelation == order.CustAccount)
                || (row.AccountCode == PriceDiscPartyCodeType.GroupId
                    && !string.IsNullOrWhiteSpace(customerGroup)
                    && row.AccountRelation == customerGroup))
            .Where(row => row.ItemCode == PriceDiscProductCodeType.All
                || (row.ItemCode == PriceDiscProductCodeType.Table && !string.IsNullOrWhiteSpace(itemId)
                    && row.ItemRelation == itemId)
                || (row.ItemCode == PriceDiscProductCodeType.GroupId
                    && !string.IsNullOrWhiteSpace(itemGroup)
                    && row.ItemRelation == itemGroup))
            .OrderByDescending(row => row.AccountCode == PriceDiscPartyCodeType.Table ? 3
                : row.AccountCode == PriceDiscPartyCodeType.GroupId ? 2 : 1)
            .ThenByDescending(row => row.ItemCode == PriceDiscProductCodeType.Table ? 3
                : row.ItemCode == PriceDiscProductCodeType.GroupId ? 2 : 1)
            .ThenByDescending(row => !string.IsNullOrEmpty(row.InventDimId))
            .ThenByDescending(row => row.QuantityAmountFrom)
            .ThenByDescending(row => row.FromDate)
            .FirstOrDefault();
    }

}
