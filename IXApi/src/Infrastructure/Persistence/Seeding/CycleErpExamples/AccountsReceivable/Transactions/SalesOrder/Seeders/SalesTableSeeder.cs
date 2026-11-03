using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Confirm;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Invoice;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip.Dtos;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Payment;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Services;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using static IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.ErpCycleExampleData;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.AccountsReceivable;

internal static class SalesTableSeeder
{
    internal static async Task<SalesTable> SeedAsync(ErpCycleExampleContext context, SalesOrderQuickCreateService orders, CancellationToken ct)
    {
        var db = context.Db;
        var order = await db.Set<SalesTable>().IgnoreQueryFilters().SingleOrDefaultAsync(x =>
            x.DataAreaId == Company && x.CustomerRef == Sales, ct);
        if (order == null)
        {
            var result = await orders.CreateAsync(SalesTableDemoData.Create(context.Date), ct);
            if (result.Error != null) throw new InvalidOperationException(result.Error);
            order = await db.Set<SalesTable>().SingleAsync(x => x.CustomerRef == Sales, ct);
        }
        if (order.IsDeleted || order.CustAccount != Customer || order.InvoiceAccount != Customer || order.CurrencyCode != CurrencyCode
            || order.InventSiteId != Site || order.InventLocationId != Warehouse || order.ReturnItemNum != "")
            throw new InvalidOperationException("ERP sales example conflicts with an existing order; it was preserved.");
        return order;
    }
}
