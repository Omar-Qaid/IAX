using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Administration.AuditLogs.Dtos;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Interfaces;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Administration.AuditLogs.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("System", "AuditLog")]
    public class SysExceptionLogController : BaseController<SysExceptionLog, SysExceptionLogDto>
    {
        public SysExceptionLogController(ISysExceptionLogService service, ILogger<SysExceptionLogController> logger)
            : base(service, logger)
        {
        }

        [NonAction]
        public override Task<ActionResult<APIResponse<SysExceptionLogDto>>> Create(
            SysExceptionLogDto dto,
            CancellationToken cancellationToken = default) =>
            base.Create(dto, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<SysExceptionLogDto>>> Update(
            string id,
            SysExceptionLogDto dto,
            CancellationToken cancellationToken = default) =>
            base.Update(id, dto, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<bool>>> Delete(
            string id,
            CancellationToken cancellationToken = default) =>
            base.Delete(id, cancellationToken);
    }
}
