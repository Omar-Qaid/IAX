using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public class PurchTableDto : EntityDto<long>
{
    public string PurchId { get; set; } = string.Empty;
    public string OrderAccount { get; set; } = string.Empty;
    public string InvoiceAccount { get; set; } = string.Empty;
    public string PurchName { get; set; } = string.Empty;
    public PurchaseType PurchaseType { get; set; } = PurchaseType.Purch;
    public PurchStatus PurchStatus { get; set; } = PurchStatus.Backorder;
    public string ReturnItemNum { get; set; } = string.Empty;
    public DocumentStatus DocumentStatus { get; set; } = DocumentStatus.None;
    public string CurrencyCode { get; set; } = string.Empty;
    public string Payment { get; set; } = string.Empty;
    public DateTime DeliveryDate { get; set; }
    public DateTime OrderDate { get; set; }
    public string InventSiteId { get; set; } = string.Empty;
    public string InventLocationId { get; set; } = string.Empty;
    public long DefaultDimension { get; set; }
}
