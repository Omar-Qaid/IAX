namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.AccountsReceivable;

/// <summary>Fictional customers and premises. No number here identifies a real party.</summary>
public static class CustomerSeedExample
{
    public static string Name(string suffix) => suffix == "LOCAL"
        ? "Al Noor Trading Company (Demo)" : "Gulf Paper Distribution (Demo)";
    public static string Email(string suffix) => suffix == "LOCAL"
        ? "finance@alnoor-trading.example.invalid" : "finance@gulf-paper.example.invalid";
    public static string Street(string suffix) => suffix == "LOCAL" ? "Prince Mohammed Bin Abdulaziz Road" : "Takhassusi Street";
    public static string Building(string suffix) => suffix == "LOCAL" ? "7100" : "7200";

    /// <summary>Sales quotations for the shared demo items, above their purchase cost.</summary>
    public const decimal PaperPriceSar = 40m;
    public const decimal MaintenancePriceSar = 500m;
    public const decimal PaperPriceUsd = 11m;
}
