using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Modules.Organization.Persistence;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Organization.Features.HcmWorkerGroup
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("SystemAdministration", "UserGroups")]
    public class HcmWorkerGroupController : BaseController<HcmWorkerGroup, HcmWorkerGroupDto>
    {
        private readonly IOrganizationDataContext _db;

        public HcmWorkerGroupController(
            IHcmWorkerGroupService service,
            ILogger<HcmWorkerGroupController> logger,
            IOrganizationDataContext db) : base(service, logger)
        {
            _db = db;
        }

        /// <summary>Users that ARE members of the group (the "Selected users" pane).</summary>
        [HttpGet("{id}/users")]
        public async Task<ActionResult<APIResponse<IEnumerable<HcmWorkerGroupMemberDto>>>> GetMembers(int id, CancellationToken cancellationToken = default)
        {
            var members = await _db.HcmWorkerGroupDetails
                .AsNoTracking()
                .Where(d => d.UserGroupID == id)
                .Select(d => new HcmWorkerGroupMemberDto
                {
                    UserId = d.User.Id,
                    UserName = d.User.UserName!,
                    DisplayName = d.User.OrganizationEntity != null ? d.User.OrganizationEntity.Name : d.User.UserName
                })
                .OrderBy(m => m.UserName)
                .ToListAsync(cancellationToken);

            return Ok(APIResponse<IEnumerable<HcmWorkerGroupMemberDto>>.Ok(members));
        }

        /// <summary>Users that are NOT members of the group (the "Remaining users" pane), optional name filter.</summary>
        [HttpGet("{id}/available-users")]
        public async Task<ActionResult<APIResponse<IEnumerable<HcmWorkerGroupMemberDto>>>> GetAvailableUsers(int id, [FromQuery] string? search = null, CancellationToken cancellationToken = default)
        {
            var memberIds = _db.HcmWorkerGroupDetails.Where(d => d.UserGroupID == id).Select(d => d.UserID);

            var query = _db.Users
                .AsNoTracking()
                .Include(u => u.OrganizationEntity)
                .Where(u => !memberIds.Contains(u.Id));

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(u =>
                    u.UserName != null && u.UserName.Contains(term) ||
                    u.OrganizationEntity != null && u.OrganizationEntity.Name != null && u.OrganizationEntity.Name.Contains(term));
            }

            var users = await query
                .OrderBy(u => u.UserName)
                .Select(u => new HcmWorkerGroupMemberDto
                {
                    UserId = u.Id,
                    UserName = u.UserName!,
                    DisplayName = u.OrganizationEntity != null ? u.OrganizationEntity.Name : u.UserName
                })
                .ToListAsync(cancellationToken);

            return Ok(APIResponse<IEnumerable<HcmWorkerGroupMemberDto>>.Ok(users));
        }

        /// <summary>Add users to the group (skips users already members).</summary>
        [HttpPost("{id}/users")]
        public async Task<ActionResult<APIResponse<bool>>> AddMembers(int id, [FromBody] AssignUsersDto dto, CancellationToken cancellationToken = default)
        {
            var groupExists = await _db.HcmWorkerGroups.AnyAsync(g => g.RecId == id, cancellationToken);
            if (!groupExists) return NotFound(APIResponse<bool>.Fail("User group not found"));

            var requested = (dto.UserIds ?? new List<string>()).Where(u => !string.IsNullOrWhiteSpace(u)).Distinct().ToList();
            var existing = await _db.HcmWorkerGroupDetails
                .Where(d => d.UserGroupID == id && requested.Contains(d.UserID))
                .Select(d => d.UserID)
                .ToListAsync(cancellationToken);

            var toAdd = requested.Except(existing)
                .Select(uid => new HcmWorkerGroupDetail { UserGroupID = id, UserID = uid })
                .ToList();

            if (toAdd.Any())
            {
                await _db.HcmWorkerGroupDetails.AddRangeAsync(toAdd, cancellationToken);
                await _db.SaveChangesAsync(cancellationToken);
            }

            return Ok(APIResponse<bool>.Ok(true, "Members added"));
        }

        /// <summary>Remove users from the group.</summary>
        [HttpDelete("{id}/users")]
        public async Task<ActionResult<APIResponse<bool>>> RemoveMembers(int id, [FromBody] AssignUsersDto dto, CancellationToken cancellationToken = default)
        {
            var requested = (dto.UserIds ?? new List<string>()).Where(u => !string.IsNullOrWhiteSpace(u)).Distinct().ToList();

            var rows = await _db.HcmWorkerGroupDetails
                .Where(d => d.UserGroupID == id && requested.Contains(d.UserID))
                .ToListAsync(cancellationToken);

            if (rows.Any())
            {
                _db.HcmWorkerGroupDetails.RemoveRange(rows);
                await _db.SaveChangesAsync(cancellationToken);
            }

            return Ok(APIResponse<bool>.Ok(true, "Members removed"));
        }
    }
}



