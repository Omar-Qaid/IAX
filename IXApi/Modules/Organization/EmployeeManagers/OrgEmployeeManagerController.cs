using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Organization.EmployeeManagers
{
    /// <summary>
    /// Manages the dynamic management hierarchy for an employee. The hierarchy is edited as a whole
    /// set per employee (replace-set semantics), so this controller is not a generic CRUD controller.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/[controller]")]
    [DomainPermission("Organization", "Managers")]
    public class OrgEmployeeManagerController : ControllerBase
    {
        private readonly IOrgEmployeeManagerService _service;
        private readonly ILogger<OrgEmployeeManagerController> _logger;

        public OrgEmployeeManagerController(IOrgEmployeeManagerService service, ILogger<OrgEmployeeManagerController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>Gets every manager assignment org-wide (for the hierarchy diagram).</summary>
        [HttpGet]
        public async Task<ActionResult<APIResponse<IEnumerable<OrgEmployeeManagerDto>>>> GetAll(CancellationToken cancellationToken = default)
        {
            var result = await _service.GetAllAssignmentsAsync(cancellationToken);
            return Ok(APIResponse<IEnumerable<OrgEmployeeManagerDto>>.Ok(result));
        }

        /// <summary>Gets all manager assignments for an employee.</summary>
        [HttpGet("by-employee/{employeeId:long}")]
        public async Task<ActionResult<APIResponse<IEnumerable<OrgEmployeeManagerDto>>>> GetByEmployee(long employeeId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[EmployeeManager] - Fetching managers for employee {EmployeeId}", employeeId);
            var result = await _service.GetForEmployeeAsync(employeeId, cancellationToken);
            return Ok(APIResponse<IEnumerable<OrgEmployeeManagerDto>>.Ok(result));
        }

        /// <summary>Gets the direct reports of a manager (for the hierarchy tree drill-down).</summary>
        [HttpGet("by-manager/{managerId:long}")]
        public async Task<ActionResult<APIResponse<IEnumerable<OrgEmployeeManagerDto>>>> GetByManager(long managerId, CancellationToken cancellationToken = default)
        {
            var result = await _service.GetReportsAsync(managerId, cancellationToken);
            return Ok(APIResponse<IEnumerable<OrgEmployeeManagerDto>>.Ok(result));
        }

        /// <summary>Replaces the full set of manager assignments for an employee.</summary>
        [HttpPut("by-employee/{employeeId:long}")]
        public async Task<ActionResult<APIResponse<IEnumerable<OrgEmployeeManagerDto>>>> ReplaceForEmployee(long employeeId, [FromBody] IEnumerable<OrgEmployeeManagerDto> rows, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[EmployeeManager] - Replacing managers for employee {EmployeeId}", employeeId);
            var result = await _service.ReplaceForEmployeeAsync(employeeId, rows ?? Enumerable.Empty<OrgEmployeeManagerDto>(), cancellationToken);
            return Ok(APIResponse<IEnumerable<OrgEmployeeManagerDto>>.Ok(result, "Saved successfully"));
        }
    }
}

