using System.ComponentModel.DataAnnotations;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public sealed class CustomerQuickCreateDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? NameAlias { get; set; }
    [Required]
    public string CustGroupId { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "SAR";
    public string? CustCategory { get; set; }
    public string? PaymTermId { get; set; }
    public string? PaymModeId { get; set; }
    public string? DlvModeId { get; set; }
    public string? TaxGroupId { get; set; }
    public string? VatNum { get; set; }
    public string? CountryRegionId { get; set; }
    public string? Memo { get; set; }
}

public sealed class CustomerListDto
{
    public long RecId { get; set; }
    public long Party { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string CustomerGroupId { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public string CustCategory { get; set; } = string.Empty;
    public string PaymTermId { get; set; } = string.Empty;
    public string PaymModeId { get; set; } = string.Empty;
    public string DlvModeId { get; set; } = string.Empty;
    public string TaxGroupId { get; set; } = string.Empty;
    public string VatNum { get; set; } = string.Empty;
    public string CountryRegionId { get; set; } = string.Empty;
    public string? Memo { get; set; }
    public string? Phone { get; set; }
    public string InvoiceAccount { get; set; } = string.Empty;
    public string InventSiteId { get; set; } = string.Empty;
    public string InventLocationId { get; set; } = string.Empty;
    public string Status { get; set; } = "active";
    public DateTime CreatedAt { get; set; }
}
