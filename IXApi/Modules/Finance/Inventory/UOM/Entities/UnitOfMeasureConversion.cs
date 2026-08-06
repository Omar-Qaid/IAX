using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Inventory.InventTable;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("UnitOfMeasureConversion")]
    public class UnitOfMeasureConversion : Entity<long>
    {
        //----------------------------------------- Core Identity & Structural Units Matrix
        // Basic Properties
        public long FromUnitOfMeasure { get; set; } // Foreign Key link pointing to the source UnitOfMeasure master record
        public long ToUnitOfMeasure { get; set; }   // Foreign Key link pointing to the destination UnitOfMeasure master record

        // ==========================================================
        // Scope & Product Isolation
        // ==========================================================
        // Basic Properties
        public long Product { get; set; } // Reference to InventTable/EcoResProduct. If 0, the rule is global across all items.

        // ==========================================================
        // Core Conversion Mathematical Parameters
        // ==========================================================
        // Basic Properties
        public decimal Factor { get; set; } // The base multiplier coefficient applied during unit transformation

        public int Numerator { get; set; }   // Multiplier fraction top component used for precise non-repeating decimal ratios
        public int Denominator { get; set; } // Multiplier fraction bottom component used for precise non-repeating decimal ratios

        // ==========================================================
        // Multi-Stage Scaling Offsets
        // ==========================================================
        // Basic Properties
        public decimal InnerOffset { get; set; } // Offset added directly to the base unit *before* the conversion factor is applied
        public decimal OuterOffset { get; set; } // Offset added to the calculated value *after* the conversion factor is applied

        // ==========================================================
        // Precision & Fractions Output Control
        // ==========================================================
        // Enum Properties
        public UnitOfMeasureRounding Rounding { get; set; } // Dictates truncation/rounding policies (Normal, Round Up, Round Down)


        #region Navigation Properties Row

//         [ForeignKey(nameof(FromUnitOfMeasure))]
//         public virtual UnitOfMeasure? SourceUnit { get; set; }

//         [ForeignKey(nameof(ToUnitOfMeasure))]
//         public virtual UnitOfMeasure? TargetUnit { get; set; }

//         [ForeignKey(nameof(Product))]
//         public virtual EcoResProduct? IsolatedProductScope { get; set; }

        #endregion
    }
}

