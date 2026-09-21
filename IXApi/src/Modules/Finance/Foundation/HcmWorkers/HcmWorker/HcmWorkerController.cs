using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Application.Contracts;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Foundation.HcmWorkers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("Organization", "Employees")]
    public class HcmWorkerController : BaseController<HcmWorker, HcmWorkerDto>
    {
        private readonly IHcmWorkerService _workerService;

        public HcmWorkerController(IHcmWorkerService service, ILogger<HcmWorkerController> logger) : base(service, logger)
        {
            _workerService = service;
        }

        protected override string[]? GetDefaultIncludes() => new[] { "Party", "Occupation", "Gender", "Nationality" };

        public override async Task<ActionResult<APIResponse<HcmWorkerDto>>> Create(
            HcmWorkerDto dto,
            CancellationToken cancellationToken = default)
        {
            var worker = dto.Adapt<HcmWorker>();
            var created = await _workerService.AddWorkerAsync(
                worker,
                dto.Name ?? string.Empty,
                dto.NameAlias,
                dto.InitialPositionId,
                cancellationToken);
            var result = await ReloadWithDefaultsAsync(created.RecId, cancellationToken) ?? created;
            return Ok(APIResponse<HcmWorkerDto>.Ok(result.Adapt<HcmWorkerDto>(), "Created successfully"));
        }

        public override async Task<ActionResult<APIResponse<HcmWorkerDto>>> Update(
            string id,
            HcmWorkerDto dto,
            CancellationToken cancellationToken = default)
        {
            var existing = await _workerService.GetByIdAsync(id, cancellationToken);
            if (existing == null)
                return NotFound(APIResponse<HcmWorkerDto>.Fail("HcmWorker not found"));

            dto.Adapt(existing);
            var updated = await _workerService.UpdateWorkerAsync(
                existing,
                dto.Name ?? string.Empty,
                dto.NameAlias,
                cancellationToken);
            var result = await ReloadWithDefaultsAsync(id, cancellationToken) ?? updated;
            return Ok(APIResponse<HcmWorkerDto>.Ok(result.Adapt<HcmWorkerDto>(), "Updated successfully"));
        }
    }
}

