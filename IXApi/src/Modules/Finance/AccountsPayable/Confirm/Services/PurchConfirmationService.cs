using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

/// <summary>
/// Purchase order confirmation: stores an immutable VendPurchOrderJour/Trans snapshot per version.
/// Confirmation never receives stock (D365: no physical or financial inventory effect).
/// </summary>
public sealed class PurchConfirmationService : BaseService<VendPurchOrderJour>, IPurchConfirmationService
{
    private readonly ICompanyExecutionContext _company;
    private readonly ISysNumberSequenceService _numbers;

    private DbContext Db => _unitOfWork.Context;

    public PurchConfirmationService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        ICompanyExecutionContext company,
        ISysNumberSequenceService numbers)
        : base(unitOfWork, currentUser)
    {
        _company = company;
        _numbers = numbers;
    }

    public PurchConfirmationService(
        IUnitOfWork unitOfWork,
        ICompanyExecutionContext company,
        ISysNumberSequenceService numbers)
        : this(unitOfWork, new NullCurrentUserService(company), company, numbers)
    {
    }

    private sealed class NullCurrentUserService(ICompanyExecutionContext company) : ICurrentUserService
    {
        public string GetCurrentUserId() => string.Empty;
        public string GetOwnerAccountId() => string.Empty;
        public string GetDataAreaId() => company.GetDataAreaId();
    }

    public async Task<PurchResult> ConfirmAsync(long recId, CancellationToken ct)
    {
        var area = _company.GetDataAreaId();
        var order = await Db.Set<PurchTable>().FirstOrDefaultAsync(x => x.RecId == recId && x.DataAreaId == area, ct);
        if (order == null) return new(404, null, "Purchase order was not found in the selected company.");
        if (order.PurchStatus == PurchStatus.Canceled)
            return new(422, null, "A canceled purchase order cannot be confirmed.");
        var lines = await Db.Set<PurchLine>().Where(x => x.DataAreaId == area && x.PurchId == order.PurchId
            && x.PurchStatus != PurchStatus.Canceled).ToListAsync(ct);
        if (lines.Count == 0) return new(422, null, "The purchase order has no lines to confirm.");

        var docNum = (await _numbers.NextAsync("VendPurchOrderJour", area, ct)).Code;
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
        await _unitOfWork.CompleteAsync(ct);
        return new(200, journal, "Purchase order confirmed.");
    }
}
