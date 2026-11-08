namespace IAX.IXApi.Modules.Finance.AccountsPayable;

/// <summary>
/// Selection criteria for a vendor payment proposal, matching the filters D365 offers on
/// *Accounts payable &gt; Payments &gt; Vendor payment journal &gt; Payment proposal*: due date,
/// vendor, currency and method of payment. Every filter is optional; omitting all of them proposes
/// every open invoice the company owes.
/// </summary>
/// <param name="ProposalDate">
/// The date the payment is assumed to be made on. It decides whether a cash discount is still
/// available, so it is the one value that changes the proposed amounts rather than just filtering.
/// Defaults to today when omitted.
/// </param>
/// <param name="DueBy">Include only invoices due on or before this date.</param>
/// <param name="VendAccount">A single vendor, or every vendor when omitted.</param>
/// <param name="CurrencyCode">A single transaction currency, or every currency when omitted.</param>
/// <param name="PaymMode">A single vendor method of payment, or every method when omitted.</param>
public sealed record VendPaymentProposalRequestDto(
    DateTime? ProposalDate = null,
    DateTime? DueBy = null,
    string? VendAccount = null,
    string? CurrencyCode = null,
    string? PaymMode = null);

/// <summary>One open vendor invoice as proposed for payment. Nothing here is posted.</summary>
/// <param name="VendTransRecId">The open transaction, so the caller can pay or settle it by id.</param>
/// <param name="OpenAmountCur">What is still owed on the invoice, as a positive magnitude.</param>
/// <param name="CashDiscountAmount">
/// The discount available <i>on the proposal date</i>, zero when none applies or the period has
/// passed. A part payment earns no discount, which is why this is stated per whole invoice.
/// </param>
/// <param name="AmountToPay">
/// <paramref name="OpenAmountCur"/> less <paramref name="CashDiscountAmount"/>: the cash that
/// settles the invoice in full on the proposal date.
/// </param>
public sealed record VendPaymentProposalLineDto(
    long VendTransRecId,
    string VendAccount,
    string Invoice,
    string Voucher,
    DateTime TransDate,
    DateTime DueDate,
    string CurrencyCode,
    string PaymMode,
    decimal OpenAmountCur,
    string CashDiscCode,
    DateTime CashDiscountDate,
    decimal CashDiscountAmount,
    decimal AmountToPay);

/// <summary>
/// A vendor payment proposal. This is a selection, not a document: creating it posts nothing and
/// reserves nothing, exactly as generating a proposal or a payment file in D365 does not post the
/// journal or move money out of the bank.
/// </summary>
/// <param name="ExcludedBlockedVendors">
/// Vendors that had open invoices matching the filters but are blocked for payment, so their
/// invoices were left out. Reported rather than silently dropped.
/// </param>
public sealed record VendPaymentProposalDto(
    string Company,
    DateTime ProposalDate,
    IReadOnlyList<VendPaymentProposalLineDto> Lines,
    IReadOnlyList<string> ExcludedBlockedVendors)
{
    /// <summary>Total cash the proposal would pay out, by transaction currency.</summary>
    public IReadOnlyDictionary<string, decimal> TotalsByCurrency { get; } = Lines
        .GroupBy(x => x.CurrencyCode)
        .ToDictionary(x => x.Key, x => x.Sum(line => line.AmountToPay));

    public decimal TotalCashDiscount => Lines.Sum(x => x.CashDiscountAmount);

    /// <summary>Always false: a proposal never posts.</summary>
    public bool Posted => false;
}
