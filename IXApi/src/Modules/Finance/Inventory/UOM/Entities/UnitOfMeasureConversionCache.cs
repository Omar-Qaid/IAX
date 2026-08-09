using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Inventory.InventTable;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("UnitOfMeasureConversionCache")]
    public class UnitOfMeasureConversionCache : Entity<long>
    {
        //----------------------------------------- Core Identity & Link Relations
        // Basic Properties
        public long FromUnitOfMeasure { get; set; } // Foreign Key source pointer to UnitOfMeasure
        public long ToUnitOfMeasure { get; set; }   // Foreign Key destination pointer to UnitOfMeasure
        public long Product { get; set; }           // Product reference scope context for the active cache loop

        // Enum Properties
        public NoYes ConversionExists { get; set; } // Fast indicator flag avoiding continuous empty-state lookups

        // ==========================================================
        // Cached Flattened Math Components
        // ==========================================================
        // Basic Properties
        public decimal Factor { get; set; }
        public decimal FactorDenominator { get; set; } // Optimized pre-calculated inverse ratio or division factor
        public int Numerator { get; set; }
        public int Denominator { get; set; }
        public decimal InnerOffset { get; set; }
        public decimal OuterOffset { get; set; }

        // ==========================================================
        // Precision & Evaluation Frameworks
        // ==========================================================
        // Enum Properties
        public UnitOfMeasureConversionRounding Rounding { get; set; }


        #region Navigation Properties Row

//         [ForeignKey(nameof(FromUnitOfMeasure))]
//         public virtual UnitOfMeasure? CachedSourceUnit { get; set; }

//         [ForeignKey(nameof(ToUnitOfMeasure))]
//         public virtual UnitOfMeasure? CachedTargetUnit { get; set; }

//         [ForeignKey(nameof(Product))]
//         public virtual EcoResProduct? IsolatedProductScope { get; set; }

        #endregion
    }
}

