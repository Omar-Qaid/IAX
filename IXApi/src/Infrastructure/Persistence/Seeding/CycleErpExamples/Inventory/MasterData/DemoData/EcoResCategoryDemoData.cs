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


namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Inventory;

internal static class EcoResCategoryDemoData
{
    internal static EcoResCategory Create(long hierarchyId) =>
        new EcoResCategory { CategoryHierarchy = hierarchyId, Name = "Office supplies" };

    internal static EcoResCategory CreateVariant2(long hierarchyId, long parentId) =>
        new EcoResCategory { CategoryHierarchy = hierarchyId, ParentCategory = parentId, Name = "Paper and stationery" };

}
