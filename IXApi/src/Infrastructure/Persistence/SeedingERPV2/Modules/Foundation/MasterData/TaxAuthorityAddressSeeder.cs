using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;
using IAX.IXApi.Modules.Finance.Foundation.HcmShowrooms;
using IAX.IXApi.Modules.Finance.Foundation.OrganizationUnits;
using IAX.IXApi.Modules.Finance.Foundation.Structure;
using IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments;
using Microsoft.EntityFrameworkCore;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Foundation;
public sealed class TaxAuthorityAddressSeeder : IDataSeeder
{
    public string Id => "ERPV2.Foundation.TaxAuthorityAddressSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.LogisticsLocationSeeder", "ERPV2.VendTableSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var location = await c.RequireAsync<LogisticsLocation>(x => x.LocationId == $"APV2-{c.Company}-LOCAL", ct);
        await c.EnsureAsync<TaxAuthorityAddress>(x => x.TaxAuthority == "FNV2", new() { TaxAuthority = "FNV2", TaxAuthorityId = "FNV2", Name = "Fictional demo tax authority", AccountNum = $"APV2-{c.Company}-LOCAL", Location = location.RecId, Email = "tax@example.invalid" }, ct);
    }
}
