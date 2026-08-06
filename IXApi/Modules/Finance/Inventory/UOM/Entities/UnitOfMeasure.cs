using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("UnitOfMeasure")]
    public class UnitOfMeasure : Entity<long>
    {
        //----------------------------------------- Core Identity & Symbol Notation
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.Symbol)]
        public string Symbol { get; set; } = string.Empty; // e.g., "kg", "pcs", "m"

        // ==========================================================
        // Classification & Context Frameworks
        // ==========================================================
        // Enum Properties
        public UnitOfMeasureClass UnitOfMeasureClass { get; set; } // e.g., Quantity, Length, Area, Liquid volume, Mass
        public SystemOfUnits SystemOfUnits { get; set; } // e.g., Metric system, US customary units, None

        // ==========================================================
        // Floating Point Display Mechanics
        // ==========================================================
        // Basic Properties
        public int DecimalPrecision { get; set; } // Number of decimal positions to support during math conversions
    }
}
