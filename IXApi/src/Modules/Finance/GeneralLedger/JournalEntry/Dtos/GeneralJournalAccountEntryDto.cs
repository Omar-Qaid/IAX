using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.GeneralLedger
{
    public class GeneralJournalAccountEntryDto : EntityDto<long>
    {
        public long GeneralJournalEntry { get; set; }
        public decimal AccountingCurrencyAmount { get; set; }
        public decimal TransactionCurrencyAmount { get; set; }
        public string TransactionCurrencyCode { get; set; } = string.Empty;
    }
}
