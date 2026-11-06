using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable.Data;

public static class ExampleIdentifiers
{
    public const string Scope = "ConfigurationAndMasterData";
    public const string Setup = "ARV2";

    /// <summary>The same released items the purchase example buys; a sale needs stock on hand first.</summary>
    public const string StockItem = InventorySeedExample.StockItem;
    public const string ServiceItem = InventorySeedExample.ServiceItem;

    public static string Customer(string company, string suffix)
    {
        if (suffix is not ("LOCAL" or "USD")) throw new ArgumentOutOfRangeException(nameof(suffix));
        return $"ARV2-{company}-{suffix}";
    }
}
