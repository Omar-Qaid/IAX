using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.LedgerAccount;

/// <summary>
/// Main account controls applied to a posted line. These fields were stored and read by nobody, so
/// an account restricted to one side, to one currency, or requiring a payment reference, posted as
/// though it were unrestricted.
///
/// Takes the posted entry, because that is the shape every posting path builds and because the side
/// must be judged on the posted signed amount rather than on entered debit and credit columns.
///
/// Field semantics are the documented D365 ones (AX 2012 MainAccount table reference, carried
/// forward in the Common Data Model entity):
///
/// - DebitCreditCheck and DebitCreditProposal share one enum, DebCredProposal (None, Debit,
///   Credit). DebitCreditCheck ("DB/CR requirement") is the side a transaction *must* be on and is
///   the only one that restricts posting. DebitCreditProposal ("DB/CR proposal") is the side a
///   transaction is *usually* on — a default for entry that restricts nothing.
/// - ValidateCurrency, ValidatePosting and ValidateUser are FieldControl: Optional performs no
///   check, FillIn requires a value, Table compares against the value held on the main account,
///   List compares against a defined list of allowed values.
///
/// List mode and ValidateUser are reported by <see cref="Unenforced"/> rather than applied: there
/// is no allowed-value list table in this module, the main account has no UserInfoId to compare
/// against, and the journal posting path carries no posting-user identity.
/// </summary>
public static class MainAccountPostingControls
{
    /// <summary>The error refusing this posted line, or null when the account's controls allow it.</summary>
    public static string? Check(MainAccount account, GeneralJournalAccountEntry line)
    {
        var display = line.LedgerAccount.Length > 0 ? line.LedgerAccount : account.MainAccountId;

        if (account.MandatoryPaymentReference == NoYes.Yes && string.IsNullOrWhiteSpace(line.PaymentReference))
            return $"Main account '{display}' requires a payment reference.";

        // A zero amount is neither side. The voucher balance check owns that case.
        if (line.AccountingCurrencyAmount != 0m)
        {
            var side = account.DebitCreditCheck switch
            {
                DebitCreditProposal.Debit when line.AccountingCurrencyAmount < 0m =>
                    $"Main account '{display}' accepts debit amounts only.",
                DebitCreditProposal.Credit when line.AccountingCurrencyAmount > 0m =>
                    $"Main account '{display}' accepts credit amounts only.",
                _ => null
            };
            if (side != null) return side;
        }

        if (account.ValidateCurrency == FieldControl.FillIn && string.IsNullOrWhiteSpace(line.TransactionCurrencyCode))
            return $"Main account '{display}' requires a currency.";
        // Table mode compares against the account's own currency. An account set to Table without
        // one has nothing to compare, which Unenforced reports rather than refusing here.
        if (account.ValidateCurrency == FieldControl.Table && !string.IsNullOrWhiteSpace(account.CurrencyCode)
            && !string.Equals(line.TransactionCurrencyCode, account.CurrencyCode, StringComparison.OrdinalIgnoreCase))
            return $"Main account '{display}' accepts currency {account.CurrencyCode} only, not {line.TransactionCurrencyCode}.";

        if (account.ValidatePosting == FieldControl.FillIn && line.PostingType == LedgerPostingType.None)
            return $"Main account '{display}' requires a posting type.";
        if (account.ValidatePosting == FieldControl.Table && account.PostingType != LedgerPostingType.None
            && line.PostingType != account.PostingType)
            return $"Main account '{display}' accepts posting type {account.PostingType} only, not {line.PostingType}.";

        return null;
    }

    /// <summary>
    /// Controls configured on these accounts that posting does not apply, for the setup diagnostic.
    /// </summary>
    public static IReadOnlyList<string> Unenforced(IEnumerable<MainAccount> accounts)
    {
        var items = new List<string>();
        var all = accounts.ToList();

        Report(all.Where(x => x.DebitCreditBalanceDemand == DebitCreditDemand.Demand),
            "demand a debit or credit balance, which posting does not check");
        Report(all.Where(x => x.ValidateCurrency == FieldControl.List || x.ValidatePosting == FieldControl.List),
            "validate a field against a list of allowed values, and no allowed-value list is set up");
        Report(all.Where(x => x.ValidateCurrency == FieldControl.Table && string.IsNullOrWhiteSpace(x.CurrencyCode)),
            "validate their currency against the account but have no currency, so nothing is compared");
        Report(all.Where(x => x.ValidatePosting == FieldControl.Table && x.PostingType == LedgerPostingType.None),
            "validate their posting type against the account but have none, so nothing is compared");
        Report(all.Where(x => x.ValidateUser != FieldControl.Optional),
            "validate the posting user, which this journal path does not carry");
        Report(all.Where(x => x.UnitOfMeasure != 0),
            "specify a unit of measure, which posting does not check");
        return items;

        void Report(IEnumerable<MainAccount> matched, string what)
        {
            var ids = matched.Select(x => x.MainAccountId).Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToList();
            if (ids.Count > 0) items.Add($"Main accounts {string.Join(", ", ids)} {what}.");
        }
    }
}
