using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
    /// <summary>Accounts Receivable pricing and customer parameters mapped to the existing AX CustParameters table.</summary>
    [Table("CustParameters")]
    public class ReceivableParameters : Entity<long>
    {
        public int Key { get; set; } = 0;

        // Customer Defaults & Credit Rules
        [StringLength(10)]
        public string CustPostingProfile { get; set; } = string.Empty;

        [Column(TypeName = "decimal(32, 6)")]
        public decimal CreditLimit { get; set; }

        public int CreditLimitCheck { get; set; }

        [StringLength(10)]
        public string PaymTermId { get; set; } = string.Empty;

        [StringLength(10)]
        public string PaymMode { get; set; } = string.Empty;

        [StringLength(10)]
        public string DlvMode { get; set; } = string.Empty;

        [StringLength(10)]
        public string DlvReasonId { get; set; } = string.Empty;

        [StringLength(3)]
        public string CurrencyCode { get; set; } = string.Empty;

        // Trade Agreement & Pricing Parameters
        public NoYes PriceDiscSearchPrice { get; set; } = NoYes.Yes;

        public NoYes PriceDiscSearchLineDisc { get; set; } = NoYes.Yes;

        public NoYes PriceDiscSearchMultilineDisc { get; set; } = NoYes.Yes;

        public NoYes PriceDiscSearchTotalDisc { get; set; } = NoYes.Yes;

        public NoYes PriceDiscMandatory { get; set; } = NoYes.No;

        [StringLength(10)]
        public string PriceDiscJournalNamePrice { get; set; } = string.Empty;

        [StringLength(10)]
        public string PriceDiscJournalNameLineDisc { get; set; } = string.Empty;

        [StringLength(10)]
        public string PriceDiscJournalNameMultilineDisc { get; set; } = string.Empty;

        [StringLength(10)]
        public string PriceDiscJournalNameTotalDisc { get; set; } = string.Empty;

        // Sales Order & Invoicing Defaults
        public int SalesOrderType { get; set; }

        public int Reservation { get; set; }

        [StringLength(10)]
        public string TaxGroup { get; set; } = string.Empty;

        [StringLength(10)]
        public string InvoiceJournalName { get; set; } = string.Empty;

        [StringLength(10)]
        public string PackingSlipJournalName { get; set; } = string.Empty;

        [StringLength(10)]
        public string CustNumSeqGroup { get; set; } = string.Empty;

        public long Partition { get; set; }
    }
}
