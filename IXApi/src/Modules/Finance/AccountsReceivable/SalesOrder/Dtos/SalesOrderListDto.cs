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
    public string DeliveryTerms { get; set; } = string.Empty;
    public string PaymentTerms { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string InventSiteId { get; set; } = string.Empty;
    public string InventLocationId { get; set; } = string.Empty;
    public string SalesNameAlias { get; set; } = string.Empty;
    public int SalesType { get; set; }
    public bool OneTimeCustomer { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime? Deadline { get; set; }
    public string CustomerRequisitionNumber { get; set; } = string.Empty;
    public string CampaignId { get; set; } = string.Empty;
    public string TaxGroupId { get; set; } = string.Empty;
    public bool PricesIncludeSalesTax { get; set; }
    public string SalesGroup { get; set; } = string.Empty;
    public string LanguageId { get; set; } = string.Empty;
}
