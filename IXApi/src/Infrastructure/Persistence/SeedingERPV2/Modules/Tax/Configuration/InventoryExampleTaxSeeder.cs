using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Tax;

/// <summary>
/// The inventory cycle's zero-rate item tax group. It holds the shared zero-rated code the Tax module
/// owns (<see cref="TaxSeedCatalog.ZeroRated"/>), which both the vendor and the customer tax groups
/// carry, instead of defining a second zero-rated code of its own.
/// </summary>
public sealed class InventoryExampleTaxSeeder : IDataSeeder
{
    public string Id => "ERPV2.InventoryExampleTaxSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.TaxVatCategorySeeder", "ERPV2.SalesTaxSeeder"];

    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        await c.RequireAsync<TaxTable>(x => x.TaxCode == TaxSeedCatalog.ZeroRated, ct);
        await c.EnsureAsync<TaxItemGroupHeading>(x => x.TaxItemGroup == TaxSeedCatalog.InventoryZeroItemGroup,
            new() { TaxItemGroup = TaxSeedCatalog.InventoryZeroItemGroup, Name = "Inventory reference zero rate" }, ct);
        await c.EnsureAsync<TaxOnItem>(x => x.TaxItemGroup == TaxSeedCatalog.InventoryZeroItemGroup && x.TaxCode == TaxSeedCatalog.ZeroRated,
            new() { TaxItemGroup = TaxSeedCatalog.InventoryZeroItemGroup, TaxCode = TaxSeedCatalog.ZeroRated }, ct);
    }
}
