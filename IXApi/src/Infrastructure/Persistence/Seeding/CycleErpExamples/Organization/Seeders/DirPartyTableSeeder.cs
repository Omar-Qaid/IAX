using System.Linq.Expressions;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;
using IAX.IXApi.Modules.Finance.Inventory.ProductCategories.Entities;
using IAX.IXApi.Modules.Finance.Inventory.InventTable;
using IAX.IXApi.Modules.Finance.Inventory.ProductLifecycle.Entities;
using Microsoft.EntityFrameworkCore;

using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Organization;

internal static class DirPartyTableSeeder
{
    internal static Task<DirPartyTable> SeedAsync(DbContext db, CancellationToken ct) =>
        ExampleSeedStore.EnsureAsync(db, x => x.PartyNumber == "DEMO-CONTACT-01", DirPartyTableDemoData.Create(), ct);

    internal static async Task<DirPartyTable> SeedCustomerPartyAsync(DbContext db,
        string number, string name, string alias, CancellationToken ct)
    {
        var party = await ExampleSeedStore.EnsureAsync(db, x => x.PartyNumber == number,
            DirPartyTableDemoData.CreateCustomerParty(number, name, alias), ct);
        if (party.DataAreaId != "dat")
            throw new InvalidOperationException("The demo customer party belongs to another company; it was preserved.");
        return party;
    }
}
