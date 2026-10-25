using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/InventItemLocation")]
[DomainPermission("Inventory", "Transactions")]
public sealed class InventItemLocationController
    : BaseController<InventItemLocation, InventItemLocationDto>
{
    private readonly IInventItemLocationService _itemLocationService;

    public InventItemLocationController(
        IInventItemLocationService service,
        ILogger<InventItemLocationController> logger)
        : base(service, logger)
    {
        _itemLocationService = service;
    }

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<InventItemLocationDto>>>> GetAll(CancellationToken cancellationToken = default) =>
        base.GetAll(cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventItemLocationDto>>> GetById(string id, CancellationToken cancellationToken = default) =>
        base.GetById(id, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventItemLocationDto>>> Create(InventItemLocationDto dto, CancellationToken cancellationToken = default) =>
        base.Create(dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventItemLocationDto>>> Update(string id, InventItemLocationDto dto, CancellationToken cancellationToken = default) =>
        base.Update(id, dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default) =>
        base.Delete(id, cancellationToken);

    [HttpPost]
    public async Task<IActionResult> Create(InventItemLocationInputDto input, CancellationToken ct)
    {
        var result = await _itemLocationService.CreateAsync(input, ct);
        return result.Location is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!))
            : Ok(APIResponse<ItemLocationRecordDto>.Ok(result.Location));
    }

    [HttpPut("{recId:long}")]
    public async Task<IActionResult> Update(long recId, InventItemLocationInputDto input, CancellationToken ct)
    {
        var result = await _itemLocationService.UpdateAsync(recId, input, ct);
        if (result.Error == "Item location was not found.") return NotFound(APIResponse<object>.Fail(result.Error));
        return result.Location is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!))
            : Ok(APIResponse<ItemLocationRecordDto>.Ok(result.Location));
    }

    [HttpDelete("{id:long}")]
    public async Task<ActionResult<APIResponse<bool>>> Delete(long id, CancellationToken cancellationToken = default)
    {
        var error = await _itemLocationService.DeleteAsync(id, cancellationToken);
        return error is null ? Ok(APIResponse<bool>.Ok(true)) : NotFound(APIResponse<bool>.Fail(error));
    }
}
