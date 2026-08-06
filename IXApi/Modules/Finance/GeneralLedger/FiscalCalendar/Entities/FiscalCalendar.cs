using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("FiscalCalendar")]
    public class FiscalCalendar : Entity<long>
    {
        //----------------------------------------- Core Information
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.CalendarId)]
        public string CalendarId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.Description)]
        public string Description { get; set; } = string.Empty;
    }
}
