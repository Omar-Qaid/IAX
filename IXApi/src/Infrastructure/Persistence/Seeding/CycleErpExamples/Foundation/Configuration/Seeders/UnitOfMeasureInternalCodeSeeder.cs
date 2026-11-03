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

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Foundation;

internal static class UnitOfMeasureInternalCodeSeeder
{
    internal static Task<UnitOfMeasureInternalCode> SeedAsync(DbContext db, long unitId, CancellationToken ct) =>
        ExampleSeedStore.EnsureAsync(db, x => x.UnitOfMeasure == unitId && x.CodeSymbol == InternalCodeSymbol.Symbol, UnitOfMeasureInternalCodeDemoData.Create(unitId), ct);

}
