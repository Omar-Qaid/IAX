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
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.AccountsPayable;
public sealed class DirPartyLocationSeeder : IDataSeeder
{
    public string Id => "ERPV2.DirPartyLocationSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.DirPartyTableSeeder", "ERPV2.LogisticsLocationSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {

        foreach (var suffix in new[] { "LOCAL", "USD" })
        {
            var code = $"APV2-{c.Company}-{suffix}";
            var party = await c.RequireAsync<DirPartyTable>(x => x.PartyNumber == code, ct);
            var location = await c.RequireAsync<LogisticsLocation>(x => x.LocationId == code, ct);
            await c.EnsureAsync<DirPartyLocation>(x => x.Party == party.RecId && x.Location == location.RecId,
                new() { Party = party.RecId, Location = location.RecId, IsPrimary = NoYes.Yes, IsPostalAddress = NoYes.Yes, IsLocationOwner = NoYes.Yes, IsRoleInvoice = NoYes.Yes }, ct);
        }
    }
}
