using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("InventModelGroupItem")]
    public class InventModelGroupItem : Entity<long>
    {
        //----------------------------------------- Core Information & Keys
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.ItemId)]
        public string ItemId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.ModelGroupId)]
        public string ModelGroupId { get; set; } = string.Empty;

        // ==========================================================
        // Multi-Company / Cross-DataArea Contexts
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.ItemDataAreaId)]
        public string ItemDataAreaId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.ModelGroupDataAreaId)]
        public string ModelGroupDataAreaId { get; set; } = string.Empty;


        #region Navigation Properties Row

//         [ForeignKey(nameof(ModelGroupId))]
//         public virtual InventModelGroup? ModelGroup { get; set; }

//         [ForeignKey(nameof(ItemId))]
//         public virtual InventTable? InventTable { get; set; }

        #endregion
    }
}

