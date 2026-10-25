using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/InventLocation")]
[DomainPermission("Inventory", "Transactions")]
public sealed class InventLocationController : BaseController<InventLocation, InventLocationResponseDto>
{
    private readonly IInventLocationService _locations;

    public InventLocationController(
        IInventLocationService locations,
        ILogger<InventLocationController>? logger = null)
        : base(locations, logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<InventLocationController>.Instance)
    {
        _locations = locations;
    }

    public sealed class LocationInput : LocationInputDto { }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
        => Ok(APIResponse<object>.Ok(await _locations.ListAsync(ct)));

    [HttpGet("lookups")]
    public async Task<IActionResult> Lookups(CancellationToken ct)
        => Ok(APIResponse<object>.Ok(await _locations.LookupsAsync(ct)));

    [HttpPost]
    public async Task<IActionResult> Create(LocationInput input, CancellationToken ct)
    {
        var result = await _locations.CreateAsync(input, ct);
        return result.Status switch
        {
            LocationOperationStatus.Duplicate => Conflict(APIResponse<object>.Fail("A warehouse with this ID already exists.")),
            LocationOperationStatus.SiteNotFound => UnprocessableEntity(APIResponse<object>.Fail("The selected site was not found.")),
            LocationOperationStatus.InvalidReference => UnprocessableEntity(APIResponse<object>.Fail(result.Error!)),
            _ => Ok(APIResponse<object>.Ok(result.Location!))
        };
    }

    [HttpPut("{recId:long}")]
    public async Task<IActionResult> Update(long recId, LocationInput input, CancellationToken ct)
    {
        var result = await _locations.UpdateAsync(recId, input, ct);
        return result.Status switch
        {
            LocationOperationStatus.NotFound => NotFound(APIResponse<object>.Fail("Warehouse was not found.")),
            LocationOperationStatus.CodeChanged => UnprocessableEntity(APIResponse<object>.Fail("Warehouse ID cannot be changed after creation.")),
            LocationOperationStatus.SiteNotFound => UnprocessableEntity(APIResponse<object>.Fail("The selected site was not found.")),
            LocationOperationStatus.InvalidReference => UnprocessableEntity(APIResponse<object>.Fail(result.Error!)),
            _ => Ok(APIResponse<object>.Ok(result.Location!))
        };
    }

    [HttpDelete("{recId:long}")]
    public async Task<IActionResult> Delete(long recId, CancellationToken ct)
    {
        var status = await _locations.DeleteAsync(recId, ct);
        return status switch
        {
            LocationOperationStatus.NotFound => NotFound(APIResponse<object>.Fail("Warehouse was not found.")),
            LocationOperationStatus.Referenced => Conflict(APIResponse<object>.Fail("This warehouse is referenced by inventory setup or documents.")),
            _ => Ok(APIResponse<bool>.Ok(true))
        };
    }

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<InventLocationResponseDto>>>> GetAll(CancellationToken cancellationToken = default) =>
        base.GetAll(cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<InventLocationResponseDto>>>> GetPaged([FromQuery] QueryFilterDto paginationParams, CancellationToken cancellationToken = default) =>
        base.GetPaged(paginationParams, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventLocationResponseDto>>> GetById(string id, CancellationToken cancellationToken = default) =>
        base.GetById(id, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventLocationResponseDto>>> Create(InventLocationResponseDto dto, CancellationToken cancellationToken = default) =>
        base.Create(dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventLocationResponseDto>>> Update(string id, InventLocationResponseDto dto, CancellationToken cancellationToken = default) =>
        base.Update(id, dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default) =>
        base.Delete(id, cancellationToken);
}
