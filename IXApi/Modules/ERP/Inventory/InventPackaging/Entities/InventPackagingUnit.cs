using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Modules.ERP.Common;


namespace IAX.IXApi.Modules.ERP.Entities
{
    [Table("InventPackagingUnit")]
    public class InventPackagingUnit : Entity<long>
    {
        //----------------------------------------- Core Information & Identity
        // Enum Properties
        public TableGroupAll ItemCode { get; set; } // Table (Specific Item), Group (Packaging Group), or All items

        [Required]
        [StringLength(FieldLengths.InventDimId)]
        public string InventDimId { get; set; } = string.Empty;

        // ==========================================================
        // Conversion Metrics & Unit Definitions
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.Unit)]
        public string Unit { get; set; } = string.Empty;

        public decimal Factor { get; set; } // Packing capacity conversion factor multiplier


        #region Navigation Properties Row

//         [ForeignKey(nameof(InventDimId))]
//         public virtual InventDim? InventDim { get; set; }

//         [ForeignKey(nameof(Unit))]
//         public virtual UnitOfMeasure? UnitOfMeasure { get; set; }

        #endregion
    }
}
