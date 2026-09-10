using System.Text.Json;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Workflow.Persistence;
using IAX.IXApi.Modules.Workflow.Requests;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Workflow.Scheduling;

[ApiController, Authorize, Route("api/v1/WFProcessScheduled")]
[DomainPermission("Workflow", "Processes")]
public sealed class WFProcessScheduledController(IWorkflowDataContext db, ICurrentUserService currentUser,
    IWfRequestService requests, UserManager<AspNetUser> users, IAppPermissionService permissions) : ControllerBase
{
    [HttpGet("{processId:long}")]
    public async Task<ActionResult<APIResponse<ProcessScheduleDto>>> Get(long processId, CancellationToken ct)
    {
        var company = currentUser.GetDataAreaId();
        var row = await db.Set<WFProcessScheduled>().AsNoTracking()
            .SingleOrDefaultAsync(x => x.ProcessId == processId && x.DataAreaId == company, ct);
        return Ok(APIResponse<ProcessScheduleDto>.Ok(row is null ? null : Read(row)));
    }

    [HttpPut("{processId:long}")]
    public async Task<ActionResult<APIResponse<ProcessScheduleDto>>> Save(long processId, ProcessScheduleDto dto, CancellationToken ct)
    {
        var company = currentUser.GetDataAreaId();
        if (!await db.WfProcesses.AnyAsync(x => x.RecId == processId && x.DataAreaId == company && x.IsActive && !x.IsDeleted, ct))
            return NotFound(APIResponse<ProcessScheduleDto>.Fail("Save an active process in the current company first."));
        var row = await db.Set<WFProcessScheduled>().Include(x => x.BackgroundJob)
            .SingleOrDefaultAsync(x => x.ProcessId == processId && x.DataAreaId == company, ct);
        if (row is not null && dto.Version != Convert.ToBase64String(row.RowVersion))
            return Conflict(APIResponse<ProcessScheduleDto>.Fail("The schedule changed. Reload before saving."));
        DateTime? next = null;
        try
        {
            if (dto.Enabled)
            {
                await ProcessScheduleAccount.ValidateAsync(users, permissions, currentUser.GetCurrentUserId(), company, ct);
                if (dto.StartsAt == default) throw new ArgumentException("A start date and time are required.");
                next = WorkflowRecurrence.NextUtc(dto.StartsAt, dto.TimeZone, dto.Frequency, DateTime.UtcNow);
                var submission = await ProcessScheduleSource.ResolveAsync(db, processId, company, dto, ct);
                var errors = await requests.ValidateSubmissionAsync(submission, ct);
                if (errors.Any(x => x.Severity.Equals("Error", StringComparison.OrdinalIgnoreCase)))
                    return BadRequest(APIResponse<ProcessScheduleDto>.Fail("The mapped source does not satisfy the request form. Check required fields and validation rules."));
            }
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            return BadRequest(APIResponse<ProcessScheduleDto>.Fail(ex.Message));
        }
        if (row is null)
        {
            row = new WFProcessScheduled
            {
                ProcessId = processId, DataAreaId = company,
                BackgroundJob = new SysBackgroundJob
                {
                    Name = $"Workflow schedule {company}/{processId}", JobKey = "WFProcessScheduled",
                    DataAreaId = company, TenantId = company, IntervalSeconds = 60, PreventOverlap = true,
                    MaxRetryCount = 3, RetryDelaySeconds = 60
                }
            };
            db.Set<WFProcessScheduled>().Add(row);
        }
        row.ExecutionUserId = currentUser.GetCurrentUserId();
        row.OwnerAccountId = currentUser.GetOwnerAccountId();
        row.Enabled = dto.Enabled;
        dto.Version = null;
        row.ConfigurationJson = JsonSerializer.Serialize(dto);
        row.NextRunAt = next;
        row.BackgroundJob.IsEnabled = dto.Enabled;
        row.BackgroundJob.NextRunAt = next;
        row.BackgroundJob.Status = SysJobStatus.Active;
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(APIResponse<ProcessScheduleDto>.Fail("The schedule changed during saving. Reload before saving."));
        }
        return Ok(APIResponse<ProcessScheduleDto>.Ok(Read(row)));
    }

    internal static ProcessScheduleDto Read(WFProcessScheduled row)
    {
        var dto = JsonSerializer.Deserialize<ProcessScheduleDto>(row.ConfigurationJson)
            ?? throw new InvalidOperationException("Schedule configuration is unavailable.");
        dto.Enabled = row.Enabled;
        dto.Version = Convert.ToBase64String(row.RowVersion);
        return dto;
    }
}
