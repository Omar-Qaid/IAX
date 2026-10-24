using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public interface IPurchConfirmationService
{
    Task<PurchResult> ConfirmAsync(long recId, CancellationToken ct);
}

/// <summary>
/// Purchase order confirmation: stores an immutable VendPurchOrderJour/Trans snapshot per version.
/// Confirmation never receives stock (D365: no physical or financial inventory effect).
/// </summary>
public sealed class PurchConfirmationService(IUnitOfWork unitOfWork, ICompanyExecutionContext company,
    ISysNumberSequenceService numbers) : IPurchConfirmationService
{
    private DbContext Db => unitOfWork.Context;

    public async Task<PurchResult> ConfirmAsync(long recId, CancellationToken ct)
    {
        var area = company.GetDataAreaId();
        var order = await Db.Set<PurchTable>().FirstOrDefaultAsync(x => x.RecId == recId && x.DataAreaId == area, ct);
        if (order == null) return new(404, null, "Purchase order was not found in the selected company.");
        if (order.PurchStatus == PurchStatus.Canceled)
            return new(422, null, "A canceled purchase order cannot be confirmed.");
        var lines = await Db.Set<PurchLine>().Where(x => x.DataAreaId == area && x.PurchId == order.PurchId
            && x.PurchStatus != PurchStatus.Canceled).ToListAsync(ct);
        if (lines.Count == 0) return new(422, null, "The purchase order has no lines to confirm.");

        var docNum = (await numbers.NextAsync("VendPurchOrderJour", area, ct)).Code;
        var journal = new VendPurchOrderJour
        {
            DataAreaId = area, PurchId = order.PurchId, PurchOrderDocNum = docNum, PurchOrderDate = DateTime.UtcNow.Date,
            OrderAccount = order.OrderAccount, InvoiceAccount = order.InvoiceAccount, CurrencyCode = order.CurrencyCode,
            Qty = lines.Sum(x => x.PurchQty), Amount = lines.Sum(x => x.LineAmount)
        };
        Db.Set<VendPurchOrderJour>().Add(journal);
        foreach (var line in lines.OrderBy(x => x.LineNumber))
            Db.Set<VendPurchOrderTrans>().Add(new VendPurchOrderTrans
            {
                DataAreaId = area, PurchId = order.PurchId, PurchOrderDocNum = docNum, LineNumber = line.LineNumber,
                ItemId = line.ItemId, Qty = line.PurchQty, PurchUnit = line.PurchUnit, PurchPrice = line.PurchPrice,
                PriceUnit = line.PriceUnit, LineAmount = line.LineAmount,
                InventTransId = line.InventTransId, InventDimId = line.InventDimId
            });
        if (order.DocumentStatus is DocumentStatus.None or DocumentStatus.PurchaseOrder)
            order.DocumentStatus = DocumentStatus.PurchaseOrder;
        await unitOfWork.CompleteAsync(ct);
        return new(200, journal, "Purchase order confirmed.");
    }
}
