using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsPayable.InvoiceRegister;

/// <summary>
/// D365 invoice register and invoice approval journals, as posted in D365 F&amp;O (USMF voucher APIR000001):
/// <list type="bullet">
/// <item>Register (journal type PurchaseLedger): the vendor transaction is created unapproved; the voucher debits the
/// posting profile's Offset account (posting type LedgerJournal) and credits its Arrival account (VendBalance).</item>
/// <item>Approval (journal type Approval), under the same voucher: debits the expense account (LedgerJournal), credits
/// Offset (LedgerJournal), debits Arrival (VendBalance) and credits the vendor summary account (VendBalance). The
/// vendor transaction becomes approved and can then be paid.</item>
/// </list>
/// </summary>
public sealed class VendInvoiceRegisterService(IUnitOfWork work, ICurrentUserService user, ISysNumberSequenceService numbers)
    : IVendInvoiceRegisterService
{
    public Task<string> RegisterAsync(VendInvoiceRegisterRequest request, CancellationToken ct) =>
        InTransactionAsync(() => RegisterCoreAsync(request, ct), ct);

    public Task<string> ApproveAsync(VendInvoiceApprovalRequest request, CancellationToken ct) =>
        InTransactionAsync(() => ApproveCoreAsync(request, ct), ct);

    private async Task<string> RegisterCoreAsync(VendInvoiceRegisterRequest request, CancellationToken ct)
    {
        var area = Area();
        var db = work.Context;
        var support = new SubledgerPostingSupport(db);
        var invoice = (request.Invoice ?? string.Empty).Trim();
        if (invoice.Length == 0 || request.Amount <= 0m || request.TransDate == default)
            throw new SubledgerPostingException("An invoice number, a positive amount and a date are required.");
        var vendor = await db.Set<VendTable>().AsNoTracking()
            .SingleOrDefaultAsync(x => x.DataAreaId == area && x.AccountNum == request.VendAccount, ct)
            ?? throw new SubledgerPostingException("The vendor was not found in this company.");
        if (vendor.Blocked is CustVendBlocked.All or CustVendBlocked.Invoicing)
            throw new SubledgerPostingException("The vendor is blocked for invoicing.");
        await support.LockVendorSettlementAsync(area, vendor.AccountNum, ct);
        if (await db.Set<VendTrans>().AnyAsync(x => x.DataAreaId == area && x.AccountNum == vendor.AccountNum && x.Invoice == invoice, ct))
            throw new SubledgerPostingException($"Invoice '{invoice}' is already posted for vendor '{vendor.AccountNum}'.");
        var (journalName, voucherSequence) = await JournalNameAsync(area, request.JournalName, LedgerJournalType.PurchaseLedger, ct);
        var rule = await support.ResolveVendorPostingRuleAsync(area, vendor, ct, request.PostingProfile);
        if (rule == null || rule.PurchasingLedgerDimension == 0 || rule.PurchasingOffsetLedgerDimension == 0)
            throw new SubledgerPostingException($"The vendor posting profile has no Arrival or Offset account for vendor '{vendor.AccountNum}'.");

        var date = request.TransDate.ToDateTime(TimeOnly.MinValue);
        var ledger = await support.LoadLedgerContextAsync(area, date, LedgerPostingModule.Vendor, user.GetCurrentUserId(), ct);
        var currency = string.IsNullOrWhiteSpace(request.CurrencyCode)
            ? (string.IsNullOrWhiteSpace(vendor.Currency) ? ledger.Ledger.AccountingCurrency : vendor.Currency)
            : request.CurrencyCode.Trim();
        var translation = await SubledgerCurrencyTranslation.LoadAsync(db, area, ledger.Ledger, currency, date, ct);
        var arrival = await support.LoadDimensionAsync(area, rule.PurchasingLedgerDimension, "vendor arrival", ct);
        var offset = await support.LoadDimensionAsync(area, rule.PurchasingOffsetLedgerDimension, "vendor offset", ct);
        var text = string.IsNullOrWhiteSpace(request.Txt) ? $"Invoice register {invoice}" : request.Txt.Trim();

        var journalNum = (await numbers.NextAsync("LedgerJournalTable", area, ct)).Code;
        var voucher = (await numbers.NextAsync(voucherSequence, area, ct)).Code;
        var trans = new VendTrans
        {
            DataAreaId = area, AccountNum = vendor.AccountNum, TransDate = date, Voucher = voucher, Invoice = invoice, Txt = text,
            CurrencyCode = currency, AmountCur = -request.Amount, AmountMst = translation.ToAccounting(-request.Amount),
            DueDate = (request.DueDate ?? request.TransDate).ToDateTime(TimeOnly.MinValue), TransType = LedgerTransType.None,
            Approved = NoYes.No
        };
        var entry = support.NewEntry(area, journalNum, voucher, date, ledger);
        db.AddRange(trans, entry, new LedgerJournalTable
        {
            DataAreaId = area, JournalNum = journalNum, JournalName = journalName.JournalName,
            NumberSequenceTable = journalName.NumberSequenceTable, Name = text, JournalType = LedgerJournalType.PurchaseLedger,
            NumOfLines = 1, CurrencyCode = currency, JournalTotalCredit = request.Amount, JournalTotalDebit = request.Amount,
            Posted = NoYes.Yes, PostedDateTime = DateTime.UtcNow
        });
        await work.CompleteAsync(ct);
        VendorOpenBalance.Add(db, trans);
        db.AddRange(
            new LedgerJournalTrans
            {
                DataAreaId = area, JournalNum = journalNum, LineNum = 1, Voucher = voucher, TransDate = date, Company = area,
                AccountType = LedgerJournalACType.Vend, LedgerDimension = arrival.RecId, LedgerDimensionName = vendor.AccountNum,
                AmountCurCredit = request.Amount, CurrencyCode = currency, OffsetAccountType = LedgerJournalACType.Ledger,
                OffsetLedgerDimension = offset.RecId, OffsetCompany = area, Txt = text, Invoice = invoice,
                DocumentDate = date, PostingProfile = rule.PostingProfile, VendTransId = trans.RecId
            },
            translation.AccountEntry(area, entry.RecId, offset, LedgerPostingType.LedgerJournal, request.Amount, text, 0m),
            translation.AccountEntry(area, entry.RecId, arrival, LedgerPostingType.VendBalance, -request.Amount, text, 0m),
            new SubledgerVoucherGeneralJournalEntry
            {
                DataAreaId = area, GeneralJournalEntry = entry.RecId, Voucher = voucher, VoucherDataAreaId = area, AccountingDate = date
            });
        await work.CompleteAsync(ct);
        await new PurchaseSourceAccounting(db).VerifyBalancedAsync(entry, ct);
        return voucher;
    }

    private async Task<string> ApproveCoreAsync(VendInvoiceApprovalRequest request, CancellationToken ct)
    {
        var area = Area();
        var db = work.Context;
        var support = new SubledgerPostingSupport(db);
        if (string.IsNullOrWhiteSpace(request.Voucher) || request.ExpenseLedgerDimension == 0 || request.TransDate == default)
            throw new SubledgerPostingException("A registered voucher, an expense account and a date are required.");
        var found = await db.Set<VendTrans>().AsNoTracking()
            .Where(x => x.DataAreaId == area && x.Voucher == request.Voucher && x.Approved == NoYes.No && x.Invoice != "")
            .Select(x => new { x.RecId, x.AccountNum }).ToListAsync(ct);
        if (found.Count != 1)
            throw new SubledgerPostingException($"No single unapproved registered invoice has voucher '{request.Voucher}'.");
        var account = found[0].AccountNum;
        await support.LockVendorSettlementAsync(area, account, ct);
        var trans = await db.Set<VendTrans>().SingleAsync(x => x.DataAreaId == area && x.RecId == found[0].RecId, ct);
        if (trans.Approved == NoYes.Yes)
            throw new SubledgerPostingException($"The invoice under voucher '{request.Voucher}' is already approved.");
        var date = request.TransDate.ToDateTime(TimeOnly.MinValue);
        if (date < trans.TransDate.Date)
            throw new SubledgerPostingException("The approval date precedes the registered invoice.");
        var (journalName, _) = await JournalNameAsync(area, request.JournalName, LedgerJournalType.Approval, ct);
        var registered = await db.Set<LedgerJournalTrans>().AsNoTracking()
            .SingleOrDefaultAsync(x => x.DataAreaId == area && x.VendTransId == trans.RecId && x.Voucher == trans.Voucher, ct)
            ?? throw new SubledgerPostingException("The invoice register line of this invoice was not found.");
        var vendor = await db.Set<VendTable>().AsNoTracking().SingleAsync(x => x.DataAreaId == area && x.AccountNum == account, ct);
        var rule = await support.ResolveVendorPostingRuleAsync(area, vendor, ct, registered.PostingProfile);
        if (rule == null || rule.SummaryLedgerDimension == 0)
            throw new SubledgerPostingException($"The vendor posting profile has no summary account for vendor '{account}'.");

        var ledger = await support.LoadLedgerContextAsync(area, date, LedgerPostingModule.Vendor, user.GetCurrentUserId(), ct);
        // The approval moves the registered amounts as they were booked, so it reuses the register's own accounts
        // (Arrival on the line, Offset as its offset) and the transaction's accounting value.
        var arrival = await support.LoadDimensionAsync(area, registered.LedgerDimension, "vendor arrival", ct);
        var offset = await support.LoadDimensionAsync(area, registered.OffsetLedgerDimension, "vendor offset", ct);
        var summary = await support.LoadDimensionAsync(area, rule.SummaryLedgerDimension, "vendor summary", ct);
        var expense = await support.LoadDimensionAsync(area, request.ExpenseLedgerDimension, "expense", ct);
        var registerTranslation = await SubledgerCurrencyTranslation.LoadAsync(db, area, ledger.Ledger, trans.CurrencyCode,
            trans.TransDate.Date, ct);
        var amount = -trans.AmountCur;
        var accounting = -trans.AmountMst;
        var reporting = registerTranslation.ToReporting(amount);
        var text = $"Invoice approval {trans.Invoice}";

        var journalNum = (await numbers.NextAsync("LedgerJournalTable", area, ct)).Code;
        var entry = support.NewEntry(area, journalNum, trans.Voucher, date, ledger);
        db.AddRange(entry, new LedgerJournalTable
        {
            DataAreaId = area, JournalNum = journalNum, JournalName = journalName.JournalName,
            NumberSequenceTable = journalName.NumberSequenceTable, Name = text, JournalType = LedgerJournalType.Approval,
            NumOfLines = 4, CurrencyCode = trans.CurrencyCode, JournalTotalCredit = 2 * amount, JournalTotalDebit = 2 * amount,
            Posted = NoYes.Yes, PostedDateTime = DateTime.UtcNow
        });
        await work.CompleteAsync(ct);
        (DimensionAttributeValueCombination Account, LedgerPostingType Type, int Sign)[] lines =
        [
            (expense, LedgerPostingType.LedgerJournal, 1),
            (offset, LedgerPostingType.LedgerJournal, -1),
            (summary, LedgerPostingType.VendBalance, -1),
            (arrival, LedgerPostingType.VendBalance, 1)
        ];
        for (var i = 0; i < lines.Length; i++)
        {
            var (dimension, type, sign) = lines[i];
            db.Add(new LedgerJournalTrans
            {
                DataAreaId = area, JournalNum = journalNum, LineNum = i + 1, Voucher = trans.Voucher, TransDate = date, Company = area,
                AccountType = LedgerJournalACType.Ledger, LedgerDimension = dimension.RecId,
                AmountCurDebit = sign > 0 ? amount : 0m, AmountCurCredit = sign < 0 ? amount : 0m, CurrencyCode = trans.CurrencyCode,
                Txt = text, Invoice = trans.Invoice, DocumentDate = date, PostingProfile = registered.PostingProfile,
                VendTransId = trans.RecId
            });
            db.Add(SubledgerPostingSupport.AccountEntry(area, entry.RecId, dimension, type,
                sign * amount, sign * accounting, sign * reporting, text, 0m, trans.CurrencyCode));
        }
        db.Add(new SubledgerVoucherGeneralJournalEntry
        {
            DataAreaId = area, GeneralJournalEntry = entry.RecId, Voucher = trans.Voucher, VoucherDataAreaId = area, AccountingDate = date
        });
        trans.Approved = NoYes.Yes;
        await work.CompleteAsync(ct);
        await new PurchaseSourceAccounting(db).VerifyBalancedAsync(entry, ct);
        return trans.Voucher;
    }

    private async Task<(LedgerJournalName Name, string VoucherSequence)> JournalNameAsync(string area, string journalName,
        LedgerJournalType type, CancellationToken ct)
    {
        var db = work.Context;
        var name = await db.Set<LedgerJournalName>().AsNoTracking().SingleOrDefaultAsync(x => x.DataAreaId == area
            && x.JournalName == journalName && !x.IsDeleted && x.IsActive, ct);
        if (name == null || name.JournalType != type)
            throw new SubledgerPostingException($"Journal name '{journalName}' must be an active {type} journal name.");
        if (name.NumberSequenceTable == 0) return (name, "Voucher");
        var sequence = await db.Set<SysNumberSequence>().AsNoTracking().SingleOrDefaultAsync(x => x.DataAreaId == area
            && x.RecId == name.NumberSequenceTable && !x.IsDeleted && x.IsActive, ct)
            ?? throw new SubledgerPostingException($"The voucher number sequence of journal name '{journalName}' is unavailable.");
        return (name, sequence.NumberSequence);
    }

    private string Area()
    {
        var area = user.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(area)) throw new SubledgerPostingException("A company must be selected.");
        return area;
    }

    private Task<string> InTransactionAsync(Func<Task<string>> body, CancellationToken ct) =>
        work.Context.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await work.BeginTransactionAsync(ct);
            try
            {
                var voucher = await body();
                await work.CommitTransactionAsync(ct);
                return voucher;
            }
            catch
            {
                await work.RollbackTransactionAsync(ct);
                work.Context.ChangeTracker.Clear();
                throw;
            }
        });
}
