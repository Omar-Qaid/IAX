using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Finance.AccountsPayable
{
    /// <summary>
    /// Vendor method of payment, the payable counterpart of
    /// <see cref="AccountsReceivable.CustPaymModeTable"/>. A vendor payment's method must be one of
    /// these, and <c>VendTable.PaymMode</c> names the vendor's default.
    /// </summary>
    /// <remarks>
    /// A reduced field set. D365's <c>VendPaymModeTable</c> also carries payment file formats and
    /// electronic reporting mappings, bridging and further-posting behavior, payment status, and a
    /// long tail of country-specific fields. None of those have a flow here that reads them, and a
    /// setup field nothing reads is an invented one, so they are added with their consumers.
    /// </remarks>
    [Table("VendPaymModeTable")]
    public class VendPaymModeTable : Entity<long>
    {
        [Required]
        [StringLength(FieldLengths.PaymModeId)]
        public string PaymMode { get; set; } = string.Empty;

        [StringLength(FieldLengths.Name)]
        public string Name { get; set; } = string.Empty;

        /// <summary>Whether the method pays from a bank account or a ledger account.</summary>
        public LedgerJournalACType AccountType { get; set; }

        /// <summary>The method's payment account. Not yet read: the payment names its bank account.</summary>
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
