using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.AccountsPayable
{
    /// <summary>Posted product receipt header (physical receipt of purchased goods).</summary>
    [Table("VendPackingSlipJour")]
    public class VendPackingSlipJour : Entity<long>
    {
        [StringLength(FieldLengths.PackingSlipId)]
        public string PackingSlipId { get; set; } = string.Empty;
        [StringLength(FieldLengths.PackingSlipId)]
        public string InternalPackingSlipId { get; set; } = string.Empty;
        [StringLength(FieldLengths.PurchId)]
        public string PurchId { get; set; } = string.Empty;
        public DateTime DeliveryDate { get; set; }
        public string OrderAccount { get; set; } = string.Empty;
        public string InvoiceAccount { get; set; } = string.Empty;
        public string LedgerVoucher { get; set; } = string.Empty;
        public decimal Qty { get; set; }
    }

    [Table("VendPackingSlipTrans")]
    public class VendPackingSlipTrans : Entity<long>
    {
        [StringLength(FieldLengths.PackingSlipId)]
        public string PackingSlipId { get; set; } = string.Empty;
        [StringLength(FieldLengths.PackingSlipId)]
        public string InternalPackingSlipId { get; set; } = string.Empty;
        [StringLength(FieldLengths.PurchId)]
        public string OrigPurchId { get; set; } = string.Empty;
        public long VendPackingSlipJour { get; set; }
        public DateTime DeliveryDate { get; set; }
        public decimal LineNum { get; set; }
        public string ItemId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Qty { get; set; }
        public decimal InventQty { get; set; }
        public decimal Ordered { get; set; }
        public decimal Remain { get; set; }
        public decimal RemainInvent { get; set; }
        public decimal ValueMst { get; set; }
        public string PurchUnit { get; set; } = string.Empty;
        public string InventTransId { get; set; } = string.Empty;
        public string InventDimId { get; set; } = string.Empty;
        public NoYes FullyMatched { get; set; } = NoYes.No;
    }
}
