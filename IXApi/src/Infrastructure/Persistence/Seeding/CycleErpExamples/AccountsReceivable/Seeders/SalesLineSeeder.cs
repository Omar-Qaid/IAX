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

internal static class SalesLineSeeder
{
    internal static async Task<SalesLine> SeedAsync(ErpCycleExampleContext context, SalesOrderLineService lines, SalesTable order, CancellationToken ct)
    {
        var db = context.Db;
        var line = await db.Set<SalesLine>().IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.DataAreaId == Company && x.SalesId == order.SalesId, ct);
        if (line == null)
        {
            var result = await lines.AddAsync(order.RecId, SalesLineDemoData.Create(context.Date), ct);
            if (result.Status != SalesLineWriteStatus.Success) throw new InvalidOperationException(result.Error);
            line = await db.Set<SalesLine>().SingleAsync(x => x.SalesId == order.SalesId, ct);
        }
        if (line.IsDeleted || line.ItemId != Item || line.SalesQty != SalesQuantity || line.QtyOrdered != SalesQuantity
            || line.SalesPrice != SalesPrice || line.SalesUnit != Unit || line.PriceUnit != 1m || line.CustAccount != Customer)
            throw new InvalidOperationException("ERP sales example lines were customized; no further posting was attempted.");
        await ErpCycleStorageIntegrity.RequireAsync(db, line.InventDimId, ct);
        return line;
    }
}
