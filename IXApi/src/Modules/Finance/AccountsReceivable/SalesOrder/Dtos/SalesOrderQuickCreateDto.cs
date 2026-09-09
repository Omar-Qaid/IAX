using System.ComponentModel.DataAnnotations;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public sealed class SalesOrderQuickCreateDto
{
    [Required]
    public string CustomerAccount { get; set; } = string.Empty;
    public bool OneTimeCustomer { get; set; }
    public string? Contact { get; set; }
    public string? DeliveryName { get; set; }
    public long? DeliveryPostalAddress { get; set; }
    public string? CustomerReference { get; set; }
    public string? InvoiceAccount { get; set; }
    public string? CurrencyCode { get; set; }
    [StringLength(100)]
    public string? PaymentTerms { get; set; }
    [StringLength(10)]
    public string? PaymentMethod { get; set; }
    public string? SalesName { get; set; }
    public string? SalesGroup { get; set; }
    public string? InventSiteId { get; set; }
    public string? InventLocationId { get; set; }
    public string? CustomerRequisitionNumber { get; set; }
    public bool Intercompany { get; set; }
    public string? IntercompanyCompanyId { get; set; }
    public DateTime? RequestedReceiptDate { get; set; }
    public DateTime? RequestedShipDate { get; set; }
    public int DeliveryDateControlType { get; set; }
    public bool ConfirmDates { get; set; }
    public string? DeliveryMode { get; set; }
    public string? DeliveryTerms { get; set; }
}
