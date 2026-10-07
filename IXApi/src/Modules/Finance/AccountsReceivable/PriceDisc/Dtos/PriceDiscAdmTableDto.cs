using System;
using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
    public class PriceDiscAdmTableDto : EntityDto<long>
    {
        [Required]
        [StringLength(100)]
        public string JournalNum { get; set; } = string.Empty;

        [StringLength(10)]
        public string JournalName { get; set; } = string.Empty;

        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        public PriceType DefaultRelation { get; set; }

        public NoYes Posted { get; set; }

        public DateTime? PostedDate { get; set; }

        public int ExportCurrentPrice { get; set; }

        public int LockedForDeletion { get; set; }

        [StringLength(10)]
        public string PriceGroup { get; set; } = string.Empty;

        public long PriceComponentCombination { get; set; }

        public int PriceApplyAdjustment { get; set; }

        public long Partition { get; set; }
    }
}
