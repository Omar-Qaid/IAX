using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Modules.Communication.Notifications.Services;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Communication.Notifications;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class SysNotificationPreferenceController(
    ISysNotificationPreferenceService preferences,
    ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<APIResponse<IEnumerable<SysNotificationPreferenceDto>>>> GetMyPreferences(
        CancellationToken ct = default)
    {
        var items = await preferences.GetMyPreferencesAsync(currentUser.GetCurrentUserId(), ct);
        return Ok(APIResponse<IEnumerable<SysNotificationPreferenceDto>>.Ok(items));
    }

    [HttpPut]
    public async Task<ActionResult<APIResponse<bool>>> SavePreferences(
        [FromBody] IEnumerable<SysNotificationPreferenceDto> dtos, CancellationToken ct = default)
    {
        await preferences.SavePreferencesAsync(currentUser.GetCurrentUserId(), dtos, ct);
        return Ok(APIResponse<bool>.Ok(true, "Preferences saved"));
    }
}
