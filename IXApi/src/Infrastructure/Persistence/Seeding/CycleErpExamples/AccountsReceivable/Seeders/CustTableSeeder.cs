using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Organization;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.AccountsReceivable;

internal static class CustTableSeeder
{
    internal static async Task SeedAsync(DbContext db, CancellationToken ct)
    {
        foreach (var seed in CustTableDemoData.Customers)
        {
            // Existing customers, including soft-deleted records, remain user-owned.
            if (await db.Set<CustTable>().IgnoreQueryFilters().AnyAsync(x =>
                x.DataAreaId == "dat" && x.AccountNum == seed.Account, ct)) continue;
            var party = await DirPartyTableSeeder.SeedCustomerPartyAsync(db,
                seed.PartyNumber, seed.Name, seed.NameAlias, ct);
            await ExampleSeedStore.EnsureAsync(db, x => x.DataAreaId == "dat" && x.AccountNum == seed.Account,
                CustTableDemoData.Create(seed.Account, party.RecId, seed.Group, seed.Currency), ct);
        }
    }
}
