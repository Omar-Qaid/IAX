using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
    [Route("api/v1/TaxItemGroup")]
    [Route("api/TaxItemGroup")]
    [Route("api/v1/ItemSalesTaxGroup")]
    [Route("api/ItemSalesTaxGroup")]
    [DomainPermission("Tax", "ItemTaxGroups")]
    public class TaxItemGroupController : BaseController<TaxItemGroupHeading, TaxItemGroupDto>
    {
        private readonly ITaxItemGroupQueryService _queries;
        private readonly ITaxItemGroupLineService _lines;
        private readonly ITaxItemGroupCommandService _commands;

        public TaxItemGroupController(ITaxItemGroupService service, ITaxItemGroupQueryService queries, ITaxItemGroupLineService lines, ITaxItemGroupCommandService commands, ILogger<TaxItemGroupController> logger)
            : base(service, logger)
        {
            _queries = queries;
            _lines = lines;
            _commands = commands;
        }

        [HttpGet]
        public override async Task<ActionResult<APIResponse<IEnumerable<TaxItemGroupDto>>>> GetAll(CancellationToken cancellationToken = default)
            => Ok(APIResponse<IEnumerable<TaxItemGroupDto>>.Ok(await _queries.GetAllAsync(cancellationToken)));

        [HttpGet("{id}")]
        public override async Task<ActionResult<APIResponse<TaxItemGroupDto>>> GetById(string id, CancellationToken cancellationToken = default)
        {
            var dto = await _queries.GetByIdAsync(id, cancellationToken);
            return dto == null
                ? NotFound(APIResponse<TaxItemGroupDto>.Fail("Item sales tax group not found"))
                : Ok(APIResponse<TaxItemGroupDto>.Ok(dto));
        }

        [HttpPost]
        public override async Task<ActionResult<APIResponse<TaxItemGroupDto>>> Create([FromBody] TaxItemGroupDto dto, CancellationToken cancellationToken = default)
        {
            var created = await _commands.CreateAsync(dto, cancellationToken);
            return await GetById(created.TaxItemGroup, cancellationToken);
        }

        [HttpPut("{id}")]
        public override async Task<ActionResult<APIResponse<TaxItemGroupDto>>> Update(string id, [FromBody] TaxItemGroupDto dto, CancellationToken cancellationToken = default)
        {
            var updated = await _commands.UpdateAsync(id, dto, cancellationToken);
            return updated == null
                ? NotFound(APIResponse<TaxItemGroupDto>.Fail("Item sales tax group not found"))
                : await GetById(updated.TaxItemGroup, cancellationToken);
        }

        [HttpPost("{id}/lines")]
        public async Task<IActionResult> AddLine(string id, [FromBody] TaxOnItemDto lineDto)
        {
            var result = await _lines.AddLineAsync(id, lineDto);
            return result == null
                ? NotFound("Item sales tax group not found")
                : Ok(APIResponse<TaxOnItemDto>.Ok(result));
        }

        [HttpDelete("lines/{lineId}")]
        public async Task<IActionResult> DeleteLine(long lineId)
            => await _lines.DeleteLineAsync(lineId) ? Ok() : NotFound();
    }
}
