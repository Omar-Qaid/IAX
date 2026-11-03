using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Modules.Identity.Roles;
using IAX.IXApi.Modules.Identity.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.AccountsPayable;

internal static class VendGroupSeeder
{
    internal static async Task SeedAsync(DbContext db, long? defaultExchTypeId, string createdBy, CancellationToken ct)
    {
        var vendGroupSeeds = VendGroupDemoData.Create(defaultExchTypeId, createdBy);
            var existingVendGroupCodes = await db.Set<VendGroup>()
                .IgnoreQueryFilters().Where(x => x.DataAreaId == "dat")
                .Select(g => g.VendGroupCode)
                .ToListAsync(ct);

            var vendGroupsToAdd = vendGroupSeeds.Where(g => !existingVendGroupCodes.Contains(g.VendGroupCode)).ToList();
            if (vendGroupsToAdd.Count > 0)
            {
                await db.Set<VendGroup>().AddRangeAsync(vendGroupsToAdd, ct);
                await db.SaveChangesAsync(ct);
            }

    }
}
