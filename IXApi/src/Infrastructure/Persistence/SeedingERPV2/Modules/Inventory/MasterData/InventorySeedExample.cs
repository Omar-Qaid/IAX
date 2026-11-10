namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;

/// <summary>Illustrative quotations and product identities for the office-supplies example.</summary>
public static class InventorySeedExample
{
    public const string StockItem = "APV2-STOCK";
    public const string ServiceItem = "APV2-SVC";
    /// <summary>Batch-tracked counterpart of the stock item, used by the MD-04 tracking cases.</summary>
    public const string BatchItem = "APV2-BATCH";
    /// <summary>Serial-tracked counterpart; one line of it identifies exactly one unit.</summary>
    public const string SerialItem = "APV2-SERIAL";
    public const decimal PaperPriceSar = 25m;
    public const decimal MaintenancePriceSar = 350m;
    public const decimal PaperPriceUsd = 7m;

    public static string Name(string suffix) => suffix switch
    {
        "STOCK" => "A4 copier paper - 500 sheet ream",
        "BATCH" => "Toner cartridge - batch tracked",
        "SERIAL" => "Label printer - serial tracked",
        _ => "Printer preventive maintenance visit"
    };
    public static decimal PurchasePrice(string item) => item switch
    {
        StockItem => PaperPriceSar,
        BatchItem => PaperPriceSar,
        SerialItem => PaperPriceSar,
        ServiceItem => MaintenancePriceSar,
        _ => throw new ArgumentOutOfRangeException(nameof(item))
    };
}
