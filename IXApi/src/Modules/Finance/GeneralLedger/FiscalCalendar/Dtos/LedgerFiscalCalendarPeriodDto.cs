using IAX.IXApi.Shared.Application.Contracts;
using System;
using System.Collections.Generic;

namespace IAX.IXApi.Modules.Finance.GeneralLedger
{
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