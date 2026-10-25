using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.GeneralLedger
{
    public class LedgerJournalNameDto : EntityDto<long>
    {
        public string JournalName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
