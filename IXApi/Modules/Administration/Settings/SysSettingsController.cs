using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Infrastructure.Identity;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Administration.Settings
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class SysSettingsController : ControllerBase
    {
        private readonly ISysSettingsService _SysSettingsService;
        private readonly ICurrentUserService _currentUserService;

        public SysSettingsController(ISysSettingsService SysSettingsService, ICurrentUserService currentUserService)
        {
            _SysSettingsService = SysSettingsService;
            _currentUserService = currentUserService;
        }

        /// <summary>Get global system settings.</summary>
        [HttpGet("global")]
        public async Task<ActionResult<APIResponse<SysSettingsDto>>> GetGlobalSettings(CancellationToken ct)
        {
            var settings = await _SysSettingsService.GetGlobalSettingsAsync(ct);
            var dto = settings.Adapt<SysSettingsDto>();
            return Ok(APIResponse<SysSettingsDto>.Ok(dto));
        }

        /// <summary>Update global system settings.</summary>
        [HttpPut("global")]
        public async Task<ActionResult<APIResponse<SysSettingsDto>>> UpdateGlobalSettings([FromBody] SysSettingsDto dto, CancellationToken ct)
        {
            var entity = dto.Adapt<SysSettings>();
            var updated = await _SysSettingsService.UpdateGlobalSettingsAsync(entity, ct);
            var resultDto = updated.Adapt<SysSettingsDto>();
            return Ok(APIResponse<SysSettingsDto>.Ok(resultDto, "System settings updated successfully"));
        }

        /// <summary>Get user-scoped settings.</summary>
        [HttpGet("user")]
        public async Task<ActionResult<APIResponse<SysUserSettingsDto>>> GetUserSettings(CancellationToken ct)
        {
            var userId = _currentUserService.GetCurrentUserId();
            var settings = await _SysSettingsService.GetUserSettingsAsync(userId, ct);
            var dto = settings.Adapt<SysUserSettingsDto>();
            return Ok(APIResponse<SysUserSettingsDto>.Ok(dto));
        }

        /// <summary>Update user-scoped settings.</summary>
        [HttpPut("user")]
        public async Task<ActionResult<APIResponse<SysUserSettingsDto>>> UpdateUserSettings([FromBody] SysUserSettingsDto dto, CancellationToken ct)
        {
            var userId = _currentUserService.GetCurrentUserId();
            var entity = dto.Adapt<SysUserSettings>();
            var updated = await _SysSettingsService.UpdateUserSettingsAsync(userId, entity, ct);
            var resultDto = updated.Adapt<SysUserSettingsDto>();
            return Ok(APIResponse<SysUserSettingsDto>.Ok(resultDto, "User settings updated successfully"));
        }
    }
}
