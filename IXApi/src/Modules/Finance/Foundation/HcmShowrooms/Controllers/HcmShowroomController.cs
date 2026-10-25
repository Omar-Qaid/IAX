using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Foundation.HcmShowrooms
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("Organization", "Showrooms")]
    public class HcmShowroomController : BaseController<HcmShowroom, HcmShowroomDto>
    {
        private readonly IHcmShowroomService _showroomService;

        public HcmShowroomController(IHcmShowroomService service, ILogger<HcmShowroomController>? logger = null)
            : base(service, logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<HcmShowroomController>.Instance)
        {
            _showroomService = service;
        }

        protected override string[]? GetDefaultIncludes() => new[] { "PartyTable" };

        public override async Task<ActionResult<APIResponse<HcmShowroomDto>>> Create(
            HcmShowroomDto dto,
            CancellationToken cancellationToken = default)
        {
            var result = await _showroomService.CreateFromDtoAsync(dto, cancellationToken);
            return Ok(APIResponse<HcmShowroomDto>.Ok(result, "Created successfully"));
        }

        public override async Task<ActionResult<APIResponse<HcmShowroomDto>>> Update(
            string id,
            HcmShowroomDto dto,
            CancellationToken cancellationToken = default)
        {
            var result = await _showroomService.UpdateFromDtoAsync(id, dto, cancellationToken);
            if (result == null)
                return NotFound(APIResponse<HcmShowroomDto>.Fail("HcmShowroom not found"));
            return Ok(APIResponse<HcmShowroomDto>.Ok(result, "Updated successfully"));
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
