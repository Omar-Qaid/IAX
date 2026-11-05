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
public sealed class PaymSchedLineSeeder : IDataSeeder
{
    public string Id => "ERPV2.Foundation.PaymSchedLineSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.Foundation.PaymSchedSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        await c.RequireAsync<PaymSched>(x => x.Name == "FNV2", ct);
        for (var i = 1; i <= 2; i++)
            await c.EnsureAsync<PaymSchedLine>(x => x.Name == "FNV2" && x.LineNum == i, new() { Name = "FNV2", LineNum = i, Value = 50m, Qty = i * 30 }, ct);
    }
}
