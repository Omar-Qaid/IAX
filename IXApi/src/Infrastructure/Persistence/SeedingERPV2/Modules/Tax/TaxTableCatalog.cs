namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Tax;

/// <summary>The tables this module owns, as covered by its registered seeders.</summary>
public static class TaxTableCatalog
{
    public static IReadOnlySet<string> MasterTypes { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        "TaxAuthorityAddress",
        "TaxData",
        "TaxExemptCodeTable",
        "TaxGroupData",
        "TaxGroupHeading",
        "TaxItemGroupHeading",
        "TaxLedgerAccountGroup",
        "TaxOnItem",
        "TaxPeriodHead",
        "TaxReportPeriod",
        "TaxTable",
    };

    public static IReadOnlySet<string> TransactionTypes { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        "TaxJournalTrans",
        "TaxTrans",
        "TaxTransGeneralJournalAccountEntry",
    };
}
