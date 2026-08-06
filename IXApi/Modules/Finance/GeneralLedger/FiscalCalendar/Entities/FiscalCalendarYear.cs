using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("FiscalCalendarYear")]
    public class FiscalCalendarYear : Entity<long>
    {
        //----------------------------------------- Core Information
        // Basic Properties
        public long FiscalCalendar { get; set; }

        [Required]
        [StringLength(FieldLengths.Name)]
        public string Name { get; set; } = string.Empty;

        // ==========================================================
        // Timeline Boundaries
        // ==========================================================
        // Basic Properties
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }


        #region Navigation Properties Row
        //FiscalCalendarYear .FiscalCalendar       =FiscalCalendar    .RecID
        [ForeignKey(nameof(FiscalCalendar))]
        public virtual FiscalCalendar? FiscalCalendarTable { get; set; }

        #endregion
    }
}

