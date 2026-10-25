using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public class VendPackingSlipJourDto : EntityDto<long>
{
    public string PackingSlipId { get; set; } = string.Empty;
    public string InternalPackingSlipId { get; set; } = string.Empty;
    public string PurchId { get; set; } = string.Empty;
    public DateTime DeliveryDate { get; set; }
    public string OrderAccount { get; set; } = string.Empty;
    public string InvoiceAccount { get; set; } = string.Empty;
    public string LedgerVoucher { get; set; } = string.Empty;
    public decimal Qty { get; set; }
}
