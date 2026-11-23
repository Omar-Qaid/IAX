namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Tax;

/// <summary>Stable business references described by the Tax manifest and checked against persisted seeders.</summary>
/// <remarks>
/// Every entry must match exactly one row in the company, so the dated and per-group tables are
/// keyed on the value that stays unique: <c>TaxReportPeriod</c> carries twelve monthly rows and
/// <c>TaxGroupData</c> carries one row per group, so neither is keyed on its period or group.
/// </remarks>
public static class TaxSeedExample
{
    public static IReadOnlyList<(string Entity, string Field, string Value)> References { get; } =
    [
        ("TaxLedgerAccountGroup","TaxAccountGroup","APV2"), ("TaxExemptCodeTable","ExemptCode","APV2-NONE"),
        ("TaxTable","TaxCode","APV2-15"), ("TaxData","TaxCode","APV2-15"),
        ("TaxGroupHeading","TaxGroup","APV2"), ("TaxItemGroupHeading","TaxItemGroup","APV2"),
        ("TaxGroupData","TaxCode","APV2-15"), ("TaxOnItem","TaxCode","APV2-15"),
        ("TaxAuthorityAddress","TaxAuthority","FNV2"), ("TaxPeriodHead","TaxPeriod","Monthly")
    ];
}
