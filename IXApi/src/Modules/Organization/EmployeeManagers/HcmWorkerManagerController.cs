using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Organization.HcmWorkerManagers
{
    /// <summary>
    /// Manages the dynamic management hierarchy for an employee. The hierarchy is edited as a whole
    /// set per employee (replace-set semantics), so this controller is not a generic CRUD controller.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/[controller]")]
    [DomainPermission("Organization", "Managers")]
    public class HcmWorkerManagerController : ControllerBase
    {
        private readonly IHcmWorkerManagerService _service;
        private readonly ILogger<HcmWorkerManagerController> _logger;

        public HcmWorkerManagerController(IHcmWorkerManagerService service, ILogger<HcmWorkerManagerController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>Gets every manager assignment org-wide (for the hierarchy diagram).</summary>
        [HttpGet]
        public async Task<ActionResult<APIResponse<IEnumerable<HcmWorkerManagerDto>>>> GetAll(CancellationToken cancellationToken = default)
        {
            var result = await _service.GetAllAssignmentsAsync(cancellationToken);
            return Ok(APIResponse<IEnumerable<HcmWorkerManagerDto>>.Ok(result));
        }

        /// <summary>Gets all manager assignments for an employee.</summary>
        [HttpGet("by-employee/{employeeId:long}")]
        public async Task<ActionResult<APIResponse<IEnumerable<HcmWorkerManagerDto>>>> GetByEmployee(long employeeId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[EmployeeManager] - Fetching managers for employee {EmployeeId}", employeeId);
            var result = await _service.GetForEmployeeAsync(employeeId, cancellationToken);
            return Ok(APIResponse<IEnumerable<HcmWorkerManagerDto>>.Ok(result));
        }

        /// <summary>Gets the direct reports of a manager (for the hierarchy tree drill-down).</summary>
        [HttpGet("by-manager/{managerId:long}")]
        public async Task<ActionResult<APIResponse<IEnumerable<HcmWorkerManagerDto>>>> GetByManager(long managerId, CancellationToken cancellationToken = default)
        {
            var result = await _service.GetReportsAsync(managerId, cancellationToken);
            return Ok(APIResponse<IEnumerable<HcmWorkerManagerDto>>.Ok(result));
        }

        /// <summary>Replaces the full set of manager assignments for an employee.</summary>
        [HttpPut("by-employee/{employeeId:long}")]
        public async Task<ActionResult<APIResponse<IEnumerable<HcmWorkerManagerDto>>>> ReplaceForEmployee(long employeeId, [FromBody] IEnumerable<HcmWorkerManagerDto> rows, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[EmployeeManager] - Replacing managers for employee {EmployeeId}", employeeId);
            var result = await _service.ReplaceForEmployeeAsync(employeeId, rows ?? Enumerable.Empty<HcmWorkerManagerDto>(), cancellationToken);
            return Ok(APIResponse<IEnumerable<HcmWorkerManagerDto>>.Ok(result, "Saved successfully"));
        }
    }
}

