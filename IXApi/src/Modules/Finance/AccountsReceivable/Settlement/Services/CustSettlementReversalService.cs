using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

using IAX.IXApi.Modules.Finance.AccountsReceivable.Payment;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Settlement;

/// <summary>
/// Reverses a posted customer settlement, independently of the payment that created it. The
/// original CustSettlement rows are kept as history and offsetting rows are written under a new
/// settlement voucher, then CustTrans and CustTransOpen are restored so the invoice is open again.
/// </summary>
/// <remarks>
/// <para>
/// No ledger voucher is produced, and that is correct rather than missing: settlement in this
/// module posts nothing to the general ledger. The payment's voucher carries Dr Bank / Cr Customer,
/// and settlement only moves the open receivable. Reversing it therefore has nothing to counter-post.
/// Cash discount, realised exchange adjustment and penny difference — the things that would make a
/// settlement post — are not implemented here, so a settlement carrying any of them is refused
/// rather than reversed with its ledger effect left behind.
/// </para>
/// <para>
/// The open row is rebuilt from the invariant the payment path already asserts before it settles:
/// CustTransOpen.AmountCur equals CustTrans.AmountCur less CustTrans.SettleAmountCur. That makes the
/// restore exact whether the row was decremented or removed when it was fully settled.
/// </para>
/// </remarks>
public sealed class CustSettlementReversalService : BaseService<CustSettlement>, ICustSettlementReversalService
{
    private readonly ICompanyExecutionContext _company;
    private readonly ISysNumberSequenceService _numbers;

    private DbContext Db => _unitOfWork.Context;

    public CustSettlementReversalService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        ICompanyExecutionContext company,
        ISysNumberSequenceService numbers)
        : base(unitOfWork, currentUser)
    {
        _company = company;
        _numbers = numbers;
    }

    public CustSettlementReversalService(
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

    public Task<CustPaymentResult> ReverseAsync(string settlementVoucher, DateTime? reversalDate, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => ReverseCoreAsync(settlementVoucher, reversalDate, ct));

    private async Task<CustPaymentResult> ReverseCoreAsync(string settlementVoucher, DateTime? reversalDate, CancellationToken ct)
    {
        var area = _company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(area)) return new(400, null, "A company must be selected.");
        var voucher = (settlementVoucher ?? string.Empty).Trim();
        if (voucher.Length == 0) return new(400, null, "A settlement voucher is required.");

        var support = new SubledgerPostingSupport(Db);
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            await support.LockAsync($"CustSettlement:{area}:{voucher}".ToUpperInvariant(), ct);
            var rows = await Db.Set<CustSettlement>()
                .Where(x => x.DataAreaId == area && x.SettlementVoucher == voucher).ToListAsync(ct);
            if (rows.Count == 0)
                return await Fail(404, $"Settlement voucher '{voucher}' was not found in the selected company.", ct);
            if (rows.Any(x => x.CanBeReversed != NoYes.Yes))
                return await Fail(422, $"Settlement '{voucher}' cannot be reversed: it is either already reversed, or it was "
                    + "settled before settlement reversal was supported and is not marked reversible.", ct);
            if (rows.Any(x => x.UtilizedCashDisc != 0m || x.ExchAdjustment != 0m || x.PennyDiff != 0m))
                return await Fail(422, $"Settlement '{voucher}' carries a cash discount, exchange adjustment or penny "
                    + "difference, which post to the ledger and cannot be reversed by this path.", ct);

            var date = (reversalDate ?? DateTime.UtcNow).Date;
            var reversalVoucher = (await _numbers.NextAsync("CustSettlement", area, ct)).Code;

            // The originals stay as history; the reversal is its own set of rows with the amounts
            // negated. There is no field linking a reversal back to the settlement it reverses, so
            // the pair is identifiable by its Trans/Offset record ids.
            foreach (var row in rows)
            {
                Db.Set<CustSettlement>().Add(new CustSettlement
                {
                    DataAreaId = area, AccountNum = row.AccountNum, TransDate = date,
                    SettlementVoucher = reversalVoucher, TransCompany = row.TransCompany, TransType = row.TransType,
                    TransRecId = row.TransRecId, TransOpen = row.TransOpen, OffsetRecId = row.OffsetRecId,
                    OffsetAccountNum = row.OffsetAccountNum, OffsetCompany = row.OffsetCompany,
                    OffsetTransVoucher = row.OffsetTransVoucher,
                    SettleAmountCur = -row.SettleAmountCur,
                    SettleAmountMst = -row.SettleAmountMst,
                    SettleAmountReporting = -row.SettleAmountReporting,
                    DueDate = row.DueDate, ClosedDate = date,
                    DefaultDimension = row.DefaultDimension,
                    CanBeReversed = NoYes.No
                });
                row.CanBeReversed = NoYes.No;
            }

            // Each transaction the settlement touched gives back exactly what it took.
            var restored = new List<object>();
            foreach (var group in rows.GroupBy(x => x.TransRecId))
            {
                var trans = await Db.Set<CustTrans>()
                    .FirstOrDefaultAsync(x => x.DataAreaId == area && x.RecId == group.Key, ct);
                if (trans == null)
                    return await Fail(422, $"The customer transaction behind settlement '{voucher}' no longer exists.", ct);

                trans.SettleAmountCur -= group.Sum(x => x.SettleAmountCur);
                trans.SettleAmountMst -= group.Sum(x => x.SettleAmountMst);
                if (trans.SettleAmountCur == 0m)
                {
                    trans.LastSettleDate = default;
                    trans.LastSettleVoucher = string.Empty;
                    trans.LastSettleAccountNum = string.Empty;
                }
                var remaining = trans.AmountCur - trans.SettleAmountCur;
                if (remaining != 0m)
                {
                    trans.Closed = default;
                    trans.Settlement = CustSettlementStatus.None;
                }

                var open = await Db.Set<CustTransOpen>()
                    .FirstOrDefaultAsync(x => x.DataAreaId == area && x.RefRecId == trans.RecId, ct);
                if (remaining == 0m)
                {
                    if (open != null) Db.Set<CustTransOpen>().Remove(open);
                }
                else if (open == null)
                {
                    Db.Set<CustTransOpen>().Add(new CustTransOpen
                    {
                        DataAreaId = area, AccountNum = trans.AccountNum, RefRecId = trans.RecId,
                        TransDate = trans.TransDate, DueDate = trans.DueDate, AmountCur = remaining,
                        AmountMst = trans.AmountMst - trans.SettleAmountMst,
                        ReportingCurrencyAmount = trans.ReportingCurrencyAmount - trans.SettleAmountMst
                    });
                }
                else
                {
                    open.AmountCur = remaining;
                    open.AmountMst = trans.AmountMst - trans.SettleAmountMst;
                    open.ReportingCurrencyAmount = trans.ReportingCurrencyAmount - trans.SettleAmountMst;
                }
                restored.Add(new { trans.RecId, invoice = trans.Invoice, open = remaining });
            }

            await _unitOfWork.CompleteAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
            return new(200, new { settlementVoucher = voucher, reversalVoucher, rows = rows.Count, restored },
                "Customer settlement reversed.");
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

    private async Task<CustPaymentResult> Fail(int status, string message, CancellationToken ct)
    {
        await _unitOfWork.RollbackTransactionAsync(ct);
        Db.ChangeTracker.Clear();
        return new(status, null, message);
    }
}
