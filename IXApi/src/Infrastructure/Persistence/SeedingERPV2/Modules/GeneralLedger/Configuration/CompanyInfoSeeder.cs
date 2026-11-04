using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using Microsoft.EntityFrameworkCore;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
public sealed class CompanyInfoSeeder : IDataSeeder
{
    public string Id => "ERPV2.CompanyInfoSeeder";
    public IReadOnlyCollection<string> Dependencies => [];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var existing = await c.Rows<CompanyInfo>().SingleOrDefaultAsync(x => x.DataArea == c.Company, ct);
        if (existing != null)
        {
            await c.RequireAsync<CompanyInfo>(x => x.RecId == existing.RecId, ct);
            await c.RequireAsync<DirPartyTable>(x => x.RecId == existing.Party, ct);
            return; // Preserve the application-owned legal entity.
        }
        var partyNumber = $"APV2-{c.Company}-LEGAL";
        var party = await c.EnsureAsync<DirPartyTable>(x => x.PartyNumber == partyNumber,
            new() { PartyNumber = partyNumber, Name = "Office Supplies Demo Company", IsActive = NoYes.Yes }, ct);
        await c.EnsureAsync<CompanyInfo>(x => x.DataArea == c.Company,
            new() { DataArea = c.Company, Name = party.Name, Party = party.RecId }, ct);
    }
}
