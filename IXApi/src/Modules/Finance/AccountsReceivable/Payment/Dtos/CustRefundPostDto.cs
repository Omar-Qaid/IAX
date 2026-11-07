namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Payment;

public sealed record CustRefundPostDto(
    string CustAccount, string BankAccountId, string JournalName,
    long CreditRecId, DateTime RefundDate, string RequestId);
