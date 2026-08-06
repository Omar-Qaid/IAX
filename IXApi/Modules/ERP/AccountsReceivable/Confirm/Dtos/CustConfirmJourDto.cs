using System;
using System.Collections.Generic;
using IAX.IXApi.Modules.ERP.Common;
using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.ERP.AccountsReceivable
{
    public class CustConfirmJourDto : EntityDto<long>
    {
      
        public List<CustConfirmTransDto> Lines { get; set; } = new List<CustConfirmTransDto>();
    }

    public class CustConfirmTransDto : EntityDto<long>
    {
      
    }
}
