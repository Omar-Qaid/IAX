using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.AccountsPayable
{
    /// <summary>Posted purchase order confirmation (immutable snapshot per confirmation version).</summary>
    [Table("VendPurchOrderJour")]
    public class VendPurchOrderJour : Entity<long>
    {
        [StringLength(FieldLengths.PurchId)]
        public string PurchId { get; set; } = string.Empty;
        public string PurchOrderDocNum { get; set; } = string.Empty;
        public DateTime PurchOrderDate { get; set; }
        public string OrderAccount { get; set; } = string.Empty;
        public string InvoiceAccount { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = string.Empty;
        public decimal Qty { get; set; }
        public decimal Amount { get; set; }
    }

    [Table("VendPurchOrderTrans")]
    public class VendPurchOrderTrans : Entity<long>
    {
        [StringLength(FieldLengths.PurchId)]
        public string PurchId { get; set; } = string.Empty;
        public string PurchOrderDocNum { get; set; } = string.Empty;
        public decimal LineNumber { get; set; }
        public string ItemId { get; set; } = string.Empty;
        public decimal Qty { get; set; }
        public string PurchUnit { get; set; } = string.Empty;
        public decimal PurchPrice { get; set; }
        public decimal PriceUnit { get; set; }
        public decimal LineAmount { get; set; }
        public string InventTransId { get; set; } = string.Empty;
        public string InventDimId { get; set; } = string.Empty;
    }
}
