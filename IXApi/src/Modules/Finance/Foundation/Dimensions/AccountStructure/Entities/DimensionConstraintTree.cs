using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Modules.Finance.Entities
{
    /// <summary>
    /// The activated constraint tree of one account structure. Activation builds this from the
    /// structure's draft; validation reads it. A structure with no tree has never been activated
    /// and therefore constrains nothing.
    /// </summary>
    [Table("DimensionConstraintTree")]
    public class DimensionConstraintTree : Entity<long>
    {
        //----------------------------------------- Core Identity
        // Basic Properties
        public long DimensionHierarchy { get; set; } // The account structure this tree belongs to

        // ==========================================================
        // Activation State
        // ==========================================================
        // Basic Properties
        public Guid LastModifiedId { get; set; }
        public int IsActivating { get; set; }
        public long ActivationBatchId { get; set; }
    }
}
