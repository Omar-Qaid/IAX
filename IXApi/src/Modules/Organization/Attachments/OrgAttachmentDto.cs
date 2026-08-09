using System;
using System.Collections.Generic;

namespace IAX.IXApi.Modules.Organization.Attachments
{
    public class OrgAttachmentDto
    {
        public long AttachmentId { get; set; }
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }
        public bool Activated { get; set; }

        public List<OrgAttachmentDetailDto> Details { get; set; } = new List<OrgAttachmentDetailDto>();
    }
}
