using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Interfaces;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Communication.Notifications.Controllers
{
    /// <summary>
    /// Standard CRUD controller for notification templates.
    /// Inherits from BaseController for full GetAll/GetPaged/GetById/Create/Update/Delete.
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("System", "NotificationTemplate")]
    public class SysNotificationTemplateController : BaseController<SysNotificationTemplate, SysNotificationTemplateDto>
    {
        public SysNotificationTemplateController(
            ISysNotificationTemplateService service,
            ILogger<SysNotificationTemplateController> logger)
            : base(service, logger)
        {
        }
    }
}
