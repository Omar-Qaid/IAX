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
    public class BatchJobActivePeriodController : BaseController<SysBackgroundJobActivePeriod, BatchJobActivePeriodDto>
    {
        private readonly IBatchJobActivePeriodService _periodService;

        public BatchJobActivePeriodController(
            IBatchJobActivePeriodService service,
            ILogger<BatchJobActivePeriodController> logger)
            : base(service, logger)
        {
            _periodService = service;
        }

        [HttpGet]
        [DomainPermission("System", "BackgroundJobs", "View")]
        public async Task<IActionResult> Get(CancellationToken ct) =>
            Ok(APIResponse<List<BatchJobActivePeriodDto>>.Ok(await _periodService.GetAsync(ct)));

        [HttpPost]
        [DomainPermission("System", "BackgroundJobs", "Create")]
        public override async Task<ActionResult<APIResponse<BatchJobActivePeriodDto>>> Create([FromBody] BatchJobActivePeriodDto dto, CancellationToken ct = default)
        {
            var result = await _periodService.CreateAsync(dto, ct);
            return result.Status == BatchSetupStatus.Conflict
                ? Conflict(APIResponse<BatchJobActivePeriodDto>.Fail(result.Error!))
                : Ok(APIResponse<BatchJobActivePeriodDto>.Ok(result.Value!, "Active period created."));
        }

        [HttpPut("{recId:long}")]
        [DomainPermission("System", "BackgroundJobs", "Edit")]
        public async Task<IActionResult> Update(long recId, BatchJobActivePeriodDto dto, CancellationToken ct)
        {
            var result = await _periodService.UpdateAsync(recId, dto, ct);
            return result.Status switch
            {
                BatchSetupStatus.NotFound => NotFound(APIResponse<BatchJobActivePeriodDto>.Fail(result.Error!)),
                BatchSetupStatus.Conflict => Conflict(APIResponse<BatchJobActivePeriodDto>.Fail(result.Error!)),
                _ => Ok(APIResponse<BatchJobActivePeriodDto>.Ok(result.Value!, "Active period updated."))
            };
        }

        [HttpDelete("{recId:long}")]
        [DomainPermission("System", "BackgroundJobs", "Delete")]
        public async Task<IActionResult> Delete(long recId, CancellationToken ct)
        {
            var result = await _periodService.DeleteAsync(recId, ct);
            return result.Status switch
            {
                BatchSetupStatus.NotFound => NotFound(APIResponse<bool>.Fail(result.Error!)),
                BatchSetupStatus.Conflict => Conflict(APIResponse<bool>.Fail(result.Error!)),
                _ => Ok(APIResponse<bool>.Ok(true, "Active period deleted."))
            };
        }

        [NonAction]
        public override Task<ActionResult<APIResponse<IEnumerable<BatchJobActivePeriodDto>>>> GetAll(CancellationToken cancellationToken = default) =>
            base.GetAll(cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<IEnumerable<BatchJobActivePeriodDto>>>> GetPaged(QueryFilterDto paginationParams, CancellationToken cancellationToken = default) =>
            base.GetPaged(paginationParams, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<BatchJobActivePeriodDto>>> GetById(string id, CancellationToken cancellationToken = default) =>
            base.GetById(id, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<BatchJobActivePeriodDto>>> Update(string id, BatchJobActivePeriodDto dto, CancellationToken cancellationToken = default) =>
            base.Update(id, dto, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default) =>
            base.Delete(id, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<IEnumerable<BatchJobActivePeriodDto>>>> CreateRange(IEnumerable<BatchJobActivePeriodDto> dtos, CancellationToken cancellationToken = default) =>
            base.CreateRange(dtos, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<IEnumerable<BatchJobActivePeriodDto>>>> UpdateRange(IEnumerable<BatchJobActivePeriodDto> dtos, CancellationToken cancellationToken = default) =>
            base.UpdateRange(dtos, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<bool>>> DeleteRange(IEnumerable<string> ids, CancellationToken cancellationToken = default) =>
            base.DeleteRange(ids, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<NumberSequenceMetadataDto>>> GetNumberSequence(CancellationToken cancellationToken = default) =>
            base.GetNumberSequence(cancellationToken);
    }
}
