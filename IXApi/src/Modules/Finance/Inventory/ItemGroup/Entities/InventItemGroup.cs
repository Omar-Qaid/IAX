using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("InventItemGroup")]
    public class InventItemGroup : Entity<long>
    {
        //----------------------------------------- Core Information
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.ItemGroupId)]
        public string ItemGroupId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.Name)]
        public string Name { get; set; } = string.Empty;

        // ==========================================================
        // Default Taxation Frameworks
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.TaxItemGroupIdSales)]
        public string TaxItemGroupIdSales { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.TaxItemGroupIdPurch)]
        public string TaxItemGroupIdPurch { get; set; } = string.Empty;

        // ==========================================================
        // Advanced Revenue Recognition Policies (RevRec)
        // ==========================================================
        // Basic Properties

        // Enum Properties
        public RevRecRevenueType RevRecRevenueType { get; set; }

        #region Navigation Properties Row

//         [ForeignKey(nameof(TaxItemGroupIdSales))]
//         public virtual TaxItemGroupHeading? TaxItemGroupSales { get; set; }

//         [ForeignKey(nameof(TaxItemGroupIdPurch))]
//         public virtual TaxItemGroupHeading? TaxItemGroupPurch { get; set; }

        #endregion
    }
}
