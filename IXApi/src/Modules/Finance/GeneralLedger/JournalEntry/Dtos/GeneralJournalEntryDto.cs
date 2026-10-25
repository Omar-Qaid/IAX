using IAX.IXApi.Shared.Application.Contracts;
using System;

namespace IAX.IXApi.Modules.Finance.GeneralLedger
{
    public class GeneralJournalEntryDto : EntityDto<long>
    {
        public string SubledgerVoucher { get; set; } = string.Empty;
        public DateTime AccountingDate { get; set; }
        public DateTime DocumentDate { get; set; }
        public string DocumentNumber { get; set; } = string.Empty;
    }
}
