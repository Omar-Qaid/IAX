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
        var invoice = await (
            from trans in db.Set<CustTrans>().AsNoTracking()
            join open in db.Set<CustTransOpen>().AsNoTracking() on trans.RecId equals open.RefRecId
            where trans.RecId == transactionRecId && trans.DataAreaId == area && open.DataAreaId == area
                && trans.AccountNum == open.AccountNum && trans.Invoice != "" && open.AmountCur > 0m
            select new { Trans = trans, Open = open }).SingleOrDefaultAsync(ct);
        if (invoice == null) return null;

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
}
