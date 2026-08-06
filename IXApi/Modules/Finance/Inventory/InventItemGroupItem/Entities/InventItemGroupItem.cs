using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("InventItemGroupItem")]
    public class InventItemGroupItem : Entity<long>
    {
        //----------------------------------------- Core Information & Keys
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.ItemId)]
        public string ItemId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.ItemGroupId)]
        public string ItemGroupId { get; set; } = string.Empty;

        // ==========================================================
        // Multi-Company / Cross-DataArea Contexts
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.ItemDataAreaId)]
        public string ItemDataAreaId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.ItemGroupDataAreaId)]
        public string ItemGroupDataAreaId { get; set; } = string.Empty;

        #region Navigation Properties Row

//         [ForeignKey(nameof(ItemId))]
//         public virtual InventTable? InventTable { get; set; }

//         [ForeignKey(nameof(ItemGroupId))]
//         public virtual InventItemGroup? InventItemGroup { get; set; }

        #endregion
    }
}

