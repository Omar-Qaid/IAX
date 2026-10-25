using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/InventSerial")]
[DomainPermission("Inventory", "Transactions")]
public sealed class InventSerialController : BaseController<InventSerial, InventSerialRecordDto>
{
    private readonly IInventSerialService _serialService;

    public InventSerialController(
        IInventSerialService service,
        ILogger<InventSerialController>? logger = null)
        : base(service, logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<InventSerialController>.Instance)
    {
        _serialService = service;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? itemId, CancellationToken ct) =>
        Ok(APIResponse<IReadOnlyList<InventSerialRecordDto>>.Ok(await _serialService.ListAsync(itemId, ct)));

    [HttpPost]
    public async Task<IActionResult> Create(InventSerialInputDto input, CancellationToken ct)
    {
        var result = await _serialService.CreateAsync(input, ct);
        return result.Serial is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!))
            : Ok(APIResponse<InventSerialRecordDto>.Ok(result.Serial));
    }

    [HttpPut("{recId:long}")]
    public async Task<IActionResult> Update(long recId, InventSerialInputDto input, CancellationToken ct)
    {
        var result = await _serialService.UpdateAsync(recId, input, ct);
        if (result.Error == "Serial was not found.") return NotFound(APIResponse<object>.Fail(result.Error));
        return result.Serial is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!))
            : Ok(APIResponse<InventSerialRecordDto>.Ok(result.Serial));
    }

    [HttpDelete("{recId:long}")]
    public async Task<IActionResult> Delete(long recId, CancellationToken ct)
    {
        var error = await _serialService.DeleteAsync(recId, ct);
        if (error == "Serial was not found.") return NotFound(APIResponse<object>.Fail(error));
        return error is null ? Ok(APIResponse<bool>.Ok(true)) : Conflict(APIResponse<object>.Fail(error));
    }

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<InventSerialRecordDto>>>> GetAll(CancellationToken cancellationToken = default) =>
        base.GetAll(cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<InventSerialRecordDto>>>> GetPaged([FromQuery] QueryFilterDto paginationParams, CancellationToken cancellationToken = default) =>
        base.GetPaged(paginationParams, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventSerialRecordDto>>> GetById(string id, CancellationToken cancellationToken = default) =>
        base.GetById(id, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventSerialRecordDto>>> Create(InventSerialRecordDto dto, CancellationToken cancellationToken = default) =>
        base.Create(dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<InventSerialRecordDto>>> Update(string id, InventSerialRecordDto dto, CancellationToken cancellationToken = default) =>
        base.Update(id, dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default) =>
        base.Delete(id, cancellationToken);
}
