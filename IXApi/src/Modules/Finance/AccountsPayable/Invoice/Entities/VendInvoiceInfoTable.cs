using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.AccountsPayable
{
    /// <summary>
    /// Pending vendor invoice header, as APTables.md section 8.1 documents it. A vendor invoice is
    /// entered against a purchase order and held here while it is matched, then posted. Posting
    /// reads this record and goes through <see cref="VendorInvoiceService"/>, so the pending invoice
    /// never carries posting logic of its own.
    /// </summary>
    /// <remarks>
    /// Lines belong to the header through <c>ParmId</c>, which APTables.md documents as the invoice
    /// processing group. <c>TableRefId</c> is on the table because the document lists it, but what
    /// generates it in D365 is not established from the repository or the cycle docs, so nothing
    /// here depends on it.
    /// </remarks>
    [Table("VendInvoiceInfoTable")]
    public class VendInvoiceInfoTable : Entity<long>
    {
        [StringLength(FieldLengths.ParmId)]
        public string ParmId { get; set; } = string.Empty;
        public int TableRefId { get; set; }

        /// <summary>Vendor invoice number.</summary>
        [StringLength(FieldLengths.Num)]
        public string Num { get; set; } = string.Empty;
        [StringLength(FieldLengths.InvoiceAccount)]
        public string InvoiceAccount { get; set; } = string.Empty;
        [StringLength(FieldLengths.OrderAccount)]
        public string OrderAccount { get; set; } = string.Empty;

        /// <summary>Vendor document date.</summary>
        public DateTime DocumentDate { get; set; }

        /// <summary>Accounting transaction date.</summary>
        public DateTime TransDate { get; set; }
        [StringLength(FieldLengths.CurrencyCode)]
        public string CurrencyCode { get; set; } = string.Empty;
        [StringLength(FieldLengths.Description)]
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Pending vendor invoice line, as APTables.md section 8.2 documents it. The association with
    /// the source purchase order line is kept through <c>OrigPurchId</c> and <c>InventTransId</c>,
    /// which is how the posting path resolves the <c>PurchLine</c> it updates.
    /// </summary>
    [Table("VendInvoiceInfoLine")]
    public class VendInvoiceInfoLine : Entity<long>
    {
        [StringLength(FieldLengths.ParmId)]
        public string ParmId { get; set; } = string.Empty;
        public int TableRefId { get; set; }

        /// <summary>Source purchase order.</summary>
        [StringLength(FieldLengths.PurchId)]
        public string OrigPurchId { get; set; } = string.Empty;
        public decimal LineNum { get; set; }
        [StringLength(FieldLengths.ItemId)]
        public string ItemId { get; set; } = string.Empty;
        [StringLength(FieldLengths.InventTransId)]
        public string InventTransId { get; set; } = string.Empty;
        [StringLength(FieldLengths.InventDimId)]
        public string InventDimId { get; set; } = string.Empty;
        [StringLength(FieldLengths.Unit)]
        public string PurchUnit { get; set; } = string.Empty;

        /// <summary>Quantity selected for invoice processing.</summary>
        public decimal ReceiveNow { get; set; }

        /// <summary>Invoice purchase price.</summary>
        public decimal PurchPrice { get; set; }

        /// <summary>Invoice line net amount.</summary>
        public decimal LineAmount { get; set; }
    }
}
