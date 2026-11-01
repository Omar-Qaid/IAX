using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

/// <summary>
/// Reverses a posted vendor settlement, independently of the payment that created it. The payable
/// mirror of <c>CustSettlementReversalService</c>: the original VendSettlement rows are kept and
/// offsetting rows are written, then VendTrans and VendTransOpen are restored so the invoice is
/// open again.
/// </summary>
/// <remarks>
/// No ledger voucher is produced, and that is correct rather than missing: settlement posts nothing
/// to the general ledger in this module. The payment's own voucher carries Dr Vendor / Cr Bank, and
/// settlement only moves the open payable.
///
/// Payable amounts are negative (a vendor invoice credits the vendor), so settling an invoice adds
/// to VendTrans.SettleAmountCur and reversing subtracts. The open row is rebuilt through
/// <see cref="VendorOpenBalance"/>, which already derives it from AmountCur less SettleAmountCur —
/// the same invariant the payment path asserts before it settles.
///
/// VendSettlement is keyed by the payment voucher rather than a settlement voucher of its own,
/// because that is the shape the table has; see the remark on the entity.
/// </remarks>
public sealed class VendSettlementReversalService : BaseService<VendSettlement>, IVendSettlementReversalService
{
    private readonly ICompanyExecutionContext _company;

    private DbContext Db => _unitOfWork.Context;

    public VendSettlementReversalService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        ICompanyExecutionContext company)
        : base(unitOfWork, currentUser)
    {
        _company = company;
    }

    public VendSettlementReversalService(IUnitOfWork unitOfWork, ICompanyExecutionContext company)
        : this(unitOfWork, new NullCurrentUserService(company), company)
    {
    }

    private sealed class NullCurrentUserService(ICompanyExecutionContext company) : ICurrentUserService
    {
        public string GetCurrentUserId() => string.Empty;
        public string GetOwnerAccountId() => string.Empty;
        public string GetDataAreaId() => company.GetDataAreaId();
    }

    public Task<PurchResult> ReverseAsync(string voucher, DateTime? reversalDate, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => ReverseCoreAsync(voucher, reversalDate, ct));

    private async Task<PurchResult> ReverseCoreAsync(string settlementVoucher, DateTime? reversalDate, CancellationToken ct)
    {
        var area = _company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(area)) return new(400, null, "A company must be selected.");
        var voucher = (settlementVoucher ?? string.Empty).Trim();
        if (voucher.Length == 0) return new(400, null, "A settlement voucher is required.");

        var support = new SubledgerPostingSupport(Db);
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            await support.LockAsync($"VendSettlement:{area}:{voucher}".ToUpperInvariant(), ct);
            var rows = await Db.Set<VendSettlement>()
                .Where(x => x.DataAreaId == area && x.Voucher == voucher).ToListAsync(ct);
            if (rows.Count == 0)
                return await Fail(404, $"Settlement voucher '{voucher}' was not found in the selected company.", ct);
            if (rows.Any(x => x.CanBeReversed != NoYes.Yes))
                return await Fail(422, $"Settlement '{voucher}' cannot be reversed: it is either already reversed, or it was "
                    + "settled before settlement reversal was supported and is not marked reversible.", ct);

            var date = (reversalDate ?? DateTime.UtcNow).Date;
            var reversalVoucher = $"{voucher}-REV";
            if (await Db.Set<VendSettlement>().AnyAsync(x => x.DataAreaId == area && x.Voucher == reversalVoucher, ct))
                return await Fail(422, $"Settlement '{voucher}' has already been reversed under '{reversalVoucher}'.", ct);

            foreach (var row in rows)
            {
                Db.Set<VendSettlement>().Add(new VendSettlement
                {
                    DataAreaId = area, AccountNum = row.AccountNum,
                    InvoiceTransRecId = row.InvoiceTransRecId, PaymentTransRecId = row.PaymentTransRecId,
                    TransDate = date, Voucher = reversalVoucher,
                    AmountCur = -row.AmountCur, AmountMst = -row.AmountMst,
                    CanBeReversed = NoYes.No
                });
                row.CanBeReversed = NoYes.No;
            }

            // Both sides of each match give back what the settlement took: the invoice and the
            // payment transaction each carry their own SettleAmount.
            var restored = new List<object>();
            var touched = rows.Select(x => x.InvoiceTransRecId)
                .Concat(rows.Select(x => x.PaymentTransRecId)).Distinct().ToList();
            foreach (var recId in touched)
            {
                var trans = await Db.Set<VendTrans>()
                    .FirstOrDefaultAsync(x => x.DataAreaId == area && x.RecId == recId, ct);
                if (trans == null)
                    return await Fail(422, $"The vendor transaction behind settlement '{voucher}' no longer exists.", ct);

                var asInvoice = rows.Where(x => x.InvoiceTransRecId == recId).ToList();
                var asPayment = rows.Where(x => x.PaymentTransRecId == recId).ToList();
                // Settling adds on the invoice side and subtracts on the payment side, so the
                // reversal does the opposite of each.
                trans.SettleAmountCur += asInvoice.Sum(x => x.AmountCur) - asPayment.Sum(x => x.AmountCur);
                trans.SettleAmountMst += asInvoice.Sum(x => x.AmountMst) - asPayment.Sum(x => x.AmountMst);

                var remaining = trans.AmountCur - trans.SettleAmountCur;
                if (remaining != 0m) trans.Closed = default;

                var open = await Db.Set<VendTransOpen>()
                    .FirstOrDefaultAsync(x => x.DataAreaId == area && x.RefRecId == trans.RecId, ct);
                if (remaining == 0m)
                {
                    if (open != null) Db.Set<VendTransOpen>().Remove(open);
                }
                else if (open == null)
                {
                    VendorOpenBalance.Add(Db, trans);
                }
                else
                {
                    open.AmountCur = remaining;
                    open.AmountMst = trans.AmountMst - trans.SettleAmountMst;
                }
                restored.Add(new { trans.RecId, invoice = trans.Invoice, open = remaining });
            }

            await _unitOfWork.CompleteAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
            return new(200, new { settlementVoucher = voucher, reversalVoucher, rows = rows.Count, restored },
                "Vendor settlement reversed.");
        }
        catch (SubledgerPostingException ex)
        {
            return await Fail(422, ex.Message, ct);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            Db.ChangeTracker.Clear();
            throw;
        }
    }

    private async Task<PurchResult> Fail(int status, string message, CancellationToken ct)
    {
        await _unitOfWork.RollbackTransactionAsync(ct);
        Db.ChangeTracker.Clear();
        return new(status, null, message);
    }
}
