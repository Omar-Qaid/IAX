using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
    /// <summary>Return disposition code: decides whether returned goods are received and credited.</summary>
    [Table("ReturnDispositionCode")]
    public class ReturnDispositionCode : Entity<long>
    {
        [StringLength(20)]
        public string DispositionCodeId { get; set; } = string.Empty;
        [StringLength(250)]
        public string Description { get; set; } = string.Empty;
        public DispositionAction DispositionAction { get; set; }
    }

    /// <summary>Return reason code recorded on the return order header.</summary>
    [Table("ReturnReasonCode")]
    public class ReturnReasonCode : Entity<long>
    {
        [StringLength(20)]
        public string ReasonCodeId { get; set; } = string.Empty;
        [StringLength(250)]
        public string Description { get; set; } = string.Empty;
        [StringLength(20)]
        public string ReasonCodeGroupId { get; set; } = string.Empty;
    }
}
