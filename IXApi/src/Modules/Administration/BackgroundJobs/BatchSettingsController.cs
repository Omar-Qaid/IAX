using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using IAX.IXApi.Modules.Administration.Persistence;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs;

[ApiController, Authorize, Route("api/v1/BatchSettings")]
public sealed class BatchSettingsController(IAdministrationDataContext db) : ControllerBase
{
    [HttpGet, DomainPermission("System", "BackgroundJobs", "View")]
    public async Task<IActionResult> Get(CancellationToken ct) =>
        Ok(APIResponse<BatchSettings>.Ok(await db.BatchSettings.AsNoTracking().SingleOrDefaultAsync(s => s.Id == 1, ct) ?? new()));

    [HttpPut, DomainPermission("System", "BackgroundJobs", "Edit")]
    public async Task<IActionResult> Save(BatchSettings settings, CancellationToken ct)
    {
        if (settings.Id != 1) return BadRequest(APIResponse<bool>.Fail("Batch settings use singleton ID 1."));
        if (settings.PollIntervalSeconds is < 1 or > 3600)
            return BadRequest(APIResponse<bool>.Fail("Polling interval must be between 1 and 3600 seconds."));
        var stored = await db.BatchSettings.SingleOrDefaultAsync(s => s.Id == 1, ct);
        if (stored == null) db.BatchSettings.Add(settings);
        else { stored.Enabled = settings.Enabled; stored.PollIntervalSeconds = settings.PollIntervalSeconds; }
        await db.SaveChangesAsync(ct);
        return Ok(APIResponse<BatchSettings>.Ok(stored ?? settings));
    }
}
