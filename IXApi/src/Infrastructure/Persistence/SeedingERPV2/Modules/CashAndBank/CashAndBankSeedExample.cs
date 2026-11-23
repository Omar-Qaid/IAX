namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.CashAndBank;

/// <summary>Stable business references described by the CashAndBank manifest and checked against persisted seeders.</summary>
public static class CashAndBankSeedExample
{
    public static IReadOnlyList<(string Entity, string Field, string Value)> References { get; } =
    [
        ("BankGroup","BankGroupId","APV2-BG"),
        ("BankAccountTable","AccountId","APV2-BANK"),
        ("BankAccountTable","AccountId","APV2-USD")
    ];
}
