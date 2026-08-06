using System;
using IAX.IXApi.Modules.ERP.Common;
using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.ERP.AccountsReceivable
{
    public class CustLedgerDto : EntityDto<long>
    {
        public string DataAreaId { get; set; } = "dat";
        public string PostingProfile { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string PostingProfileName { get; set; } = string.Empty;
        public NoYes CollectionLetter { get; set; }
        public NoYes Interest { get; set; }
        public NoYes Settlement { get; set; }
    }
}
