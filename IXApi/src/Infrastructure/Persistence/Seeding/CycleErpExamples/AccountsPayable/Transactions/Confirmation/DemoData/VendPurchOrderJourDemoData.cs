using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Services;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using Microsoft.EntityFrameworkCore;
using static IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.ErpCycleExampleData;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.AccountsPayable;

internal static class VendPurchOrderJourDemoData
{
    // Expected values only: confirmation services create the persisted header and lines.
    internal static VendPurchOrderJour Expected(PurchTable order, PurchLine line) => new()
    {
        OrderAccount = order.OrderAccount, InvoiceAccount = order.InvoiceAccount,
        CurrencyCode = order.CurrencyCode, Qty = line.PurchQty, Amount = line.LineAmount
    };
}
