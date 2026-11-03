using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory.InventTable;
using Microsoft.EntityFrameworkCore;

using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Inventory;

internal static class EcoResDistinctProductSeeder
{
    internal static Task<EcoResDistinctProduct> SeedAsync(DbContext db, long product, CancellationToken ct) =>
        ExampleSeedStore.EnsureAsync(db, x => x.RecId == product, EcoResDistinctProductDemoData.Create(product), ct);
}
