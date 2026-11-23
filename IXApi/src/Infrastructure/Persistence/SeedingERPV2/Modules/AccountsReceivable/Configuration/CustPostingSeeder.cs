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
/// The customer posting profile: its summary account carries the receivable, and its clearing
/// account holds a customer prepayment until it is applied to an invoice.
/// </summary>
public sealed class CustPostingSeeder : IDataSeeder
{
    public const string PrepaymentProfile = "ARV2-PRE";
    public string Id => "ERPV2.CustPostingSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.CustGroupSeeder", "ERPV2.SalesAccountStructureSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var summary = await c.RequireAsync<DimensionAttributeValueCombination>(x => x.MainAccountValue == GeneralLedgerSeedExample.CustomerControl, ct);
        var advance = await c.RequireAsync<DimensionAttributeValueCombination>(x => x.MainAccountValue == GeneralLedgerSeedExample.CustomerAdvance, ct);
        await c.EnsureAsync<CustLedger>(x => x.PostingProfile == "ARV2",
            new() { PostingProfile = "ARV2", Name = "AR example posting profile", Settlement = NoYes.Yes }, ct);
        await c.EnsureAsync<CustLedgerAccounts>(x => x.PostingProfile == "ARV2" && x.AccountCode == AccountCode.Group && x.Num == "ARV2",
            new() { PostingProfile = "ARV2", AccountCode = AccountCode.Group, Num = "ARV2", SummaryLedgerDimension = summary.RecId, ClearingLedgerDimension = advance.RecId }, ct);
        // D365 posts a customer prepayment through its own posting profile, whose summary account is the
        // customer-advance liability; settling it against the invoice moves the amount to the receivable.
        await c.EnsureAsync<CustLedger>(x => x.PostingProfile == PrepaymentProfile,
            new() { PostingProfile = PrepaymentProfile, Name = "AR example prepayment posting profile", Settlement = NoYes.Yes }, ct);
        await c.EnsureAsync<CustLedgerAccounts>(x => x.PostingProfile == PrepaymentProfile && x.AccountCode == AccountCode.Group && x.Num == "ARV2",
            new() { PostingProfile = PrepaymentProfile, AccountCode = AccountCode.Group, Num = "ARV2", SummaryLedgerDimension = advance.RecId }, ct);
    }
}
