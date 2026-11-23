using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Tax;

/// <summary>
/// VAT setup: authorities, settlement periods with their monthly and quarterly intervals for the report year,
/// exemption codes, sales tax and item sales tax groups, ledger posting groups, tax codes with their rates, and
/// the group-to-code mappings. Posting groups are written before the codes that reference them.
/// </summary>
/// <remarks>
/// A tax authority carries a required reference to its settlement vendor, so an authority is added only once that
/// vendor exists, and its settlement periods and their intervals only once the authority exists. Until then the
/// codes, groups and rates are still seeded; the authority chain follows on the first run after the vendor is
/// created. (The legacy seeder blanked the vendor instead, which the required reference rejects.)
/// </remarks>
public sealed class BaselineTaxSeeder : IBaselineSeeder
{
    public const string SeederId = "Finance.GeneralLedger.Tax";
    public string Id => SeederId;
    public IReadOnlyCollection<string> Dependencies => [BaselineUnitOfMeasureSeeder.SeederId];

    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var catalog = TaxBaselineCatalog.Load();
        foreach (var authority in catalog.TaxAuthorities)
            if (await c.Rows<VendTable>().AnyAsync(x => x.AccountNum == authority.AccountNum, ct))
                await c.AddMissingAsync<TaxAuthorityAddress>(x => x.TaxAuthority == authority.TaxAuthority, authority, ct);
        var authorities = await c.Rows<TaxAuthorityAddress>().Select(x => x.TaxAuthority).ToListAsync(ct);
        var periods = catalog.TaxPeriods.Where(x => authorities.Contains(x.TaxAuthority)).ToArray();
        await c.AddMissingAsync(periods, row => x => x.TaxPeriod == row.TaxPeriod, ct);
        var seededPeriods = periods.Select(x => x.TaxPeriod).ToHashSet();
        await c.AddMissingAsync(ReportPeriods(catalog.ReportYear).Where(x => seededPeriods.Contains(x.TaxPeriod)),
            row => x => x.TaxPeriod == row.TaxPeriod && x.FromDate == row.FromDate, ct);
        await c.AddMissingAsync(catalog.ExemptCodes, row => x => x.ExemptCode == row.ExemptCode, ct);
        await c.AddMissingAsync(catalog.TaxGroups, row => x => x.TaxGroup == row.TaxGroup, ct);
        await c.AddMissingAsync(catalog.ItemTaxGroups, row => x => x.TaxItemGroup == row.TaxItemGroup, ct);
        await c.AddMissingAsync(catalog.LedgerAccountGroups, row => x => x.TaxAccountGroup == row.TaxAccountGroup, ct);
        await c.AddMissingAsync(catalog.TaxCodes, row => x => x.TaxCode == row.TaxCode, ct);
        await c.AddMissingAsync(catalog.TaxValues, row => x => x.TaxCode == row.TaxCode, ct);
        await c.AddMissingAsync(catalog.TaxGroupCodes, row => x => x.TaxGroup == row.TaxGroup && x.TaxCode == row.TaxCode, ct);
        await c.AddMissingAsync(catalog.ItemTaxGroupCodes, row => x => x.TaxItemGroup == row.TaxItemGroup && x.TaxCode == row.TaxCode, ct);
    }

    /// <summary>Twelve monthly and four quarterly intervals; the last interval of each period closes the year.</summary>
    private static IEnumerable<TaxReportPeriod> ReportPeriods(int year)
    {
        for (var month = 1; month <= 12; month++)
            yield return Interval("Monthly", year, month, month, month == 12);
        for (var quarter = 0; quarter < 4; quarter++)
            yield return Interval("Quarterly", year, quarter * 3 + 1, quarter * 3 + 3, quarter == 3);
    }

    private static TaxReportPeriod Interval(string period, int year, int fromMonth, int toMonth, bool last) => new()
    {
        TaxPeriod = period, FromDate = new DateTime(year, fromMonth, 1),
        ToDate = new DateTime(year, toMonth, DateTime.DaysInMonth(year, toMonth)),
        Closed = NoYes.No, LastPeriod = last ? NoYes.Yes : NoYes.No
    };
}
