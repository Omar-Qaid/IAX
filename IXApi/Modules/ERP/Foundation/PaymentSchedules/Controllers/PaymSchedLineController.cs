using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mapster;

namespace IAX.IXApi.Modules.ERP.Shared.Features
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("AccountsReceivable", "PaymSched")]
    public class PaymSchedLineController : BaseController<PaymSchedLine, PaymSchedLineDto>
    {
        private readonly IPaymSchedLineService _lineService;

        public PaymSchedLineController(IPaymSchedLineService service, ILogger<PaymSchedLineController> logger)
            : base(service, logger)
        {
            _lineService = service;
        }

        /// <summary>
        /// Override Create to detach the navigation property before EF Core tracks the entity.
        /// PaymSchedLine uses an identifying FK (Name → PaymSched.Name via alternate key),
        /// so Mapster must not populate the navigation; only the scalar FK (Name) is needed.
        /// </summary>
        [HttpPost]
        public override async Task<ActionResult<APIResponse<PaymSchedLineDto>>> Create(
            [FromBody] PaymSchedLineDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{EntityName}] - Creating new record", _entityName);

            var entity = dto.Adapt<PaymSchedLine>();
            // Clear the navigation so EF doesn't try to attach/track the principal
            entity.PaymSchedTable = null;
            await OnBeforeCreateAsync(entity);

            var createdEntity = await _service.AddAsync(entity, cancellationToken);
            var resultDto = createdEntity.Adapt<PaymSchedLineDto>();
            await OnAfterCreateAsync(resultDto);
            return Ok(APIResponse<PaymSchedLineDto>.Ok(resultDto, "Created successfully"));
        }
    }
}
