using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("TaxPeriodHead")]
    public class TaxPeriodHead : Entity<long>
    {
        //----------------------------------------- Core Identity & Descriptive Data
        // Basic Properties
        [Required]
        [StringLength(10)]
        public string TaxPeriod { get; set; } = string.Empty; // Primary tax settlement period identifier key

        [Required]
        [StringLength(60)]
        public string Name { get; set; } = string.Empty; // Descriptive user label (e.g., "Monthly VAT Settlement")

        [Required]
        [StringLength(10)]
        public string TaxAuthority { get; set; } = string.Empty; // Code identifying the associated tax authority master

        public long TaxId { get; set; } // Reference link to regulatory registration details or tax identification records

        [Required]
        [StringLength(100)]
        public string PaymentCode { get; set; } = string.Empty; // Electronic payment reference or vendor payment identifier

        // ==========================================================
        // Interval & Recurrence Frequency Configuration
        // ==========================================================
        // Basic Properties
        public int QtyUnit { get; set; } // Numerical multiplier for interval calculation (e.g., 1, 3, 12)

        // Enum Properties
        public TaxPeriodUnit PeriodUnit { get; set; } // Frequency unit type (e.g., Days, Months, Years)

        // ==========================================================
        // Settlement Engine & Accounting Ledger Controls
        // ==========================================================
        // Enum Properties
        public NoYes NotGenerateOffsetTaxTrans { get; set; } // Prevents posting offset tax transactions during settlement execution
        public NoYes ReportAdjustment { get; set; }           // Directs whether adjustments are included in current period declarations
        public NoYes UseBatch { get; set; }                    // Controls background batch job execution for periodic settlements


        #region Navigation Properties Row

        [ForeignKey(nameof(TaxAuthority))]
        public virtual TaxAuthorityAddress? TaxAuthorityAddressTable { get; set; }

        #endregion
    }
}
