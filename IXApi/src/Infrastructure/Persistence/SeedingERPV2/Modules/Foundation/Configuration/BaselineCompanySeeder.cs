using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Foundation;

/// <summary>The legal entities and their parties. Each company's rows live in that company's own area.</summary>
public sealed class BaselineCompanySeeder : IBaselineSeeder
{
    public const string SeederId = "Finance.Foundation.CompanyInfo";
    public string Id => SeederId;
    public IReadOnlyCollection<string> Dependencies => [];

    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        foreach (var company in FoundationBaselineCatalog.Load().Companies)
        {
            var party = await c.AddMissingAsync<DirPartyTable>(x => x.PartyNumber == company.PartyNumber,
                new() { PartyNumber = company.PartyNumber, Name = company.Name, NameAlias = company.PartyNumber, IsActive = NoYes.Yes },
                ct, company.DataArea);
            await c.AddMissingAsync<CompanyInfo>(x => x.DataArea == company.DataArea,
                new() { DataArea = company.DataArea, Name = company.Name, Party = party.RecId, LanguageId = company.LanguageId, TimeZone = company.TimeZone },
                ct, company.DataArea);
        }
    }
}
