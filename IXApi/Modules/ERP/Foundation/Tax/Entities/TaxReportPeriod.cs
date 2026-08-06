using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Entities
{
    [Table("TaxReportPeriod")]
    public class TaxReportPeriod : Entity<long>
    {
        //----------------------------------------- Core Identity & Parent Tax Period Mapping
        // Basic Properties
        [Required]
        [StringLength(10)]
        public string TaxPeriod { get; set; } = string.Empty; // Foreign key code pointing to parent TaxPeriodHead

        // ==========================================================
        // Effective Date Boundaries & Version Control
        // ==========================================================
        // Basic Properties
        public DateTime FromDate { get; set; } // Interval starting date threshold

        public DateTime ToDate { get; set; }   // Interval ending date threshold

        public int VersionNum { get; set; }    // Iteration sequence number tracking recalculations/settlement runs

        // ==========================================================
        // Reporting Ledger Pagination Markers
        // ==========================================================
        // Basic Properties
        public int LastPageNumSales { get; set; } // Sequence bookmark for sales tax reporting layout pages

        public int LastPageNumPurch { get; set; } // Sequence bookmark for purchase tax reporting layout pages

        // ==========================================================
        // Settlement Status & Lifecycle Controls
        // ==========================================================
        // Enum Properties
        public NoYes Closed { get; set; }     // Hard locking flag preventing further tax postings into this date range

        public NoYes LastPeriod { get; set; } // Flags the final period in the fiscal/tax year framework


        #region Navigation Properties Row

        [ForeignKey(nameof(TaxPeriod))]
        public virtual TaxPeriodHead? TaxPeriodHeadTable { get; set; }

        #endregion
    }
}