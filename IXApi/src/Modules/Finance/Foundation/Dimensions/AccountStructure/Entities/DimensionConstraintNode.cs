using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    /// <summary>
    /// One node of an account structure's constraint tree: the allowed values of a single segment,
    /// reached by a particular path through the segments before it.
    ///
    /// The tree is what makes a later segment's allowed values depend on the earlier ones — the
    /// account structure page shows it flattened into a grid, where one row is one root-to-leaf
    /// path. A node with no parent is a root, at the structure's first segment.
    /// </summary>
    [Table("DimensionConstraintNode")]
    public class DimensionConstraintNode : Entity<long>
    {
        //----------------------------------------- Core Identity & Position
        // Basic Properties
        public long DimensionConstraintTree { get; set; }  // The activated tree this node belongs to
        public long DimensionHierarchyLevel { get; set; }  // The segment this node constrains
        public long ParentConstraintNode { get; set; }     // The node one segment to the left, 0 at the root
        public int Ordinal { get; set; }                   // Order among siblings

        [Required]
        [StringLength(FieldLengths.Name)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.Description)]
        public string Description { get; set; } = string.Empty;

        // ==========================================================
        // Segment Policy
        // ==========================================================
        // Enum Properties
        public NoYes IsOptional { get; set; }   // Whether this segment may be left blank
        public NoYes IsOverlapping { get; set; } // Set by activation when the node's criteria overlap a sibling's

        // ==========================================================
        // Validity Window
        // ==========================================================
        // Basic Properties
        public DateTime ActiveFrom { get; set; }
        public DateTime ActiveTo { get; set; }

        // ==========================================================
        // Draft Staging & Activation Diagnostics
        // ==========================================================
        // Basic Properties
        public long TemporaryConstraintNode { get; set; }

        // Enum Properties
        public NoYes HasErrorInChildren { get; set; }
    }
}
