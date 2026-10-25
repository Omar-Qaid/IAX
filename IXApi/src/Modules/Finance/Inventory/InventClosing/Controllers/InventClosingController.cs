using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/inventory-costing/closings")]
[DomainPermission("Inventory", "Transactions", "View")]
public sealed class InventClosingController : BaseController<InventClosing, InventClosingRecordDto>
{
    private readonly IInventClosingQueryService _closingService;

    public InventClosingController(
        IInventClosingQueryService service,
        ILogger<InventClosingController>? logger = null)
        : base(service, logger ?? NullLogger<InventClosingController>.Instance)
    {
        _closingService = service;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) =>
        Ok(APIResponse<IReadOnlyList<InventClosingRecordDto>>.Ok(await _closingService.ListAsync(ct)));

    [HttpGet("{recId:long}")]
    public async Task<IActionResult> Get(long recId, CancellationToken ct)
    {
        var closing = await _closingService.GetAsync(recId, ct);
        return closing is null
            ? NotFound(APIResponse<object>.Fail("Inventory closing was not found."))
            : Ok(APIResponse<InventClosingRecordDto>.Ok(closing));
    }

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<InventClosingRecordDto>>>> GetAll(CancellationToken cancellationToken = default) =>
        base.GetAll(cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<InventClosingRecordDto>>>> GetPaged([FromQuery] QueryFilterDto paginationParams, CancellationToken cancellationToken = default) =>
        base.GetPaged(paginationParams, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventClosingRecordDto>>> GetById(string id, CancellationToken cancellationToken = default) =>
        base.GetById(id, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventClosingRecordDto>>> Create(InventClosingRecordDto dto, CancellationToken cancellationToken = default) =>
        base.Create(dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventClosingRecordDto>>> Update(string id, InventClosingRecordDto dto, CancellationToken cancellationToken = default) =>
        base.Update(id, dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default) =>
        base.Delete(id, cancellationToken);
}
