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

internal static class InventCountGroupDemoData
{
    internal static InventCountGroup Create() =>
        new InventCountGroup { CountGroupId = "DEMO-MONTH", Name = "Example monthly count", CountCode = InventCountCode.Period, CountPeriod = 30 };

    internal static InventCountGroup CreateVariant2() =>
        new InventCountGroup { CountGroupId = "DEMO-MAN", Name = "Example manual count", CountCode = InventCountCode.Manual };

}
