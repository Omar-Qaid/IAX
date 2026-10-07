using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
    /// <summary>Trade Agreement Journal Name / Setup (AX PriceDiscAdmName).</summary>
    [Table("PriceDiscAdmName")]
    public class PriceDiscAdmName : Entity<long>
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
