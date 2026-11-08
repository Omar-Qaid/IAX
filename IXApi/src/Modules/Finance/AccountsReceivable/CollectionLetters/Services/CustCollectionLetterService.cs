using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Common;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.CollectionLetters;

public sealed class CustCollectionLetterService(IUnitOfWork work, ICurrentUserService user, ISysNumberSequenceService numbers)
    : ICustCollectionLetterService
{
    public const string NumberSequence = "CustCollectionLetter";

    public async Task<bool> PostWarningAsync(string number, DateOnly postingDate, CancellationToken ct)
    {
        var area = user.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(area) || string.IsNullOrWhiteSpace(number) || postingDate == default)
            throw new SubledgerPostingException("A company, collection letter number and posting date are required.");
        var date = postingDate.ToDateTime(TimeOnly.MinValue);
        var db = work.Context;
        return await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await work.BeginTransactionAsync(ct);
            try
            {
                var account = await db.Set<CustCollectionLetterJour>().AsNoTracking()
                    .Where(x => x.DataAreaId == area && x.CollectionLetterNum == number)
                    .Select(x => x.AccountNum).SingleOrDefaultAsync(ct)
                    ?? throw new SubledgerPostingException("The collection letter was not found in the selected company.");
                await new SubledgerPostingSupport(db).LockCustomerSettlementAsync(area, account, ct);
                var journal = await db.Set<CustCollectionLetterJour>().SingleAsync(
                    x => x.DataAreaId == area && x.CollectionLetterNum == number && x.AccountNum == account, ct);
                if (journal.Canceled != null || date < journal.CollectionLetterDate
                    || journal.Fee != 0m || journal.TaxAmount != 0m
                    || journal.CollectionLetterCode != (int)CustCollectionLetterCode.CollectionLetter1)
                    throw new SubledgerPostingException("Select an uncanceled first warning without fees and a valid posting date.");
                if (journal.Updated != null)
                {
                    await work.CommitTransactionAsync(ct);
                    return false;
                }

                var lines = await db.Set<CustCollectionLetterTrans>().AsNoTracking().Where(x => x.DataAreaId == area
                    && x.CollectionLetterNum == number && x.AccountNum == account).ToListAsync(ct);
                var openIds = lines.Select(x => x.CustTransOpenRecId).ToList();
                var openRows = await (from open in db.Set<CustTransOpen>()
                                      join trans in db.Set<CustTrans>() on open.RefRecId equals trans.RecId
                                      where open.DataAreaId == area && trans.DataAreaId == area
                                          && open.AccountNum == account && trans.AccountNum == account
                                          && trans.CurrencyCode == journal.CurrencyCode && openIds.Contains(open.RecId)
                                      select new { Open = open, Trans = trans }).ToDictionaryAsync(x => x.Open.RecId, ct);
                if (lines.Count == 0 || lines.Any(line => !openRows.TryGetValue(line.CustTransOpenRecId, out var row)
                    || row.Trans.RecId != line.CustTransId || row.Open.AmountCur != line.RemainAmount
                    || row.Open.AmountCur <= 0m || row.Open.DueDate.Date != line.DueDate.Date
                    || row.Open.CollectionLetterCode != CustCollectionLetterCode.None
                    || row.Trans.CollectionLetterCode != CustCollectionLetterCode.None))
                    throw new SubledgerPostingException("The open invoices have changed since this warning was generated.");

                foreach (var row in openRows.Values)
                {
                    row.Open.CollectionLetterCode = CustCollectionLetterCode.CollectionLetter1;
                    row.Trans.CollectionLetterCode = CustCollectionLetterCode.CollectionLetter1;
                }
                journal.Updated = date;
                await work.CompleteAsync(ct);
                await work.CommitTransactionAsync(ct);
                return true;
            }
            catch
            {
                await work.RollbackTransactionAsync(ct);
                db.ChangeTracker.Clear();
                throw;
            }
        });
    }

    public async Task<IReadOnlyList<string>> GenerateFirstLettersAsync(string account, string currency, DateOnly letterDate,
        CancellationToken ct)
    {
        var area = user.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(area) || string.IsNullOrWhiteSpace(account)
            || string.IsNullOrWhiteSpace(currency) || letterDate == default)
            throw new SubledgerPostingException("A company, customer, currency and collection letter date are required.");

        var db = work.Context;
        var date = letterDate.ToDateTime(TimeOnly.MinValue);
        return await db.Database.CreateExecutionStrategy().ExecuteAsync<IReadOnlyList<string>>(async () =>
        {
            await work.BeginTransactionAsync(ct);
            try
            {
                var customer = await db.Set<CustTable>().AsNoTracking().SingleOrDefaultAsync(
                    x => x.DataAreaId == area && x.AccountNum == account, ct)
                    ?? throw new SubledgerPostingException("The customer was not found in the selected company.");
                var support = new SubledgerPostingSupport(db);
                await support.LockCustomerSettlementAsync(area, customer.AccountNum, ct);
                const int code = (int)CustCollectionLetterCode.CollectionLetter1;
                var existing = from line in db.Set<CustCollectionLetterTrans>()
                               join journal in db.Set<CustCollectionLetterJour>()
                                   on new { line.DataAreaId, line.CollectionLetterNum } equals new { journal.DataAreaId, journal.CollectionLetterNum }
                               where journal.DataAreaId == area && journal.AccountNum == account && journal.Canceled == null
                                   && line.CollectionLetterCode == code
                               select line.CustTransOpenRecId;
                var invoices = await (from open in db.Set<CustTransOpen>().AsNoTracking()
                                      join trans in db.Set<CustTrans>().AsNoTracking() on open.RefRecId equals trans.RecId
                                      where open.DataAreaId == area && trans.DataAreaId == area
                                          && open.AccountNum == account && trans.AccountNum == account
                                          && trans.CurrencyCode == currency && trans.Invoice != "" && trans.TransDate <= date
                                          && open.AmountCur > 0m && open.DueDate != default && open.DueDate < date
                                          && open.CollectionLetterCode == CustCollectionLetterCode.None
                                          && trans.CollectionLetterCode == CustCollectionLetterCode.None
                                          && !existing.Contains(open.RecId)
                                      orderby open.DueDate, open.RecId
                                      select new { Open = open, Trans = trans }).ToListAsync(ct);

                var letters = new List<string>();
                foreach (var invoicesByProfile in invoices.GroupBy(x => x.Trans.PostingProfile))
                {
                    var rule = await support.ResolveCustomerPostingRuleAsync(area, invoicesByProfile.Key, customer, ct);
                    if (rule == null || string.IsNullOrWhiteSpace(rule.CollectionLetterCourse))
                        continue;
                    var enabled = await db.Set<CustLedger>().AnyAsync(x => x.DataAreaId == area
                        && x.PostingProfile == rule.PostingProfile && x.CollectionLetter == NoYes.Yes, ct);
                    if (!enabled)
                        continue;
                    var course = await db.Set<CustCollectionLetterTable>().AsNoTracking().SingleOrDefaultAsync(
                        x => x.DataAreaId == area && x.CollectionLetterCourse == rule.CollectionLetterCourse, ct)
                        ?? throw new SubledgerPostingException("The configured collection letter sequence was not found.");
                    var stage = await db.Set<CustCollectionLetterLine>().AsNoTracking().SingleOrDefaultAsync(
                        x => x.DataAreaId == area && x.CollectionLetterCourse == course.CollectionLetterCourse
                            && x.CurrencyCode == currency && x.CollectionLetterCode == code, ct);
                    if (stage == null)
                        continue;
                    if (stage.NumOfDays < 0 || stage.MinAmount < 0m || stage.Fee < 0m)
                        throw new SubledgerPostingException("The collection letter sequence has invalid days or amounts.");

                    var eligible = invoicesByProfile.Where(x => (date - x.Open.DueDate.Date).Days >= stage.NumOfDays).ToList();
                    if (eligible.Count == 0 || eligible.Sum(x => x.Open.AmountCur) < stage.MinAmount)
                        continue;
                    if (stage.Fee != 0m || stage.Block != 0)
                        throw new SubledgerPostingException("This path supports warning letters without fees or customer blocking.");

                    var number = (await numbers.NextAsync(NumberSequence, area, ct)).Code;
                    db.Add(new CustCollectionLetterJour
                    {
                        DataAreaId = area, AccountNum = customer.AccountNum, CurrencyCode = currency,
                        CollectionLetterCode = code, CollectionLetterDate = date, CollectionLetterNum = number,
                        PostingProfile = rule.PostingProfile, PaymTermId = course.PaymTermId
                    });
                    db.AddRange(eligible.Select(x => new CustCollectionLetterTrans
                    {
                        DataAreaId = area, AccountNum = customer.AccountNum, CurrencyCode = currency,
                        CollectionLetterCode = code, CollectionLetterNum = number, CustTransId = x.Trans.RecId,
                        CustTransOpenRecId = x.Open.RecId, DueDate = x.Open.DueDate, TransDate = x.Trans.TransDate,
                        Invoice = x.Trans.Invoice, Voucher = x.Trans.Voucher, RemainAmount = x.Open.AmountCur,
                        CalculationAmount = x.Open.AmountCur
                    }));
                    letters.Add(number);
                }

                await work.CompleteAsync(ct);
                await work.CommitTransactionAsync(ct);
                return letters;
            }
            catch
            {
                await work.RollbackTransactionAsync(ct);
                db.ChangeTracker.Clear();
                throw;
            }
        });
    }
}
