using IAX.IXApi.Shared.Domain.Entities;
using System;
using System.Collections.Generic;

namespace IAX.IXApi.Modules.Organization.Attachments
{
    public class OrgAttachment: Entity<long>
    {
        public virtual ICollection<OrgAttachmentDetail> Details { get; set; } = new List<OrgAttachmentDetail>();
    }
}

