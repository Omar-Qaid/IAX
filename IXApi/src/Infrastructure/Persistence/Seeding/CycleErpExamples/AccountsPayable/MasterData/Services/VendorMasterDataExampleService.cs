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

internal sealed class VendorMasterDataExampleService
{
    internal async Task SeedAsync(ApplicationDbContext db, UserManager<AspNetUser> users, CancellationToken ct)
    {
        var sysUser = await users.FindByNameAsync("sys");
        var createdBy = sysUser?.Id ?? "sys";
        var exchange = await db.Set<ExchangeRateType>().IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.DataAreaId == "dat" && x.Name == "Default", ct);
        await VendGroupSeeder.SeedAsync(db, exchange?.RecId, createdBy, ct);
        await VendTableSeeder.SeedAsync(db, createdBy, ct);
    }
}
