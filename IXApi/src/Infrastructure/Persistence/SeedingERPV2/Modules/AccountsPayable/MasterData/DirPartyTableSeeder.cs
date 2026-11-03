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
public sealed class DirPartyTableSeeder : IDataSeeder
{
    public string Id => "ERPV2.DirPartyTableSeeder";
    public IReadOnlyCollection<string> Dependencies => [];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {

        foreach (var suffix in new[] { "LOCAL", "USD" })
        {
            var number = $"APV2-{c.Company}-{suffix}";
            await c.EnsureAsync<DirPartyTable>(x => x.PartyNumber == number, new() { PartyNumber = number, Name = "AP example vendor " + suffix, NameAlias = "AP example vendor " + suffix, LanguageId = "en-us", IsActive = NoYes.Yes }, ct,
                nameof(DirPartyTable.Name), nameof(DirPartyTable.NameAlias), nameof(DirPartyTable.LanguageId), nameof(DirPartyTable.IsActive));
        }
    }
}
