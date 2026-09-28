using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Application.Contracts;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Foundation.HcmShowrooms
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("Organization", "Showrooms")]
    public class HcmShowroomController : BaseController<HcmShowroom, HcmShowroomDto>
    {
        private readonly IHcmShowroomService _showroomService;

        public HcmShowroomController(IHcmShowroomService service, ILogger<HcmShowroomController> logger) : base(service, logger)
        {
            _showroomService = service;
        }

        protected override string[]? GetDefaultIncludes() => new[] { "PartyTable" };

        public override async Task<ActionResult<APIResponse<HcmShowroomDto>>> Create(
            HcmShowroomDto dto,
            CancellationToken cancellationToken = default)
        {
            var entity = dto.Adapt<HcmShowroom>();
            var created = await _showroomService.AddShowroomAsync(
                entity,
                dto.Name ?? string.Empty,
                dto.NameAlias,
                cancellationToken);
            var result = await ReloadWithDefaultsAsync(created.RecId, cancellationToken) ?? created;
            return Ok(APIResponse<HcmShowroomDto>.Ok(result.Adapt<HcmShowroomDto>(), "Created successfully"));
        }

        public override async Task<ActionResult<APIResponse<HcmShowroomDto>>> Update(
            string id,
            HcmShowroomDto dto,
            CancellationToken cancellationToken = default)
        {
            var existing = await _showroomService.GetByIdAsync(id, cancellationToken);
            if (existing == null)
                return NotFound(APIResponse<HcmShowroomDto>.Fail("HcmShowroom not found"));

            dto.Adapt(existing);
            var updated = await _showroomService.UpdateShowroomAsync(
                existing,
                dto.Name ?? string.Empty,
                dto.NameAlias,
                cancellationToken);
            var result = await ReloadWithDefaultsAsync(id, cancellationToken) ?? updated;
            return Ok(APIResponse<HcmShowroomDto>.Ok(result.Adapt<HcmShowroomDto>(), "Updated successfully"));
        }

        [HttpGet("{showroomId:long}/worker-assignments")]
        public async Task<ActionResult<APIResponse<IReadOnlyList<HcmShowroomWorkerAssignmentDto>>>> GetWorkerAssignments(
            long showroomId,
            CancellationToken cancellationToken = default)
        {
            var assignments = await _showroomService.GetWorkerAssignmentsAsync(showroomId, cancellationToken);
            return Ok(APIResponse<IReadOnlyList<HcmShowroomWorkerAssignmentDto>>.Ok(assignments));
        }

        [HttpPost("{showroomId:long}/worker-assignments")]
        public async Task<ActionResult<APIResponse<bool>>> CreateWorkerAssignment(
            long showroomId,
            SaveHcmShowroomWorkerAssignmentRequest request,
            CancellationToken cancellationToken = default)
        {
            await _showroomService.SaveWorkerAssignmentAsync(showroomId, null, request, cancellationToken);
            return Ok(APIResponse<bool>.Ok(true, "Created successfully"));
        }

        [HttpPut("{showroomId:long}/worker-assignments/{assignmentId:long}")]
        public async Task<ActionResult<APIResponse<bool>>> UpdateWorkerAssignment(
            long showroomId,
            long assignmentId,
            SaveHcmShowroomWorkerAssignmentRequest request,
            CancellationToken cancellationToken = default)
        {
            await _showroomService.SaveWorkerAssignmentAsync(showroomId, assignmentId, request, cancellationToken);
            return Ok(APIResponse<bool>.Ok(true, "Updated successfully"));
        }
    }
}
