using IAX.IXApi.Shared.Application.Contracts;
using System;
using System.Collections.Generic;

namespace IAX.IXApi.Modules.Finance.GeneralLedger
{
    public class FiscalCalendarYearDto : EntityDto<long>
    {
        public long FiscalCalendar { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<FiscalCalendarPeriodDto> Periods { get; set; } = new List<FiscalCalendarPeriodDto>();
    }
}