using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/WMSLocation")]
[DomainPermission("Inventory", "Transactions")]
public sealed class WMSLocationController
    : BaseController<WMSLocation, WMSLocationDto>
{
    private readonly IWMSLocationService _wmsLocationService;

    public WMSLocationController(
        IWMSLocationService service,
        ILogger<WMSLocationController> logger)
        : base(service, logger)
    {
        _wmsLocationService = service;
    }

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<WMSLocationDto>>>> GetAll(CancellationToken cancellationToken = default) =>
        base.GetAll(cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<WMSLocationDto>>> GetById(string id, CancellationToken cancellationToken = default) =>
        base.GetById(id, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<WMSLocationDto>>> Create(WMSLocationDto dto, CancellationToken cancellationToken = default) =>
        base.Create(dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<WMSLocationDto>>> Update(string id, WMSLocationDto dto, CancellationToken cancellationToken = default) =>
        base.Update(id, dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default) =>
        base.Delete(id, cancellationToken);

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) =>
        Ok(APIResponse<object>.Ok(await _wmsLocationService.ListAsync(ct)));

    [HttpPost]
    public async Task<IActionResult> Create(WMSLocationInputDto input, CancellationToken ct)
    {
        var result = await _wmsLocationService.CreateAsync(input, ct);
        return result.Error == null
            ? Ok(APIResponse<object>.Ok(result.Location!))
            : UnprocessableEntity(APIResponse<object>.Fail(result.Error));
    }

    [HttpPut("{recId:long}")]
    public async Task<IActionResult> Update(long recId, WMSLocationInputDto input, CancellationToken ct)
    {
        var result = await _wmsLocationService.UpdateAsync(recId, input, ct);
        if (result.Error == "Inventory location was not found.")
            return NotFound(APIResponse<object>.Fail(result.Error));
        return result.Error == null
            ? Ok(APIResponse<object>.Ok(result.Location!))
            : UnprocessableEntity(APIResponse<object>.Fail(result.Error));
    }

    [HttpDelete("{id:long}")]
    public async Task<ActionResult<APIResponse<bool>>> Delete(long id, CancellationToken cancellationToken = default)
    {
        var error = await _wmsLocationService.DeleteAsync(id, cancellationToken);
        if (error == "Inventory location was not found.")
            return NotFound(APIResponse<bool>.Fail(error));
        return error == null
            ? Ok(APIResponse<bool>.Ok(true))
            : Conflict(APIResponse<bool>.Fail(error));
    }
}
