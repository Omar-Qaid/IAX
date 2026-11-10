using System.Text.Json;
using System.Text.Json.Serialization;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory.Reports;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory.Data;

internal sealed record ItemDefinition(string Suffix, string Name, string ProductType, string Unit,
    string ModelGroup, string ItemGroup, string ItemTaxGroup, bool Stocked, decimal PurchasePrice, decimal SalesPrice);

internal sealed record PostingProfileDefinition(string AccountType, string MainAccount);

internal sealed record LedgerAccountDefinition(string Inventory, string CostOfGoodsSold);

internal sealed record InventorySetupDefinition(string CostingMethod, string Site, string Warehouse,
    string InventDimId, string Unit, string ItemGroup, PostingProfileDefinition[] PostingProfiles,
    LedgerAccountDefinition LedgerAccounts);

internal sealed record PreparationDefinition(string Scope, bool TransactionScenariosEnabled,
    InventoryPreparationStep[] Steps);

/// <summary>
/// Read-only descriptions of the module-owned Inventory fixtures. These files never insert a row: each manifest is
/// checked against the constants of the seeder that owns the record, so a manifest can never certify setup that
/// does not exist, and a changed fixture fails here instead of surfacing as a wrong closing value later.
/// </summary>
internal sealed class InventoryExampleCatalog
{
    public ItemDefinition[] Items { get; } = Read<ItemDefinition[]>("items.json");
    public InventorySetupDefinition Setup { get; } = Read<InventorySetupDefinition>("inventory-setup.json");
    public PreparationDefinition Preparation { get; } = Read<PreparationDefinition>("scenarios.json");

    public void Validate()
    {
        static void Require(bool value, string message)
        {
            if (!value) throw new InvalidOperationException("Inventory master-data manifest: " + message);
        }

        Require(Preparation.Scope == ExampleIdentifiers.Scope && !Preparation.TransactionScenariosEnabled,
            "preparation covers configuration and master data only.");
        Require(Preparation.Steps is { Length: > 0 }
            && Preparation.Steps.Select(x => x.Id).Distinct(StringComparer.Ordinal).Count() == Preparation.Steps.Length
            && Preparation.Steps.Select(x => x.Order).SequenceEqual(Enumerable.Range(1, Preparation.Steps.Length))
            && Preparation.Steps.All(x => !string.IsNullOrWhiteSpace(x.Id) && !string.IsNullOrWhiteSpace(x.Description)),
            "preparation steps are invalid.");

        // Periodic weighted average is the whole point of the reference cycle: a second receipt arriving after the
        // sales still lifts the period average, so declaring any other method here would invalidate every figure.
        Require(Setup.CostingMethod == nameof(InventModel.WeightedAvg), "the costing method must be weighted average.");
        Require(Setup.Site == ExampleIdentifiers.Site && Setup.Warehouse == ExampleIdentifiers.Warehouse
            && Setup.InventDimId == ExampleIdentifiers.Dimension && Setup.Unit == InventoryCycleSeedExample.Unit
            && Setup.ItemGroup == InventoryCycleSeedExample.ItemGroup, "the setup keys differ from the owning seeders.");
        Require(Setup.LedgerAccounts.Inventory == GeneralLedgerSeedExample.Inventory
            && Setup.LedgerAccounts.CostOfGoodsSold == GeneralLedgerSeedExample.CostOfGoodsSold,
            "the reconciliation accounts differ from the seeded demo chart.");

        var profiles = InventoryCycleSeedExample.PostingProfiles;
        Require(Setup.PostingProfiles.Length == profiles.Count, "the inventory posting profile count differs.");
        foreach (var (accountType, mainAccount) in profiles)
            Require(Setup.PostingProfiles.Count(x => x.AccountType == accountType.ToString() && x.MainAccount == mainAccount) == 1,
                $"posting profile {accountType} differs from InventPostingSeeder.");

        Require(Items.Length == InventoryCycleSeedExample.ItemSuffixes.Count && Items.Select(x => x.Suffix).Order(StringComparer.Ordinal)
                .SequenceEqual(InventoryCycleSeedExample.ItemSuffixes
                    .Order(StringComparer.Ordinal), StringComparer.Ordinal), "the item keys differ.");
        foreach (var item in Items)
        {
            var service = item.Suffix == InventoryCycleSeedExample.ServiceSuffix;
            Require(item.Name == InventoryCycleSeedExample.Name(item.Suffix)
                && item.ProductType == InventoryCycleSeedExample.ProductType(item.Suffix).ToString()
                && item.Unit == InventoryCycleSeedExample.Unit
                && item.ModelGroup == InventoryCycleSeedExample.ModelGroup(item.Suffix)
                && item.ItemGroup == InventoryCycleSeedExample.ItemGroup
                && item.ItemTaxGroup == InventoryCycleSeedExample.ItemTaxGroup
                && item.Stocked == !service
                && item.PurchasePrice == InventoryCycleSeedExample.Price(item.Suffix, ModuleInventPurchSales.Purchase)
                && item.SalesPrice == InventoryCycleSeedExample.Price(item.Suffix, ModuleInventPurchSales.Sales),
                $"item {item.Suffix} differs from its owning seeder.");
        }
    }

    private static T Read<T>(string file)
    {
        using var stream = typeof(InventoryExampleCatalog).Assembly
            .GetManifestResourceStream("SeedingERPV2.Inventory." + file)
            ?? throw new InvalidOperationException("Missing embedded Inventory master-data manifest: " + file);
        return JsonSerializer.Deserialize<T>(stream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true, UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
        }) ?? throw new InvalidOperationException("Empty Inventory master-data manifest: " + file);
    }
}
