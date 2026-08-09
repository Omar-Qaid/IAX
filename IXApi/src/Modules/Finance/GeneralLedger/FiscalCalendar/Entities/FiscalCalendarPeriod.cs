using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.GeneralLedger;
using IAX.IXApi.Modules.Finance.Shared.Features;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("FiscalCalendarPeriod")]
    public class FiscalCalendarPeriod : Entity<long>
    {
        //----------------------------------------- Core Information
        // Basic Properties
        public long FiscalCalendar { get; set; }
        public long FiscalCalendarYear { get; set; }

        [Required]
        [StringLength(FieldLengths.Name)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.ShortName)]
        public string ShortName { get; set; } = string.Empty;

        // Enum Properties
        public FiscalPeriodType Type { get; set; } // e.g., Opening, Operating, Closing

        // ==========================================================
        // Timeline & Date Buckets
        // ==========================================================
        // Basic Properties
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Month { get; set; }
        public int Quarter { get; set; }


        #region Navigation Properties Row

        //FiscalCalendarPeriod .FiscalCalendar       =FiscalCalendar  .RecID
        [ForeignKey(nameof(FiscalCalendar))]
        public virtual FiscalCalendar? FiscalCalendarTable { get; set; }



        //FiscalCalendarPeriod .FiscalCalendarYear       =FiscalCalendarYear  .RecID
        [ForeignKey(nameof(FiscalCalendarYear))]
        public virtual FiscalCalendarYear? FiscalCalendarYearTable { get; set; }

        #endregion
    }
}

