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

internal static class EcoResProductCategorySeeder
{
    internal static Task<EcoResProductCategory> SeedAsync(DbContext db, long hierarchyId, long categoryId, long productId, CancellationToken ct) =>
        ExampleSeedStore.EnsureAsync(db, x => x.Product == productId && x.Category == categoryId, EcoResProductCategoryDemoData.Create(hierarchyId, categoryId, productId), ct);

}
