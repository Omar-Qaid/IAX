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

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Inventory;

internal static class EcoResCategorySeeder
{
    internal static Task<EcoResCategory> SeedAsync(DbContext db, long hierarchyId, CancellationToken ct) =>
        ExampleSeedStore.EnsureAsync(db, x => x.CategoryHierarchy == hierarchyId && x.Name == "Office supplies", EcoResCategoryDemoData.Create(hierarchyId), ct);

    internal static Task<EcoResCategory> SeedVariant2Async(DbContext db, long hierarchyId, long parentId, CancellationToken ct) =>
        ExampleSeedStore.EnsureAsync(db, x => x.CategoryHierarchy == hierarchyId && x.Name == "Paper and stationery", EcoResCategoryDemoData.CreateVariant2(hierarchyId, parentId), ct);

}
