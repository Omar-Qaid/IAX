using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Invoice;

public sealed class CustRelatedInvoice : Entity<long>
{
    public long? CustInvoice { get; set; }
    public int? InvoiceAssociationType { get; set; }
    public long? OriginalCustInvoice { get; set; }
    public long? ParentCustInvoice { get; set; }
}
