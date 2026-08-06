using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Entities
{
    [Table("DimensionHierarchy")]
    public class DimensionHierarchy : Entity<long>
    {
        //----------------------------------------- Core Identity & Descriptive Layouts
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.Name)]
        public string Name { get; set; } = string.Empty; // The active structural name utilized during posting processes

        [Required]
        [StringLength(FieldLengths.Description)]
        public string Description { get; set; } = string.Empty;

        // Enum Properties
        public DimensionHierarchyStructureType StructureType { get; set; } // 0: AccountStructure, 1: AdvancedRuleStructure

        // ==========================================================
        // Staging Configuration Engine (Draft Mode Management)
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.DraftName)]
        public string DraftName { get; set; } = string.Empty; // Holds layout adjustments prior to formal schema activation

        [Required]
        [StringLength(FieldLengths.DraftDescription)]
        public string DraftDescription { get; set; } = string.Empty;

        // Enum Properties
        public NoYes IsDraft { get; set; } // Flag identifying if changes are pending structural review

        // ==========================================================
        // Optimization & Governance Attributes
        // ==========================================================
        // Basic Properties
        public Guid HashKey { get; set; } // Binary uniqueness token validation trace checking for rule structural alterations

        public long DeletedVersion { get; set; } // Historical tracking index marker handling soft-delete archiving cleanup logs

        // Enum Properties
        public NoYes IsSystemGenerated { get; set; } // Hard restriction flag blocking human modification of foundational framework lines
        public DimensionHierarchyFocusState FocusState { get; set; } // UI or calculation engine optimization tracking focus indicators
    }
}