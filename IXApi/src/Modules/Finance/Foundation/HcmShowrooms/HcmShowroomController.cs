using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Application.Contracts;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Foundation.HcmShowrooms
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("Organization", "Showrooms")]
    public class HcmShowroomController : BaseController<HcmShowroom, HcmShowroomDto>
    {
        private readonly IHcmShowroomService _showroomService;

        public HcmShowroomController(IHcmShowroomService service, ILogger<HcmShowroomController> logger) : base(service, logger)
        {
            _showroomService = service;
        }

        protected override string[]? GetDefaultIncludes() => new[] { "PartyTable" };

        public override async Task<ActionResult<APIResponse<HcmShowroomDto>>> Create(
            HcmShowroomDto dto,
            CancellationToken cancellationToken = default)
        {
            var entity = dto.Adapt<HcmShowroom>();
            var created = await _showroomService.AddShowroomAsync(
                entity,
                dto.Name ?? string.Empty,
                dto.NameAlias,
                cancellationToken);
            var result = await ReloadWithDefaultsAsync(created.RecId, cancellationToken) ?? created;
            return Ok(APIResponse<HcmShowroomDto>.Ok(result.Adapt<HcmShowroomDto>(), "Created successfully"));
        }

        public override async Task<ActionResult<APIResponse<HcmShowroomDto>>> Update(
            string id,
            HcmShowroomDto dto,
            CancellationToken cancellationToken = default)
        {
            var existing = await _showroomService.GetByIdAsync(id, cancellationToken);
            if (existing == null)
                return NotFound(APIResponse<HcmShowroomDto>.Fail("HcmShowroom not found"));

            dto.Adapt(existing);
            var updated = await _showroomService.UpdateShowroomAsync(
                existing,
                dto.Name ?? string.Empty,
                dto.NameAlias,
                cancellationToken);
            var result = await ReloadWithDefaultsAsync(id, cancellationToken) ?? updated;
            return Ok(APIResponse<HcmShowroomDto>.Ok(result.Adapt<HcmShowroomDto>(), "Updated successfully"));
        }
    }
}
