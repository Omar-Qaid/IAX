using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/EcoResTrackingDimensionGroup")]
[DomainPermission("Inventory", "Transactions")]
public sealed class EcoResTrackingDimensionGroupController
    : BaseController<EcoResTrackingDimensionGroup, TrackingDimensionGroupDto>
{
    private readonly ITrackingDimensionGroupService _trackingService;

    public EcoResTrackingDimensionGroupController(
        ITrackingDimensionGroupService service,
        ILogger<EcoResTrackingDimensionGroupController> logger)
        : base(service, logger)
    {
        _trackingService = service;
    }

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<TrackingDimensionGroupDto>>>> GetAll(CancellationToken cancellationToken = default) =>
        base.GetAll(cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<TrackingDimensionGroupDto>>> GetById(string id, CancellationToken cancellationToken = default) =>
        base.GetById(id, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<TrackingDimensionGroupDto>>> Create(TrackingDimensionGroupDto dto, CancellationToken cancellationToken = default) =>
        base.Create(dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<TrackingDimensionGroupDto>>> Update(string id, TrackingDimensionGroupDto dto, CancellationToken cancellationToken = default) =>
        base.Update(id, dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default) =>
        base.Delete(id, cancellationToken);

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) =>
        Ok(APIResponse<List<TrackingDimensionGroupDto>>.Ok(await _trackingService.ListAsync(ct)));

    [HttpPost]
    public async Task<IActionResult> Create(TrackingDimensionGroupInputDto input, CancellationToken ct)
    {
        var result = await _trackingService.CreateAsync(input, ct);
        return result.Group is null
            ? Conflict(APIResponse<TrackingDimensionGroupDto>.Fail(result.Error!))
            : Ok(APIResponse<TrackingDimensionGroupDto>.Ok(result.Group));
    }

    [HttpPut("{recId:long}")]
    public async Task<IActionResult> Update(long recId, TrackingDimensionGroupInputDto input, CancellationToken ct)
    {
        var result = await _trackingService.UpdateAsync(recId, input, ct);
        return result.Group is null
            ? BadRequest(APIResponse<TrackingDimensionGroupDto>.Fail(result.Error!))
            : Ok(APIResponse<TrackingDimensionGroupDto>.Ok(result.Group));
    }

    [HttpDelete("{id:long}")]
    public async Task<ActionResult<APIResponse<bool>>> Delete(long id, CancellationToken cancellationToken = default)
    {
        var result = await _trackingService.DeleteAsync(id, cancellationToken);
        return result.Deleted
            ? Ok(APIResponse<bool>.Ok(true))
            : Conflict(APIResponse<bool>.Fail(result.Error!));
    }
}
