using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using IAX.IXApi.Modules.Finance.Foundation.CashDiscount;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

/// <summary>
/// Selects open vendor invoices for payment, the read side of
/// *Accounts payable &gt; Payments &gt; Vendor payment journal &gt; Payment proposal*.
/// </summary>
/// <remarks>
/// <para>
/// <b>This posts nothing.</b> It opens no transaction, takes no lock and writes no row: a proposal
/// in D365 is a selection to review before posting the journal, and generating one neither posts
/// the journal nor moves money out of the bank. Paying the proposed lines is still
/// <see cref="IVendPaymentService"/>'s job, which is why each line carries its
/// <c>VendTrans.RecId</c>.
/// </para>
/// <para>
/// The cash discount is resolved through the same <see cref="CashDiscountResolver"/> and
/// <see cref="CashDiscountCalculator"/> that <c>VendPaymentService</c> uses, so a proposal cannot
/// promise a discount the payment would then decline to take. As there, the discount is stated for
/// settling the <b>whole</b> invoice: a part payment earns none.
/// </para>
/// <para>
/// Only credit balances are proposed. A vendor's debit open rows - an unapplied payment or a credit
/// note - are deliberately left out: those are settled against invoices through
/// <see cref="IVendSettleOpenTransactionsService"/> and are not something to pay out again.
/// </para>
/// </remarks>
public sealed class VendPaymentProposalService(IUnitOfWork unitOfWork, ICompanyExecutionContext company)
    : IVendPaymentProposalService
{
    private DbContext Db => unitOfWork.Context;

    public async Task<PurchResult> ProposeAsync(VendPaymentProposalRequestDto input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        var area = company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(area)) return new(400, null, "A company must be selected.");

        var proposalDate = (input.ProposalDate ?? DateTime.UtcNow).Date;
        var vendAccount = input.VendAccount?.Trim();
        var currency = input.CurrencyCode?.Trim();
        var paymMode = input.PaymMode?.Trim();

        if (!string.IsNullOrEmpty(vendAccount) && !await Db.Set<VendTable>().AsNoTracking()
                .AnyAsync(x => x.DataAreaId == area && x.AccountNum == vendAccount, ct))
            return new(404, null, $"Vendor '{vendAccount}' was not found in the selected company.");
        if (!string.IsNullOrEmpty(paymMode) && !await Db.Set<VendPaymModeTable>().AsNoTracking()
                .AnyAsync(x => x.DataAreaId == area && x.PaymMode == paymMode, ct))
            return new(404, null, $"Method of payment '{paymMode}' was not found in the selected company.");

        // Open rows joined to their transactions. A payable open amount is negative, so a credit
        // balance is what is owed; see the remarks on why debit rows are excluded.
        var open = await (from row in Db.Set<VendTransOpen>().AsNoTracking()
                          join trans in Db.Set<VendTrans>().AsNoTracking() on row.RefRecId equals trans.RecId
                          where row.DataAreaId == area && trans.DataAreaId == area && row.AmountCur < 0m
                            && (vendAccount == null || vendAccount == "" || trans.AccountNum == vendAccount)
                            && (currency == null || currency == "" || trans.CurrencyCode == currency)
                          select new { Open = row, Trans = trans }).ToListAsync(ct);
        if (input.DueBy is DateTime dueBy)
            open = open.Where(x => x.Trans.DueDate.Date <= dueBy.Date).ToList();

        var accounts = open.Select(x => x.Trans.AccountNum).Distinct().ToList();
        var vendors = await Db.Set<VendTable>().AsNoTracking()
            .Where(x => x.DataAreaId == area && accounts.Contains(x.AccountNum))
            .Select(x => new { x.AccountNum, x.Blocked, x.PaymMode }).ToListAsync(ct);

        var excluded = new List<string>();
        var lines = new List<VendPaymentProposalLineDto>();
        foreach (var candidate in open.OrderBy(x => x.Trans.DueDate).ThenBy(x => x.Trans.RecId))
        {
            var vendor = vendors.FirstOrDefault(x => x.AccountNum == candidate.Trans.AccountNum);
            if (vendor == null) continue;

            // D365 keeps a vendor blocked for payment out of the proposal entirely.
            if (vendor.Blocked is CustVendBlocked.All or CustVendBlocked.Payment)
            {
                if (!excluded.Contains(vendor.AccountNum)) excluded.Add(vendor.AccountNum);
                continue;
            }
            // The method of payment is the vendor's, which is what the payment itself defaults from.
            if (!string.IsNullOrEmpty(paymMode) && vendor.PaymMode != paymMode) continue;

            var openAmount = -candidate.Open.AmountCur;
            var terms = await CashDiscountResolver.ResolveAsync(Db, area, candidate.Trans.CashDiscCode,
                candidate.Trans.CashDiscBaseDate == default ? candidate.Trans.TransDate : candidate.Trans.CashDiscBaseDate,
                openAmount, ct);
            var discount = terms.Applies && CashDiscountCalculator.IsWithinPeriod(proposalDate, terms.DiscountDate)
                ? terms.PossibleAmount : 0m;

            lines.Add(new VendPaymentProposalLineDto(
                candidate.Trans.RecId, candidate.Trans.AccountNum, candidate.Trans.Invoice,
                candidate.Trans.Voucher, candidate.Trans.TransDate.Date, candidate.Trans.DueDate.Date,
                candidate.Trans.CurrencyCode, vendor.PaymMode, openAmount,
                candidate.Trans.CashDiscCode, terms.DiscountDate, discount, openAmount - discount));
        }

        var proposal = new VendPaymentProposalDto(area, proposalDate, lines.AsReadOnly(), excluded.AsReadOnly());
        return new(200, proposal, lines.Count == 0
            ? "No open vendor invoices match the payment proposal criteria."
            : $"{lines.Count} open vendor invoice(s) proposed for payment.");
    }
}
