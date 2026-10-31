using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.GeneralLedger.LedgerAccount;
using Xunit;

namespace IAX.IXApi.Tests;

/// <summary>
/// The main account controls were stored and read by nobody, so an account restricted to one side,
/// or requiring a payment reference, posted as though it were unrestricted. The side is judged on
/// the posted signed amount: positive debit, negative credit.
/// </summary>
public class MainAccountPostingControlsTests
{
    [Fact]
    public void An_unrestricted_account_allows_either_side()
    {
        var account = new MainAccount { RecId = 1, MainAccountId = "110100" };
        Assert.Null(MainAccountPostingControls.Check(account, "110100", 100m, ""));
        Assert.Null(MainAccountPostingControls.Check(account, "110100", -100m, ""));
        Assert.Empty(MainAccountPostingControls.Unenforced([account]));
    }

    [Theory]
    [InlineData(DebitCreditProposal.Debit, 100, null)]
    [InlineData(DebitCreditProposal.Debit, -100, "debit amounts only")]
    [InlineData(DebitCreditProposal.Credit, -100, null)]
    [InlineData(DebitCreditProposal.Credit, 100, "credit amounts only")]
    public void A_checked_account_accepts_only_the_proposed_side(
        DebitCreditProposal proposal, int postedAmount, string? expected)
    {
        decimal amount = postedAmount;
        var account = new MainAccount
        {
            RecId = 1, MainAccountId = "611000",
            DebitCreditCheck = DebitCreditCheck.Check, DebitCreditProposal = proposal
        };
        var result = MainAccountPostingControls.Check(account, "611000-ADMIN", amount, "");
        if (expected == null) Assert.Null(result);
        else
        {
            Assert.Contains(expected, result);
            // The message names the combination that was posted, not just the main account.
            Assert.Contains("611000-ADMIN", result);
        }
    }

    // A proposed side with the check switched off is a default for entry, not a restriction.
    [Fact]
    public void A_proposed_side_alone_restricts_nothing()
    {
        var account = new MainAccount
        {
            RecId = 1, MainAccountId = "611000", DebitCreditProposal = DebitCreditProposal.Debit
        };
        Assert.Null(MainAccountPostingControls.Check(account, "611000", -100m, ""));
        Assert.Empty(MainAccountPostingControls.Unenforced([account]));
    }

    // The voucher balance check owns the zero-amount case, so the side check stays out of it.
    [Fact]
    public void A_zero_amount_is_left_to_the_balance_check()
    {
        var account = new MainAccount
        {
            RecId = 1, MainAccountId = "611000",
            DebitCreditCheck = DebitCreditCheck.Check, DebitCreditProposal = DebitCreditProposal.Debit
        };
        Assert.Null(MainAccountPostingControls.Check(account, "611000", 0m, ""));
    }

    [Fact]
    public void A_mandatory_payment_reference_is_required_and_whitespace_does_not_satisfy_it()
    {
        var account = new MainAccount
        {
            RecId = 1, MainAccountId = "110100", MandatoryPaymentReference = NoYes.Yes
        };
        Assert.Contains("requires a payment reference",
            MainAccountPostingControls.Check(account, "110100", 100m, ""));
        Assert.Contains("requires a payment reference",
            MainAccountPostingControls.Check(account, "110100", 100m, "   "));
        Assert.Null(MainAccountPostingControls.Check(account, "110100", 100m, "PAY-1"));
    }

    // The payment reference is checked before the side, so the missing reference is reported first.
    [Fact]
    public void A_missing_payment_reference_is_reported_before_a_wrong_side()
    {
        var account = new MainAccount
        {
            RecId = 1, MainAccountId = "110100", MandatoryPaymentReference = NoYes.Yes,
            DebitCreditCheck = DebitCreditCheck.Check, DebitCreditProposal = DebitCreditProposal.Debit
        };
        Assert.Contains("payment reference", MainAccountPostingControls.Check(account, "110100", -5m, ""));
    }

    [Fact]
    public void Controls_this_path_does_not_apply_are_reported_per_account()
    {
        var accounts = new[]
        {
            // Check with no proposed side names nothing to enforce.
            new MainAccount { RecId = 1, MainAccountId = "130100", DebitCreditCheck = DebitCreditCheck.Check },
            new MainAccount { RecId = 2, MainAccountId = "220100", DebitCreditBalanceDemand = DebitCreditDemand.Demand },
            new MainAccount { RecId = 3, MainAccountId = "230100", CurrencyCode = "USD" },
            new MainAccount { RecId = 4, MainAccountId = "310100", ValidateCurrency = 1 },
            new MainAccount { RecId = 5, MainAccountId = "320100", ValidatePosting = 1 },
            new MainAccount { RecId = 6, MainAccountId = "410100", ValidateUser = 1 }
        };

        var reported = MainAccountPostingControls.Unenforced(accounts);
        Assert.Contains(reported, x => x.Contains("130100") && x.Contains("propose neither"));
        Assert.Contains(reported, x => x.Contains("220100") && x.Contains("balance"));
        // Both the account currency and the currency validation flag mean a currency restriction.
        Assert.Contains(reported, x => x.Contains("230100") && x.Contains("310100") && x.Contains("currency"));
        Assert.Contains(reported, x => x.Contains("320100") && x.Contains("posting types"));
        Assert.Contains(reported, x => x.Contains("410100") && x.Contains("which users"));

        // An applied control is not reported as unapplied.
        Assert.DoesNotContain(reported, x => x.Contains("payment reference"));
    }
}
