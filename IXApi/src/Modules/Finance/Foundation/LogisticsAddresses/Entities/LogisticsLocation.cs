using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("LogisticsLocation")]
    public class LogisticsLocation : Entity<long>
    {
        //----------------------------------------- Core Identity & Descriptive Data
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.LocationId)]
        public string LocationId { get; set; } = string.Empty; // System-generated unique identifier code

        [Required]
        [StringLength(FieldLengths.Description)]
        public string Description { get; set; } = string.Empty; // User-defined description label (e.g., "Warehouse Dock 2")

        // ==========================================================
        // Structural Hierarchy & Typing
        // ==========================================================
        // Basic Properties
        public long ParentLocation { get; set; } // Self-referencing link for building hierarchical facility spaces

        // Enum Properties
        public NoYes IsPostalAddress { get; set; } // Indicates whether this node breaks down into a physical mailing layout

        // ==========================================================
        // Corporate Operations & Regulatory Registry
        // ==========================================================
        // Basic Properties
        public long DunsNumberRecId { get; set; } // Dun & Bradstreet identifier record link for tracking corporate facilities


        #region Navigation Properties Row
        //LogisticsLocation.ParentLocation == LogisticsLocation.RecId
        //[ForeignKey(nameof(ParentLocation))]
        //public virtual LogisticsLocation? LogisticsLocationParentTable { get; set; }

        #endregion

    }
}

