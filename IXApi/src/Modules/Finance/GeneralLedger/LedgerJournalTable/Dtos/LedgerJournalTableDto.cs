using IAX.IXApi.Shared.Application.Contracts;
using System.Collections.Generic;

namespace IAX.IXApi.Modules.Finance.GeneralLedger
{
    public class LedgerJournalTableDto : EntityDto<long>
    {
      
        public List<LedgerJournalTransDto> Lines { get; set; } = new List<LedgerJournalTransDto>();
    }
}

