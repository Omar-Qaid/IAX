using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Inventory.InventTable;
using IAX.IXApi.Modules.Finance.Inventory.ProductCategories.Entities;
using IAX.IXApi.Modules.Finance.Foundation.Dimensions.LedgerDimension;
using IAX.IXApi.Modules.Finance.Foundation.Dimensions.AccountStructure;
using Microsoft.EntityFrameworkCore;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Tax;
public sealed class TaxTableSeeder : IDataSeeder
{
    public string Id => "ERPV2.TaxTableSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.TaxPostingSeeder", "ERPV2.CurrencySeeder", "ERPV2.UnitOfMeasureSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {

        await c.EnsureAsync<TaxExemptCodeTable>(x => x.ExemptCode == "APV2-NONE",
            new() { ExemptCode = "APV2-NONE", Description = "No exemption - AP example" }, ct);
        await c.EnsureAsync<TaxTable>(x => x.TaxCode == "APV2-15", new() { TaxCode = "APV2-15", TaxName = "AP example 15 percent",
            TaxCurrencyCode = "SAR", TaxAccountGroup = "APV2", TaxUnit = "EA", TaxBase = TaxBaseType.PctPerNet,
            TaxCalcMethod = TaxCalcMethod.Line, TaxRoundOff = 0.01m }, ct);
        var from = new DateTime(c.FiscalYear, 1, 1);
        await c.EnsureAsync<TaxData>(x => x.TaxCode == "APV2-15" && x.TaxFromDate == from,
            new() { TaxCode = "APV2-15", TaxValue = 15m, TaxFromDate = from, TaxToDate = new(c.FiscalYear, 12, 31) }, ct);
    }
}
