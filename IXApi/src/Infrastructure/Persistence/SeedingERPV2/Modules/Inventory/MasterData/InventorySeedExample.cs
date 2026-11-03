namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;

/// <summary>Illustrative quotations and product identities for the office-supplies example.</summary>
public static class InventorySeedExample
{
    public const string StockItem = "APV2-STOCK";
    public const string ServiceItem = "APV2-SVC";
    public const decimal PaperPriceSar = 25m;
    public const decimal MaintenancePriceSar = 350m;
    public const decimal PaperPriceUsd = 7m;

    public static string Name(string suffix) => suffix == "STOCK"
        ? "A4 copier paper - 500 sheet ream" : "Printer preventive maintenance visit";
    public static decimal PurchasePrice(string item) => item switch
    {
        StockItem => PaperPriceSar,
        ServiceItem => MaintenancePriceSar,
        _ => throw new ArgumentOutOfRangeException(nameof(item))
    };
}
