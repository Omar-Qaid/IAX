using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Services;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs
{
    /// <summary>
    /// REST API for the Background Job Management system: create/schedule jobs, trigger them
    /// manually, inspect execution history, control lifecycle, and feed a monitoring dashboard.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/[controller]")]
    public class SysBackgroundJobController : ControllerBase
    {
        private readonly ISysBackgroundJobManager _jobs;
        private readonly ISysBackgroundJobRegistry _registry;
        private readonly ICurrentUserService _currentUser;

        public SysBackgroundJobController(
            ISysBackgroundJobManager jobs,
            ISysBackgroundJobRegistry registry,
            ICurrentUserService currentUser)
        {
            _jobs = jobs;
            _registry = registry;
            _currentUser = currentUser;
        }

        // ── Queries ──────────────────────────────────────────────────────

        /// <summary>Lists jobs with search/filter/pagination.</summary>
        [HttpGet]
        [DomainPermission("System", "BackgroundJobs", "View")]
        public async Task<ActionResult<APIResponse<IEnumerable<SysBackgroundJobDto>>>> GetJobs(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] SysJobStatus? status = null,
            [FromQuery] string? jobKey = null,
            CancellationToken ct = default)
        {
            var (items, total) = await _jobs.GetJobsAsync(pageNumber, pageSize, search, status, jobKey, ct);
            var response = APIResponse<IEnumerable<SysBackgroundJobDto>>.Ok(items);
            response.Pagination = new PaginationMetadata(pageNumber, pageSize, total);
            return Ok(response);
        }

        /// <summary>Gets a single job by id.</summary>
        [HttpGet("{id:long}")]
        [DomainPermission("System", "BackgroundJobs", "View")]
        public async Task<ActionResult<APIResponse<SysBackgroundJobDto>>> GetJob(long id, CancellationToken ct = default)
        {
            var job = await _jobs.GetByIdAsync(id, ct);
            return job is null
                ? NotFound(APIResponse<SysBackgroundJobDto>.Fail($"Job {id} not found."))
                : Ok(APIResponse<SysBackgroundJobDto>.Ok(job));
        }

        /// <summary>Gets a job's execution history (paged).</summary>
        [HttpGet("{id:long}/executions")]
        [DomainPermission("System", "BackgroundJobs", "View")]
        public async Task<ActionResult<APIResponse<IEnumerable<SysBackgroundJobExecutionDto>>>> GetExecutions(
            long id, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        {
            var (items, total) = await _jobs.GetExecutionsAsync(id, pageNumber, pageSize, ct);
            var response = APIResponse<IEnumerable<SysBackgroundJobExecutionDto>>.Ok(items);
            response.Pagination = new PaginationMetadata(pageNumber, pageSize, total);
            return Ok(response);
        }

        /// <summary>Dashboard-ready aggregate snapshot of the job subsystem.</summary>
        [HttpGet("dashboard")]
        [DomainPermission("System", "BackgroundJobs", "View")]
        public async Task<ActionResult<APIResponse<SysBackgroundJobDashboardDto>>> GetDashboard(CancellationToken ct = default)
            => Ok(APIResponse<SysBackgroundJobDashboardDto>.Ok(await _jobs.GetDashboardAsync(ct)));

        /// <summary>Lists the registered handler keys available to bind new jobs to.</summary>
        [HttpGet("handlers")]
        [DomainPermission("System", "BackgroundJobs", "View")]
        public ActionResult<APIResponse<IEnumerable<string>>> GetHandlers()
            => Ok(APIResponse<IEnumerable<string>>.Ok(_registry.RegisteredKeys.OrderBy(k => k)));

        // ── Mutations ─────────────────────────────────────────────────────

        /// <summary>Creates and schedules a new job.</summary>
        [HttpPost]
        [DomainPermission("System", "BackgroundJobs", "Create")]
        public async Task<ActionResult<APIResponse<SysBackgroundJobDto>>> Create(
            [FromBody] CreateSysBackgroundJobDto dto, CancellationToken ct = default)
        {
            try
            {
                var result = await _jobs.CreateAsync(dto, ct);
                return Ok(APIResponse<SysBackgroundJobDto>.Ok(result, "Job created"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(APIResponse<SysBackgroundJobDto>.Fail(ex.Message));
            }
        }

        /// <summary>Updates a job's schedule and reliability settings.</summary>
        [HttpPut("{id:long}/schedule")]
        [DomainPermission("System", "BackgroundJobs", "Edit")]
        public async Task<ActionResult<APIResponse<SysBackgroundJobDto>>> UpdateSchedule(
            long id, [FromBody] UpdateSysBackgroundJobScheduleDto dto, CancellationToken ct = default)
        {
            try
            {
                var result = await _jobs.UpdateScheduleAsync(id, dto, ct);
                return Ok(APIResponse<SysBackgroundJobDto>.Ok(result, "Schedule updated"));
            }
            catch (KeyNotFoundException ex) { return NotFound(APIResponse<SysBackgroundJobDto>.Fail(ex.Message)); }
            catch (InvalidOperationException ex) { return BadRequest(APIResponse<SysBackgroundJobDto>.Fail(ex.Message)); }
        }

        /// <summary>Manually triggers an immediate run. Returns the created execution id.</summary>
        [HttpPost("{id:long}/trigger")]
        [DomainPermission("System", "BackgroundJobs", "Run")]
        public async Task<ActionResult<APIResponse<long>>> Trigger(long id, CancellationToken ct = default)
        {
            try
            {
                var execId = await _jobs.TriggerAsync(id, _currentUser.GetCurrentUserId(), ct);
                return Ok(APIResponse<long>.Ok(execId, "Job triggered"));
            }
            catch (KeyNotFoundException ex) { return NotFound(APIResponse<long>.Fail(ex.Message)); }
        }

        /// <summary>Pauses a job (stops scheduling until resumed).</summary>
        [HttpPut("{id:long}/pause")]
        [DomainPermission("System", "BackgroundJobs", "Edit")]
        public async Task<ActionResult<APIResponse<bool>>> Pause(long id, CancellationToken ct = default)
        {
            try { await _jobs.PauseAsync(id, ct); return Ok(APIResponse<bool>.Ok(true, "Paused")); }
            catch (KeyNotFoundException ex) { return NotFound(APIResponse<bool>.Fail(ex.Message)); }
        }

        /// <summary>Resumes a paused job and recomputes its next run.</summary>
        [HttpPut("{id:long}/resume")]
        [DomainPermission("System", "BackgroundJobs", "Edit")]
        public async Task<ActionResult<APIResponse<bool>>> Resume(long id, CancellationToken ct = default)
        {
            try { await _jobs.ResumeAsync(id, ct); return Ok(APIResponse<bool>.Ok(true, "Resumed")); }
            catch (KeyNotFoundException ex) { return NotFound(APIResponse<bool>.Fail(ex.Message)); }
        }

        /// <summary>Cancels a job (will not run again).</summary>
        [HttpPut("{id:long}/cancel")]
        [DomainPermission("System", "BackgroundJobs", "Cancel")]
        public async Task<ActionResult<APIResponse<bool>>> Cancel(long id, CancellationToken ct = default)
        {
            try { await _jobs.CancelAsync(id, ct); return Ok(APIResponse<bool>.Ok(true, "Cancelled")); }
            catch (KeyNotFoundException ex) { return NotFound(APIResponse<bool>.Fail(ex.Message)); }
        }

        /// <summary>Soft-deletes a job.</summary>
        [HttpDelete("{id:long}")]
        [DomainPermission("System", "BackgroundJobs", "Delete")]
        public async Task<ActionResult<APIResponse<bool>>> Delete(long id, CancellationToken ct = default)
        {
            try { await _jobs.DeleteAsync(id, ct); return Ok(APIResponse<bool>.Ok(true, "Deleted")); }
            catch (KeyNotFoundException ex) { return NotFound(APIResponse<bool>.Fail(ex.Message)); }
        }
    }
}
