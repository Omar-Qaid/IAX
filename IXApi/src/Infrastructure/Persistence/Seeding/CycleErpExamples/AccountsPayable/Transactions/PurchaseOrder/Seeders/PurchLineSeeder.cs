using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Services;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using Microsoft.EntityFrameworkCore;
using static IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.ErpCycleExampleData;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.AccountsPayable;

internal static class PurchLineSeeder
{
    internal static async Task<PurchLine> SeedAsync(ErpCycleExampleContext context, PurchOrderService orders, PurchTable order, CancellationToken ct)
    {
        var db = context.Db;
        var line = await db.Set<PurchLine>().IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.DataAreaId == Company && x.PurchId == order.PurchId, ct);
        if (line == null)
        {
            var result = await orders.AddLineAsync(order.RecId, PurchLineDemoData.Create(context.Date), ct);
            ErpCycleExampleContext.Require(result.StatusCode, result.Message);
            line = await db.Set<PurchLine>().SingleAsync(x => x.PurchId == order.PurchId, ct);
        }
        if (line.IsDeleted || line.ItemId != Item || line.QtyOrdered != PurchaseQuantity || line.PurchQty != PurchaseQuantity
            || line.PurchPrice != PurchasePrice || line.PurchUnit != Unit || line.PriceUnit != 1m
            || line.CurrencyCode != CurrencyCode || line.VendAccount != Vendor || line.LineAmount != PurchaseQuantity * PurchasePrice)
            throw new InvalidOperationException("ERP purchase example lines were customized; no further posting was attempted.");
        await ErpCycleStorageIntegrity.RequireAsync(db, line.InventDimId, ct);
        return line;
    }
}
