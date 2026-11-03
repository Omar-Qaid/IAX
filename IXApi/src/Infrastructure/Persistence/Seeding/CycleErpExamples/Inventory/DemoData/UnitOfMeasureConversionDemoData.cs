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

internal static class UnitOfMeasureConversionDemoData
{
    internal static UnitOfMeasureConversion Create(long product, long from, long to, decimal factor, string company) =>
        new UnitOfMeasureConversion { Product = product, FromUnitOfMeasure = from, ToUnitOfMeasure = to, Factor = factor, Numerator = 1, Denominator = 1, DataAreaId = company, IsActive = true, CreatedBy = "sys", OwnerAccountId = "sys" };
}
