using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Organization.Announcements
{
    [ApiController]
    [Route("api/v1/[controller]")]
[DomainPermission("Organization", "Announcements")]
    public class OrgAnnouncementController : BaseController<OrgAnnouncement, OrgAnnouncementDto>
    {
        public OrgAnnouncementController(IOrgAnnouncementService service, ILogger<OrgAnnouncementController> logger) : base(service, logger)
        {
        }
    }
}
