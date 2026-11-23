using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.Common;
using Microsoft.EntityFrameworkCore;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Tax;

/// <summary>
/// The other three VAT treatments next to the standard 15 percent code. D365 applies the codes that
/// the party's sales tax group (TaxGroupData) and the item's sales tax group (TaxOnItem) share:
/// <list type="bullet">
/// <item><b>Zero-rated</b> (APV2-0): a taxable supply at 0 percent, posted as an ordinary zero-tax TaxTrans.</item>
/// <item><b>Exempt</b> (APV2-EX): marked ExemptTax on the tax group with its exemption reason code, posted as a
/// zero-tax TaxTrans with direction TaxExemptSales or TaxExemptPurchase.</item>
/// <item><b>Out of scope</b> (item group APV2-OOS): shares no code with any tax group, so no TaxTrans is posted.</item>
/// </list>
/// The identifiers come from <see cref="TaxSeedCatalog"/>. The vendor tax group carries the zero-rated and
/// exempt codes here; the customer tax group adds them in the AR sales tax seeder. The standard item group
/// keeps only the standard codes, so the documents already seeded are unchanged.
/// </summary>
public sealed class TaxVatCategorySeeder : IDataSeeder
{
    public string Id => "ERPV2.TaxVatCategorySeeder";
    public IReadOnlyCollection<string> Dependencies =>
        ["ERPV2.TaxTableSeeder", "ERPV2.TaxGroupHeadingSeeder", "ERPV2.TaxItemGroupHeadingSeeder"];

    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var from = new DateTime(c.FiscalYear, 1, 1);
        var to = new DateTime(c.FiscalYear, 12, 31);
        await c.EnsureAsync<TaxExemptCodeTable>(x => x.ExemptCode == TaxSeedCatalog.ExemptReason,
            new() { ExemptCode = TaxSeedCatalog.ExemptReason, Description = "Exempt supply - financial services" }, ct);
        foreach (var (code, name) in new[] { (TaxSeedCatalog.ZeroRated, "AP example zero-rated"), (TaxSeedCatalog.Exempt, "AP example exempt") })
        {
            await c.EnsureAsync<TaxTable>(x => x.TaxCode == code, new()
            {
                TaxCode = code, TaxName = name, TaxCurrencyCode = "SAR", TaxAccountGroup = TaxSeedCatalog.PurchasePostingGroup, TaxUnit = "EA",
                TaxBase = TaxBaseType.PctPerNet, TaxCalcMethod = TaxCalcMethod.Line, TaxRoundOff = 0.01m
            }, ct);
            await c.EnsureAsync<TaxData>(x => x.TaxCode == code && x.TaxFromDate == from,
                new() { TaxCode = code, TaxValue = 0m, TaxFromDate = from, TaxToDate = to }, ct);
        }
        await c.EnsureAsync<TaxGroupData>(x => x.TaxGroup == TaxSeedCatalog.VendorTaxGroup && x.TaxCode == TaxSeedCatalog.ZeroRated,
            new() { TaxGroup = TaxSeedCatalog.VendorTaxGroup, TaxCode = TaxSeedCatalog.ZeroRated, TaxExemptCode = TaxSeedCatalog.NoExemption }, ct);
        await c.EnsureAsync<TaxGroupData>(x => x.TaxGroup == TaxSeedCatalog.VendorTaxGroup && x.TaxCode == TaxSeedCatalog.Exempt,
            new() { TaxGroup = TaxSeedCatalog.VendorTaxGroup, TaxCode = TaxSeedCatalog.Exempt, ExemptTax = NoYes.Yes, TaxExemptCode = TaxSeedCatalog.ExemptReason }, ct);
        await c.EnsureAsync<TaxItemGroupHeading>(x => x.TaxItemGroup == TaxSeedCatalog.ZeroItemGroup,
            new() { TaxItemGroup = TaxSeedCatalog.ZeroItemGroup, Name = "AP example zero-rated items" }, ct);
        await c.EnsureAsync<TaxItemGroupHeading>(x => x.TaxItemGroup == TaxSeedCatalog.ExemptItemGroup,
            new() { TaxItemGroup = TaxSeedCatalog.ExemptItemGroup, Name = "AP example exempt items" }, ct);
        await c.EnsureAsync<TaxItemGroupHeading>(x => x.TaxItemGroup == TaxSeedCatalog.OutOfScopeItemGroup,
            new() { TaxItemGroup = TaxSeedCatalog.OutOfScopeItemGroup, Name = "AP example out-of-scope items" }, ct);
        await c.EnsureAsync<TaxOnItem>(x => x.TaxItemGroup == TaxSeedCatalog.ZeroItemGroup && x.TaxCode == TaxSeedCatalog.ZeroRated,
            new() { TaxItemGroup = TaxSeedCatalog.ZeroItemGroup, TaxCode = TaxSeedCatalog.ZeroRated, TaxExemptCode = TaxSeedCatalog.NoExemption }, ct);
        await c.EnsureAsync<TaxOnItem>(x => x.TaxItemGroup == TaxSeedCatalog.ExemptItemGroup && x.TaxCode == TaxSeedCatalog.Exempt,
            new() { TaxItemGroup = TaxSeedCatalog.ExemptItemGroup, TaxCode = TaxSeedCatalog.Exempt, TaxExemptCode = TaxSeedCatalog.NoExemption }, ct);
    }
}
