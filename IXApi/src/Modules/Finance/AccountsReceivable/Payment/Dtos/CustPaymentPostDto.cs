namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Payment;

public sealed record CustPaymentPostDto(string CustAccount, string BankAccountId, decimal Amount,
    DateTime? PaymentDate = null, string? PaymMode = null, string? SettleInvoiceId = null, string? RequestId = null);
