using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.AccountsPayable
{
    /// <summary>Posted vendor invoice header (financial update of received goods).</summary>
    [Table("VendInvoiceJour")]
    public class VendInvoiceJour : Entity<long>
    {
        [StringLength(FieldLengths.InvoiceId)]
        public string InvoiceId { get; set; } = string.Empty;
        [StringLength(FieldLengths.InvoiceId)]
        public string InternalInvoiceId { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        [StringLength(FieldLengths.PurchId)]
        public string PurchId { get; set; } = string.Empty;
        public string OrderAccount { get; set; } = string.Empty;
        public string InvoiceAccount { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = string.Empty;
        public string LedgerVoucher { get; set; } = string.Empty;
        public decimal Qty { get; set; }
        public decimal SalesBalance { get; set; }
        public decimal InvoiceAmount { get; set; }
    }

    [Table("VendInvoiceTrans")]
    public class VendInvoiceTrans : Entity<long>
    {
        [StringLength(FieldLengths.InvoiceId)]
        public string InvoiceId { get; set; } = string.Empty;
        [StringLength(FieldLengths.InvoiceId)]
        public string InternalInvoiceId { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        [StringLength(FieldLengths.PurchId)]
        public string PurchId { get; set; } = string.Empty;
        public decimal LineNum { get; set; }
        public string ItemId { get; set; } = string.Empty;
        public decimal Qty { get; set; }
        public decimal InventQty { get; set; }
        public string PurchUnit { get; set; } = string.Empty;
        public decimal PurchPrice { get; set; }
        public decimal PriceUnit { get; set; }
        public decimal LineAmount { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public string InventTransId { get; set; } = string.Empty;
        public string InventDimId { get; set; } = string.Empty;
    }
}
