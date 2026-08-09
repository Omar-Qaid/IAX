using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.Organization.Features.OrgEmployeeCategory;
using IAX.IXApi.Modules.Identity.Permissions;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Organization.Features.OrgEmployeeCategory
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("SystemAdministration", "UserCategories")]
    public class OrgEmployeeCategoryController : BaseController<OrgEmployeeCategory, OrgEmployeeCategoryDto>
    {
        private readonly IOrgEmployeeCategoryService _categories;

        public OrgEmployeeCategoryController(IOrgEmployeeCategoryService service, ILogger<OrgEmployeeCategoryController> logger) : base(service, logger)
        {
            _categories = service;
        }

        /// <summary>Fetch a single category including its linkage Groups.</summary>
        public override async Task<ActionResult<APIResponse<OrgEmployeeCategoryDto>>> GetById(string id, CancellationToken cancellationToken = default)
        {
            if (!long.TryParse(id, out var catId))
                return NotFound(APIResponse<OrgEmployeeCategoryDto>.Fail("OrgEmployeeCategory not found"));

            var entity = await _categories.GetWithGroupsAsync(catId, cancellationToken);
            if (entity == null)
                return NotFound(APIResponse<OrgEmployeeCategoryDto>.Fail("OrgEmployeeCategory not found"));

            return Ok(APIResponse<OrgEmployeeCategoryDto>.Ok(entity.Adapt<OrgEmployeeCategoryDto>()));
        }

        /// <summary>Create a category together with its linkage Groups (inserted as a graph).</summary>
        public override async Task<ActionResult<APIResponse<OrgEmployeeCategoryDto>>> Create([FromBody] OrgEmployeeCategoryDto dto, CancellationToken cancellationToken = default)
        {
            var entity = dto.Adapt<OrgEmployeeCategory>(); // scalars + Groups (new rows; ids ignored on destination)
            var created = await _service.AddAsync(entity, cancellationToken);
            var result = await _categories.GetWithGroupsAsync(created.RecId, cancellationToken) ?? created;
            return Ok(APIResponse<OrgEmployeeCategoryDto>.Ok(result.Adapt<OrgEmployeeCategoryDto>(), "Created successfully"));
        }

        /// <summary>Update scalars and reconcile linkage Groups (add / update / delete).</summary>
        public override async Task<ActionResult<APIResponse<OrgEmployeeCategoryDto>>> Update(string id, [FromBody] OrgEmployeeCategoryDto dto, CancellationToken cancellationToken = default)
        {
            if (!long.TryParse(id, out var catId))
                return NotFound(APIResponse<OrgEmployeeCategoryDto>.Fail("OrgEmployeeCategory not found"));

            var scalars = dto.Adapt<OrgEmployeeCategory>();
            // Mapster ignores the entity Id on the destination side, so map child ids explicitly
            // to drive add/update/delete reconciliation. Null => leave groups untouched.
            var groups = dto.Groups?.Select(g => new OrgEmployeeCategoryGroup
            {
                RecId = g.RecId,
                DepartmentID = g.DepartmentID,
                OccupationID = g.OccupationID,
                UserGroupID = g.UserGroupID,
            }).ToList();

            var updated = await _categories.UpdateWithGroupsAsync(catId, scalars, groups, cancellationToken);
            if (updated == null)
                return NotFound(APIResponse<OrgEmployeeCategoryDto>.Fail("OrgEmployeeCategory not found"));

            return Ok(APIResponse<OrgEmployeeCategoryDto>.Ok(updated.Adapt<OrgEmployeeCategoryDto>(), "Updated successfully"));
        }
    }
}

