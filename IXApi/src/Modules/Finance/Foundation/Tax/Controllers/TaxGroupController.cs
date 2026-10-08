using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;


namespace IAX.IXApi.Modules.Finance.Foundation.Tax
{
    [ApiController]
    [Route("api/v1/TaxGroup")]
    [Route("api/TaxGroup")]
    [Route("api/v1/SalesTaxGroup")]
    [Route("api/SalesTaxGroup")]
    [DomainPermission("Tax", "TaxGroups")]
    public class TaxGroupController : BaseController<TaxGroupHeading, TaxGroupDto>
    {
        private readonly ITaxGroupQueryService _queries;
        private readonly ITaxGroupLineService _lines;
        private readonly ITaxGroupCommandService _commands;

        public TaxGroupController(ITaxGroupService service, ITaxGroupQueryService queries, ITaxGroupLineService lines, ITaxGroupCommandService commands, ILogger<TaxGroupController> logger)
            : base(service, logger)
        {
            _queries = queries;
            _lines = lines;
            _commands = commands;
        }

        [HttpGet]
        public override async Task<ActionResult<APIResponse<IEnumerable<TaxGroupDto>>>> GetAll(CancellationToken cancellationToken = default)
            => Ok(APIResponse<IEnumerable<TaxGroupDto>>.Ok(await _queries.GetAllAsync(cancellationToken)));

        [HttpGet("{id}")]
        public override async Task<ActionResult<APIResponse<TaxGroupDto>>> GetById(string id, CancellationToken cancellationToken = default)
        {
            var dto = await _queries.GetByIdAsync(id, cancellationToken);
            return dto == null
                ? NotFound(APIResponse<TaxGroupDto>.Fail("Tax group not found"))
                : Ok(APIResponse<TaxGroupDto>.Ok(dto));
        }

        [HttpPost]
        public override async Task<ActionResult<APIResponse<TaxGroupDto>>> Create([FromBody] TaxGroupDto dto, CancellationToken cancellationToken = default)
        {
            var created = await _commands.CreateAsync(dto, cancellationToken);
            return await GetById(created.RecId.ToString(), cancellationToken);
        }

        [HttpPut("{id}")]
        public override async Task<ActionResult<APIResponse<TaxGroupDto>>> Update(string id, [FromBody] TaxGroupDto dto, CancellationToken cancellationToken = default)
        {
            var updated = await _commands.UpdateAsync(id, dto, cancellationToken);
            return updated == null
                ? NotFound(APIResponse<TaxGroupDto>.Fail("Sales tax group not found"))
                : await GetById(updated.RecId.ToString(), cancellationToken);
        }

        [HttpPost("{id}/lines")]
        public async Task<IActionResult> AddLine(string id, [FromBody] TaxGroupDataDto lineDto)
        {
            var result = await _lines.AddLineAsync(id, lineDto);
            if (result.Line == null) return NotFound("Tax group not found");
            return Ok(APIResponse<TaxGroupDataDto>.Ok(result.Line,
                result.Updated ? "Line updated successfully" : "Line added successfully"));
        }

        [HttpDelete("lines/{lineId}")]
        public async Task<IActionResult> DeleteLine(long lineId)
            => await _lines.DeleteLineAsync(lineId) ? Ok() : NotFound();
    }
}
