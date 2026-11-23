namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.CashAndBank;

/// <summary>The tables this module owns, as covered by its registered seeders.</summary>
public static class CashAndBankTableCatalog
{
    public static IReadOnlySet<string> MasterTypes { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        "BankAccountTable",
        "BankGroup",
    };

    /// <summary>Bank movement is posted by the payment services, never seeded directly.</summary>
    public static IReadOnlySet<string> TransactionTypes { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        "BankAccountTrans",
    };
}
