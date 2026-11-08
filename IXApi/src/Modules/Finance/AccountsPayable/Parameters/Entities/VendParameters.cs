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
        /// D365 <c>TOTALPRICETOLERANCEPERCENTAGE</c> (price totals match tolerance), verified against
        /// real <c>VendParameters</c> metadata. Percentage by which a vendor invoice line's net amount
        /// may differ from the ordered amount for the same quantity and still post, when a matching
        /// policy applies. Zero means an exact match is required, which is the behavior before this
        /// field existed and remains the default. Read by <c>VendorInvoiceService</c>; nothing else
        /// consumes it. Gated by <see cref="UseInvoiceMatching"/> and
        /// <see cref="UseTotalPriceMatching"/>, as in D365.
        /// </summary>
        [Column(TypeName = "decimal(32, 6)")]
        public decimal TotalPriceTolerancePercentage { get; set; }

        /// <summary>
        /// D365 <c>USEINVOICEMATCHING</c>: the master switch for invoice matching. When this is
        /// <see cref="NoYes.No"/> no matching runs at all, whatever the policies say.
        /// </summary>
        /// <remarks>
        /// D365 ships this as No for a new company. It defaults to <see cref="NoYes.Yes"/> here, and
        /// the column default matches, because matching already ran before this field existed:
        /// shipping D365's default would silently stop matching invoices that are matched today.
        /// Turning matching off is therefore an explicit opt-out rather than the starting state.
        /// </remarks>
        public NoYes UseInvoiceMatching { get; set; } = NoYes.Yes;

        /// <summary>
        /// D365 <c>USETOTALPRICEMATCHING</c>: gates price totals matching specifically, inside
        /// <see cref="UseInvoiceMatching"/>. Defaults to <see cref="NoYes.Yes"/> for the same reason.
        /// </summary>
        public NoYes UseTotalPriceMatching { get; set; } = NoYes.Yes;

        /// <summary>
        /// D365 <c>MATCHINGPOLICY</c>: the company-wide default matching policy, used when the
        /// vendor and the item say nothing or may not override it. <see cref="MatchingPolicy.None"/>
        /// by default, so it adds nothing until it is configured.
        /// </summary>
        public MatchingPolicy MatchingPolicy { get; set; } = MatchingPolicy.None;

        /// <summary>
        /// D365 <c>MATCHINGPOLICYALLOWOVERRIDE</c>: whether a vendor or item policy may override
        /// <see cref="MatchingPolicy"/>. Defaults to <see cref="NoYes.Yes"/> because the vendor and
        /// item policies are what drove matching before this field existed.
        /// </summary>
        public NoYes MatchingPolicyAllowOverride { get; set; } = NoYes.Yes;

        /// <summary>
        /// D365 <c>USEEXTENDEDPRICEMATCHING</c>: gates net unit price (extended price) matching,
        /// inside <see cref="UseInvoiceMatching"/>. This compares the invoice's net <b>unit</b>
        /// price with the ordered one, where <see cref="UseTotalPriceMatching"/> compares the net
        /// <b>amount</b> for the quantity; the two are independent gates, as in D365.
        /// </summary>
        /// <remarks>
        /// Unlike <see cref="UseInvoiceMatching"/> and <see cref="UseTotalPriceMatching"/>, this one
        /// takes D365's own default of <see cref="NoYes.No"/>. Those two default to Yes here only
        /// because matching already ran before they existed; net unit price matching never ran, so
        /// there is no behavior to preserve and defaulting to No changes nothing for any caller.
        /// </remarks>
        public NoYes UseExtendedPriceMatching { get; set; } = NoYes.No;

        /// <summary>
        /// D365 <c>EXTENDEDPRICETOLERANCEPERCENTAGE</c>: percentage by which the invoice's net unit
        /// price may differ from the ordered unit price. Applies only when greater than zero.
        /// </summary>
        [Column(TypeName = "decimal(32, 6)")]
        public decimal ExtendedPriceTolerancePercentage { get; set; }

        /// <summary>
        /// D365 <c>EXTENDEDPRICETOLERANCEAMOUNT</c>: absolute amount by which the invoice's net unit
        /// price may differ from the ordered unit price, in the document currency. Applies only when
        /// greater than zero, and is checked alongside
        /// <see cref="ExtendedPriceTolerancePercentage"/> rather than instead of it: any configured
        /// tolerance that is exceeded fails the match.
        /// </summary>
        [Column(TypeName = "decimal(32, 6)")]
        public decimal ExtendedPriceToleranceAmount { get; set; }

        public long Partition { get; set; }
    }
}
