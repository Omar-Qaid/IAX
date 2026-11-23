using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;

/// <summary>Units of measure, the base unit of each unit class, and the standard conversions into those base units.</summary>
public sealed class BaselineUnitOfMeasureSeeder : IBaselineSeeder
{
    public const string SeederId = "Finance.Inventory.UnitOfMeasure";
    public string Id => SeederId;
    public IReadOnlyCollection<string> Dependencies => [];

    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var catalog = InventoryBaselineCatalog.Load();
        var units = new Dictionary<string, long>(StringComparer.Ordinal);
        foreach (var symbol in catalog.Units)
            units[symbol] = (await c.AddMissingAsync<UnitOfMeasure>(x => x.Symbol == symbol, new() { Symbol = symbol }, ct)).RecId;
        foreach (var baseUnit in catalog.BaseUnits)
            await c.AddMissingAsync<UnitOfMeasureBaseUnit>(x => x.UnitOfMeasure == units[baseUnit.Unit] && x.UnitOfMeasureClass == baseUnit.Class,
                new() { UnitOfMeasure = units[baseUnit.Unit], UnitOfMeasureClass = baseUnit.Class }, ct);
        foreach (var conversion in catalog.Conversions)
            await c.AddMissingAsync<UnitOfMeasureConversion>(x => x.FromUnitOfMeasure == units[conversion.From] && x.ToUnitOfMeasure == units[conversion.To], new()
            {
                FromUnitOfMeasure = units[conversion.From], ToUnitOfMeasure = units[conversion.To], Factor = conversion.Factor,
                Numerator = 1, Denominator = 1, InnerOffset = 0, OuterOffset = 0, Rounding = UnitOfMeasureRounding.Nearest
            }, ct);
    }
}
