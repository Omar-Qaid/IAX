using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Entities
{
    [Table("LedgerFiscalCalendarYear")]
    public class LedgerFiscalCalendarYear : Entity<long>
    {
        //----------------------------------------- Core Identity & Structural Mapping Coordinates
        // Basic Properties
        public long Ledger { get; set; } // Foreign Key link pointing to the primary Ledger master record (Legal Entity scope reference)

        public long FiscalCalendarYear { get; set; } // Foreign Key link pointing to the shared structural FiscalCalendarYear template layout

        // ==========================================================
        // Period Close & Lifecycle Governance Controls
        // ==========================================================
        // Enum Properties
        public FiscalPeriodStatus Status { get; set; } // 0: Open (Unrestricted Posting), 1: OnHold (Temporarily Locked), 2: Closed (Permanently Locked)


        #region Navigation Properties Row

        //LedgerFiscalCalendarYear .Ledger       =Ledger  .RecID

        [ForeignKey(nameof(Ledger))]
        public virtual Ledger? LedgerTable { get; set; }

        //LedgerFiscalCalendarYear .FiscalCalendarYear       =FiscalCalendarYear  .RecID
        [ForeignKey(nameof(FiscalCalendarYear))]
        public virtual FiscalCalendarYear? FiscalCalendarYearTable { get; set; }

        #endregion
    }
}