using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Services;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using Microsoft.EntityFrameworkCore;
using static IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.ErpCycleExampleData;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.AccountsPayable;

internal sealed class PurchaseConfirmationExampleService
{
    internal async Task SeedAsync(ErpCycleExampleContext context, PurchTable order, PurchLine line, CancellationToken ct)
    {
        var db = context.Db;
        var hasReceipt = await db.Set<VendPackingSlipJour>().IgnoreQueryFilters()
            .AnyAsync(x => x.DataAreaId == Company && x.PurchId == order.PurchId, ct);
        await EnsureConfirmationAsync(db, order, line,
            token => new PurchConfirmationService(context.Work, context.Actor, context.Actor, context.Numbers)
                .ConfirmAsync(order.RecId, token), ct, allowCreate: !hasReceipt);
    }

    internal async Task EnsureConfirmationAsync(DbContext db, PurchTable order, PurchLine line,
        Func<CancellationToken, Task<PurchResult>> confirm, CancellationToken ct = default, bool allowCreate = true)
    {
        if (order.DataAreaId != Company || line.DataAreaId != Company || order.PurchId != line.PurchId)
            throw new InvalidOperationException("ERP purchase confirmation references a different order or company.");
        var existing = await db.Set<VendPurchOrderJour>().IgnoreQueryFilters().SingleOrDefaultAsync(x =>
            x.DataAreaId == Company && x.PurchId == order.PurchId, ct);
        if (existing == null)
        {
            if (!allowCreate)
                throw new InvalidOperationException("A received ERP purchase example has no confirmation snapshot; historical data was preserved.");
            var result = await confirm(ct);
            ErpCycleExampleContext.Require(result.StatusCode, result.Message);
            return;
        }
        var expected = VendPurchOrderJourDemoData.Expected(order, line);
        if (existing.IsDeleted || existing.OrderAccount != expected.OrderAccount || existing.InvoiceAccount != expected.InvoiceAccount
            || existing.CurrencyCode != expected.CurrencyCode || existing.Qty != expected.Qty || existing.Amount != expected.Amount)
            throw new InvalidOperationException("ERP purchase confirmation was customized; the existing snapshot was preserved.");
        var snapshot = await db.Set<VendPurchOrderTrans>().IgnoreQueryFilters().SingleOrDefaultAsync(x =>
            x.DataAreaId == Company && x.PurchId == order.PurchId && x.PurchOrderDocNum == existing.PurchOrderDocNum, ct);
        if (snapshot == null || snapshot.IsDeleted || snapshot.ItemId != line.ItemId || snapshot.Qty != line.PurchQty
            || snapshot.PurchUnit != line.PurchUnit || snapshot.PurchPrice != line.PurchPrice || snapshot.PriceUnit != line.PriceUnit
            || snapshot.LineAmount != line.LineAmount || snapshot.InventTransId != line.InventTransId || snapshot.InventDimId != line.InventDimId)
            throw new InvalidOperationException("ERP purchase confirmation lines conflict with the example; no replacement was created.");
    }
}
