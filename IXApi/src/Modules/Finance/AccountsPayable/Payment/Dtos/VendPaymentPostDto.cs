namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public sealed record VendPaymentPostDto(string VendAccount, string BankAccountId, decimal Amount, string RequestId,
    DateTime? PaymentDate = null, string? PaymMode = null, string? SettleInvoiceId = null,
    bool SettleAutomatically = true);

/// <summary>
/// A vendor refund: the vendor returns money it owes us and the receipt is settled against that
/// open debit. <paramref name="CreditRecId"/> is the VendTrans the refund clears, normally a
/// vendor credit note or an unapplied payment.
/// </summary>
public sealed record VendRefundPostDto(string VendAccount, string BankAccountId, long CreditRecId,
    string RequestId, DateTime RefundDate);
