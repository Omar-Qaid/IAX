using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public sealed class SalesUnitConversionService
{
    private readonly IFinanceDataContext _db;

    public SalesUnitConversionService(IFinanceDataContext db) => _db = db;

    public async Task<decimal?> ToInventoryQuantityAsync(InventTable item, string salesUnit,
        decimal salesQuantity, CancellationToken cancellationToken)
    {
        var inventoryUnit = await _db.Set<InventTableModule>().AsNoTracking()
            .Where(row => row.ItemId == item.ItemId && row.DataAreaId == item.DataAreaId
                && row.ModuleType == IAX.IXApi.Modules.Finance.Common.ModuleInventPurchSales.Inventory)
            .Select(row => row.UnitId)
            .FirstOrDefaultAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(inventoryUnit)) return null;
        if (string.Equals(salesUnit, inventoryUnit, StringComparison.OrdinalIgnoreCase))
            return salesQuantity;

        var symbols = new[] { salesUnit.ToUpperInvariant(), inventoryUnit.ToUpperInvariant() };
        var units = await _db.Set<UnitOfMeasure>().AsNoTracking()
            .Where(row => symbols.Contains(row.Symbol.ToUpper()))
            .ToListAsync(cancellationToken);
        var from = units.FirstOrDefault(row => string.Equals(row.Symbol, salesUnit, StringComparison.OrdinalIgnoreCase));
        var to = units.FirstOrDefault(row => string.Equals(row.Symbol, inventoryUnit, StringComparison.OrdinalIgnoreCase));
        if (from == null || to == null) return null;

        var rules = await _db.Set<UnitOfMeasureConversion>().AsNoTracking()
            .Where(row => (row.Product == item.Product || row.Product == 0)
                && ((row.FromUnitOfMeasure == from.RecId && row.ToUnitOfMeasure == to.RecId)
                    || (row.FromUnitOfMeasure == to.RecId && row.ToUnitOfMeasure == from.RecId)))
            .ToListAsync(cancellationToken);
        var rule = rules.OrderByDescending(row => row.Product == item.Product)
            .ThenByDescending(row => row.FromUnitOfMeasure == from.RecId)
            .FirstOrDefault();
        if (rule == null) return null;

        var factor = rule.Factor > 0 ? rule.Factor :
            rule.Numerator > 0 && rule.Denominator > 0
                ? (decimal)rule.Numerator / rule.Denominator : 0m;
        if (factor <= 0 || rule.InnerOffset != 0 || rule.OuterOffset != 0) return null;
        var converted = rule.FromUnitOfMeasure == from.RecId
            ? salesQuantity * factor : salesQuantity / factor;
        var precision = Math.Clamp(to.DecimalPrecision, 0, 28);
        return decimal.Round(converted, precision, MidpointRounding.AwayFromZero);
    }
}
