using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Administration.Settings.Dtos;
using IAX.IXApi.Modules.Administration.Settings.Entities;
using IAX.IXApi.Modules.Administration.Settings.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Administration.Settings.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class SysUserSettingsController : BaseController<SysUserSettings, SysUserSettingsDto>
    {
        public SysUserSettingsController(
            ISysUserSettingsService service,
            ILogger<SysUserSettingsController> logger)
            : base(service, logger)
        {
        }
    }
}
