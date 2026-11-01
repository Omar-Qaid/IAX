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
    }
}
