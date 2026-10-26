using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Interfaces;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Communication.Notifications.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/[controller]")]
    [DomainPermission("System", "Notifications")]
    public class SysNotificationRecipientController : BaseController<SysNotificationRecipient, SysNotificationRecipientDto>
    {
        public SysNotificationRecipientController(
            ISysNotificationRecipientService service,
            ILogger<SysNotificationRecipientController> logger)
            : base(service, logger)
        {
        }
    }
}
