using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.Organization.Features.HcmWorkerCategory;
using IAX.IXApi.Modules.Identity.Permissions;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Organization.Features.HcmWorkerCategory
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("SystemAdministration", "UserCategories")]
    public class HcmWorkerCategoryController : BaseController<HcmWorkerCategory, HcmWorkerCategoryDto>
    {
        private readonly IHcmWorkerCategoryService _categories;

        public HcmWorkerCategoryController(IHcmWorkerCategoryService service, ILogger<HcmWorkerCategoryController> logger) : base(service, logger)
        {
            _categories = service;
        }

        /// <summary>Fetch a single category including its linkage Groups.</summary>
        public override async Task<ActionResult<APIResponse<HcmWorkerCategoryDto>>> GetById(string id, CancellationToken cancellationToken = default)
        {
            if (!long.TryParse(id, out var catId))
                return NotFound(APIResponse<HcmWorkerCategoryDto>.Fail("HcmWorkerCategory not found"));

            var entity = await _categories.GetWithGroupsAsync(catId, cancellationToken);
            if (entity == null)
                return NotFound(APIResponse<HcmWorkerCategoryDto>.Fail("HcmWorkerCategory not found"));

            return Ok(APIResponse<HcmWorkerCategoryDto>.Ok(entity.Adapt<HcmWorkerCategoryDto>()));
        }

        /// <summary>Create a category together with its linkage Groups (inserted as a graph).</summary>
        public override async Task<ActionResult<APIResponse<HcmWorkerCategoryDto>>> Create([FromBody] HcmWorkerCategoryDto dto, CancellationToken cancellationToken = default)
        {
            var entity = dto.Adapt<HcmWorkerCategory>(); // scalars + Groups (new rows; ids ignored on destination)
            var created = await _service.AddAsync(entity, cancellationToken);
            var result = await _categories.GetWithGroupsAsync(created.RecId, cancellationToken) ?? created;
            return Ok(APIResponse<HcmWorkerCategoryDto>.Ok(result.Adapt<HcmWorkerCategoryDto>(), "Created successfully"));
        }

        /// <summary>Update scalars and reconcile linkage Groups (add / update / delete).</summary>
        public override async Task<ActionResult<APIResponse<HcmWorkerCategoryDto>>> Update(string id, [FromBody] HcmWorkerCategoryDto dto, CancellationToken cancellationToken = default)
        {
            if (!long.TryParse(id, out var catId))
                return NotFound(APIResponse<HcmWorkerCategoryDto>.Fail("HcmWorkerCategory not found"));

            var scalars = dto.Adapt<HcmWorkerCategory>();
            // Mapster ignores the entity Id on the destination side, so map child ids explicitly
            // to drive add/update/delete reconciliation. Null => leave groups untouched.
            var groups = dto.Groups?.Select(g => new HcmWorkerCategoryGroup
            {
                RecId = g.RecId,
                DepartmentID = g.DepartmentID,
                OccupationID = g.OccupationID,
                UserGroupID = g.UserGroupID,
            }).ToList();

            var updated = await _categories.UpdateWithGroupsAsync(catId, scalars, groups, cancellationToken);
            if (updated == null)
                return NotFound(APIResponse<HcmWorkerCategoryDto>.Fail("HcmWorkerCategory not found"));

            return Ok(APIResponse<HcmWorkerCategoryDto>.Ok(updated.Adapt<HcmWorkerCategoryDto>(), "Updated successfully"));
        }
    }
}

