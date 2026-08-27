using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Organization.Announcements
{
    [ApiController]
    [Route("api/v1/[controller]")]
[DomainPermission("Organization", "Announcements")]
    public class AnnouncementController : BaseController<Announcement, AnnouncementDto>
    {
        public AnnouncementController(IAnnouncementService service, ILogger<AnnouncementController> logger) : base(service, logger)
        {
        }
    }
}
