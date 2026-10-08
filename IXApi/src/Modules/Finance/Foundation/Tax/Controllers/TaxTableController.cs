using System;
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
    [Route("api/v1/[controller]")]
    [Route("api/[controller]")]
    [Route("api/v1/SalesTaxCode")]
    [Route("api/SalesTaxCode")]
    [DomainPermission("Tax", "TaxCodes")]
    public class TaxTableController : BaseController<TaxTable, TaxTableDto>
    {
        private readonly ITaxTableService _taxService;

        public TaxTableController(ITaxTableService service, ILogger<TaxTableController> logger)
            : base(service, logger) => _taxService = service;

        [HttpGet]
        public override async Task<ActionResult<APIResponse<IEnumerable<TaxTableDto>>>> GetAll(CancellationToken cancellationToken = default)
            => Ok(APIResponse<IEnumerable<TaxTableDto>>.Ok(await _taxService.GetDetailsAsync(cancellationToken)));

        [HttpGet("{id}")]
        public override async Task<ActionResult<APIResponse<TaxTableDto>>> GetById(string id, CancellationToken cancellationToken = default)
        {
            var result = await _taxService.GetDetailAsync(id, cancellationToken);
            return result == null ? NotFound(APIResponse<TaxTableDto>.Fail("Sales tax code not found")) : Ok(APIResponse<TaxTableDto>.Ok(result));
        }

        [HttpPost]
        public override async Task<ActionResult<APIResponse<TaxTableDto>>> Create([FromBody] TaxTableDto dto, CancellationToken cancellationToken = default)
        {
            var result = await _taxService.CreateFromDtoAsync(dto, cancellationToken);
            return result == null ? NotFound(APIResponse<TaxTableDto>.Fail("Sales tax code not found")) : Ok(APIResponse<TaxTableDto>.Ok(result));
        }

        [HttpPut("{id}")]
        public override async Task<ActionResult<APIResponse<TaxTableDto>>> Update(string id, [FromBody] TaxTableDto dto, CancellationToken cancellationToken = default)
        {
            var result = await _taxService.UpdateFromDtoAsync(id, dto, cancellationToken);
            return result == null ? NotFound(APIResponse<TaxTableDto>.Fail("Sales tax code not found")) : Ok(APIResponse<TaxTableDto>.Ok(result));
        }

        [HttpDelete("{id}")]
        public override async Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default)
            => await _taxService.DeleteByCodeAsync(id, cancellationToken)
                ? Ok(APIResponse<bool>.Ok(true)) : NotFound(APIResponse<bool>.Fail("Sales tax code not found"));
    }
}
