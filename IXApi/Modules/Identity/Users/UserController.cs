using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Application.Contracts;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Identity.Users
{
    [ApiController]
    [Route("api/v1/[controller]")]
[DomainPermission("SystemAdministration", "Users")]
    public class UserController : BaseController<AspNetUser, AspNetUserDto>
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<AspNetUser> _userManager;

        public UserController(
            IUserService service,
            ILogger<UserController> logger,
            ApplicationDbContext db,
            UserManager<AspNetUser> userManager) : base(service, logger)
        {
            _db = db;
            _userManager = userManager;
        }

        // Include the linked employee so list rows can show EmployeeName.
        protected override string[]? GetDefaultIncludes() => ["OrgEntity"];

        /// <summary>
        /// Users active within the last <paramref name="minutes"/> (by LastLoginDate). Paged.
        /// </summary>
        [HttpGet("online")]
        public async Task<ActionResult<APIResponse<IEnumerable<AspNetUserDto>>>> GetOnline(
            [FromQuery] int minutes = 15,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var threshold = DateTime.Now.AddMinutes(-Math.Abs(minutes));

            var query = _db.Users
                .AsNoTracking()
                .Include(u => u.OrgEntity)
                .Where(u => u.LastLoginDate >= threshold)
                .OrderByDescending(u => u.LastLoginDate);

            var total = await query.CountAsync(cancellationToken);
            var users = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var dtos = users.Adapt<IEnumerable<AspNetUserDto>>();
            var response = APIResponse<IEnumerable<AspNetUserDto>>.Ok(dtos);
            response.Pagination = new PaginationMetadata(pageNumber, pageSize, total);
            return Ok(response);
        }

        /// <summary>Role names currently assigned to the user.</summary>
        [HttpGet("{id}/roles")]
        public async Task<ActionResult<APIResponse<IEnumerable<string>>>> GetRoles(string id, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound(APIResponse<IEnumerable<string>>.Fail("User not found"));

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(APIResponse<IEnumerable<string>>.Ok(roles));
        }

        /// <summary>Set the exact set of roles the user should have (adds missing, removes extra).</summary>
        [HttpPut("{id}/roles")]
        public async Task<ActionResult<APIResponse<bool>>> SetRoles(string id, [FromBody] AssignRolesDto dto, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound(APIResponse<bool>.Fail("User not found"));

            var desired = (dto.Roles ?? new List<string>()).Where(r => !string.IsNullOrWhiteSpace(r)).Distinct().ToList();
            var current = await _userManager.GetRolesAsync(user);

            var toAdd = desired.Except(current, StringComparer.OrdinalIgnoreCase).ToList();
            var toRemove = current.Except(desired, StringComparer.OrdinalIgnoreCase).ToList();

            if (toRemove.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, toRemove);
                if (!removeResult.Succeeded)
                    return BadRequest(APIResponse<bool>.Fail(string.Join("; ", removeResult.Errors.Select(e => e.Description))));
            }
            if (toAdd.Any())
            {
                var addResult = await _userManager.AddToRolesAsync(user, toAdd);
                if (!addResult.Succeeded)
                    return BadRequest(APIResponse<bool>.Fail(string.Join("; ", addResult.Errors.Select(e => e.Description))));
            }

            return Ok(APIResponse<bool>.Ok(true, "Roles updated"));
        }

        /// <summary>
        /// Update only the user-administration fields (email, phone, employee link, enabled state).
        /// Avoids a blanket Adapt that could disturb identity columns (password hash, stamps).
        /// </summary>
        [HttpPut("{id}")]
        public override async Task<ActionResult<APIResponse<AspNetUserDto>>> Update(string id, [FromBody] AspNetUserDto dto, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound(APIResponse<AspNetUserDto>.Fail("User not found"));

            user.Email = dto.Email;
            user.NormalizedEmail = dto.Email?.ToUpperInvariant();
            user.PhoneNumber = dto.PhoneNumber;
            user.OrgEntityId = (dto.EmployeeId == null || dto.EmployeeId <= 0) ? null : dto.EmployeeId;

            // Enabled toggle ↔ lockout. Disabled = locked out far in the future.
            user.LockoutEnabled = true;
            user.LockoutEnd = dto.Enabled ? null : DateTimeOffset.MaxValue;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(APIResponse<AspNetUserDto>.Fail(string.Join("; ", result.Errors.Select(e => e.Description))));

            var resultDto = user.Adapt<AspNetUserDto>();
            return Ok(APIResponse<AspNetUserDto>.Ok(resultDto, "Updated successfully"));
        }
    }
}
