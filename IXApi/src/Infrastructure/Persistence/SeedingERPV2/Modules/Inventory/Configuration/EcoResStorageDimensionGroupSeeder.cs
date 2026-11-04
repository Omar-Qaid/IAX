using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using Microsoft.EntityFrameworkCore;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;
public sealed class EcoResStorageDimensionGroupSeeder : IDataSeeder
{
    public string Id => "ERPV2.EcoResStorageDimensionGroupSeeder";
    public IReadOnlyCollection<string> Dependencies => [];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var name = $"APV2-{c.Company}-STORE";
        await c.EnsureAsync<EcoResStorageDimensionGroup>(x => x.Name == name,
            new() { Name = name, Description = "Site and warehouse; no locations" }, ct);
    }
}
