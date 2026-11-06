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
public sealed class CustPaymModeTableSeeder : IDataSeeder
{
    public string Id => "ERPV2.CustPaymModeTableSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.BankAccountTableSeeder", "ERPV2.SalesLedgerJournalNameSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var bank = await c.RequireAsync<BankAccountTable>(x => x.AccountId == "APV2-BANK", ct);
        await c.EnsureAsync<CustPaymModeTable>(x => x.PaymMode == "ARV2", new() { PaymMode = "ARV2", Name = "AR example bank collection",
            AccountType = LedgerJournalACType.Bank, PaymentLedgerDimension = bank.LedgerDimension, PaymJournalNameId = "ARV2-PAY" }, ct);
    }
}
