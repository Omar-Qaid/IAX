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
            var rows = await Db.Set<VendSettlement>().AsNoTracking()
                .Where(x => x.DataAreaId == area && x.Voucher == voucher).ToListAsync(ct);
            if (rows.Count == 0)
                return await Fail(404, $"Settlement voucher '{voucher}' was not found in the selected company.", ct);

            var date = (reversalDate ?? DateTime.UtcNow).Date;
            var reversalVoucher = $"{voucher}-REV";
            var error = await VendSettlementRestore.ApplyAsync(Db, area, voucher, date, reversalVoucher, ct);
            if (error != null) return await Fail(422, error, ct);

            await _unitOfWork.CompleteAsync(ct);

            // Read back after the save: the restore mutates tracked entities, which an
            // AsNoTracking read taken before the save would not see.
            var touched = rows.Select(x => x.InvoiceTransRecId)
                .Concat(rows.Select(x => x.PaymentTransRecId)).Distinct().ToList();
            var restored = await Db.Set<VendTrans>().AsNoTracking()
                .Where(x => x.DataAreaId == area && touched.Contains(x.RecId))
                .Select(x => new { x.RecId, invoice = x.Invoice, open = x.AmountCur - x.SettleAmountCur })
                .ToListAsync(ct);

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
