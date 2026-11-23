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
/// The existing APV2 item group keeps only the standard code, so the documents already seeded are unchanged.
/// </summary>
public sealed class TaxVatCategorySeeder : IDataSeeder
{
    public const string ZeroRated = "APV2-0";
    public const string Exempt = "APV2-EX";
    public const string ExemptReason = "VATEX-SA-29";
    public const string ZeroItemGroup = "APV2-ZERO";
    public const string ExemptItemGroup = "APV2-EXEMPT";
    public const string OutOfScopeItemGroup = "APV2-OOS";

    public string Id => "ERPV2.TaxVatCategorySeeder";
    public IReadOnlyCollection<string> Dependencies =>
        ["ERPV2.TaxTableSeeder", "ERPV2.TaxGroupHeadingSeeder", "ERPV2.TaxItemGroupHeadingSeeder"];

    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var from = new DateTime(c.FiscalYear, 1, 1);
        var to = new DateTime(c.FiscalYear, 12, 31);
        await c.EnsureAsync<TaxExemptCodeTable>(x => x.ExemptCode == ExemptReason,
            new() { ExemptCode = ExemptReason, Description = "Exempt supply - financial services" }, ct);
        foreach (var (code, name) in new[] { (ZeroRated, "AP example zero-rated"), (Exempt, "AP example exempt") })
        {
            await c.EnsureAsync<TaxTable>(x => x.TaxCode == code, new()
            {
                TaxCode = code, TaxName = name, TaxCurrencyCode = "SAR", TaxAccountGroup = "APV2", TaxUnit = "EA",
                TaxBase = TaxBaseType.PctPerNet, TaxCalcMethod = TaxCalcMethod.Line, TaxRoundOff = 0.01m
            }, ct);
            await c.EnsureAsync<TaxData>(x => x.TaxCode == code && x.TaxFromDate == from,
                new() { TaxCode = code, TaxValue = 0m, TaxFromDate = from, TaxToDate = to }, ct);
        }
        await c.EnsureAsync<TaxGroupData>(x => x.TaxGroup == "APV2" && x.TaxCode == ZeroRated,
            new() { TaxGroup = "APV2", TaxCode = ZeroRated }, ct);
        await c.EnsureAsync<TaxGroupData>(x => x.TaxGroup == "APV2" && x.TaxCode == Exempt,
            new() { TaxGroup = "APV2", TaxCode = Exempt, ExemptTax = NoYes.Yes, TaxExemptCode = ExemptReason }, ct);
        await c.EnsureAsync<TaxItemGroupHeading>(x => x.TaxItemGroup == ZeroItemGroup,
            new() { TaxItemGroup = ZeroItemGroup, Name = "AP example zero-rated items" }, ct);
        await c.EnsureAsync<TaxItemGroupHeading>(x => x.TaxItemGroup == ExemptItemGroup,
            new() { TaxItemGroup = ExemptItemGroup, Name = "AP example exempt items" }, ct);
        await c.EnsureAsync<TaxItemGroupHeading>(x => x.TaxItemGroup == OutOfScopeItemGroup,
            new() { TaxItemGroup = OutOfScopeItemGroup, Name = "AP example out-of-scope items" }, ct);
        await c.EnsureAsync<TaxOnItem>(x => x.TaxItemGroup == ZeroItemGroup && x.TaxCode == ZeroRated,
            new() { TaxItemGroup = ZeroItemGroup, TaxCode = ZeroRated }, ct);
        await c.EnsureAsync<TaxOnItem>(x => x.TaxItemGroup == ExemptItemGroup && x.TaxCode == Exempt,
            new() { TaxItemGroup = ExemptItemGroup, TaxCode = Exempt }, ct);
    }
}
