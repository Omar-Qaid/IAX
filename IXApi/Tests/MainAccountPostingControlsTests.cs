using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.GeneralLedger.LedgerAccount;
using Xunit;

namespace IAX.IXApi.Tests;

/// <summary>
/// The main account controls were stored and read by nobody, so an account restricted to one side,
/// to one currency, or requiring a payment reference, posted as though it were unrestricted.
///
/// Semantics are the documented D365 ones: DebitCreditCheck is the side a transaction must be on,
/// DebitCreditProposal is only the usual side and restricts nothing, and the three Validate fields
/// are FieldControl (Optional, FillIn, Table, List). The side is judged on the posted signed
/// amount: positive debit, negative credit.
/// </summary>
public class MainAccountPostingControlsTests
{
    [Fact]
    public void An_unrestricted_account_allows_either_side()
    {
        var account = Account("110100");
        Assert.Null(MainAccountPostingControls.Check(account, Line("110100", 100m)));
        Assert.Null(MainAccountPostingControls.Check(account, Line("110100", -100m)));
        Assert.Empty(MainAccountPostingControls.Unenforced([account]));
    }

    [Theory]
    [InlineData(DebitCreditProposal.Debit, 100, null)]
    [InlineData(DebitCreditProposal.Debit, -100, "debit amounts only")]
    [InlineData(DebitCreditProposal.Credit, -100, null)]
    [InlineData(DebitCreditProposal.Credit, 100, "credit amounts only")]
    public void The_required_side_comes_from_DebitCreditCheck(
        DebitCreditProposal required, int postedAmount, string? expected)
    {
        var account = Account("611000");
        account.DebitCreditCheck = required;
        var result = MainAccountPostingControls.Check(account, Line("611000-ADMIN", postedAmount));
        if (expected == null) Assert.Null(result);
        else
        {
            Assert.Contains(expected, result);
            // The message names the combination that was posted, not just the main account.
            Assert.Contains("611000-ADMIN", result);
        }
    }

    // DB/CR proposal is the side a transaction is usually on: a default for entry, not a rule.
    [Fact]
    public void DebitCreditProposal_restricts_nothing_on_its_own()
    {
        var account = Account("611000");
        account.DebitCreditProposal = DebitCreditProposal.Debit;
        Assert.Null(MainAccountPostingControls.Check(account, Line("611000", -100m)));
        Assert.Empty(MainAccountPostingControls.Unenforced([account]));

        // It also does not override the required side when the two disagree.
        account.DebitCreditCheck = DebitCreditProposal.Credit;
        Assert.Null(MainAccountPostingControls.Check(account, Line("611000", -100m)));
        Assert.Contains("credit amounts only", MainAccountPostingControls.Check(account, Line("611000", 100m)));
    }

    // The voucher balance check owns the zero-amount case, so the side check stays out of it.
    [Fact]
    public void A_zero_amount_is_left_to_the_balance_check()
    {
        var account = Account("611000");
        account.DebitCreditCheck = DebitCreditProposal.Debit;
        Assert.Null(MainAccountPostingControls.Check(account, Line("611000", 0m)));
    }

    [Fact]
    public void A_mandatory_payment_reference_is_required_and_whitespace_does_not_satisfy_it()
    {
        var account = Account("110100");
        account.MandatoryPaymentReference = NoYes.Yes;
        Assert.Contains("requires a payment reference",
            MainAccountPostingControls.Check(account, Line("110100", 100m)));
        Assert.Contains("requires a payment reference",
            MainAccountPostingControls.Check(account, Line("110100", 100m, paymentReference: "   ")));
        Assert.Null(MainAccountPostingControls.Check(account, Line("110100", 100m, paymentReference: "PAY-1")));
    }

    [Fact]
    public void Currency_validation_follows_FieldControl()
    {
        var account = Account("110100");
        account.CurrencyCode = "SAR";

        // Optional performs no check at all, even against the account's own currency.
        Assert.Null(MainAccountPostingControls.Check(account, Line("110100", 100m, currency: "USD")));

        account.ValidateCurrency = FieldControl.FillIn;
        Assert.Contains("requires a currency",
            MainAccountPostingControls.Check(account, Line("110100", 100m, currency: "")));
        // FillIn only wants a value; it does not compare it to the account.
        Assert.Null(MainAccountPostingControls.Check(account, Line("110100", 100m, currency: "USD")));

        account.ValidateCurrency = FieldControl.Table;
        Assert.Null(MainAccountPostingControls.Check(account, Line("110100", 100m, currency: "SAR")));
        var refusal = MainAccountPostingControls.Check(account, Line("110100", 100m, currency: "USD"));
        Assert.Contains("accepts currency SAR only", refusal);
        Assert.Contains("not USD", refusal);
    }

    [Fact]
    public void Posting_type_validation_follows_FieldControl()
    {
        var account = Account("110100");
        account.PostingType = LedgerPostingType.LedgerJournal;

        account.ValidatePosting = FieldControl.FillIn;
        Assert.Contains("requires a posting type",
            MainAccountPostingControls.Check(account, Line("110100", 100m, postingType: LedgerPostingType.None)));

        account.ValidatePosting = FieldControl.Table;
        Assert.Null(MainAccountPostingControls.Check(account, Line("110100", 100m)));
        Assert.Contains("accepts posting type LedgerJournal only",
            MainAccountPostingControls.Check(account, Line("110100", 100m, postingType: LedgerPostingType.Bank)));
    }

    [Fact]
    public void Controls_this_path_cannot_apply_are_reported_per_account()
    {
        var demand = Account("220100");
        demand.DebitCreditBalanceDemand = DebitCreditDemand.Demand;
        var list = Account("230100");
        list.ValidateCurrency = FieldControl.List;
        // Table mode with nothing on the account to compare against.
        var tableWithoutCurrency = Account("310100");
        tableWithoutCurrency.ValidateCurrency = FieldControl.Table;
        var user = Account("320100");
        user.ValidateUser = FieldControl.Table;
        var uom = Account("410100");
        uom.UnitOfMeasure = 7;

        var reported = MainAccountPostingControls.Unenforced([demand, list, tableWithoutCurrency, user, uom]);
        Assert.Contains(reported, x => x.Contains("220100") && x.Contains("balance"));
        Assert.Contains(reported, x => x.Contains("230100") && x.Contains("list of allowed values"));
        Assert.Contains(reported, x => x.Contains("310100") && x.Contains("have no currency"));
        Assert.Contains(reported, x => x.Contains("320100") && x.Contains("posting user"));
        Assert.Contains(reported, x => x.Contains("410100") && x.Contains("unit of measure"));

        // An applied control is never reported as unapplied.
        Assert.DoesNotContain(reported, x => x.Contains("payment reference"));
        Assert.DoesNotContain(reported, x => x.Contains("debit amounts"));
    }

    // Table mode that has something to compare is applied, so it is not also reported.
    [Fact]
    public void A_table_check_with_a_value_to_compare_is_not_reported_as_unapplied()
    {
        var account = Account("110100");
        account.CurrencyCode = "SAR";
        account.ValidateCurrency = FieldControl.Table;
        Assert.Empty(MainAccountPostingControls.Unenforced([account]));
    }

    private static MainAccount Account(string id) => new() { RecId = 1, MainAccountId = id };

    private static GeneralJournalAccountEntry Line(string ledgerAccount, decimal amount,
        string paymentReference = "", string currency = "SAR",
        LedgerPostingType postingType = LedgerPostingType.LedgerJournal) => new()
        {
            LedgerAccount = ledgerAccount, AccountingCurrencyAmount = amount,
            PaymentReference = paymentReference, TransactionCurrencyCode = currency, PostingType = postingType
        };
}
