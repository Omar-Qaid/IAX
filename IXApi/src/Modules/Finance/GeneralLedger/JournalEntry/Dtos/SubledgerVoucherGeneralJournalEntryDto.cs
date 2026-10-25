using IAX.IXApi.Shared.Application.Contracts;
using System;

namespace IAX.IXApi.Modules.Finance.GeneralLedger
{
    public class SubledgerVoucherGeneralJournalEntryDto : EntityDto<long>
    {
        public long GeneralJournalEntry { get; set; }
        public string Voucher { get; set; } = string.Empty;
        public DateTime AccountingDate { get; set; }
    }
}
