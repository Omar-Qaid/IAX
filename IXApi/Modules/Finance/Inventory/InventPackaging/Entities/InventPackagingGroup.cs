using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("InventPackagingGroup")]
    public class InventPackagingGroup : Entity<long>
    {
        //----------------------------------------- Core Information
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.PackagingGroupId)]
        public string PackagingGroupId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.Name)]
        public string Name { get; set; } = string.Empty;
    }
}
