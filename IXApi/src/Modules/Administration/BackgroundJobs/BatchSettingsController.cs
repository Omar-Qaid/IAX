using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Services;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs;

[ApiController, Authorize, Route("api/v1/BatchSettings")]
public sealed class BatchSettingsController(IBatchSettingsService settingsService) : ControllerBase
{
    [HttpGet, DomainPermission("System", "BackgroundJobs", "View")]
    public async Task<IActionResult> Get(CancellationToken ct) =>
        Ok(APIResponse<BatchSettings>.Ok(await settingsService.GetAsync(ct)));

    [HttpPut, DomainPermission("System", "BackgroundJobs", "Edit")]
    public async Task<IActionResult> Save(BatchSettings settings, CancellationToken ct)
    {
        var result = await settingsService.SaveAsync(settings, ct);
        return result.Error != null
            ? BadRequest(APIResponse<bool>.Fail(result.Error))
            : Ok(APIResponse<BatchSettings>.Ok(result.Settings!));
    }
}
