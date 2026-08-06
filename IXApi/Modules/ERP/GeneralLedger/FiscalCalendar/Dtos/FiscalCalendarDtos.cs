using IAX.IXApi.Shared.Application.Contracts;
using System;
using System.Collections.Generic;

namespace IAX.IXApi.Modules.ERP.GeneralLedger
{
    public class FiscalCalendarDto : EntityDto<long>
    {
        public string CalendarId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<FiscalCalendarYearDto> Years { get; set; } = new List<FiscalCalendarYearDto>();
    }

    public class FiscalCalendarYearDto : EntityDto<long>
    {
        public long FiscalCalendar { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<FiscalCalendarPeriodDto> Periods { get; set; } = new List<FiscalCalendarPeriodDto>();
    }

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

    public class LedgerFiscalCalendarPeriodDto : EntityDto<long>
    {
        public long Ledger { get; set; }
        public long FiscalCalendarPeriod { get; set; }
        public int Status { get; set; } // FiscalPeriodStatus enum

        // UI helper properties to match LedgerCalendarsPage.tsx
        public string LegalEntity { get; set; } = string.Empty;
        public string PeriodStatus { get; set; } = string.Empty;
        public string LedgerModule { get; set; } = "Open";
        public string SalesTax { get; set; } = "Open";
        public string Bank { get; set; } = "Open";
        public string Customer { get; set; } = "Open";
        public string Vendor { get; set; } = "Open";
    }
}
