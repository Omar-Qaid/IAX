using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory.Data;

/// <summary>
/// The business keys this package reads. They are declared once here because the manifests, the preparation
/// validator and the transaction flow must all name the same records; a key spelled differently in one of the
/// three would validate setup that the flow never touches.
/// </summary>
public static class ExampleIdentifiers
{
    public const string Scope = "ConfigurationAndMasterData";
    public const string Setup = "IV2";

    /// <summary>Site, warehouse and dimension are owned by the AP example seeders; Inventory reuses them.</summary>
    public const string Site = "APV2-SITE";
    public const string Warehouse = "APV2-WH";
    public const string Dimension = "APV2-DIM";
    public const string Unit = "EA";
    public const string ItemGroup = "APV2";
    public const string StockModelGroup = "APV2";
    public const string ServiceModelGroup = "APV2-SVC";
    public const string ItemTaxGroup = "IV2-ZERO";
    public const string CustomerTaxGroup = "ARV2";

    /// <summary>
    /// Stock and service items are scoped by fiscal year. The reference cycle asserts an exact closing value, so a
    /// prior year's close or an AP/AR demonstration on a shared item would make the assertion depend on run order.
    /// </summary>
    public static string StockItem(int fiscalYear) => InventoryCycleMasterSeeder.StockItem(fiscalYear);
    public static string ServiceItem(int fiscalYear) => InventoryCycleMasterSeeder.ServiceItem(fiscalYear);

    public static string Vendor(string company) => $"APV2-{company}-LOCAL";
    public static string Customer(string company) => $"ARV2-{company}-LOCAL";

    /// <summary>The stable document key every scenario derives its document numbers from, so a rerun resumes.</summary>
    public static string ScenarioKey(int fiscalYear, string scenarioId) => $"IV2-{fiscalYear}-{scenarioId}";
}
