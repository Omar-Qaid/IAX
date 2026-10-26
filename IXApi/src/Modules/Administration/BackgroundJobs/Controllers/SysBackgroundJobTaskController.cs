using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Dtos;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Interfaces;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Services;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Shared.Application.NumberSequences;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Controllers
{
    [ApiController]
    [Route("api/v1/SysBackgroundJob/{jobId:long}/tasks")]
    public class SysBackgroundJobTaskController : BaseController<SysBackgroundJobTask, SysBackgroundJobTaskDto>
    {
        private readonly ISysBackgroundJobTaskService _taskService;

        public SysBackgroundJobTaskController(
            ISysBackgroundJobTaskService service,
            ILogger<SysBackgroundJobTaskController> logger)
            : base(service, logger)
        {
            _taskService = service;
        }

        [HttpGet]
        [DomainPermission("System", "BackgroundJobs", "View")]
        public async Task<IActionResult> Get(long jobId, CancellationToken ct)
        {
            var result = await _taskService.GetAsync(jobId, ct);
            return result == null ? NotFound() : Ok(APIResponse<List<SysBackgroundJobTaskDto>>.Ok(result));
        }

        [HttpPut]
        [DomainPermission("System", "BackgroundJobs", "Edit")]
        public async Task<IActionResult> Save(long jobId, List<SysBackgroundJobTaskDto> submitted, CancellationToken ct)
        {
            var result = await _taskService.SaveAsync(jobId, submitted, ct);
            return result.Status switch
            {
                BatchSetupStatus.NotFound => NotFound(),
                BatchSetupStatus.Invalid => BadRequest(APIResponse<bool>.Fail(result.Error!)),
                BatchSetupStatus.Conflict => Conflict(APIResponse<bool>.Fail(result.Error!)),
                _ => await Get(jobId, ct)
            };
        }

        [HttpGet("history")]
        [DomainPermission("System", "BackgroundJobs", "View")]
        public async Task<IActionResult> History(long jobId, CancellationToken ct)
        {
            var result = await _taskService.HistoryAsync(jobId, ct);
            return result == null ? NotFound() : Ok(APIResponse<object>.Ok(result));
        }

        [NonAction]
        public override Task<ActionResult<APIResponse<SysBackgroundJobTaskDto>>> Create(SysBackgroundJobTaskDto dto, CancellationToken cancellationToken = default) =>
            base.Create(dto, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<IEnumerable<SysBackgroundJobTaskDto>>>> GetAll(CancellationToken cancellationToken = default) =>
            base.GetAll(cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<IEnumerable<SysBackgroundJobTaskDto>>>> GetPaged(QueryFilterDto paginationParams, CancellationToken cancellationToken = default) =>
            base.GetPaged(paginationParams, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<SysBackgroundJobTaskDto>>> GetById(string id, CancellationToken cancellationToken = default) =>
            base.GetById(id, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<SysBackgroundJobTaskDto>>> Update(string id, SysBackgroundJobTaskDto dto, CancellationToken cancellationToken = default) =>
            base.Update(id, dto, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default) =>
            base.Delete(id, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<IEnumerable<SysBackgroundJobTaskDto>>>> CreateRange(IEnumerable<SysBackgroundJobTaskDto> dtos, CancellationToken cancellationToken = default) =>
            base.CreateRange(dtos, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<IEnumerable<SysBackgroundJobTaskDto>>>> UpdateRange(IEnumerable<SysBackgroundJobTaskDto> dtos, CancellationToken cancellationToken = default) =>
            base.UpdateRange(dtos, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<bool>>> DeleteRange(IEnumerable<string> ids, CancellationToken cancellationToken = default) =>
            base.DeleteRange(ids, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<NumberSequenceMetadataDto>>> GetNumberSequence(CancellationToken cancellationToken = default) =>
            base.GetNumberSequence(cancellationToken);
    }
}
