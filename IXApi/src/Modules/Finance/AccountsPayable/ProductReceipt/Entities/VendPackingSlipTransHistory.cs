using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public sealed class VendPackingSlipTransHistory : Entity<long>
{
    public long VendPackingSlipTrans { get; set; }
    public VendPackingSlipTrans PackingSlipLine { get; set; } = null!;
    public decimal InventQty { get; set; }
    public decimal Qty { get; set; }
    public decimal Remain { get; set; }
    public decimal RemainInvent { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public decimal ValueMst { get; set; }
}
