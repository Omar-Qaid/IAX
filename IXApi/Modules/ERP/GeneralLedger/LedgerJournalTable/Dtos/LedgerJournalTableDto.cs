using IAX.IXApi.Modules.ERP.Common;
using IAX.IXApi.Shared.Application.Contracts;
using System;
using System.Collections.Generic;

namespace IAX.IXApi.Modules.ERP.GeneralLedger
{
    public class LedgerJournalTableDto : EntityDto<long>
    {
      
        public List<LedgerJournalTransDto> Lines { get; set; } = new List<LedgerJournalTransDto>();
    }
}
