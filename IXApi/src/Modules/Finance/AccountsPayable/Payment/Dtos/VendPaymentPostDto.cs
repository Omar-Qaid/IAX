namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public sealed record VendPaymentPostDto(string VendAccount, string BankAccountId, decimal Amount, string RequestId,
    DateTime? PaymentDate = null, string? PaymMode = null, string? SettleInvoiceId = null);
