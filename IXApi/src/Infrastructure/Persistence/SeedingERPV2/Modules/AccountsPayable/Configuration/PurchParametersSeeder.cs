using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using Microsoft.EntityFrameworkCore;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.AccountsPayable;
public sealed class PurchParametersSeeder : IDataSeeder
{
    public string Id => "ERPV2.PurchParametersSeeder";
    public IReadOnlyCollection<string> Dependencies => [];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        await c.EnsureAsync<PurchParameters>(x => x.DataAreaId == c.Company,
            new() { PurchaseType = PurchaseType.Purch }, ct);
    }
}
