using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Services;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Communication.Notifications.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/[controller]")]
    public class SysNotificationPreferenceController : BaseController<SysNotificationPreference, SysNotificationPreferenceDto>
    {
        private readonly ISysNotificationPreferenceService _preferences;
        private readonly ICurrentUserService _currentUser;

        public SysNotificationPreferenceController(
            ISysNotificationPreferenceService service,
            ILogger<SysNotificationPreferenceController> logger,
            ICurrentUserService currentUser)
            : base(service, logger)
        {
            _preferences = service;
            _currentUser = currentUser;
        }

        [HttpGet]
        public async Task<ActionResult<APIResponse<IEnumerable<SysNotificationPreferenceDto>>>> GetMyPreferences(
            CancellationToken ct = default)
        {
            var items = await _preferences.GetMyPreferencesAsync(_currentUser.GetCurrentUserId(), ct);
            return Ok(APIResponse<IEnumerable<SysNotificationPreferenceDto>>.Ok(items));
        }

        [HttpPut]
        public async Task<ActionResult<APIResponse<bool>>> SavePreferences(
            [FromBody] IEnumerable<SysNotificationPreferenceDto> dtos, CancellationToken ct = default)
        {
            await _preferences.SavePreferencesAsync(_currentUser.GetCurrentUserId(), dtos, ct);
            return Ok(APIResponse<bool>.Ok(true, "Preferences saved"));
        }

        [NonAction]
        public override Task<ActionResult<APIResponse<IEnumerable<SysNotificationPreferenceDto>>>> GetAll(CancellationToken cancellationToken = default) =>
            base.GetAll(cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<SysNotificationPreferenceDto>>> Create(SysNotificationPreferenceDto dto, CancellationToken cancellationToken = default) =>
            base.Create(dto, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<SysNotificationPreferenceDto>>> Update(string id, SysNotificationPreferenceDto dto, CancellationToken cancellationToken = default) =>
            base.Update(id, dto, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default) =>
            base.Delete(id, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<IEnumerable<SysNotificationPreferenceDto>>>> GetPaged(QueryFilterDto paginationParams, CancellationToken cancellationToken = default) =>
            base.GetPaged(paginationParams, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<SysNotificationPreferenceDto>>> GetById(string id, CancellationToken cancellationToken = default) =>
            base.GetById(id, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<IEnumerable<SysNotificationPreferenceDto>>>> CreateRange(IEnumerable<SysNotificationPreferenceDto> dtos, CancellationToken cancellationToken = default) =>
            base.CreateRange(dtos, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<IEnumerable<SysNotificationPreferenceDto>>>> UpdateRange(IEnumerable<SysNotificationPreferenceDto> dtos, CancellationToken cancellationToken = default) =>
            base.UpdateRange(dtos, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<bool>>> DeleteRange(IEnumerable<string> ids, CancellationToken cancellationToken = default) =>
            base.DeleteRange(ids, cancellationToken);
    }
}
