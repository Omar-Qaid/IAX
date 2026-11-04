using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Data;

public static class ExampleIdentifiers
{
    public const string Scope = "ConfigurationAndMasterData";
    public const string Setup = "APV2";
    public const string StockItem = InventorySeedExample.StockItem;
    public const string ServiceItem = InventorySeedExample.ServiceItem;

    public static string Vendor(string company, string suffix)
    {
        if (suffix is not ("LOCAL" or "USD")) throw new ArgumentOutOfRangeException(nameof(suffix));
        return $"APV2-{company}-{suffix}";
    }
}
