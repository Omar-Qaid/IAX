using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Administration.NumberSequences
{
    [Area("System")]
    [Route("api/v1/[controller]")]
[DomainPermission("System", "NumberSequences")]
    public class SysNumberSequenceController : BaseController<SysNumberSequence, SysNumberSequenceDto>
    {
        private readonly ISysNumberSequenceService _seqService;

        public SysNumberSequenceController(ISysNumberSequenceService service, ILogger<SysNumberSequenceController> logger)
            : base(service, logger)
        {
            _seqService = service;
        }

        /// <summary>Reserve the next sequence value (consumes it).</summary>
        [HttpPost("next")]
        public async Task<ActionResult<APIResponse<NextSequenceResultDto>>> Next([FromBody] NextSequenceRequestDto request, CancellationToken ct)
        {
            var result = await _seqService.NextAsync(request.EntityName, request.TenantId, ct);
            return Ok(APIResponse<NextSequenceResultDto>.Ok(result));
        }

        /// <summary>Peek at the upcoming code without consuming it.</summary>
        [HttpGet("peek")]
        public async Task<ActionResult<APIResponse<NextSequenceResultDto>>> Peek([FromQuery] string entityName, [FromQuery] string? tenantId, CancellationToken ct)
        {
            var result = await _seqService.PeekAsync(entityName, tenantId, ct);
            if (result == null) return NotFound(APIResponse<NextSequenceResultDto>.Fail("Sequence not configured for entity"));
            return Ok(APIResponse<NextSequenceResultDto>.Ok(result));
        }

        /// <summary>Reset a sequence (optionally to a specific value).</summary>
        [HttpPost("{id:int}/reset")]
        public async Task<ActionResult<APIResponse<SysNumberSequenceDto>>> Reset(int id, [FromBody] ResetSequenceRequestDto request, CancellationToken ct)
        {
            var entity = await _seqService.ResetAsync(id, request?.NextValue, ct);
            return Ok(APIResponse<SysNumberSequenceDto>.Ok(Mapster.TypeAdapter.Adapt<SysNumberSequenceDto>(entity), "Reset successful"));
        }
    }
}
