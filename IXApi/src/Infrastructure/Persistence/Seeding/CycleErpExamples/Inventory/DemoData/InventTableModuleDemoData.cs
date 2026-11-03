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

internal static class InventTableModuleDemoData
{
    internal static InventTableModule Create(string code, ModuleInventPurchSales module, string unit, decimal price) =>
        new InventTableModule { ItemId = code, DataAreaId = "dat", ModuleType = module, UnitId = unit, Price = price, PriceUnit = 1m, LineDisc = "", TaxItemGroupId = "FULL", IsActive = true, CreatedBy = "sys", OwnerAccountId = "sys" };
}
