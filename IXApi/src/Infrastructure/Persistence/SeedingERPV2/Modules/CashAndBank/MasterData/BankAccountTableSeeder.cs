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
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.CashAndBank;
public sealed class BankAccountTableSeeder : IDataSeeder
{
    public string Id => "ERPV2.BankAccountTableSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.DimensionAttributeValueCombinationSeeder", "ERPV2.CurrencySeeder", "ERPV2.LedgerJournalNameSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {

        var dimension = await c.RequireAsync<DimensionAttributeValueCombination>(x => x.MainAccountValue == "181001", ct);
        await c.EnsureAsync<BankAccountTable>(x => x.AccountId == "APV2-BANK", new() { AccountId = "APV2-BANK", AccountNum = "000000009001", Name = "Treasury operating account (demo)", CurrencyCode = "SAR", LedgerDimension = dimension.RecId, BankAccountStatus = BankAccountStatus.Active, VendPaymentJournalName = "APV2-PAY", ActiveFrom = new(2000, 1, 1), ActiveTo = new(2099, 12, 31) }, ct);
    }
}
