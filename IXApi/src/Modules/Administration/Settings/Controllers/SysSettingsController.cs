using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Modules.Administration.Settings.Dtos;
using IAX.IXApi.Modules.Administration.Settings.Entities;
using IAX.IXApi.Modules.Administration.Settings.Interfaces;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Shared.Application.NumberSequences;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Administration.Settings.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class SysSettingsController : BaseController<SysSettings, SysSettingsDto>
    {
        private readonly ISysSettingsService _sysSettingsService;
        private readonly ICurrentUserService _currentUserService;

        public SysSettingsController(
            ISysSettingsService service,
            ILogger<SysSettingsController> logger,
            ICurrentUserService currentUserService)
            : base(service, logger)
        {
            _sysSettingsService = service;
            _currentUserService = currentUserService;
        }

        /// <summary>Get global system settings.</summary>
        [HttpGet("global")]
        [DomainPermission("System", "Settings", "View")]
        public async Task<ActionResult<APIResponse<SysSettingsDto>>> GetGlobalSettings(CancellationToken ct)
        {
            var settings = await _sysSettingsService.GetGlobalSettingsAsync(ct);
            var dto = settings.Adapt<SysSettingsDto>();
            return Ok(APIResponse<SysSettingsDto>.Ok(dto));
        }

        /// <summary>Update global system settings.</summary>
        [HttpPut("global")]
        [DomainPermission("System", "Settings", "Edit")]
        public async Task<ActionResult<APIResponse<SysSettingsDto>>> UpdateGlobalSettings([FromBody] SysSettingsDto dto, CancellationToken ct)
        {
            var entity = dto.Adapt<SysSettings>();
            var updated = await _sysSettingsService.UpdateGlobalSettingsAsync(entity, ct);
            var resultDto = updated.Adapt<SysSettingsDto>();
            return Ok(APIResponse<SysSettingsDto>.Ok(resultDto, "System settings updated successfully"));
        }

        /// <summary>Get user-scoped settings.</summary>
        [HttpGet("user")]
        public async Task<ActionResult<APIResponse<SysUserSettingsDto>>> GetUserSettings(CancellationToken ct)
        {
            var userId = _currentUserService.GetCurrentUserId();
            var settings = await _sysSettingsService.GetUserSettingsAsync(userId, ct);
            var dto = settings.Adapt<SysUserSettingsDto>();
            return Ok(APIResponse<SysUserSettingsDto>.Ok(dto));
        }

        /// <summary>Update user-scoped settings.</summary>
        [HttpPut("user")]
        public async Task<ActionResult<APIResponse<SysUserSettingsDto>>> UpdateUserSettings([FromBody] SysUserSettingsDto dto, CancellationToken ct)
        {
            var userId = _currentUserService.GetCurrentUserId();
            var entity = dto.Adapt<SysUserSettings>();
            var updated = await _sysSettingsService.UpdateUserSettingsAsync(userId, entity, ct);
            var resultDto = updated.Adapt<SysUserSettingsDto>();
            return Ok(APIResponse<SysUserSettingsDto>.Ok(resultDto, "User settings updated successfully"));
        }

        [NonAction]
        public override Task<ActionResult<APIResponse<IEnumerable<SysSettingsDto>>>> GetAll(CancellationToken cancellationToken = default) =>
            base.GetAll(cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<IEnumerable<SysSettingsDto>>>> GetPaged(QueryFilterDto paginationParams, CancellationToken cancellationToken = default) =>
            base.GetPaged(paginationParams, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<SysSettingsDto>>> GetById(string id, CancellationToken cancellationToken = default) =>
            base.GetById(id, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<SysSettingsDto>>> Create(SysSettingsDto dto, CancellationToken cancellationToken = default) =>
            base.Create(dto, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<SysSettingsDto>>> Update(string id, SysSettingsDto dto, CancellationToken cancellationToken = default) =>
            base.Update(id, dto, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default) =>
            base.Delete(id, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<IEnumerable<SysSettingsDto>>>> CreateRange(IEnumerable<SysSettingsDto> dtos, CancellationToken cancellationToken = default) =>
            base.CreateRange(dtos, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<IEnumerable<SysSettingsDto>>>> UpdateRange(IEnumerable<SysSettingsDto> dtos, CancellationToken cancellationToken = default) =>
            base.UpdateRange(dtos, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<bool>>> DeleteRange(IEnumerable<string> ids, CancellationToken cancellationToken = default) =>
            base.DeleteRange(ids, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<NumberSequenceMetadataDto>>> GetNumberSequence(CancellationToken cancellationToken = default) =>
            base.GetNumberSequence(cancellationToken);
    }
}
