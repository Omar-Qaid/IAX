using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Foundation;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;

/// <summary>The company chart of accounts. An account that already exists is left as the operator keeps it.</summary>
public sealed class BaselineMainAccountSeeder : IBaselineSeeder
{
    public const string SeederId = "Finance.GeneralLedger.MainAccounts";
    public string Id => SeederId;
    public IReadOnlyCollection<string> Dependencies => [BaselineCompanySeeder.SeederId];

    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        foreach (var account in GeneralLedgerBaselineCatalog.Load().MainAccounts)
            await c.AddMissingAsync<MainAccount>(x => x.MainAccountId == account.MainAccountId, new()
            {
                MainAccountId = account.MainAccountId, Name = account.Name, Type = account.Type, CurrencyCode = account.CurrencyCode,
                ConsolidationMainAccount = account.ConsolidationMainAccount,
                GroupLevel01 = account.GroupLevel01, GroupLevel02 = account.GroupLevel02, GroupLevel03 = account.GroupLevel03
            }, ct);
    }
}
