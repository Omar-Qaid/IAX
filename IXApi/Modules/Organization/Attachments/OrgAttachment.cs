using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using System;
using System.Collections.Generic;

namespace IAX.IXApi.Modules.Organization.Attachments
{
    public class OrgAttachment: Entity<long>
    {
        public virtual ICollection<OrgAttachmentDetail> Details { get; set; } = new List<OrgAttachmentDetail>();
    }
}

