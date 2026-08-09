using System;
using System.Collections.Generic;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
    public class CustConfirmJourDto : EntityDto<long>
    {
      
        public List<CustConfirmTransDto> Lines { get; set; } = new List<CustConfirmTransDto>();
    }
}