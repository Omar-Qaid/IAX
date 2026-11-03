using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Services;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using Microsoft.EntityFrameworkCore;
using static IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.ErpCycleExampleData;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.AccountsPayable;

internal static class PurchTableSeeder
{
    internal static async Task<PurchTable> SeedAsync(ErpCycleExampleContext context, PurchOrderService orders, CancellationToken ct)
    {
        var db = context.Db;
        var order = await db.Set<PurchTable>().IgnoreQueryFilters().SingleOrDefaultAsync(x =>
            x.DataAreaId == Company && x.PurchName == Purchase && x.ReturnItemNum == "", ct);
        if (order == null)
        {
            var result = await orders.CreateAsync(PurchTableDemoData.Create(context.Date), ct);
            ErpCycleExampleContext.Require(result.StatusCode, result.Message);
            order = (PurchTable)result.Data!;
        }
        if (order.IsDeleted || order.OrderAccount != Vendor || order.InvoiceAccount != Vendor || order.CurrencyCode != CurrencyCode
            || order.InventSiteId != Site || order.InventLocationId != Warehouse || order.PurchaseType != PurchaseType.Purch)
            throw new InvalidOperationException("ERP purchase example conflicts with an existing order; it was preserved.");
        return order;
    }
}
