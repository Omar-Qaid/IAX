using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("UnitOfMeasureBaseUnit")]
    public class UnitOfMeasureBaseUnit : Entity<long>
    {
        //----------------------------------------- Core Identity & Structural Mapping
        // Basic Properties
        public long UnitOfMeasure { get; set; } // Foreign Key RecId pointing directly to UnitOfMeasure

        // Enum Properties
        public UnitOfMeasureClass UnitOfMeasureClass { get; set; } // The measurement domain class (Length, Mass, etc.)


        #region Navigation Properties Row

//         [ForeignKey(nameof(UnitOfMeasure))]
//         public virtual UnitOfMeasure? BaselineUnit { get; set; }

        #endregion
    }
}

