using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
    public class CustParametersDto : EntityDto<long>
    {
        public int Key { get; set; }

        // Customer Defaults & Credit Rules
        [StringLength(10)]
        public string CustPostingProfile { get; set; } = string.Empty;

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
        public NoYes PriceDiscSearchPrice { get; set; }

        public NoYes PriceDiscSearchLineDisc { get; set; }

        public NoYes PriceDiscSearchMultilineDisc { get; set; }

        public NoYes PriceDiscSearchTotalDisc { get; set; }

        public NoYes PriceDiscMandatory { get; set; }

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
