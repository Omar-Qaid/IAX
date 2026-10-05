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
    public string DeliveryName { get; set; } = string.Empty;
    public string DeliveryPostalAddress { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public DateTime? ShippingDateConfirmed { get; set; }
    public DateTime? ReceiptDateConfirmed { get; set; }
    public int DeliveryDateControlType { get; set; }
    public int MpsFullRunCtpStatus { get; set; }
    public bool BlindShipment { get; set; }
    public bool ResidentialDestination { get; set; }
    public bool ExcludeFromMasterPlanning { get; set; }
    public string DeliveryReason { get; set; } = string.Empty;
    public string ExportReason { get; set; } = string.Empty;
    public string ShippingCarrier { get; set; } = string.Empty;
    public string CarrierId { get; set; } = string.Empty;
    public string CarrierGroup { get; set; } = string.Empty;
    public string BrokerId { get; set; } = string.Empty;
    public string TransportMode { get; set; } = string.Empty;
    public int CarrierService { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentSchedule { get; set; } = string.Empty;
    public string PaymentSpecification { get; set; } = string.Empty;
    public DateTime? FixedDueDate { get; set; }
    public DateTime? PaymentTermsBaseDate { get; set; }
    public string CashDiscountCode { get; set; } = string.Empty;
    public decimal DiscountPercent { get; set; }
    public decimal TotalDiscountPercent { get; set; }
    public decimal FixedExchangeRate { get; set; }
    public decimal ReportingCurrencyFixedExchangeRate { get; set; }
    public string PriceGroup { get; set; } = string.Empty;
    public string LineDiscountGroup { get; set; } = string.Empty;
    public string MultiLineDiscountGroup { get; set; } = string.Empty;
    public string TotalDiscountGroup { get; set; } = string.Empty;
    public string ChargesGroup { get; set; } = string.Empty;
    public string CustomerRebateGroup { get; set; } = string.Empty;
    public string CustomerTmaGroup { get; set; } = string.Empty;
    public string RebateReference { get; set; } = string.Empty;
    public bool TotalDiscountOverride { get; set; }
    public string SalesPool { get; set; } = string.Empty;
    public string CarrierCustomerAccount { get; set; } = string.Empty;
    public string FreightZone { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public bool IntercompanyAutoCreateOrders { get; set; }
    public bool IntercompanyDirectDelivery { get; set; }
    public int IntercompanyOrigin { get; set; }
    public bool IntercompanyAllowIndirectCreation { get; set; }
    public string ReleaseStatus { get; set; } = string.Empty;
    public int Reservation { get; set; }
}
