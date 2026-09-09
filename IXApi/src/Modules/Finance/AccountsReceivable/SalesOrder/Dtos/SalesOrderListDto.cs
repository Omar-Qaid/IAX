namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public sealed class SalesOrderListDto
{
    public long RecId { get; set; }
    public string SalesId { get; set; } = string.Empty;
    public string CustomerAccount { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string InvoiceAccount { get; set; } = string.Empty;
    public string CustomerGroup { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public string SalesStatus { get; set; } = string.Empty;
    public string DocumentStatus { get; set; } = string.Empty;
    public DateTime DeliveryDate { get; set; }
    public DateTime ShippingDateRequested { get; set; }
    public decimal OrderTotal { get; set; }
    public string CustomerReference { get; set; } = string.Empty;
    public string DeliveryMode { get; set; } = string.Empty;
    public string PaymentTerms { get; set; } = string.Empty;
}
