using IAX.IXApi.Shared.Application.Contracts;
using System;
using System.Collections.Generic;

namespace IAX.IXApi.Modules.Finance.GeneralLedger
{
    public class FiscalCalendarDto : EntityDto<long>
    {
        public string CalendarId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<FiscalCalendarYearDto> Years { get; set; } = new List<FiscalCalendarYearDto>();
    }
}