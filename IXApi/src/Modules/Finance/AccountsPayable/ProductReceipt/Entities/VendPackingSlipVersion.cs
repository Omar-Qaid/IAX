using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public sealed class VendPackingSlipVersion : SharedEntity<long>
{
    public long VendPackingSlipJour { get; set; }
    public string InternalPackingSlipId { get; set; } = string.Empty;
    public string LedgerVoucher { get; set; } = string.Empty;
    public string ParmId { get; set; } = string.Empty;
    public DateTime AccountingDate { get; set; }
    public DateTime VersionDateTime { get; set; }
    public decimal Qty { get; set; }
}
