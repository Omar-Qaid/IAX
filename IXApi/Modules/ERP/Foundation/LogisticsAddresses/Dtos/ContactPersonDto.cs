using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.ERP.Common;
using System;

namespace IAX.IXApi.Modules.ERP.Shared.Features
{
    public class ContactPersonDto : EntityDto<long>
    {
        public string ContactPersonId { get; set; } = string.Empty;
        public long Party { get; set; }
        public long ContactForParty { get; set; }
        public string CustAccount { get; set; } = string.Empty;
        public NoYes Inactive { get; set; }
        public NoYes Vip { get; set; }
        public NoYes Imported { get; set; }
        public NoYes IsContactPersonExternallyMaintained { get; set; }
        public ContactSensitivity Sensitivity { get; set; }
        public long MainResponsibleWorker { get; set; }
        public int TimeAvailableFrom { get; set; }
        public int TimeAvailableTo { get; set; }
        public NoYes DirectMail { get; set; }
        public NoYes McrIsDefaultContact { get; set; }
        public NoYes VendorPortalAccessAllowed { get; set; }
        public NoYes WebRequestAccess { get; set; }
        public VendorContactRole VendRole { get; set; }
        public DateTime LastEditAxDateTime { get; set; }
        public int LastEditAxDateTimeTzId { get; set; }
    }
}
