using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    /// <summary>
    /// One allowed-value criterion of a constraint node. A node may carry several; a value is
    /// allowed when it satisfies any of them.
    ///
    /// A criterion is either a range (RangeFrom..RangeTo, with IsFromOpen / IsToOpen marking an
    /// exclusive bound) or a wildcard expression (WildCardString). Values are compared as strings,
    /// never as numbers, so 1000000 falls inside the range 100000..399999.
    /// </summary>
    [Table("DimensionConstraintNodeCriteria")]
    public class DimensionConstraintNodeCriteria : Entity<long>
    {
        //----------------------------------------- Core Identity
        // Basic Properties
        public long DimensionConstraintNode { get; set; }
        public int Ordinal { get; set; }

        // ==========================================================
        // Range Criterion
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.DisplayValue)]
        public string RangeFrom { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.DisplayValue)]
        public string RangeTo { get; set; } = string.Empty;

        // Enum Properties
        public NoYes IsFromOpen { get; set; } // The from bound is exclusive
        public NoYes IsToOpen { get; set; }   // The to bound is exclusive

        // ==========================================================
        // Wildcard Criterion
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.DisplayValue)]
        public string WildCardString { get; set; } = string.Empty;

        // ==========================================================
        // Draft Staging
        // ==========================================================
        // Basic Properties
        public int TemporaryOrdinal { get; set; }
    }
}
