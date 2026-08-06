using IAX.IXApi.Shared.Application.Contracts;
using System;
using System.Collections.Generic;

namespace IAX.IXApi.Modules.Finance.Inventory           {
    public class InventJournalTableDto : EntityDto<long>
    {
    

        public List<InventJournalTransDto> Lines { get; set; } = new List<InventJournalTransDto>();
    }

    public class InventJournalTransDto : EntityDto<long>
    {
       
    }
}

