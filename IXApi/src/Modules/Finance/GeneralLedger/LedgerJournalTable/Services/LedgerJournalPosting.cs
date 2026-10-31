using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using JournalHeader = IAX.IXApi.Modules.Finance.Entities.LedgerJournalTable;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable;

/// <summary>
/// Posts an entered general journal to the ledger: LedgerJournalTable/Trans become
/// GeneralJournalEntry, GeneralJournalAccountEntry and SubledgerVoucherGeneralJournalEntry
/// through the shared subledger posting engine. One voucher and accounting date form one
/// posted entry; a line with an offset produces two account entries under that voucher.
///
/// Scope is the ledger-account journal. Customer, vendor, bank and asset account types,
/// intercompany lines, transaction currencies other than the accounting currency, journal
/// tax calculation and automatic reversal entries are rejected rather than approximated,
/// because each needs its own documented process.
/// </summary>
internal sealed class LedgerJournalPosting(DbContext db)
{
    public async Task<LedgerJournalPostResultDto> PostAsync(string area, JournalHeader header, CancellationToken ct)
    {
        var support = new SubledgerPostingSupport(db);
        await support.LockAsync($"GeneralJournal:{area}:{header.JournalNum}", ct);

        if (header.Posted == NoYes.Yes) throw new SubledgerPostingException("The journal is already posted.");
        if (header.SystemBlocked == NoYes.Yes) throw new SubledgerPostingException("The journal is system blocked.");
        if (header.ReverseEntry == NoYes.Yes)
            throw new SubledgerPostingException("Automatic reversal journals are not supported; post the reversal as its own journal.");

        var setup = await new LedgerJournalSetupCheck(db).CheckAsync(area, header.RecId, ct)
            ?? throw new SubledgerPostingException("The journal setup could not be verified.");
        if (setup.Errors.Count > 0) throw new SubledgerPostingException(setup.Errors[0]);

        // A posting control the company switched on must not be silently skipped. The setup check
        // reports this too; posting does not rely on a diagnostic for its own correctness.
        var parameters = await LedgerParameters.LedgerParametersService.PeekAsync(db, area, ct);
        if (LedgerJournalParameterControls.Refusal(parameters) is { } refusal)
            throw new SubledgerPostingException(refusal);

        var lines = await db.Set<LedgerJournalTrans>()
            .Where(x => x.DataAreaId == area && x.JournalNum == header.JournalNum).ToListAsync(ct);
        lines = lines.OrderBy(x => x.LineNum).ThenBy(x => x.RecId).ToList();

        var arithmetic = LedgerVoucherBalanceCheck.Check(lines);
        if (arithmetic.Count > 0) throw new SubledgerPostingException(arithmetic[0]);

        var ledger = await db.Set<Entities.Ledger>().AsNoTracking().FirstAsync(x => x.DataAreaId == area, ct);
        var accountingCurrency = ledger.AccountingCurrency;
        foreach (var line in lines) EnsureInScope(area, header, line, accountingCurrency);
        await EnsureManualEntryAllowedAsync(area, lines, ct);

        var vouchers = 0;
        var accountEntries = 0;
        var totalDebit = 0m;
        var totalCredit = 0m;
        foreach (var voucher in lines.GroupBy(x => new { x.Voucher, Date = x.TransDate.Date })
            .OrderBy(x => x.Key.Date).ThenBy(x => x.Key.Voucher, StringComparer.Ordinal))
        {
            var context = await support.LoadLedgerContextAsync(area, voucher.Key.Date, ct);
            var entry = support.NewEntry(area, header.JournalNum, voucher.Key.Voucher, voucher.Key.Date, context);
            db.Add(entry);
            await db.SaveChangesAsync(ct); // Persists the posted header key inside the caller's transaction.

            var posted = new List<GeneralJournalAccountEntry>();
            foreach (var line in voucher)
            {
                var amount = line.AmountCurDebit - line.AmountCurCredit;
                var dimension = await support.LoadDimensionAsync(area, line.LedgerDimension, $"line {line.LineNum} account", ct);
                posted.Add(SubledgerPostingSupport.AccountEntry(area, entry.RecId, dimension,
                    LedgerPostingType.LedgerJournal, amount, line.Txt, 0m, accountingCurrency, line.PaymReference));
                if (line.OffsetLedgerDimension != 0)
                {
                    var offset = await support.LoadDimensionAsync(area, line.OffsetLedgerDimension, $"line {line.LineNum} offset account", ct);
                    posted.Add(SubledgerPostingSupport.AccountEntry(area, entry.RecId, offset,
                        LedgerPostingType.LedgerJournal, -amount, line.OffsetTxt.Length > 0 ? line.OffsetTxt : line.Txt, 0m,
                        accountingCurrency, line.PaymReference));
                }
                totalDebit += line.AmountCurDebit;
                totalCredit += line.AmountCurCredit;
            }

            // The account's own controls are judged on the posted signed amount, so a line and its
            // offset are each checked against the account they actually hit.
            await EnsureAccountControlsAllowAsync(area, posted, ct);

            // The posted entry must balance on its own signed accounting amounts, not on the entered lines.
            if (posted.Sum(x => x.AccountingCurrencyAmount) != 0m)
                throw new SubledgerPostingException(
                    $"Voucher {voucher.Key.Voucher} on {voucher.Key.Date:yyyy-MM-dd} does not balance in the accounting currency.");
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
    private static void EnsureInScope(string area, JournalHeader header, LedgerJournalTrans line, string accountingCurrency)
    {
        var label = $"Line {line.LineNum}";
        if (line.Cancel == NoYes.Yes)
            throw new SubledgerPostingException($"{label} is canceled; remove it before posting the journal.");
        if (line.AccountType != LedgerJournalACType.Ledger)
            throw new SubledgerPostingException($"{label}: only the Ledger account type posts through the general journal.");
        if (line.OffsetLedgerDimension != 0 && line.OffsetAccountType != LedgerJournalACType.Ledger)
            throw new SubledgerPostingException($"{label}: only the Ledger offset account type posts through the general journal.");
        if (line.LedgerDimension == 0)
            throw new SubledgerPostingException($"{label}: a ledger account combination is required.");
        if ((line.Company.Length > 0 && line.Company != area) || (line.OffsetCompany.Length > 0 && line.OffsetCompany != area))
            throw new SubledgerPostingException($"{label}: intercompany journal posting is not supported.");
        if (line.CurrencyCode != accountingCurrency)
            throw new SubledgerPostingException(
                $"{label}: currency '{line.CurrencyCode}' requires exchange-rate conversion, which this journal path does not perform.");
        if (line.TaxCode.Length > 0 || header.LedgerJournalInclTax == NoYes.Yes)
            throw new SubledgerPostingException($"{label}: journal tax calculation is not supported; post tax through its own process.");
        if (line.ReverseEntry == NoYes.Yes)
            throw new SubledgerPostingException($"{label}: automatic reversal is not supported; post the reversal as its own journal.");
    }
}
