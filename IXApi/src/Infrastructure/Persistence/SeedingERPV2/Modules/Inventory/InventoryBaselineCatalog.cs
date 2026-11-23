using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;

/// <summary>The startup data of the Inventory module, read from <c>Data/baseline-inventory.json</c>.</summary>
public sealed record InventoryBaselineCatalog(
    string[] Units,
    InventoryBaselineCatalog.BaseUnitRow[] BaseUnits,
    InventoryBaselineCatalog.ConversionRow[] Conversions,
    InventoryBaselineCatalog.ItemGroupRow[] ItemGroups,
    InventoryBaselineCatalog.ModelGroupRow[] ModelGroups,
    InventoryBaselineCatalog.StorageGroupRow[] StorageGroups,
    InventoryBaselineCatalog.TrackingGroupRow[] TrackingGroups,
    InventoryBaselineCatalog.JournalNameRow[] JournalNames,
    string VoucherSequence,
    InventoryBaselineCatalog.ParametersRow Parameters,
    InventoryBaselineCatalog.StatusRow[] Statuses,
    string DefaultStatus,
    InventoryBaselineCatalog.SiteRow[] Sites,
    InventoryBaselineCatalog.WmsLocationRow[] WarehouseLocations,
    InventoryBaselineCatalog.ProductRow Product,
    InventoryBaselineCatalog.PostingRuleRow[] PostingRules)
{
    public static InventoryBaselineCatalog Load() => BaselineManifest.Read<InventoryBaselineCatalog>("baseline-inventory.json");

    public sealed record BaseUnitRow(string Unit, UnitOfMeasureClass Class);
    public sealed record ConversionRow(string From, string To, decimal Factor);
    public sealed record ItemGroupRow(string ItemGroupId, string Name, string? TaxItemGroup);
    public sealed record ModelGroupRow(string ModelGroupId, string Name, InventModel Model, bool Stocked);
    public sealed record StorageGroupRow(string Name, string Description, bool Location);
    public sealed record TrackingGroupRow(string Name, string Description, bool Batch, bool Serial);
    public sealed record JournalNameRow(string JournalNameId, string Description, InventJournalType Type);
    public sealed record ParametersRow(string DefaultUnitId, string DefaultSite, string DefaultWarehouse,
        string MovementJournal, string LossProfitJournal, string TransferJournal, string CountJournal);
    public sealed record StatusRow(string StatusId, string Name, bool Blocking);
    /// <summary>A site with its main warehouse (<c>{SiteId}-1</c>), quarantine (<c>-QC</c>) and transit (<c>-TRS</c>) warehouses.</summary>
    public sealed record SiteRow(string SiteId, string Name, string Warehouse);
    public sealed record WmsLocationRow(string WmsLocationId, string CheckText, int LocationType);
    public sealed record TranslationRow(string Language, string Name, string Description);
    public sealed record ProductRow(string ItemId, string Name, string NameAlias, string Unit, string ItemGroup, string ModelGroup,
        string StorageGroup, string TrackingGroup, string DefaultDimension, string Site, string Warehouse, TranslationRow[] Translations,
        decimal PurchasePrice, decimal SalesPrice, string TaxItemGroup);
    public sealed record PostingRuleRow(InventAccountType AccountType, string MainAccount);
}
