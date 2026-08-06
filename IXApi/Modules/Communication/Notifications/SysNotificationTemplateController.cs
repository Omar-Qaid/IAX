using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Communication.Notifications
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
            IBaseService<SysNotificationTemplate> service,
            ILogger<SysNotificationTemplateController> logger)
            : base(service, logger)
        {
        }
    }
}
