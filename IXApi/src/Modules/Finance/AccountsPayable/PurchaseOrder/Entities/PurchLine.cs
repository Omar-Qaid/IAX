using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.AccountsPayable
{
    [Table("PurchLine")]
    public class PurchLine : Entity<long>
    {
        [StringLength(FieldLengths.PurchId)]
        public string PurchId { get; set; } = string.Empty;
        public decimal LineNumber { get; set; }
        [StringLength(FieldLengths.ItemId)]
        public string ItemId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        [StringLength(FieldLengths.VendAccount)]
        public string VendAccount { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = string.Empty;

        // Quantities: purchase unit (Purch*) and inventory unit (Invent*/QtyOrdered).
        public decimal PurchQty { get; set; }
        public string PurchUnit { get; set; } = string.Empty;
        public decimal QtyOrdered { get; set; }
        public decimal PurchPrice { get; set; }
        public decimal PriceUnit { get; set; } = 1m;
        public decimal LineAmount { get; set; }

        public decimal RemainPurchPhysical { get; set; }
        public decimal RemainPurchFinancial { get; set; }
        public decimal RemainInventPhysical { get; set; }
        public decimal RemainInventFinancial { get; set; }

        public PurchaseType PurchaseType { get; set; } = PurchaseType.Purch;
        public PurchStatus PurchStatus { get; set; } = PurchStatus.Backorder;
        /// <summary>Original receipt this return line reverses (D365 InventTransIdReturn).</summary>
        [StringLength(FieldLengths.InventTransId)]
        public string InventTransIdReturn { get; set; } = string.Empty;
        [StringLength(FieldLengths.InventTransId)]
        public string InventTransId { get; set; } = string.Empty;
        public string InventDimId { get; set; } = string.Empty;
        public DateTime DeliveryDate { get; set; }
        public long DefaultDimension { get; set; }
    }
}
