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
            var rows = await Db.Set<CustSettlement>().AsNoTracking()
                .Where(x => x.DataAreaId == area && x.SettlementVoucher == voucher).ToListAsync(ct);
            if (rows.Count == 0)
                return await Fail(404, $"Settlement voucher '{voucher}' was not found in the selected company.", ct);

            var date = (reversalDate ?? DateTime.UtcNow).Date;
            var reversalVoucher = (await _numbers.NextAsync("CustSettlement", area, ct)).Code;
            var error = await CustSettlementRestore.ApplyAsync(Db, area, voucher, date, reversalVoucher, ct);
            if (error != null) return await Fail(422, error, ct);

            await _unitOfWork.CompleteAsync(ct);

            // Read back after the save: the restore mutates tracked entities, which an
            // AsNoTracking read taken before the save would not see.
            var touched = rows.Select(r => r.TransRecId).ToList();
            var restored = await Db.Set<CustTrans>().AsNoTracking()
                .Where(x => x.DataAreaId == area && touched.Contains(x.RecId))
                .Select(x => new { x.RecId, invoice = x.Invoice, open = x.AmountCur - x.SettleAmountCur })
                .ToListAsync(ct);

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
