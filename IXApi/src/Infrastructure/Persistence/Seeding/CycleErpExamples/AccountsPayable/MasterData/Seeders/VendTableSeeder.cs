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

using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Organization;
namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.AccountsPayable;

internal static class VendTableSeeder
{
    internal static async Task SeedAsync(DbContext db, string createdBy, CancellationToken ct)
    {
        var vendorSeeds = VendTableDemoData.Vendors;
            var existingVendors = await db.Set<VendTable>()
                .IgnoreQueryFilters().Where(x => x.DataAreaId == "dat")
                .Where(v => v.DataAreaId == "dat")
                .ToDictionaryAsync(v => v.AccountNum, StringComparer.OrdinalIgnoreCase, ct);

            var vendorPartyNumbers = vendorSeeds.Select(x => x.PartyNumber).ToArray();
            var vendorParties = await db.Set<DirPartyTable>()
                .IgnoreQueryFilters().Where(x => x.DataAreaId == "dat")
                .Where(x => vendorPartyNumbers.Contains(x.PartyNumber))
                .ToDictionaryAsync(x => x.PartyNumber, StringComparer.OrdinalIgnoreCase, ct);

            foreach (var seed in vendorSeeds.Where(x => !existingVendors.ContainsKey(x.Account)))
            {
                if (vendorParties.ContainsKey(seed.PartyNumber)) continue;
                var party = DirPartyTableDemoData.CreateVendorParty(seed.PartyNumber, seed.Name, seed.NameAlias, createdBy);
                db.Set<DirPartyTable>().Add(party);
                vendorParties[seed.PartyNumber] = party;
            }
            await db.SaveChangesAsync(ct);

            var vendsToAdd = new List<VendTable>();
            foreach (var seed in vendorSeeds.Where(x => !existingVendors.ContainsKey(x.Account)))
            {
                vendsToAdd.Add(VendTableDemoData.Create(seed.Account, vendorParties[seed.PartyNumber].RecId,
                    seed.Group, seed.Currency, seed.TaxGroup, seed.PaymTermId, seed.PaymMode, createdBy));
            }

            if (vendsToAdd.Count > 0)
            {
                await db.Set<VendTable>().AddRangeAsync(vendsToAdd, ct);
                await db.SaveChangesAsync(ct);
            }
    }
}
