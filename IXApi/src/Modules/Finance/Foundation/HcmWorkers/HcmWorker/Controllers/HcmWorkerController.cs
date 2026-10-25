using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Foundation.HcmWorkers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("Organization", "Employees")]
    public class HcmWorkerController : BaseController<HcmWorker, HcmWorkerDto>
    {
        private readonly IHcmWorkerService _workerService;

        public HcmWorkerController(IHcmWorkerService service, ILogger<HcmWorkerController>? logger = null)
            : base(service, logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<HcmWorkerController>.Instance)
        {
            _workerService = service;
        }

        protected override string[]? GetDefaultIncludes() => new[] { "Party", "Gender", "Nationality", "WorkerOrganizationAssignmentsV1", "WorkerShowroomAssignments" };

        public override async Task<ActionResult<APIResponse<IEnumerable<HcmWorkerDto>>>> GetAll(
            CancellationToken cancellationToken = default)
        {
            var workers = await _workerService.GetWorkerListAsync(cancellationToken);
            return Ok(APIResponse<IEnumerable<HcmWorkerDto>>.Ok(workers));
        }

        [HttpGet("lookup")]
        public async Task<ActionResult<APIResponse<HcmWorkerLookupPageDto>>> GetLookup(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            [FromQuery] string? search = null,
            [FromQuery] long? selectedId = null,
            CancellationToken cancellationToken = default)
        {
            var workers = await _workerService.GetWorkerLookupAsync(
                pageNumber, pageSize, search, selectedId, cancellationToken);
            return Ok(APIResponse<HcmWorkerLookupPageDto>.Ok(workers));
        }

        [HttpGet("{workerId:long}/organization-assignments-v1")]
        public async Task<ActionResult<APIResponse<IReadOnlyList<HcmWorkerOrganizationAssignmentV1Dto>>>> GetOrganizationAssignmentsV1(
            long workerId,
            CancellationToken cancellationToken = default)
        {
            var assignments = await _workerService.GetOrganizationAssignmentsV1Async(workerId, cancellationToken);
            return Ok(APIResponse<IReadOnlyList<HcmWorkerOrganizationAssignmentV1Dto>>.Ok(assignments));
        }

        [HttpGet("{workerId:long}/assignment-chain")]
        public async Task<ActionResult<APIResponse<IReadOnlyList<HcmWorkerAssignmentChainNodeDto>>>> GetAssignmentChain(
            long workerId,
            CancellationToken cancellationToken = default)
        {
            var chain = await _workerService.GetAssignmentChainAsync(workerId, cancellationToken);
            return Ok(APIResponse<IReadOnlyList<HcmWorkerAssignmentChainNodeDto>>.Ok(chain));
        }

        [HttpGet("{workerId:long}/showroom-assignments")]
        public async Task<ActionResult<APIResponse<IReadOnlyList<HcmWorkerShowroomAssignmentDto>>>> GetShowroomAssignments(
            long workerId,
            CancellationToken cancellationToken = default)
        {
            var assignments = await _workerService.GetShowroomAssignmentsAsync(workerId, cancellationToken);
            return Ok(APIResponse<IReadOnlyList<HcmWorkerShowroomAssignmentDto>>.Ok(assignments));
        }

        [HttpPost("{workerId:long}/organization-assignments-v1")]
        public async Task<ActionResult<APIResponse<bool>>> CreateOrganizationAssignmentV1(
            long workerId,
            SaveHcmWorkerOrganizationAssignmentV1Request request,
            CancellationToken cancellationToken = default)
        {
            await _workerService.SaveOrganizationAssignmentV1Async(workerId, null, request, cancellationToken);
            return Ok(APIResponse<bool>.Ok(true, "Created successfully"));
        }

        [HttpPut("{workerId:long}/organization-assignments-v1/{assignmentId:long}")]
        public async Task<ActionResult<APIResponse<bool>>> UpdateOrganizationAssignmentV1(
            long workerId,
            long assignmentId,
            SaveHcmWorkerOrganizationAssignmentV1Request request,
            CancellationToken cancellationToken = default)
        {
            await _workerService.SaveOrganizationAssignmentV1Async(workerId, assignmentId, request, cancellationToken);
            return Ok(APIResponse<bool>.Ok(true, "Updated successfully"));
        }

        [HttpPost("{workerId:long}/showroom-assignments")]
        public async Task<ActionResult<APIResponse<bool>>> CreateShowroomAssignment(
            long workerId,
            SaveHcmWorkerShowroomAssignmentRequest request,
            CancellationToken cancellationToken = default)
        {
            await _workerService.SaveShowroomAssignmentAsync(workerId, null, request, cancellationToken);
            return Ok(APIResponse<bool>.Ok(true, "Created successfully"));
        }

        [HttpPut("{workerId:long}/showroom-assignments/{assignmentId:long}")]
        public async Task<ActionResult<APIResponse<bool>>> UpdateShowroomAssignment(
            long workerId,
            long assignmentId,
            SaveHcmWorkerShowroomAssignmentRequest request,
            CancellationToken cancellationToken = default)
        {
            await _workerService.SaveShowroomAssignmentAsync(workerId, assignmentId, request, cancellationToken);
            return Ok(APIResponse<bool>.Ok(true, "Updated successfully"));
        }

        public override async Task<ActionResult<APIResponse<HcmWorkerDto>>> Create(
            HcmWorkerDto dto,
            CancellationToken cancellationToken = default)
        {
            var result = await _workerService.CreateFromDtoAsync(dto, cancellationToken);
            return Ok(APIResponse<HcmWorkerDto>.Ok(result, "Created successfully"));
        }

        public override async Task<ActionResult<APIResponse<HcmWorkerDto>>> Update(
            string id,
            HcmWorkerDto dto,
            CancellationToken cancellationToken = default)
        {
            var result = await _workerService.UpdateFromDtoAsync(id, dto, cancellationToken);
            if (result == null)
                return NotFound(APIResponse<HcmWorkerDto>.Fail("HcmWorker not found"));
            return Ok(APIResponse<HcmWorkerDto>.Ok(result, "Updated successfully"));
        }

    }
}
