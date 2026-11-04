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

        /// <summary>Zero requires an exact invoice price match, as before this field existed.</summary>
        public decimal TotalPriceTolerancePercentage { get; set; }

        public NoYes UseInvoiceMatching { get; set; } = NoYes.Yes;

        public NoYes UseTotalPriceMatching { get; set; } = NoYes.Yes;

        /// <summary>Company default; a vendor or item policy overrides it when allowed.</summary>
        public MatchingPolicy MatchingPolicy { get; set; } = MatchingPolicy.None;

        public NoYes MatchingPolicyAllowOverride { get; set; } = NoYes.Yes;

        public long Partition { get; set; }
    }
}
