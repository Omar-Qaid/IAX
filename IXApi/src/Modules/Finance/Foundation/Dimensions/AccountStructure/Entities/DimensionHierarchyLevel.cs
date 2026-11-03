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
        private long? _dimensionAttribute;
        // The main-account segment has no financial-dimension principal. Preserve incoming zero as "none".
        public long? DimensionAttribute
        {
            get => _dimensionAttribute;
            set => _dimensionAttribute = value == 0 ? null : value;
        }
        public long DimensionHierarchy { get; set; } // The account structure that owns the segment
        public int Level { get; set; }               // Position of the segment within the structure

        // ==========================================================
        // Draft Staging
        // ==========================================================
        // Basic Properties
        public long TemporaryDimensionHierarchyLevel { get; set; }
    }
}
