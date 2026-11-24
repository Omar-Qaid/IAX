using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Foundation.PaymentTerms;

/// <summary>One open row (CustTransOpen/VendTransOpen) of a posted invoice, in all three currencies.</summary>
public sealed record OpenRowSplit(DateTime DueDate, decimal AmountCur, decimal AmountMst, decimal ReportingAmount);

/// <summary>
/// As in D365, an invoice whose terms of payment carry a payment schedule gets one open transaction per
/// installment, each with its own due date; without a schedule it gets a single open row. Installments come from
/// <see cref="PaymentScheduleCalculator"/> in the transaction currency. The accounting and reporting amounts are
/// split in the same proportion, and the last installment takes the remainder so every currency adds up exactly.
/// </summary>
internal static class PaymentScheduleOpenRows
{
    public static async Task<IReadOnlyList<OpenRowSplit>> SplitAsync(DbContext db, string area, string paymTermId,
        DateTime dueDate, decimal amountCur, decimal amountMst, decimal reportingAmount, CancellationToken ct)
    {
        var single = new[] { new OpenRowSplit(dueDate.Date, amountCur, amountMst, reportingAmount) };
        if (string.IsNullOrWhiteSpace(paymTermId)) return single;
        var term = await db.Set<PaymTerm>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.DataAreaId == area && x.PaymTermId == paymTermId, ct);
        if (term == null || string.IsNullOrWhiteSpace(term.PaymSched)) return single;

        var schedule = await db.Set<PaymSched>().AsNoTracking().FirstOrDefaultAsync(x => x.Name == term.PaymSched, ct)
            ?? throw new SubledgerPostingException($"Payment schedule '{term.PaymSched}' on terms '{paymTermId}' was not found.");
        if (schedule.TaxDistribution != PaymSchedTaxDist.Proportionate)
            throw new SubledgerPostingException(
                $"Payment schedule '{schedule.Name}' uses sales tax distribution {schedule.TaxDistribution}; only Proportionate is supported.");
        var lines = await db.Set<PaymSchedLine>().AsNoTracking().Where(x => x.Name == schedule.Name).ToListAsync(ct);
        IReadOnlyList<PaymentInstallment> installments;
        try { installments = PaymentScheduleCalculator.Calculate(amountCur, dueDate, schedule, lines); }
        catch (InvalidOperationException ex) { throw new SubledgerPostingException(ex.Message); }
        return Split(installments, amountCur, amountMst, reportingAmount);
    }

    /// <summary>Carries each installment's share into the accounting and reporting currencies.</summary>
    public static IReadOnlyList<OpenRowSplit> Split(IReadOnlyList<PaymentInstallment> installments,
        decimal amountCur, decimal amountMst, decimal reportingAmount)
    {
        var rows = new List<OpenRowSplit>(installments.Count);
        decimal mst = 0m, rep = 0m;
        for (var i = 0; i < installments.Count; i++)
        {
            var item = installments[i];
            var last = i == installments.Count - 1;
            var share = amountCur == 0m ? 0m : item.Amount / amountCur;
            var rowMst = last ? amountMst - mst : decimal.Round(amountMst * share, 2, MidpointRounding.AwayFromZero);
            var rowRep = last ? reportingAmount - rep : decimal.Round(reportingAmount * share, 2, MidpointRounding.AwayFromZero);
            mst += rowMst;
            rep += rowRep;
            rows.Add(new OpenRowSplit(item.DueDate, item.Amount, rowMst, rowRep));
        }
        return rows;
    }
}
