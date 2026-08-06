
using IAX.IXApi.Modules.ERP.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Modules.ERP.Entities
{
    [Table("TaxGroupData")]
    public class TaxGroupData : Entity<long>
    {
        //----------------------------------------- Core Identity & Matrix Mapping
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.TaxGroup)]
        public string TaxGroup { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.TaxCode)]
        public string TaxCode { get; set; } = string.Empty;

        // ==========================================================
        // Tax Exemption Configurations
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.TaxExemptCode)]
        public string TaxExemptCode { get; set; } = string.Empty; // Reason code pointing to standard regulatory definitions

        // Enum Properties
        public NoYes ExemptTax { get; set; } // Flag explicitly overriding or zeroing out the calculated tax amount

        // ==========================================================
        // Advanced International & Multi-Jurisdictional Frameworks
        // ==========================================================
        // Enum Properties
        public NoYes UseTax { get; set; }          // Self-assessed tax handling indicator for out-of-state vendor purchases
        public NoYes IntracomVat { get; set; }     // Intra-community European VAT processing schema marker
        public NoYes ReverseCharge_W { get; set; } // Global/Regional reverse charge accounting mechanism rule activation


        #region Navigation Properties Row

        [ForeignKey(nameof(TaxExemptCode))]
        public virtual TaxExemptCodeTable? TaxExemptCodeTable { get; set; }

        [ForeignKey(nameof(TaxGroup))]
        public virtual TaxGroupHeading? TaxGroupHeadingTable { get; set; }

        [ForeignKey(nameof(TaxCode))]
        public virtual TaxTable? TaxTable { get; set; }

        #endregion
    }
}
