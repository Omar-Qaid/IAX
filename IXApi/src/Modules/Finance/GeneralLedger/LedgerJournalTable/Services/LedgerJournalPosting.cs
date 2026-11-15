using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Settlement;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using IAX.IXApi.Modules.Finance.Foundation.Currency;
using JournalHeader = IAX.IXApi.Modules.Finance.Entities.LedgerJournalTable;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable;

/// <summary>
/// Posts an entered general journal to the ledger: LedgerJournalTable/Trans become
/// GeneralJournalEntry, GeneralJournalAccountEntry and SubledgerVoucherGeneralJournalEntry
/// through the shared subledger posting engine. One voucher and accounting date form one
/// posted entry; a line with an offset produces two account entries under that voucher.
///
/// Ledger, customer, vendor and bank account types post, on the account side and on the offset
/// side alike. A customer, vendor or bank side also writes its subledger movement, so the balance
/// the ledger shows exists in the subledger too. An offset of those types is identified by the
/// account combination <c>OffsetLedgerDimension</c> references, whose display value is the
/// account number. Asset and project types, intercompany lines, journal tax calculation and
/// automatic reversal entries are rejected rather than approximated, because each needs its own
/// documented process.
/// </summary>
internal sealed class LedgerJournalPosting(DbContext db, string userId = "sys")
{
    public async Task<LedgerJournalPostResultDto> PostAsync(string area, JournalHeader header, CancellationToken ct)
    {
        var support = new SubledgerPostingSupport(db);
        await support.LockAsync($"GeneralJournal:{area}:{header.JournalNum}", ct);

        db.ChangeTracker.DetectChanges();
        if (db.Entry(header).State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            throw new SubledgerPostingException("Save journal header changes before posting.");

        // The caller may have loaded this header before waiting for the posting lock.
        // Use persisted controls and status after acquiring it, not the tracked snapshot.
        await db.Entry(header).ReloadAsync(ct);
        if (db.Entry(header).State == EntityState.Detached || header.DataAreaId != area)
            throw new SubledgerPostingException("The journal was not found in the selected company.");

        if (header.Posted == NoYes.Yes) throw new SubledgerPostingException("The journal is already posted.");
        if (header.SystemBlocked == NoYes.Yes) throw new SubledgerPostingException("The journal is system blocked.");
        if (header.ReverseEntry == NoYes.Yes)
            throw new SubledgerPostingException("Automatic reversal journals are not supported; post the reversal as its own journal.");
        if (db.ChangeTracker.Entries<LedgerJournalTrans>().Any(x =>
            (x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted) &&
            ((x.Entity.JournalNum == header.JournalNum &&
                (x.Entity.DataAreaId == area || (x.State == EntityState.Added && string.IsNullOrWhiteSpace(x.Entity.DataAreaId)))) ||
             (x.State != EntityState.Added &&
                x.OriginalValues.GetValue<string>(nameof(LedgerJournalTrans.DataAreaId)) == area &&
                x.OriginalValues.GetValue<string>(nameof(LedgerJournalTrans.JournalNum)) == header.JournalNum))))
            throw new SubledgerPostingException("Save journal line changes before posting.");
        if (db.ChangeTracker.HasChanges())
            throw new SubledgerPostingException("Save or discard other pending changes before posting.");
        var setup = await new LedgerJournalSetupCheck(db).CheckAsync(area, header.RecId, ct)
            ?? throw new SubledgerPostingException("The journal setup could not be verified.");
        if (setup.Errors.Count > 0) throw new SubledgerPostingException(setup.Errors[0]);
        var ledger = await db.Set<Entities.Ledger>().AsNoTracking().SingleAsync(x => x.DataAreaId == area, ct);

        // A posting control the company switched on must not be silently skipped. The setup check
        // reports this too; posting does not rely on a diagnostic for its own correctness.
        var parameters = await LedgerParameters.LedgerParametersService.PeekAsync(db, area, ct);
        if (LedgerJournalParameterControls.Refusal(parameters) is { } refusal)
            throw new SubledgerPostingException(refusal);

        var lines = await db.Set<LedgerJournalTrans>()
            .Where(x => x.DataAreaId == area && x.JournalNum == header.JournalNum).ToListAsync(ct);
        foreach (var line in lines)
        {
            await db.Entry(line).ReloadAsync(ct);
            if (db.Entry(line).State == EntityState.Detached || line.DataAreaId != area || line.JournalNum != header.JournalNum)
                throw new SubledgerPostingException("The journal lines changed while posting; reload the journal and retry.");
        }
        lines = lines.OrderBy(x => x.LineNum).ThenBy(x => x.RecId).ToList();
        if (lines.Any(x => x.AccountType == LedgerJournalACType.Cust && x.Prepayment == NoYes.Yes))
            throw new SubledgerPostingException("Customer prepayment journals require advance-liability posting and application, which are not implemented.");

        var journalName = await db.Set<LedgerJournalName>().AsNoTracking()
            .SingleAsync(x => x.DataAreaId == area && x.JournalName == header.JournalName, ct);
        await LedgerJournalVoucherNumberSequence.AllocateForPostingAsync(db, area, header, journalName, lines, ct);

        var arithmetic = LedgerVoucherBalanceCheck.Check(lines);
        if (arithmetic.Count > 0) throw new SubledgerPostingException(arithmetic[0]);

        var accountingCurrency = ledger.AccountingCurrency.Trim().ToUpperInvariant();
        var reportingCurrency = string.IsNullOrWhiteSpace(ledger.ReportingCurrency)
            ? accountingCurrency : ledger.ReportingCurrency.Trim().ToUpperInvariant();
        foreach (var line in lines) EnsureInScope(area, header, line);
        // An offset that names no customer, vendor or bank account is refused before anything is written.
        foreach (var line in lines.Where(x => x.OffsetLedgerDimension != 0 && x.OffsetAccountType != LedgerJournalACType.Ledger))
            await OffsetAccountNumAsync(area, line, $"line {line.LineNum} offset account", ct);
        await EnsureManualEntryAllowedAsync(area, lines, ct);

        var vouchers = 0;
        var accountEntries = 0;
        var totalDebit = 0m;
        var totalCredit = 0m;
        var totalReportingDebit = 0m;
        var totalReportingCredit = 0m;
        var converter = new LedgerCurrencyConverter(db);
        var defaulting = new LedgerJournalDimensionDefaulting(db);
        foreach (var voucher in lines.GroupBy(x => new { x.Voucher, Date = x.TransDate.Date })
            .OrderBy(x => x.Key.Date).ThenBy(x => x.Key.Voucher, StringComparer.Ordinal))
        {
            var context = await support.LoadLedgerContextAsync(area, voucher.Key.Date,
                LedgerPostingModule.Ledger, userId, ct);
            var entry = support.NewEntry(area, header.JournalNum, voucher.Key.Voucher, voucher.Key.Date, context);
            db.Add(entry);
            await db.SaveChangesAsync(ct); // Persists the posted header key inside the caller's transaction.

            var posted = new List<GeneralJournalAccountEntry>();
            foreach (var line in voucher)
            {
                var amount = line.AmountCurDebit - line.AmountCurCredit;
                var side = await ResolveSideAsync(support, area, line, false, $"line {line.LineNum} account", ct,
                    entered => defaulting.ApplyAsync(area, entered, ct, header.DefaultDimension, line.DefaultDimension));
                var dimension = side.Dimension;
                line.LedgerDimension = dimension.RecId;
                var accountingAmount = await converter.ConvertAsync(area, amount, line.CurrencyCode,
                    accountingCurrency, ledger.DefaultExchangeRateType, line.TransDate, ct);
                var reportingAmount = await converter.ConvertAsync(area, amount, line.CurrencyCode,
                    reportingCurrency, ledger.ReportingCurrencyExchangeRateType == 0
                        ? ledger.DefaultExchangeRateType : ledger.ReportingCurrencyExchangeRateType, line.TransDate, ct);
                posted.Add(SubledgerPostingSupport.AccountEntry(area, entry.RecId, dimension,
                    side.PostingType, amount, accountingAmount, reportingAmount,
                    line.Txt, 0m, line.CurrencyCode, line.PaymReference));
                // A customer, vendor or bank line owes a subledger movement as well as the ledger
                // entry, or the balance the ledger shows would exist nowhere else.
                await WriteSubledgerAsync(area, header, line, side, amount, accountingAmount, reportingAmount,
                    voucher.Key.Voucher, ct);
                if (line.OffsetLedgerDimension != 0)
                {
                    var offsetSide = await ResolveSideAsync(support, area, line, true, $"line {line.LineNum} offset account", ct,
                        entered => defaulting.ApplyAsync(area, entered, ct, header.DefaultDimension, line.OffsetDefaultDimension));
                    var offset = offsetSide.Dimension;
                    // A customer, vendor or bank offset keeps the reference that names the account.
                    if (offsetSide.AccountType == LedgerJournalACType.Ledger) line.OffsetLedgerDimension = offset.RecId;
                    posted.Add(SubledgerPostingSupport.AccountEntry(area, entry.RecId, offset,
                        offsetSide.PostingType, -amount, -accountingAmount, -reportingAmount,
                        line.OffsetTxt.Length > 0 ? line.OffsetTxt : line.Txt, 0m, line.CurrencyCode, line.PaymReference));
                    await WriteSubledgerAsync(area, header, line, offsetSide, -amount, -accountingAmount, -reportingAmount,
                        voucher.Key.Voucher, ct);
                    posted.AddRange(await new VendorJournalTaxPosting(db).BuildAsync(area, header, line, entry.RecId, offset, ct));
                }
            }

            // The account's own controls are judged on the posted signed amount, so a line and its
            // offset are each checked against the account they actually hit.
            await EnsureAccountControlsAllowAsync(area, posted, ct);

            // The posted entry must balance on its own signed accounting amounts, not on the entered lines.
            if (posted.Sum(x => x.AccountingCurrencyAmount) != 0m)
                throw new SubledgerPostingException(
                    $"Voucher {voucher.Key.Voucher} on {voucher.Key.Date:yyyy-MM-dd} does not balance in the accounting currency.");
            if (posted.Sum(x => x.ReportingCurrencyAmount) != 0m)
                throw new SubledgerPostingException(
                    $"Voucher {voucher.Key.Voucher} on {voucher.Key.Date:yyyy-MM-dd} does not balance in the reporting currency.");
            totalDebit += posted.Where(x => x.AccountingCurrencyAmount > 0m).Sum(x => x.AccountingCurrencyAmount);
            totalCredit -= posted.Where(x => x.AccountingCurrencyAmount < 0m).Sum(x => x.AccountingCurrencyAmount);
            totalReportingDebit += posted.Where(x => x.ReportingCurrencyAmount > 0m).Sum(x => x.ReportingCurrencyAmount);
            totalReportingCredit -= posted.Where(x => x.ReportingCurrencyAmount < 0m).Sum(x => x.ReportingCurrencyAmount);
            db.AddRange(posted);
            db.Add(new SubledgerVoucherGeneralJournalEntry
            {
                DataAreaId = area, GeneralJournalEntry = entry.RecId, Voucher = voucher.Key.Voucher,
                VoucherDataAreaId = area, AccountingDate = voucher.Key.Date
            });
            vouchers++;
            accountEntries += posted.Count;
        }

        var postedAt = DateTime.UtcNow;
        header.Posted = NoYes.Yes;
        header.PostedDateTime = postedAt;
        header.JournalTotalDebit = totalDebit;
        header.JournalTotalCredit = totalCredit;
        header.JournalBalance = totalDebit - totalCredit;
        header.JournalTotalDebitReportingCurrency = totalReportingDebit;
        header.JournalTotalCreditReportingCurrency = totalReportingCredit;
        header.NumOfLines = lines.Count;
        foreach (var line in lines) line.NoEdit = NoYes.Yes;
        return new(header.JournalNum, vouchers, accountEntries, totalDebit, totalCredit, postedAt);
    }

    /// <summary>
    /// Main account controls that restrict a posted line: the debit or credit side the account
    /// accepts, and a required payment reference. An account whose other controls this path does
    /// not apply is reported by the setup check, not silently satisfied here.
    /// </summary>
    private async Task EnsureAccountControlsAllowAsync(string area, List<GeneralJournalAccountEntry> posted, CancellationToken ct)
    {
        var accountIds = posted.Select(x => x.MainAccount).Where(x => x > 0).Distinct().ToList();
        if (accountIds.Count == 0) return;
        var accounts = await db.Set<MainAccount>().AsNoTracking()
            .Where(x => accountIds.Contains(x.RecId)).ToListAsync(ct);
        foreach (var line in posted)
        {
            var account = accounts.FirstOrDefault(x => x.RecId == line.MainAccount);
            if (account == null) continue; // LoadDimensionAsync already established the account exists.
            if (LedgerAccount.MainAccountPostingControls.Check(account, line) is { } refusal)
                throw new SubledgerPostingException(refusal);
        }
    }

    /// <summary>
    /// A main account the company blocks for manual entry cannot be posted from a general journal.
    /// The override is keyed by legal entity, so an account open in one company may be blocked here.
    /// </summary>
    private async Task EnsureManualEntryAllowedAsync(string area, List<LedgerJournalTrans> lines, CancellationToken ct)
    {
        var company = await db.Set<CompanyInfo>().AsNoTracking()
            .Where(x => x.DataAreaId == area).Select(x => x.RecId).FirstOrDefaultAsync(ct);
        if (company == 0) return; // No legal-entity record, so no per-company override can apply.
        var dimensionIds = lines.SelectMany(x => new[] { x.LedgerDimension, x.OffsetLedgerDimension })
            .Where(x => x > 0).Distinct().ToList();
        var accounts = await db.Set<DimensionAttributeValueCombination>().AsNoTracking()
            .Where(x => x.DataAreaId == area && dimensionIds.Contains(x.RecId))
            .Select(x => new { x.RecId, x.MainAccount, x.DisplayValue, x.MainAccountValue }).ToListAsync(ct);
        var accountIds = accounts.Select(x => x.MainAccount).Where(x => x > 0).Distinct().ToList();
        var blocked = await db.Set<MainAccountLegalEntity>().AsNoTracking()
            .Where(x => x.LegalEntity == company && accountIds.Contains(x.MainAccount)
                && x.DoNotAllowManualEntry == NoYes.Yes)
            .Select(x => x.MainAccount).ToListAsync(ct);
        if (blocked.Count == 0) return;
        var account = accounts.First(x => blocked.Contains(x.MainAccount));
        throw new SubledgerPostingException(
            $"Main account '{(account.DisplayValue.Length == 0 ? account.MainAccountValue : account.DisplayValue)}' does not allow manual entry in this company.");
    }

    /// <summary>Rejects what this journal path does not implement, instead of posting an approximation.</summary>
    /// <summary>The account types the basic general journal posts. Others are rejected, not approximated.</summary>
    private static readonly LedgerJournalACType[] BasicAccountTypes =
    [
        LedgerJournalACType.Ledger, LedgerJournalACType.Cust,
        LedgerJournalACType.Vend, LedgerJournalACType.Bank
    ];

    /// <summary>
    /// One side of a journal line: the account combination the ledger entry hits, the posting type
    /// that describes it, and - for a subledger type - the account it belongs to. The document
    /// fields of the line (invoice, posting profile, prepayment, settlement) describe its account
    /// side, so the offset side carries none of them.
    /// </summary>
    private sealed record PostingSide(DimensionAttributeValueCombination Dimension, LedgerPostingType PostingType,
        LedgerJournalACType AccountType, string AccountNum, bool IsOffset);

    /// <summary>
    /// Resolves the account side or the offset side of the line. A Ledger side names the
    /// combination directly; a customer or vendor side resolves its summary account from the
    /// posting profile, exactly as an invoice or payment does, and a bank side uses the bank
    /// account's own ledger account.
    /// </summary>
    private async Task<PostingSide> ResolveSideAsync(SubledgerPostingSupport support, string area,
        LedgerJournalTrans line, bool isOffset, string what, CancellationToken ct,
        Func<DimensionAttributeValueCombination, Task<DimensionAttributeValueCombination>> applyDefaults)
    {
        var accountType = isOffset ? line.OffsetAccountType : line.AccountType;
        var accountNum = !isOffset ? line.LedgerDimensionName.Trim()
            : accountType == LedgerJournalACType.Ledger ? string.Empty
            : await OffsetAccountNumAsync(area, line, what, ct);
        var invoice = isOffset ? string.Empty : line.Invoice;
        var postingProfile = isOffset ? string.Empty : line.PostingProfile;
        switch (accountType)
        {
            case LedgerJournalACType.Cust:
            {
                var customer = await db.Set<CustTable>().AsNoTracking()
                    .FirstOrDefaultAsync(x => x.DataAreaId == area && x.AccountNum == accountNum, ct)
                    ?? throw new SubledgerPostingException($"Customer '{accountNum}' was not found for {what}.");
                if (customer.Blocked == CustVendorBlocked.All
                    || (customer.Blocked == CustVendorBlocked.Invoicing && !string.IsNullOrWhiteSpace(invoice)))
                    throw new SubledgerPostingException("The customer is blocked for this journal transaction.");
                var dimension = await support.ResolveCustomerSummaryAsync(area, postingProfile, customer, ct, applyDefaults);
                return new PostingSide(dimension, LedgerPostingType.CustBalance, accountType, customer.AccountNum, isOffset);
            }
            case LedgerJournalACType.Vend:
            {
                var vendor = await db.Set<VendTable>().AsNoTracking()
                    .FirstOrDefaultAsync(x => x.DataAreaId == area && x.AccountNum == accountNum, ct)
                    ?? throw new SubledgerPostingException($"Vendor '{accountNum}' was not found for {what}.");
                var dimension = await support.ResolveVendorSummaryAsync(area, vendor, ct,
                    string.IsNullOrWhiteSpace(postingProfile) ? null : postingProfile,
                    prepayment: !isOffset && line.Prepayment == NoYes.Yes, applyDefaults: applyDefaults);
                return new PostingSide(dimension, LedgerPostingType.VendBalance, accountType, vendor.AccountNum, isOffset);
            }
            case LedgerJournalACType.Bank:
            {
                var bank = await db.Set<BankAccountTable>().AsNoTracking()
                    .FirstOrDefaultAsync(x => x.DataAreaId == area && x.AccountId == accountNum, ct)
                    ?? throw new SubledgerPostingException($"Bank account '{accountNum}' was not found for {what}.");
                if (bank.LedgerDimension == 0 || bank.BankAccountStatus != BankAccountStatus.Active)
                    throw new SubledgerPostingException(
                        $"Bank account '{accountNum}' must be active and have a ledger account for {what}.");
                var dimension = await support.LoadDimensionAsync(area, bank.LedgerDimension, what, ct, applyDefaults);
                return new PostingSide(dimension, LedgerPostingType.Bank, accountType, bank.AccountId, isOffset);
            }
            default:
            {
                var dimension = await support.LoadDimensionAsync(area,
                    isOffset ? line.OffsetLedgerDimension : line.LedgerDimension, what, ct, applyDefaults);
                return new PostingSide(dimension, LedgerPostingType.LedgerJournal, accountType, string.Empty, isOffset);
            }
        }
    }

    /// <summary>
    /// The customer, vendor or bank account an offset names. The line has no offset account-number
    /// field: <c>OffsetLedgerDimension</c> references an account combination whose display value
    /// is that account number and which carries no main account, since the main account comes
    /// from the posting setup of the account it names.
    /// </summary>
    private async Task<string> OffsetAccountNumAsync(string area, LedgerJournalTrans line, string what, CancellationToken ct)
    {
        var account = await db.Set<DimensionAttributeValueCombination>().AsNoTracking()
            .Where(x => x.DataAreaId == area && x.RecId == line.OffsetLedgerDimension)
            .Select(x => new { x.DisplayValue, x.MainAccount, x.MainAccountValue }).FirstOrDefaultAsync(ct)
            ?? throw new SubledgerPostingException($"The {what} references an invalid ledger dimension.");
        if (account.MainAccount != 0 || account.MainAccountValue.Length > 0 || string.IsNullOrWhiteSpace(account.DisplayValue))
            throw new SubledgerPostingException(
                $"The {what} must reference a {line.OffsetAccountType} account, not a main account combination.");
        return account.DisplayValue.Trim();
    }

    /// <summary>
    /// The subledger movement a non-Ledger line creates: a customer or vendor transaction with its
    /// open row, or a bank account movement. Signs follow the ledger entry - a debit is positive -
    /// so a customer debit raises what the customer owes and a vendor debit reduces what is owed.
    /// </summary>
    private async Task WriteSubledgerAsync(string area, JournalHeader header, LedgerJournalTrans line,
        PostingSide side, decimal amount, decimal accountingAmount, decimal reportingAmount, string voucher, CancellationToken ct)
    {
        if (side.AccountType == LedgerJournalACType.Ledger || amount == 0m) return;
        var date = line.TransDate.Date;
        var due = line.Due == default ? date : line.Due.Date;
        var primary = !side.IsOffset;
        var invoice = primary ? line.Invoice : string.Empty;
        var txt = !primary && line.OffsetTxt.Length > 0 ? line.OffsetTxt : line.Txt;
        switch (side.AccountType)
        {
            case LedgerJournalACType.Cust:
            {
                var custTrans = new CustTrans
                {
                    DataAreaId = area, AccountNum = side.AccountNum, OrderAccount = side.AccountNum,
                    TransDate = date, Voucher = voucher, DocumentNum = header.JournalNum,
                    DocumentDate = line.DocumentDate == default ? date : line.DocumentDate.Date,
                    Invoice = invoice, PaymReference = line.PaymReference,
                    Txt = txt, TransType = LedgerTransType.Cust, CurrencyCode = line.CurrencyCode,
                    AmountCur = amount, AmountMst = accountingAmount, ReportingCurrencyAmount = reportingAmount,
                    DueDate = due, PaymTermId = line.Payment, PaymMode = line.PaymMode,
                    DefaultDimension = primary ? line.DefaultDimension : line.OffsetDefaultDimension,
                    PostingProfile = primary ? line.PostingProfile : string.Empty
                };
                db.Add(custTrans);
                await db.SaveChangesAsync(ct); // The open row references the transaction key.
                if (primary) line.CustTransId = custTrans.RecId;
                var open = new CustTransOpen
                {
                    DataAreaId = area, AccountNum = side.AccountNum, RefRecId = custTrans.RecId,
                    TransDate = date, DueDate = due,
                    AmountCur = amount, AmountMst = accountingAmount, ReportingCurrencyAmount = reportingAmount
                };
                db.Add(open);
                if (primary && line.SettleVoucher == SettleVoucher.Selection)
                    await SettleMarkedInvoiceAsync(area, line, custTrans, open, ct);
                return;
            }
            case LedgerJournalACType.Vend:
            {
                var vendor = await db.Set<VendTable>().AsNoTracking().SingleAsync(x => x.DataAreaId == area && x.AccountNum == side.AccountNum, ct);
                if (vendor.Blocked == CustVendBlocked.All || (invoice.Length > 0 && vendor.Blocked == CustVendBlocked.Invoicing))
                    throw new SubledgerPostingException("The vendor is blocked for this journal transaction.");
                if (invoice.Length > 0)
                {
                    await new SubledgerPostingSupport(db).LockAsync($"VendInvoice:{area}:{vendor.AccountNum}:{invoice}".ToUpperInvariant(), ct);
                    if (await db.Set<VendTrans>().AnyAsync(x => x.DataAreaId == area && x.AccountNum == vendor.AccountNum && x.Invoice == invoice, ct)
                        || await db.Set<VendInvoiceJour>().AnyAsync(x => x.DataAreaId == area && x.InvoiceAccount == vendor.AccountNum && x.InvoiceId == invoice, ct))
                        throw new SubledgerPostingException("The vendor invoice has already been posted in this company.");
                }
                var vendTrans = new VendTrans
                {
                    DataAreaId = area, AccountNum = side.AccountNum, TransDate = date, Voucher = voucher,
                    Txt = txt, TransType = primary && line.Prepayment == NoYes.Yes ? LedgerTransType.Payment : LedgerTransType.Vend, CurrencyCode = line.CurrencyCode,
                    AmountCur = amount, AmountMst = accountingAmount, DueDate = due,
                    Invoice = invoice, CashDiscCode = primary ? line.CashDiscCode : string.Empty,
                    CashDiscBaseDate = line.CashDiscBaseDate == default ? date : line.CashDiscBaseDate
                };
                db.Set<VendTrans>().Add(vendTrans);
                await db.SaveChangesAsync(ct); // The open row references the transaction key.
                if (primary) line.VendTransId = vendTrans.RecId;
                VendorOpenBalance.Add(db, vendTrans);
                return;
            }
            case LedgerJournalACType.Bank:
            {
                db.Add(new BankAccountTrans
                {
                    DataAreaId = area, AccountId = side.AccountNum, TransDate = date, Voucher = voucher,
                    Txt = txt, LedgerTransType = LedgerTransType.Bank,
                    CurrencyCode = line.CurrencyCode, BankTransCurrencyCode = line.CurrencyCode,
                    AmountCur = amount, AmountMst = accountingAmount, AmountReportingCurrency = reportingAmount,
                    BankTransAmountCur = amount, LedgerDimension = side.Dimension.RecId,
                    SourceTableId = FinanceTableNumbers.LedgerJournalTrans, SourceRecId = line.RecId,
                    PaymentMode = line.PaymMode, PaymReference = line.PaymReference
                });
                return;
            }
        }
    }

    private async Task SettleMarkedInvoiceAsync(string area, LedgerJournalTrans line, CustTrans credit,
        CustTransOpen creditOpen, CancellationToken ct)
    {
        if (line.MarkedInvoiceRecId <= 0 || credit.AmountCur >= 0
            || (line.MarkedInvoiceCompany.Length > 0 && line.MarkedInvoiceCompany != area))
            throw new SubledgerPostingException("Selected settlement requires a customer credit and a marked invoice in this company.");

        await new SubledgerPostingSupport(db).LockCustomerSettlementAsync(area, credit.AccountNum, ct);
        var marked = await (
            from debit in db.Set<CustTrans>()
            join open in db.Set<CustTransOpen>() on debit.RecId equals open.RefRecId
            where debit.RecId == line.MarkedInvoiceRecId && debit.DataAreaId == area && open.DataAreaId == area
                && debit.AccountNum == credit.AccountNum && open.AccountNum == credit.AccountNum
            select new { Debit = debit, Open = open }).SingleOrDefaultAsync(ct);
        if (marked != null)
        {
            // A tracking query can reuse balances loaded before the settlement lock.
            await db.Entry(marked.Debit).ReloadAsync(ct);
            await db.Entry(marked.Open).ReloadAsync(ct);
        }
        if (marked == null || string.IsNullOrWhiteSpace(marked.Debit.Invoice)
            || db.Entry(marked.Debit).State == EntityState.Detached || db.Entry(marked.Open).State == EntityState.Detached
            || marked.Debit.DataAreaId != area || marked.Open.DataAreaId != area
            || marked.Debit.AccountNum != credit.AccountNum || marked.Open.AccountNum != credit.AccountNum
            || marked.Open.RefRecId != marked.Debit.RecId
            || marked.Debit.CurrencyCode != credit.CurrencyCode || marked.Debit.TransDate.Date > credit.TransDate
            || marked.Open.AmountCur != -creditOpen.AmountCur)
            throw new SubledgerPostingException("The marked invoice must have an open balance matching the credit amount and currency.");

        await db.SaveChangesAsync(ct);
        CustSettleOpenTransactionsService.ApplyPair(db, area, credit.AccountNum, credit.TransDate,
            credit.Voucher, marked.Debit, marked.Open, credit, creditOpen, marked.Open.AmountCur);
    }

    private static void EnsureInScope(string area, JournalHeader header, LedgerJournalTrans line)
    {
        var label = $"Line {line.LineNum}";
        if (line.Cancel == NoYes.Yes)
            throw new SubledgerPostingException($"{label} is canceled; remove it before posting the journal.");
        if (!BasicAccountTypes.Contains(line.AccountType))
            throw new SubledgerPostingException(
                $"{label}: the {line.AccountType} account type does not post through the basic general journal. Use Ledger, Cust, Vend or Bank.");
        if (line.OffsetLedgerDimension != 0 && !BasicAccountTypes.Contains(line.OffsetAccountType))
            throw new SubledgerPostingException(
                $"{label}: the {line.OffsetAccountType} offset account type does not post through the basic general journal. Use Ledger, Cust, Vend or Bank.");
        // A Ledger line names an account combination; a customer, vendor or bank line names the
        // account and the combination comes from its posting setup.
        if (line.AccountType == LedgerJournalACType.Ledger && line.LedgerDimension == 0)
            throw new SubledgerPostingException($"{label}: a ledger account combination is required.");
        if (line.AccountType != LedgerJournalACType.Ledger && line.LedgerDimensionName.Length == 0)
            throw new SubledgerPostingException($"{label}: a {line.AccountType} account is required.");
        if ((line.Company.Length > 0 && line.Company != area) || (line.OffsetCompany.Length > 0 && line.OffsetCompany != area))
            throw new SubledgerPostingException($"{label}: intercompany journal posting is not supported.");
        if (string.IsNullOrWhiteSpace(line.CurrencyCode))
            throw new SubledgerPostingException($"{label}: a transaction currency is required.");
        var taxRequested = header.LedgerJournalInclTax == NoYes.Yes || !string.IsNullOrWhiteSpace(line.TaxCode)
            || !string.IsNullOrWhiteSpace(line.TaxGroup) || !string.IsNullOrWhiteSpace(line.TaxItemGroup);
        var supportedTax = line.AccountType == LedgerJournalACType.Vend && header.LedgerJournalInclTax == NoYes.Yes
            && !string.IsNullOrWhiteSpace(line.TaxGroup) && !string.IsNullOrWhiteSpace(line.TaxItemGroup)
            && line.OffsetLedgerDimension != 0 && line.OffsetAccountType == LedgerJournalACType.Ledger
            && string.IsNullOrWhiteSpace(line.TaxCode);
        if (taxRequested && !supportedTax)
            throw new SubledgerPostingException($"{label}: journal tax calculation is not supported; post tax through its own process.");
        if (line.SettleVoucher != SettleVoucher.PostedVoucher
            && !(line.AccountType == LedgerJournalACType.Cust && line.SettleVoucher == SettleVoucher.Selection))
            throw new SubledgerPostingException(
                $"{label}: {line.SettleVoucher} settlement for {line.AccountType} journal lines is not supported; use the settlement process.");
        if (line.ReverseEntry == NoYes.Yes)
            throw new SubledgerPostingException($"{label}: automatic reversal is not supported; post the reversal as its own journal.");
    }

}
