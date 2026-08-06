using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Entities
{
    [Table("UnitOfMeasureInternalCode")]
    public class UnitOfMeasureInternalCode : Entity<long>
    {
        //----------------------------------------- Core Identity & Mapping Pointers
        // Basic Properties
        public long UnitOfMeasure { get; set; } // Foreign Key RecId pointing directly to UnitOfMeasure

        // Enum/Code Properties
        public InternalCodeSymbol CodeSymbol { get; set; } // Internal system integer symbol enumeration lookup


        #region Navigation Properties Row

//         [ForeignKey(nameof(UnitOfMeasure))]
//         public virtual UnitOfMeasure? AssociatedUnitOfMeasure { get; set; }

        #endregion
    }
}
