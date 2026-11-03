namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.AccountsPayable;

/// <summary>Fictional suppliers and premises. Bank numbers are synthetic, not payment destinations.</summary>
public static class VendorSeedExample
{
    public static string Name(string suffix) => suffix == "LOCAL"
        ? "Riyadh Office Supplies and Services (Demo)" : "Gulf Office Equipment Trading (Demo)";
    public static string Email(string suffix) => suffix == "LOCAL"
        ? "accounts@riyadh-office.example.invalid" : "accounts@gulf-office.example.invalid";
    public static string Street(string suffix) => suffix == "LOCAL" ? "King Fahd Road" : "Olaya Street";
    public static string Building(string suffix) => suffix == "LOCAL" ? "4100" : "4200";
    public static string BankNumber(string suffix) => suffix == "LOCAL" ? "000000000001" : "000000000002";
}
