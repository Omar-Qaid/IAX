using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.AccountsPayable
{
    public class VendPaymModeDto : EntityDto<long>
    {
        [Required]
        [StringLength(FieldLengths.PaymModeId)]
        public string PaymMode { get; set; } = string.Empty;

        [StringLength(FieldLengths.Name)]
        public string Name { get; set; } = string.Empty;

        public LedgerJournalACType AccountType { get; set; }

        public long? PaymentLedgerDimension { get; set; }

        [StringLength(FieldLengths.JournalNameId)]
        public string PaymJournalNameId { get; set; } = string.Empty;

        // D365 VendPaymModeTable fields (localization fields such as _RU/_BR/_IT/_MX/_W are not carried).
        public CustVendPaymStatus PaymStatus { get; set; }
        public TypeOfDraft TypeOfDraft { get; set; }
        [StringLength(FieldLengths.BankTransType)]
        public string BankTransType { get; set; } = string.Empty;
        public long? DimensionAttributeSet { get; set; }
        public long? InterCompanyLedgerDimension { get; set; }
        public int DiscGraceDays { get; set; }
        public int PaymSumBy { get; set; }
        public int LastSequenceNumber { get; set; }
        public DateTime? LastSequenceNumDate { get; set; }
        public int LastSequenceNumToday { get; set; }
        public int PaymGenerationLineLimit { get; set; }
        public NoYes AllowPaymentCopies { get; set; }
        public NoYes PdcClearingPosting { get; set; }
        public NoYes ExportOnInvoice { get; set; }
        public NoYes PaymOnInvoice { get; set; }
        public NoYes FurtherPosting { get; set; }
        public NoYes DirectDebit { get; set; }
        public NoYes BridgingAccountByBank { get; set; }
    }
}
