namespace IAX.IXApi.Modules.Finance.AccountsPayable.InvoiceRegister;

/// <summary>An invoice registered in a D365 Invoice register journal (journal type PurchaseLedger).</summary>
public sealed record VendInvoiceRegisterRequest(string JournalName, string VendAccount, string Invoice, decimal Amount,
    DateOnly TransDate, string? CurrencyCode = null, DateOnly? DueDate = null, string? PostingProfile = null, string? Txt = null);

/// <summary>Approves a registered invoice in a D365 Invoice approval journal (journal type Approval).</summary>
public sealed record VendInvoiceApprovalRequest(string JournalName, string Voucher, long ExpenseLedgerDimension, DateOnly TransDate);
