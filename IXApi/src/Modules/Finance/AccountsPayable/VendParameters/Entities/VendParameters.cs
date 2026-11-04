using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.AccountsPayable
{
    /// <summary>
    /// Company-wide Accounts payable defaults, the payable counterpart of
    /// <see cref="AccountsReceivable.CustParameters"/>. One row per company, keyed by
    /// <see cref="Key"/> exactly as the receivable table is.
    /// </summary>
    /// <remarks>
    /// Only the posting profile default and the invoice price-match tolerance are modelled. D365's
    /// <c>VendParameters</c> carries a great deal more — invoice defaults, number sequence groups —
    /// but the features that read those do not exist here yet, and a setup field nothing reads is
    /// an invented one. They are added with the flows that consume them. Purchase order and
    /// purchase trade agreement defaults are deliberately absent: in D365 those live in
    /// <c>PurchParameters</c>, not here.
    /// </remarks>
    [Table("VendParameters")]
    public class VendParameters : Entity<long>
    {
        /// <summary>Single-row key, as on <c>CustParameters</c>.</summary>
        public int Key { get; set; } = 0;

        /// <summary>
        /// The posting profile <c>SubledgerPostingSupport.ResolveVendorSummaryAsync</c> uses when a
        /// document names none. Without it, every <c>VendLedgerAccounts</c> rule in the company is
        /// a candidate and two posting profiles silently mix.
        /// </summary>
        [StringLength(FieldLengths.PostingProfile)]
        public string VendPostingProfile { get; set; } = string.Empty;

        /// <summary>
        /// Percentage by which a vendor invoice line's net amount may differ from the ordered amount
        /// for the same quantity and still post, when a matching policy applies. Zero means an exact
        /// match is required, which is the behavior before this field existed and remains the
        /// default. Read by <c>VendorInvoiceService</c>; nothing else consumes it.
        /// </summary>
        [Column(TypeName = "decimal(32, 6)")]
        public decimal PriceTotalMatchToleranceInPercent { get; set; }

        public long Partition { get; set; }
    }
}
