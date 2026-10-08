using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.PriceDisc
{
    public class PriceDiscGroupDto : EntityDto<long>
    {
        [Required]
        [StringLength(10)]
        public string GroupId { get; set; } = string.Empty;

        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        public PriceType Type { get; set; }

        public ModuleInventCustVend Module { get; set; }

        public int McrPriceDiscGroupType { get; set; }

        public int RetailCheckSalesPriceStatus { get; set; }

        public int RetailPricingPriorityNumber { get; set; }

        public long PricingRuleRecId { get; set; }

        public int PriceGroupAttributeEnable { get; set; }

        public long Partition { get; set; }
    }
}
