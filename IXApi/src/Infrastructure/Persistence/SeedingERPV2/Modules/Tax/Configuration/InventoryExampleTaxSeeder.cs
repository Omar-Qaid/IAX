using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Tax;

/// <summary>Explicit zero-rate intersection for Inventory's no-tax reference; existing AP/AR item groups are unchanged.</summary>
public sealed class InventoryExampleTaxSeeder : IDataSeeder
{
    public string Id => "ERPV2.InventoryExampleTaxSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.SalesTaxSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var exempt = await c.RequireAsync<TaxExemptCodeTable>(x => x.ExemptCode == "APV2-NONE", ct);
        await c.RequireAsync<TaxLedgerAccountGroup>(x => x.TaxAccountGroup == "ARV2", ct);
        await c.EnsureAsync<TaxTable>(x => x.TaxCode == "IV2-ZERO", new()
        {
            TaxCode = "IV2-ZERO", TaxName = "Inventory reference zero rate", TaxCurrencyCode = "SAR",
            TaxAccountGroup = "ARV2", TaxUnit = "EA", TaxBase = TaxBaseType.PctPerNet,
            TaxCalcMethod = TaxCalcMethod.Line, TaxRoundOff = 0.01m
        }, ct);
        var from = new DateTime(c.FiscalYear, 1, 1);
        await c.EnsureAsync<TaxData>(x => x.TaxCode == "IV2-ZERO" && x.TaxFromDate == from,
            new() { TaxCode = "IV2-ZERO", TaxValue = 0, TaxFromDate = from, TaxToDate = new(c.FiscalYear, 12, 31) }, ct);
        await c.EnsureAsync<TaxItemGroupHeading>(x => x.TaxItemGroup == "IV2-ZERO",
            new() { TaxItemGroup = "IV2-ZERO", Name = "Inventory reference zero rate" }, ct);
        await c.EnsureAsync<TaxOnItem>(x => x.TaxItemGroup == "IV2-ZERO" && x.TaxCode == "IV2-ZERO",
            new() { TaxItemGroup = "IV2-ZERO", TaxCode = "IV2-ZERO", TaxExemptCode = exempt.ExemptCode }, ct);
        foreach (var group in new[] { "APV2", "ARV2" })
        {
            await c.RequireAsync<TaxGroupHeading>(x => x.TaxGroup == group, ct);
            await c.EnsureAsync<TaxGroupData>(x => x.TaxGroup == group && x.TaxCode == "IV2-ZERO",
                new() { TaxGroup = group, TaxCode = "IV2-ZERO", TaxExemptCode = exempt.ExemptCode, ExemptTax = NoYes.No }, ct);
        }
    }
}
