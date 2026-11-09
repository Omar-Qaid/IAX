namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public sealed record VendorPrepaymentInvoicePostDto(string InvoiceId, DateTime InvoiceDate, decimal Amount);
public sealed record VendorPrepaymentApplicationDto(long PrepaymentInvoiceRecId, long FinalInvoiceRecId, DateTime AccountingDate, decimal Amount);
public sealed record VendorPrepaymentApplicationReversalDto(DateTime AccountingDate);
