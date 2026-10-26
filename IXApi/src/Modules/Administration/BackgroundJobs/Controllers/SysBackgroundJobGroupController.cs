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
    [Route("api/v1/[controller]")]
    public class SysBackgroundJobGroupController : BaseController<SysBackgroundJobGroup, SysBackgroundJobGroupDto>
    {
        private readonly ISysBackgroundJobGroupService _groupsService;

        public SysBackgroundJobGroupController(
            ISysBackgroundJobGroupService service,
            ILogger<SysBackgroundJobGroupController> logger)
            : base(service, logger)
        {
            _groupsService = service;
        }

        [HttpGet]
        [DomainPermission("System", "BackgroundJobs", "View")]
        public async Task<IActionResult> Get(CancellationToken ct) =>
            Ok(APIResponse<List<SysBackgroundJobGroupDto>>.Ok(await _groupsService.GetAsync(ct)));

        [HttpPost]
        [DomainPermission("System", "BackgroundJobs", "Create")]
        public override async Task<ActionResult<APIResponse<SysBackgroundJobGroupDto>>> Create([FromBody] SysBackgroundJobGroupDto dto, CancellationToken ct = default)
        {
            var result = await _groupsService.CreateAsync(dto, ct);
            return result.Status == BatchSetupStatus.Conflict
                ? Conflict(APIResponse<SysBackgroundJobGroupDto>.Fail(result.Error!))
                : Ok(APIResponse<SysBackgroundJobGroupDto>.Ok(result.Value!, "Batch group created."));
        }

        [HttpPut("{id:long}")]
        [DomainPermission("System", "BackgroundJobs", "Edit")]
        public async Task<IActionResult> Update(long id, SysBackgroundJobGroupDto dto, CancellationToken ct)
        {
            var result = await _groupsService.UpdateAsync(id, dto, ct);
            return result.Status switch
            {
                BatchSetupStatus.NotFound => NotFound(APIResponse<SysBackgroundJobGroupDto>.Fail(result.Error!)),
                BatchSetupStatus.Conflict => Conflict(APIResponse<SysBackgroundJobGroupDto>.Fail(result.Error!)),
                _ => Ok(APIResponse<SysBackgroundJobGroupDto>.Ok(result.Value!, "Batch group updated."))
            };
        }

        [HttpDelete("{id:long}")]
        [DomainPermission("System", "BackgroundJobs", "Delete")]
        public async Task<IActionResult> Delete(long id, CancellationToken ct)
        {
            var result = await _groupsService.DeleteAsync(id, ct);
            return result.Status switch
            {
                BatchSetupStatus.NotFound => NotFound(APIResponse<bool>.Fail(result.Error!)),
                BatchSetupStatus.Conflict => Conflict(APIResponse<bool>.Fail(result.Error!)),
                _ => Ok(APIResponse<bool>.Ok(true, "Batch group deleted."))
            };
        }

        [NonAction]
        public override Task<ActionResult<APIResponse<IEnumerable<SysBackgroundJobGroupDto>>>> GetAll(CancellationToken cancellationToken = default) =>
            base.GetAll(cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<IEnumerable<SysBackgroundJobGroupDto>>>> GetPaged(QueryFilterDto paginationParams, CancellationToken cancellationToken = default) =>
            base.GetPaged(paginationParams, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<SysBackgroundJobGroupDto>>> GetById(string id, CancellationToken cancellationToken = default) =>
            base.GetById(id, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<SysBackgroundJobGroupDto>>> Update(string id, SysBackgroundJobGroupDto dto, CancellationToken cancellationToken = default) =>
            base.Update(id, dto, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default) =>
            base.Delete(id, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<IEnumerable<SysBackgroundJobGroupDto>>>> CreateRange(IEnumerable<SysBackgroundJobGroupDto> dtos, CancellationToken cancellationToken = default) =>
            base.CreateRange(dtos, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<IEnumerable<SysBackgroundJobGroupDto>>>> UpdateRange(IEnumerable<SysBackgroundJobGroupDto> dtos, CancellationToken cancellationToken = default) =>
            base.UpdateRange(dtos, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<bool>>> DeleteRange(IEnumerable<string> ids, CancellationToken cancellationToken = default) =>
            base.DeleteRange(ids, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<NumberSequenceMetadataDto>>> GetNumberSequence(CancellationToken cancellationToken = default) =>
            base.GetNumberSequence(cancellationToken);
    }
}
