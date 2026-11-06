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

/// <summary>The customer early-settlement discount, posted to its own sales discount account.</summary>
public sealed class SalesCashDiscSeeder : IDataSeeder
{
    public string Id => "ERPV2.SalesCashDiscSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.SalesAccountStructureSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var dimension = await c.RequireAsync<DimensionAttributeValueCombination>(x => x.MainAccountValue == GeneralLedgerSeedExample.SalesCashDiscount, ct);
        await c.EnsureAsync<CashDisc>(x => x.CashDiscCode == "ARV2-2", new() { CashDiscCode = "ARV2-2",
            Description = "AR example 2 percent in 10 days", NumOfDays = 10, DiscPct = 2m, CashDiscLedgerDimension = dimension.RecId }, ct);
    }
}
