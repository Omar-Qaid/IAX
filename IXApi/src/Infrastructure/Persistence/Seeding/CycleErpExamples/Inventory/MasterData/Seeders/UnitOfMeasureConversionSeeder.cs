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

internal static class UnitOfMeasureConversionSeeder
{
    internal static Task<UnitOfMeasureConversion> SeedAsync(DbContext db, long product, long from, long to, decimal factor, string company, CancellationToken ct) =>
        ExampleSeedStore.EnsureAsync(db, x => x.Product == product && ((x.FromUnitOfMeasure == from && x.ToUnitOfMeasure == to) || (x.FromUnitOfMeasure == to && x.ToUnitOfMeasure == from)), UnitOfMeasureConversionDemoData.Create(product, from, to, factor, company), ct);
}
