using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.Identity.Roles;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Identity.Permissions
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    [DomainPermission("SystemAdministration", "Permissions")]
    public class AppPermissionController : ControllerBase
    {
        private readonly IAppPermissionService _service;
        private readonly RoleManager<AspNetRole> _roleManager;

        public AppPermissionController(IAppPermissionService service, RoleManager<AspNetRole> roleManager)
        {
            _service = service;
            _roleManager = roleManager;
        }

        /// <summary>Returns every permission defined in the database, grouped by module.</summary>
        [HttpGet]
        public async Task<ActionResult<APIResponse<List<AppPermissionDto>>>> GetAll(CancellationToken ct)
        {
            var list = await _service.GetAllAsync(ct);
            return Ok(APIResponse<List<AppPermissionDto>>.Ok(list.Adapt<List<AppPermissionDto>>()));
        }

        /// <summary>Returns the permissions currently assigned to a role.</summary>
        [HttpGet("role/{roleId}")]
        public async Task<ActionResult<APIResponse<RolePermissionsDto>>> GetByRole(string roleId, CancellationToken ct)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role is null)
                return NotFound(APIResponse<RolePermissionsDto>.Fail("Role not found"));

            var perms = await _service.GetByRoleAsync(roleId, ct);
            var dto = new RolePermissionsDto
            {
                RoleId = role.Id,
                RoleName = role.Name ?? string.Empty,
                Permissions = perms.Adapt<List<AppPermissionDto>>(),
            };
            return Ok(APIResponse<RolePermissionsDto>.Ok(dto));
        }

        /// <summary>
        /// Replaces the full permission set of a role (idempotent set operation).
        /// Send an empty list to revoke all permissions from the role.
        /// </summary>
        [HttpPost("role/set")]
        public async Task<ActionResult<APIResponse<bool>>> SetRolePermissions([FromBody] AssignPermissionsDto dto, CancellationToken ct)
        {
            await _service.SetRolePermissionsAsync(dto.RoleId, dto.PermissionIds, ct);
            return Ok(APIResponse<bool>.Ok(true, "Permissions updated"));
        }

        /// <summary>Adds permissions to a role without removing existing ones.</summary>
        [HttpPost("role/assign")]
        public async Task<ActionResult<APIResponse<bool>>> AssignToRole([FromBody] AssignPermissionsDto dto, CancellationToken ct)
        {
            await _service.AssignToRoleAsync(dto.RoleId, dto.PermissionIds, ct);
            return Ok(APIResponse<bool>.Ok(true, "Permissions assigned"));
        }

        /// <summary>Removes specific permissions from a role.</summary>
        [HttpPost("role/remove")]
        public async Task<ActionResult<APIResponse<bool>>> RemoveFromRole([FromBody] AssignPermissionsDto dto, CancellationToken ct)
        {
            await _service.RemoveFromRoleAsync(dto.RoleId, dto.PermissionIds, ct);
            return Ok(APIResponse<bool>.Ok(true, "Permissions removed"));
        }
    }
}


