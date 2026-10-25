using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using InventParametersEntity = IAX.IXApi.Modules.Finance.Entities.InventParameters;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/InventParameters")]
[DomainPermission("Inventory", "Transactions")]
public sealed class InventParametersController : BaseController<InventParametersEntity, InventParametersDto>
{
    private readonly IInventParametersService _parametersService;

    public InventParametersController(
        IInventParametersService service,
        ILogger<InventParametersController>? logger = null)
        : base(service, logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<InventParametersController>.Instance)
    {
        _parametersService = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var value = await _parametersService.GetAsync(ct);
        return value is null ? BadRequest(APIResponse<object>.Fail("A company must be selected.")) : Ok(APIResponse<InventParametersDto>.Ok(value));
    }

    [HttpGet("lookups")]
    public async Task<IActionResult> Lookups(CancellationToken ct)
    {
        var value = await _parametersService.GetLookupsAsync(ct);
        return value is null ? BadRequest(APIResponse<object>.Fail("A company must be selected.")) : Ok(APIResponse<InventParametersLookupsDto>.Ok(value));
    }

    [HttpGet("readiness")]
    public async Task<IActionResult> Readiness(CancellationToken ct)
    {
        var value = await _parametersService.GetReadinessAsync(ct);
        return value is null ? BadRequest(APIResponse<object>.Fail("A company must be selected.")) : Ok(APIResponse<InventoryReadinessDto>.Ok(value));
    }

    [HttpPut]
    public async Task<IActionResult> Save(InventParametersDto input, CancellationToken ct)
    {
        var result = await _parametersService.SaveAsync(input, ct);
        return result.Value is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!)) : Ok(APIResponse<InventParametersDto>.Ok(result.Value));
    }

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<InventParametersDto>>>> GetAll(CancellationToken cancellationToken = default) =>
        base.GetAll(cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<InventParametersDto>>>> GetPaged([FromQuery] QueryFilterDto paginationParams, CancellationToken cancellationToken = default) =>
        base.GetPaged(paginationParams, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventParametersDto>>> GetById(string id, CancellationToken cancellationToken = default) =>
        base.GetById(id, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventParametersDto>>> Create(InventParametersDto dto, CancellationToken cancellationToken = default) =>
        base.Create(dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventParametersDto>>> Update(string id, InventParametersDto dto, CancellationToken cancellationToken = default) =>
        base.Update(id, dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default) =>
        base.Delete(id, cancellationToken);
}
