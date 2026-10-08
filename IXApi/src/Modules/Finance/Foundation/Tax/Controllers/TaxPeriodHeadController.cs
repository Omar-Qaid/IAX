using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Modules.Finance.Persistence;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Identity.Permissions;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax
{
    [ApiController]
    [Route("api/v1/TaxPeriodHead")]
    [Route("api/TaxPeriodHead")]
    [Route("api/v1/TaxPeriod")]
    [Route("api/TaxPeriod")]
    [DomainPermission("Tax", "SettlementPeriods")]
    public class TaxPeriodHeadController : BaseController<TaxPeriodHead, TaxPeriodHeadDto>
    {
        private readonly TaxPeriodService _periods;

        public TaxPeriodHeadController(ITaxPeriodHeadCrudService service, TaxPeriodService periods, ILogger<TaxPeriodHeadController> logger)
            : base(service, logger)
        {
            _periods = periods;
        }

        [HttpGet]
        public override async Task<ActionResult<APIResponse<IEnumerable<TaxPeriodHeadDto>>>> GetAll(CancellationToken cancellationToken = default)
            => Ok(APIResponse<IEnumerable<TaxPeriodHeadDto>>.Ok(await _periods.GetAllAsync(cancellationToken)));

        [HttpGet("{id}")]
        public override async Task<ActionResult<APIResponse<TaxPeriodHeadDto>>> GetById(string id, CancellationToken cancellationToken = default)
        {
            var dto = await _periods.GetByIdAsync(id, cancellationToken);
            return dto == null ? NotFound(APIResponse<TaxPeriodHeadDto>.Fail("TaxPeriodHead not found"))
                : Ok(APIResponse<TaxPeriodHeadDto>.Ok(dto));
        }

        [HttpPost]
        public override async Task<ActionResult<APIResponse<TaxPeriodHeadDto>>> Create([FromBody] TaxPeriodHeadDto dto, CancellationToken cancellationToken = default)
        {
            var created = await _periods.CreateAsync(dto, cancellationToken);
            return await GetById(created.TaxPeriod, cancellationToken);
        }

        [HttpPut("{id}")]
        public override async Task<ActionResult<APIResponse<TaxPeriodHeadDto>>> Update(string id, [FromBody] TaxPeriodHeadDto dto, CancellationToken cancellationToken = default)
        {
            var updated = await _periods.UpdateAsync(id, dto, cancellationToken);
            return updated == null ? NotFound(APIResponse<TaxPeriodHeadDto>.Fail("Sales tax settlement period not found"))
                : await GetById(updated.TaxPeriod, cancellationToken);
        }

        [HttpDelete("{id}")]
        public override async Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default)
            => await _periods.DeleteAsync(id, cancellationToken)
                ? Ok(APIResponse<bool>.Ok(true))
                : NotFound(APIResponse<bool>.Fail("Sales tax settlement period not found"));
    }
}
