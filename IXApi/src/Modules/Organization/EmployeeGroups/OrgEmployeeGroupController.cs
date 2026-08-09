using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Modules.Organization.Persistence;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Organization.Features.OrgEmployeeGroup
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("SystemAdministration", "UserGroups")]
    public class OrgEmployeeGroupController : BaseController<OrgEmployeeGroup, OrgEmployeeGroupDto>
    {
        private readonly IOrganizationDataContext _db;

        public OrgEmployeeGroupController(
            IOrgEmployeeGroupService service,
            ILogger<OrgEmployeeGroupController> logger,
            IOrganizationDataContext db) : base(service, logger)
        {
            _db = db;
        }

        /// <summary>Users that ARE members of the group (the "Selected users" pane).</summary>
        [HttpGet("{id}/users")]
        public async Task<ActionResult<APIResponse<IEnumerable<OrgEmployeeGroupMemberDto>>>> GetMembers(int id, CancellationToken cancellationToken = default)
        {
            var members = await _db.OrgEmployeeGroupDetails
                .AsNoTracking()
                .Where(d => d.UserGroupID == id)
                .Select(d => new OrgEmployeeGroupMemberDto
                {
                    UserId = d.User.Id,
                    UserName = d.User.UserName!,
                    DisplayName = d.User.OrgEntity != null ? d.User.OrgEntity.Name : d.User.UserName
                })
                .OrderBy(m => m.UserName)
                .ToListAsync(cancellationToken);

            return Ok(APIResponse<IEnumerable<OrgEmployeeGroupMemberDto>>.Ok(members));
        }

        /// <summary>Users that are NOT members of the group (the "Remaining users" pane), optional name filter.</summary>
        [HttpGet("{id}/available-users")]
        public async Task<ActionResult<APIResponse<IEnumerable<OrgEmployeeGroupMemberDto>>>> GetAvailableUsers(int id, [FromQuery] string? search = null, CancellationToken cancellationToken = default)
        {
            var memberIds = _db.OrgEmployeeGroupDetails.Where(d => d.UserGroupID == id).Select(d => d.UserID);

            var query = _db.Users
                .AsNoTracking()
                .Include(u => u.OrgEntity)
                .Where(u => !memberIds.Contains(u.Id));

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(u =>
                    u.UserName != null && u.UserName.Contains(term) ||
                    u.OrgEntity != null && u.OrgEntity.Name != null && u.OrgEntity.Name.Contains(term));
            }

            var users = await query
                .OrderBy(u => u.UserName)
                .Select(u => new OrgEmployeeGroupMemberDto
                {
                    UserId = u.Id,
                    UserName = u.UserName!,
                    DisplayName = u.OrgEntity != null ? u.OrgEntity.Name : u.UserName
                })
                .ToListAsync(cancellationToken);

            return Ok(APIResponse<IEnumerable<OrgEmployeeGroupMemberDto>>.Ok(users));
        }

        /// <summary>Add users to the group (skips users already members).</summary>
        [HttpPost("{id}/users")]
        public async Task<ActionResult<APIResponse<bool>>> AddMembers(int id, [FromBody] AssignUsersDto dto, CancellationToken cancellationToken = default)
        {
            var groupExists = await _db.OrgEmployeeGroups.AnyAsync(g => g.RecId == id, cancellationToken);
            if (!groupExists) return NotFound(APIResponse<bool>.Fail("User group not found"));

            var requested = (dto.UserIds ?? new List<string>()).Where(u => !string.IsNullOrWhiteSpace(u)).Distinct().ToList();
            var existing = await _db.OrgEmployeeGroupDetails
                .Where(d => d.UserGroupID == id && requested.Contains(d.UserID))
                .Select(d => d.UserID)
                .ToListAsync(cancellationToken);

            var toAdd = requested.Except(existing)
                .Select(uid => new OrgEmployeeGroupDetail { UserGroupID = id, UserID = uid })
                .ToList();

            if (toAdd.Any())
            {
                await _db.OrgEmployeeGroupDetails.AddRangeAsync(toAdd, cancellationToken);
                await _db.SaveChangesAsync(cancellationToken);
            }

            return Ok(APIResponse<bool>.Ok(true, "Members added"));
        }

        /// <summary>Remove users from the group.</summary>
        [HttpDelete("{id}/users")]
        public async Task<ActionResult<APIResponse<bool>>> RemoveMembers(int id, [FromBody] AssignUsersDto dto, CancellationToken cancellationToken = default)
        {
            var requested = (dto.UserIds ?? new List<string>()).Where(u => !string.IsNullOrWhiteSpace(u)).Distinct().ToList();

            var rows = await _db.OrgEmployeeGroupDetails
                .Where(d => d.UserGroupID == id && requested.Contains(d.UserID))
                .ToListAsync(cancellationToken);

            if (rows.Any())
            {
                _db.OrgEmployeeGroupDetails.RemoveRange(rows);
                await _db.SaveChangesAsync(cancellationToken);
            }

            return Ok(APIResponse<bool>.Ok(true, "Members removed"));
        }
    }
}



