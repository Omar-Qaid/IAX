using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.AccountsPayable
{
    public class VendParametersDto : EntityDto<long>
    {
        public int Key { get; set; }

        [StringLength(FieldLengths.PostingProfile)]
        public string VendPostingProfile { get; set; } = string.Empty;
        public string AdvancePostingProfile { get; set; } = string.Empty;

        /// <summary>Zero requires an exact invoice price match, as before this field existed.</summary>
        public decimal TotalPriceTolerancePercentage { get; set; }

        public NoYes UseInvoiceMatching { get; set; } = NoYes.Yes;

        public NoYes UseTotalPriceMatching { get; set; } = NoYes.Yes;

        /// <summary>Company default; a vendor or item policy overrides it when allowed.</summary>
        public MatchingPolicy MatchingPolicy { get; set; } = MatchingPolicy.None;

        public NoYes MatchingPolicyAllowOverride { get; set; } = NoYes.Yes;

        /// <summary>
        /// Gates net unit price matching. D365's own default of No, because this matching never ran
        /// before the field existed.
        /// </summary>
        public NoYes UseExtendedPriceMatching { get; set; } = NoYes.No;

        /// <summary>Zero means this tolerance is not configured.</summary>
        public decimal ExtendedPriceTolerancePercentage { get; set; }

        /// <summary>Zero means this tolerance is not configured. An absolute per-unit amount.</summary>
        public decimal ExtendedPriceToleranceAmount { get; set; }

        public long Partition { get; set; }
    }
}
