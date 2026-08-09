using IAX.IXApi.Shared.Application.Contracts;
using System;
using System.Collections.Generic;

namespace IAX.IXApi.Modules.Finance.GeneralLedger
{
    public class FiscalCalendarPeriodDto : EntityDto<long>
    {
        public long FiscalCalendar { get; set; }
        public long FiscalCalendarYear { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public int Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Month { get; set; }
        public int Quarter { get; set; }
    }
}