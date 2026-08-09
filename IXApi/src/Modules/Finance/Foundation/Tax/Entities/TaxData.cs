using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("TaxData")]
    public class TaxData : Entity<long>
    {
        //----------------------------------------- Core Identity & Code Assignment
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.TaxCode)]
        public string TaxCode { get; set; } = string.Empty;

        // ==========================================================
        // Chronological Validity Windows
        // ==========================================================
        // Basic Properties
        public DateTime TaxFromDate { get; set; }
        public DateTime TaxToDate { get; set; }

        // ==========================================================
        // Financial Tax Rates, Exemptions & Limits
        // ==========================================================
        // Basic Properties
        public decimal TaxValue { get; set; } // The active tax percentage or flat rate multiplier
        public decimal VatExemptPct { get; set; } // Percentage portion of the tax value that is exempt
        public decimal TaxLimitMin { get; set; }  // Minimum threshold boundary for this specific rate scale
        public decimal TaxLimitMax { get; set; }  // Maximum threshold boundary for this specific rate scale

        // ==========================================================
        // Regulatory Substitution & Markup Parameters
        // ==========================================================
        // Basic Properties
        public decimal TaxSubstitutionMarkupValue { get; set; } // Regulatory equalization tax markup offset value


        #region Navigation Properties Row

        [ForeignKey(nameof(TaxCode))]
        public virtual TaxTable? TaxTable { get; set; }

        #endregion
    }
}

