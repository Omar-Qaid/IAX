using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Modules.Finance.Entities
{
    /// <summary>
    /// One segment of an account structure, in its position. The account structure itself is a
    /// DimensionHierarchy row; its segment order lives here.
    /// </summary>
    [Table("DimensionHierarchyLevel")]
    public class DimensionHierarchyLevel : Entity<long>
    {
        //----------------------------------------- Core Identity
        // Basic Properties
        public long DimensionAttribute { get; set; } // The financial dimension this segment constrains
        public long DimensionHierarchy { get; set; } // The account structure that owns the segment
        public int Level { get; set; }               // Position of the segment within the structure

        // ==========================================================
        // Draft Staging
        // ==========================================================
        // Basic Properties
        public long TemporaryDimensionHierarchyLevel { get; set; }
    }
}
