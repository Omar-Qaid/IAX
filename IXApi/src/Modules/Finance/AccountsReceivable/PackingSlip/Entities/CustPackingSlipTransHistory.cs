using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public sealed class CustPackingSlipTransHistory : Entity<long>
{
    public long CustPackingSlipTrans { get; set; }
    public CustPackingSlipTrans PackingSlipLine { get; set; } = null!;
    public decimal InventQty { get; set; }
    public decimal PdsCwQty { get; set; }
    public decimal PdsCwRemain { get; set; }
    public decimal Qty { get; set; }
    public decimal Remain { get; set; }
    public decimal RemainInvent { get; set; }
    public decimal StatValueMst { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public decimal ValueMst { get; set; }
    public decimal Weight { get; set; }
}
