using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory.Reports;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using IAX.IXApi.Modules.Finance.Inventory.InventTable;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory.Validation;

/// <summary>
/// Validates the Inventory fixtures and captures their references inside the seed transaction. It makes no
/// posted-balance assertion: a scenario's quantities and values are checked afterwards by
/// <see cref="InventoryReconciliationSnapshot"/>, which can only read rows a posting service wrote.
/// </summary>
public sealed class InventoryCycleValidator : ISeedValidator
{
    private readonly InventoryExampleCatalog catalog = new();
    private InventoryReconciliationReport? prepared;

    internal IReadOnlyList<InventoryPreparationStep> PreparationSteps => catalog.Preparation.Steps;

    /// <summary>Rejects a stale or malformed manifest before the seed runner writes anything.</summary>
    internal void ValidateDefinition()
    {
        prepared = null;
        catalog.Validate();
    }

    internal InventoryReconciliationReport TakePreparedReport()
    {
        var result = prepared ?? throw new InvalidOperationException("Inventory configuration/master data has not been validated.");
        prepared = null;
        return result;
    }

    public async Task ValidateAsync(SeedContext c, CancellationToken ct)
    {
        catalog.Validate();
        prepared = null;
        static void Check(bool valid, string message)
        {
            if (!valid) throw new InvalidOperationException("Inventory cycle master-data manifest: " + message);
        }

        var ledger = await c.RequireAsync<Ledger>(x => x.Name == "APV2", ct);
        var parameters = await c.RequireAsync<InventParameters>(x => x.DataAreaId == c.Company, ct);
        var warehouse = await c.RequireAsync<InventLocation>(x => x.InventLocationId == ExampleIdentifiers.Warehouse, ct);
        var dimension = await c.RequireAsync<InventDim>(x => x.InventDimId == ExampleIdentifiers.Dimension, ct);
        Check(warehouse.InventSiteId == ExampleIdentifiers.Site && dimension.InventSiteId == ExampleIdentifiers.Site
            && dimension.InventLocationId == warehouse.InventLocationId,
            "the default inventory dimension does not point at the example site and warehouse.");

        // Each selected journal name must exist and draw an allocatable voucher sequence, or the
        // journal services refuse to create or post. The selection alone proves nothing.
        foreach (var (read, type) in new (string Selected, InventJournalType Type)[]
        {
            (parameters.MovementJournalNameId, InventJournalType.Movement),
            (parameters.LossProfitJournalNameId, InventJournalType.Adjustment),
            (parameters.TransferJournalNameId, InventJournalType.Transfer),
            (parameters.CountJournalNameId, InventJournalType.Counting)
        })
        {
            Check(read == InventJournalSeedExample.Name(type),
                $"the {type} journal selection on InventParameters differs from the owning seeder.");
            var journalName = await c.RequireAsync<InventJournalName>(x => x.JournalNameId == read, ct);
            Check(journalName.JournalType == type && journalName.VoucherNumberSequenceTable != 0,
                $"journal name {read} is not a {type} name with a voucher sequence.");
        }

        var profiles = new List<InventoryPostingProfileReference>();
        foreach (var (accountType, mainAccount) in InventoryCycleSeedExample.PostingProfiles)
        {
            var account = await c.RequireAsync<DimensionAttributeValueCombination>(x => x.MainAccountValue == mainAccount, ct);
            var profile = await c.RequireAsync<InventPosting>(x => x.InventAccountType == accountType
                && x.ItemCode == TableGroupAll.Group && x.ItemRelation == InventoryCycleSeedExample.ItemGroup
                && x.CustVendCode == TableGroupAll.All, ct);
            Check(profile.LedgerDimension == account.RecId,
                $"posting profile {accountType} does not resolve to main account {mainAccount}.");
            profiles.Add(new(profile.RecId, accountType.ToString(), profile.ItemRelation, mainAccount, profile.LedgerDimension));
        }

        var items = new List<InventoryItemReference>();
        foreach (var definition in catalog.Items)
        {
            var service = !definition.Stocked;
            var itemId = InventoryCycleMasterSeeder.Item(c.FiscalYear, definition.Suffix);
            var item = await c.RequireAsync<InventTable>(x => x.ItemId == itemId, ct);
            var product = await c.RequireAsync<EcoResProduct>(x => x.RecId == item.Product, ct);
            var assignment = await c.RequireAsync<InventModelGroupItem>(x => x.ItemId == itemId && x.ItemDataAreaId == c.Company, ct);
            var modelGroup = await c.RequireAsync<InventModelGroup>(x => x.ModelGroupId == assignment.ModelGroupId, ct);
            var group = await c.RequireAsync<InventItemGroupItem>(x => x.ItemId == itemId && x.ItemDataAreaId == c.Company, ct);

            Check(item.Name == definition.Name && item.ItemType.ToString() == definition.ProductType
                && assignment.ModelGroupId == definition.ModelGroup && group.ItemGroupId == definition.ItemGroup,
                $"persisted item {itemId} differs from items.json.");

            // The whole cycle is asserted in weighted-average terms, and only the stocked item may carry stock.
            Check(modelGroup.InventModel == InventModel.WeightedAvg
                && modelGroup.StockedProduct == (service ? NoYes.No : NoYes.Yes),
                $"item model group {assignment.ModelGroupId} is not the weighted-average policy this cycle asserts.");

            foreach (var module in new[] { ModuleInventPurchSales.Inventory, ModuleInventPurchSales.Purchase, ModuleInventPurchSales.Sales })
            {
                var row = await c.RequireAsync<InventTableModule>(x => x.ItemId == itemId && x.ModuleType == module, ct);
                Check(row.UnitId == definition.Unit && row.TaxItemGroupId == definition.ItemTaxGroup
                    && row.PriceUnit == InventoryCycleSeedExample.PriceUnit
                    && row.Price == InventoryCycleSeedExample.Price(definition.Suffix, module),
                    $"persisted {module} setup for {itemId} differs from items.json.");
            }

            long? storage = null, tracking = null;
            if (!service)
            {
                storage = (await c.RequireAsync<EcoResStorageDimensionGroupItem>(x => x.ItemId == itemId && x.ItemDataAreaId == c.Company, ct)).StorageDimensionGroup;
                tracking = (await c.RequireAsync<EcoResTrackingDimensionGroupItem>(x => x.ItemId == itemId && x.ItemDataAreaId == c.Company, ct)).TrackingDimensionGroup;
                await c.RequireAsync<InventItemPurchSetup>(x => x.ItemId == itemId && x.ItemDataAreaId == c.Company && x.InventDimId == "", ct);
                await c.RequireAsync<InventItemInventSetup>(x => x.ItemId == itemId && x.ItemDataAreaId == c.Company && x.InventDimId == "", ct);
            }

            items.Add(new(item.RecId, item.ItemId, product.RecId, item.Name, item.ItemType.ToString(),
                definition.Unit, assignment.ModelGroupId, group.ItemGroupId, definition.ItemTaxGroup,
                definition.Stocked, definition.PurchasePrice, definition.SalesPrice, storage, tracking));
        }

        prepared = new(
            new(ledger.RecId, ledger.AccountingCurrency, ExampleIdentifiers.Site, warehouse.InventLocationId,
                dimension.InventDimId, InventoryCycleSeedExample.ItemGroup, catalog.Setup.CostingMethod,
                InventoryCycleSeedExample.ItemTaxGroup, GeneralLedgerSeedExample.Inventory,
                GeneralLedgerSeedExample.CostOfGoodsSold, parameters.RecId),
            items.AsReadOnly(), profiles.AsReadOnly());
    }
}
