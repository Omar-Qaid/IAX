using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Dtos;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Interfaces;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Shared.Application.NumberSequences;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Controllers
{
    [ApiController]
    [Route("api/v1/BatchSettings")]
    public class BatchSettingsController : BaseController<BatchSettings, BatchSettingsDto>
    {
        private readonly IBatchSettingsService _settingsService;

        public BatchSettingsController(
            IBatchSettingsService service,
            ILogger<BatchSettingsController> logger)
            : base(service, logger)
        {
            _settingsService = service;
        }

        [HttpGet]
        [DomainPermission("System", "BackgroundJobs", "View")]
        public async Task<IActionResult> Get(CancellationToken ct) =>
            Ok(APIResponse<BatchSettings>.Ok(await _settingsService.GetAsync(ct)));

        [HttpPut]
        [DomainPermission("System", "BackgroundJobs", "Edit")]
        public async Task<IActionResult> Save(BatchSettings settings, CancellationToken ct)
        {
            var result = await _settingsService.SaveAsync(settings, ct);
            return result.Error != null
                ? BadRequest(APIResponse<bool>.Fail(result.Error))
                : Ok(APIResponse<BatchSettings>.Ok(result.Settings!));
        }

        [NonAction]
        public override Task<ActionResult<APIResponse<BatchSettingsDto>>> Create(BatchSettingsDto dto, CancellationToken cancellationToken = default) =>
            base.Create(dto, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<IEnumerable<BatchSettingsDto>>>> GetAll(CancellationToken cancellationToken = default) =>
            base.GetAll(cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<IEnumerable<BatchSettingsDto>>>> GetPaged(QueryFilterDto paginationParams, CancellationToken cancellationToken = default) =>
            base.GetPaged(paginationParams, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<BatchSettingsDto>>> GetById(string id, CancellationToken cancellationToken = default) =>
            base.GetById(id, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<BatchSettingsDto>>> Update(string id, BatchSettingsDto dto, CancellationToken cancellationToken = default) =>
            base.Update(id, dto, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default) =>
            base.Delete(id, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<IEnumerable<BatchSettingsDto>>>> CreateRange(IEnumerable<BatchSettingsDto> dtos, CancellationToken cancellationToken = default) =>
            base.CreateRange(dtos, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<IEnumerable<BatchSettingsDto>>>> UpdateRange(IEnumerable<BatchSettingsDto> dtos, CancellationToken cancellationToken = default) =>
            base.UpdateRange(dtos, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<bool>>> DeleteRange(IEnumerable<string> ids, CancellationToken cancellationToken = default) =>
            base.DeleteRange(ids, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<NumberSequenceMetadataDto>>> GetNumberSequence(CancellationToken cancellationToken = default) =>
            base.GetNumberSequence(cancellationToken);
    }
}
