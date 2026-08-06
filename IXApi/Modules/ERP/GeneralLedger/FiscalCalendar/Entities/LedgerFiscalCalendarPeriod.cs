using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Entities
{
    [Table("LedgerFiscalCalendarPeriod")]
    public class LedgerFiscalCalendarPeriod : Entity<long>
    {
        //----------------------------------------- Core Identity & Structural Mapping Coordinates
        // Basic Properties
        public long Ledger { get; set; } // Foreign Key link pointing to the primary Ledger master record (Legal Entity scope reference)

        public long FiscalCalendarPeriod { get; set; } // Foreign Key link pointing to the shared global FiscalCalendarPeriod partition definition

        // ==========================================================
        // Sub-Ledger Posting & Period Close Governance Controls
        // ==========================================================
        // Enum Properties
        public FiscalPeriodStatus Status { get; set; } // 0: Open (Unrestricted Posting), 1: OnHold (Temporarily Locked), 2: Closed (Permanently Locked)


        #region Navigation Properties Row
        //FiscalCalendarPeriod .Ledger       =Ledger  .RecID
        [ForeignKey(nameof(Ledger))]
        public virtual Ledger? LedgerTable { get; set; }


        //FiscalCalendarPeriod .FiscalCalendarPeriod       =FiscalCalendarPeriod  .RecID
        [ForeignKey(nameof(FiscalCalendarPeriod))]
        public virtual FiscalCalendarPeriod? FiscalCalendarPeriodTable { get; set; }

        #endregion
    }
}