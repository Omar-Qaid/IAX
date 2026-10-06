using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
    [Table("PriceDiscGroup")]
    public class PriceDiscGroup : Entity<long>
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
