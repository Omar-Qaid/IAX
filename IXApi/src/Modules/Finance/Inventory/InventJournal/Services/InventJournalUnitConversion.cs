using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Inventory;

internal static class InventJournalUnitConversion
{
    public static async Task<decimal?> ToInventoryQuantityAsync(DbContext db, long product,
        string fromSymbol, string inventorySymbol, decimal unitQuantity, CancellationToken ct)
    {
        if (string.Equals(fromSymbol, inventorySymbol, StringComparison.OrdinalIgnoreCase))
            return unitQuantity;

        var symbols = new[] { fromSymbol.ToUpperInvariant(), inventorySymbol.ToUpperInvariant() };
        var units = await db.Set<UnitOfMeasure>().IgnoreQueryFilters().AsNoTracking()
            .Where(x => !x.IsDeleted && symbols.Contains(x.Symbol.ToUpper()))
            .ToListAsync(ct);
        var from = units.FirstOrDefault(x => string.Equals(x.Symbol, fromSymbol, StringComparison.OrdinalIgnoreCase));
        var to = units.FirstOrDefault(x => string.Equals(x.Symbol, inventorySymbol, StringComparison.OrdinalIgnoreCase));
        if (from == null || to == null) return null;

        var rule = await db.Set<UnitOfMeasureConversion>().AsNoTracking()
            .Where(x => (x.Product == product || x.Product == 0) &&
                ((x.FromUnitOfMeasure == from.RecId && x.ToUnitOfMeasure == to.RecId) ||
                 (x.FromUnitOfMeasure == to.RecId && x.ToUnitOfMeasure == from.RecId)))
            .OrderByDescending(x => x.Product == product)
            .ThenByDescending(x => x.FromUnitOfMeasure == from.RecId)
            .ThenByDescending(x => x.RecId)
            .FirstOrDefaultAsync(ct);
        if (rule == null || rule.InnerOffset != 0 || rule.OuterOffset != 0) return null;

        var factor = rule.Factor > 0 ? rule.Factor :
            rule.Numerator > 0 && rule.Denominator > 0
                ? (decimal)rule.Numerator / rule.Denominator : 0m;
        if (factor <= 0) return null;

        var converted = rule.FromUnitOfMeasure == from.RecId
            ? unitQuantity * factor : unitQuantity / factor;
        var precision = Math.Clamp(to.DecimalPrecision, 0, 28);
        var scale = (decimal)Math.Pow(10, precision);
        return rule.Rounding switch
        {
            UnitOfMeasureRounding.Up => Math.Ceiling(converted * scale) / scale,
            UnitOfMeasureRounding.Down => Math.Floor(converted * scale) / scale,
            _ => Math.Round(converted, precision, MidpointRounding.AwayFromZero)
        };
    }
}
