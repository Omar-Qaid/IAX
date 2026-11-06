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
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.AccountsReceivable;

/// <summary>
/// Output tax for the sales example. A sales-only code keeps payable and receivable tax on their own
/// ledger accounts; the shared item tax group holds both codes, and the customer tax group selects
/// this one, so the group intersection resolves a single rate per sales line.
/// </summary>
public sealed class SalesTaxSeeder : IDataSeeder
{
    public string Id => "ERPV2.SalesTaxSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.SalesAccountStructureSeeder", "ERPV2.TaxItemGroupHeadingSeeder", "ERPV2.CurrencySeeder", "ERPV2.UnitOfMeasureSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var output = await c.RequireAsync<DimensionAttributeValueCombination>(x => x.MainAccountValue == GeneralLedgerSeedExample.PayableSalesTax, ct);
        await c.EnsureAsync<TaxLedgerAccountGroup>(x => x.TaxAccountGroup == "ARV2",
            new() { TaxAccountGroup = "ARV2", Name = "AR example output tax", TaxOutgoingLedgerDimension = output.RecId }, ct);
        await c.EnsureAsync<TaxTable>(x => x.TaxCode == "ARV2-15", new() { TaxCode = "ARV2-15", TaxName = "AR example 15 percent",
            TaxCurrencyCode = "SAR", TaxAccountGroup = "ARV2", TaxUnit = "EA", TaxBase = TaxBaseType.PctPerNet,
            TaxCalcMethod = TaxCalcMethod.Line, TaxRoundOff = 0.01m }, ct);
        var from = new DateTime(c.FiscalYear, 1, 1);
        await c.EnsureAsync<TaxData>(x => x.TaxCode == "ARV2-15" && x.TaxFromDate == from,
            new() { TaxCode = "ARV2-15", TaxValue = 15m, TaxFromDate = from, TaxToDate = new(c.FiscalYear, 12, 31) }, ct);
        await c.EnsureAsync<TaxGroupHeading>(x => x.TaxGroup == "ARV2",
            new() { TaxGroup = "ARV2", TaxGroupName = "AR example customer tax group" }, ct);
        await c.EnsureAsync<TaxGroupData>(x => x.TaxGroup == "ARV2" && x.TaxCode == "ARV2-15",
            new() { TaxGroup = "ARV2", TaxCode = "ARV2-15", ExemptTax = NoYes.No }, ct);
        await c.RequireAsync<TaxItemGroupHeading>(x => x.TaxItemGroup == "APV2", ct);
        await c.EnsureAsync<TaxOnItem>(x => x.TaxItemGroup == "APV2" && x.TaxCode == "ARV2-15",
            new() { TaxItemGroup = "APV2", TaxCode = "ARV2-15" }, ct);
    }
}
