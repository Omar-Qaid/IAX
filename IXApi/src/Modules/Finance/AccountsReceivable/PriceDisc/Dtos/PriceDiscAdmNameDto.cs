using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.PriceDisc
{
    public class PriceDiscAdmNameDto : EntityDto<long>
    {
        [Required]
        [StringLength(10)]
        public string JournalName { get; set; } = string.Empty;

        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        public PriceType DefaultRelation { get; set; }

        public int PriceDiscPriceAttributeEnable { get; set; }

        public long Partition { get; set; }
    }
}
