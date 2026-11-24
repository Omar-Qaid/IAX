using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.CurrencyRevaluation;

public sealed partial class CustCurrencyRevaluationService(IUnitOfWork work, ICurrentUserService user,
    ISysNumberSequenceService numbers) : ICustCurrencyRevaluationService
{
    public async Task<CustRevaluationPreview?> PreviewAsync(long transactionRecId, DateOnly rateDate, CancellationToken ct)
    {
        var area = user.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(area))
            throw new SubledgerPostingException("A company must be selected.");

        var db = work.Context;
        var found = await db.Set<CustTrans>().AsNoTracking()
            .SingleOrDefaultAsync(x => x.RecId == transactionRecId && x.DataAreaId == area && x.Invoice != "", ct);
        var total = found == null ? null : await OpenTotalAsync(db, area, transactionRecId, found.AccountNum, ct);
        if (found == null || total == null || total.AmountCur <= 0m) return null;
        var invoice = new { Trans = found, Open = total };

        var ledger = await db.Set<Ledger>().AsNoTracking().SingleOrDefaultAsync(x => x.DataAreaId == area, ct)
            ?? throw new SubledgerPostingException("A ledger is required for the selected company.");
        var translation = await SubledgerCurrencyTranslation.LoadAsync(db, area, ledger, invoice.Trans.CurrencyCode,
            rateDate.ToDateTime(TimeOnly.MinValue), ct);
        if (translation.IsIdentity)
            throw new SubledgerPostingException("The invoice already uses the ledger accounting and reporting currency.");

        return new CustRevaluationPreview(transactionRecId, translation.TransactionCurrency, invoice.Open.AmountCur,
            translation.AccountingCurrency, invoice.Open.AmountMst + invoice.Trans.CustExchAdjustmentUnrealized,
            translation.ToAccounting(invoice.Open.AmountCur), translation.ReportingCurrency,
            invoice.Open.ReportingCurrencyAmount + invoice.Trans.ReportingExchAdjustmentUnrealized,
            translation.ToReporting(invoice.Open.AmountCur));
    }

    /// <summary>
    /// The open balance of an invoice across all its open rows. An invoice on a payment schedule has one open row
    /// per installment; revaluation, as in D365, revalues the transaction's whole open balance.
    /// </summary>
    internal static async Task<CustTransOpen?> OpenTotalAsync(Microsoft.EntityFrameworkCore.DbContext db, string area,
        long transRecId, string account, CancellationToken ct)
    {
        var rows = await db.Set<CustTransOpen>().AsNoTracking()
            .Where(x => x.DataAreaId == area && x.RefRecId == transRecId && x.AccountNum == account).ToListAsync(ct);
        if (rows.Count == 0) return null;
        return new CustTransOpen
        {
            DataAreaId = area, AccountNum = account, RefRecId = transRecId, AmountCur = rows.Sum(x => x.AmountCur),
            AmountMst = rows.Sum(x => x.AmountMst), ReportingCurrencyAmount = rows.Sum(x => x.ReportingCurrencyAmount)
        };
    }
}
