using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public sealed class CustPackingSlipVersion : SharedEntity<long>
{
    public long CustPackingSlipJour { get; set; }
    public string InternalPackingSlipId { get; set; } = string.Empty;
    public string LedgerVoucher { get; set; } = string.Empty;
    public string ParmId { get; set; } = string.Empty;
    public DateTime AccountingDate { get; set; }
    public DateTime VersionDateTime { get; set; }
    public decimal Qty { get; set; }
    public decimal Volume { get; set; }
    public decimal Weight { get; set; }
}
