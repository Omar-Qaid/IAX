namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Tax;

/// <summary>
/// The one list of sales tax identifiers every module seeds against. Tax owns the codes, groups and
/// posting groups; AP, AR, Inventory and charges only reference them, so a module never invents a
/// second code for a treatment that already exists.
/// </summary>
public static class TaxSeedCatalog
{
    // Ledger posting groups (TaxLedgerAccountGroup). D365 carries both the payable (outgoing) and the
    // receivable (incoming) account on one group, so a code posts on either side.
    public const string PurchasePostingGroup = "APV2";
    public const string SalesPostingGroup = "ARV2";

    // Party sales tax groups (TaxGroupHeading / TaxGroupData).
    public const string VendorTaxGroup = "APV2";
    public const string CustomerTaxGroup = "ARV2";

    // Tax codes (TaxTable / TaxData).
    public const string StandardPurchase = "APV2-15";
    public const string StandardSales = "ARV2-15";
    public const string ZeroRated = "APV2-0";
    public const string Exempt = "APV2-EX";

    // Exemption reasons (TaxExemptCodeTable).
    public const string ExemptReason = "VATEX-SA-29";

    // Item sales tax groups (TaxItemGroupHeading / TaxOnItem).
    public const string StandardItemGroup = "APV2";
    public const string ZeroItemGroup = "APV2-ZERO";
    public const string ExemptItemGroup = "APV2-EXEMPT";
    public const string OutOfScopeItemGroup = "APV2-OOS";
    /// <summary>The inventory cycle's item group; it holds the shared zero-rated code.</summary>
    public const string InventoryZeroItemGroup = "IV2-ZERO";
}
