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
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.AccountsReceivable;
public sealed class CustGroupSeeder : IDataSeeder
{
    public string Id => "ERPV2.CustGroupSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.PaymTermSeeder", "ERPV2.SalesTaxSeeder", "ERPV2.ExchangeRateTypeSeeder", "ERPV2.AccountsReceivableNumberSequenceSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var rate = await c.RequireAsync<ExchangeRateType>(x => x.Name == "APV2", ct);
        var sequence = await c.RequireAsync<SysNumberSequence>(x => x.NumberSequence == "ARV2-CUST", ct);
        await c.EnsureAsync<CustGroup>(x => x.CustGroupId == "ARV2", new() { CustGroupId = "ARV2", Name = "AR example customers",
            PaymTermId = "APV2-N30", TaxGroupId = "ARV2", AccountingCurrencyExchangeRateType = rate.RecId, CustAccountNumSeq = sequence.RecId }, ct);
    }
}
