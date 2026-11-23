using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Tax;

/// <summary>
/// The startup VAT setup, read from <c>Data/baseline-tax.json</c>. The rows are flat reference data, so the
/// manifest holds them in their entity shape; the seeder only adds keys and resolves the authority vendor.
/// </summary>
public sealed record TaxBaselineCatalog(
    TaxAuthorityAddress[] TaxAuthorities,
    TaxPeriodHead[] TaxPeriods,
    int ReportYear,
    TaxExemptCodeTable[] ExemptCodes,
    TaxGroupHeading[] TaxGroups,
    TaxItemGroupHeading[] ItemTaxGroups,
    TaxLedgerAccountGroup[] LedgerAccountGroups,
    TaxTable[] TaxCodes,
    TaxData[] TaxValues,
    TaxGroupData[] TaxGroupCodes,
    TaxOnItem[] ItemTaxGroupCodes)
{
    public static TaxBaselineCatalog Load() => BaselineManifest.Read<TaxBaselineCatalog>("baseline-tax.json");
}
