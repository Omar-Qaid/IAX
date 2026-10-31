using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.LedgerAccount;

/// <summary>
/// Main account controls applied to a posted line. These fields were stored and read by nobody, so
/// an account restricted to one side, or requiring a payment reference, posted as though it were
/// unrestricted.
///
/// Deliberately line-agnostic — an account, a signed posted amount and a payment reference — so any
/// posting path can apply the same controls, and so the side is judged on the posted signed amount
/// rather than on entered debit and credit columns.
///
/// Two controls are applied. The rest are reported by <see cref="Unenforced"/> rather than guessed:
/// their meaning is not established by this repository's cycle documents or by the reduced enums
/// carried here, and a guessed restriction refuses valid journals.
///
/// Assumption, stated because it is an inference and not a documented rule: this project's
/// <see cref="DebitCreditCheck"/> is a switch with no side of its own (None/Check), so the side it
/// enforces is read from <see cref="DebitCreditProposal"/>. An account set to Check with no
/// proposed side names nothing to enforce and is reported, not applied. Confirm against target
/// metadata before relying on it.
/// </summary>
public static class MainAccountPostingControls
{
    /// <summary>
    /// The error refusing this posted line, or null when the account's applied controls allow it.
    /// </summary>
    /// <param name="amount">The posted signed accounting amount: positive debit, negative credit.</param>
    public static string? Check(MainAccount account, string accountDisplay, decimal amount, string paymentReference)
    {
        if (account.MandatoryPaymentReference == NoYes.Yes && string.IsNullOrWhiteSpace(paymentReference))
            return $"Main account '{accountDisplay}' requires a payment reference.";

        // A zero amount is neither side. The voucher balance check owns that case.
        if (account.DebitCreditCheck != DebitCreditCheck.Check || amount == 0m) return null;
        return account.DebitCreditProposal switch
        {
            DebitCreditProposal.Debit when amount < 0m =>
                $"Main account '{accountDisplay}' accepts debit amounts only.",
            DebitCreditProposal.Credit when amount > 0m =>
                $"Main account '{accountDisplay}' accepts credit amounts only.",
            _ => null
        };
    }

    /// <summary>
    /// Controls configured on these accounts that posting does not apply, for the setup diagnostic.
    /// </summary>
    public static IReadOnlyList<string> Unenforced(IEnumerable<MainAccount> accounts)
    {
        var items = new List<string>();
        var all = accounts.ToList();

        Report(all.Where(x => x.DebitCreditCheck == DebitCreditCheck.Check
                && x.DebitCreditProposal == DebitCreditProposal.None),
            "require a debit or credit side but propose neither, so no side is enforced for them");
        Report(all.Where(x => x.DebitCreditBalanceDemand == DebitCreditDemand.Demand),
            "demand a debit or credit balance, which posting does not check");
        Report(all.Where(x => x.ValidateCurrency != 0 || !string.IsNullOrWhiteSpace(x.CurrencyCode)),
            "restrict their currency, which posting does not check");
        Report(all.Where(x => x.ValidatePosting != 0),
            "restrict their posting types, which posting does not check");
        Report(all.Where(x => x.ValidateUser != 0),
            "restrict which users may post to them, which posting does not check");
        return items;

        void Report(IEnumerable<MainAccount> matched, string what)
        {
            var ids = matched.Select(x => x.MainAccountId).Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToList();
            if (ids.Count > 0) items.Add($"Main accounts {string.Join(", ", ids)} {what}.");
        }
    }
}
