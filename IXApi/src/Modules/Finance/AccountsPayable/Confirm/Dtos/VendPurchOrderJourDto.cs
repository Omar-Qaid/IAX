using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public class VendPurchOrderJourDto : EntityDto<long>
{
    public string PurchId { get; set; } = string.Empty;
    public string PurchOrderDocNum { get; set; } = string.Empty;
    public DateTime PurchOrderDate { get; set; }
    public string OrderAccount { get; set; } = string.Empty;
    public string InvoiceAccount { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal Qty { get; set; }
    public decimal Amount { get; set; }
}
