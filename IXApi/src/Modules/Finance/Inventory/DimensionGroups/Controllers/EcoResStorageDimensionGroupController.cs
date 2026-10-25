using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/EcoResStorageDimensionGroup")]
[DomainPermission("Inventory", "Transactions")]
public sealed class EcoResStorageDimensionGroupController
    : BaseController<EcoResStorageDimensionGroup, StorageDimensionGroupDto>
{
    private readonly IStorageDimensionGroupService _storageService;

    public EcoResStorageDimensionGroupController(
        IStorageDimensionGroupService service,
        ILogger<EcoResStorageDimensionGroupController> logger)
        : base(service, logger)
    {
        _storageService = service;
    }

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<StorageDimensionGroupDto>>>> GetAll(CancellationToken cancellationToken = default) =>
        base.GetAll(cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<StorageDimensionGroupDto>>> GetById(string id, CancellationToken cancellationToken = default) =>
        base.GetById(id, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<StorageDimensionGroupDto>>> Create(StorageDimensionGroupDto dto, CancellationToken cancellationToken = default) =>
        base.Create(dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<StorageDimensionGroupDto>>> Update(string id, StorageDimensionGroupDto dto, CancellationToken cancellationToken = default) =>
        base.Update(id, dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default) =>
        base.Delete(id, cancellationToken);

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) =>
        Ok(APIResponse<List<StorageDimensionGroupDto>>.Ok(await _storageService.ListAsync(ct)));

    [HttpPost]
    public async Task<IActionResult> Create(StorageDimensionGroupInputDto input, CancellationToken ct)
    {
        var result = await _storageService.CreateAsync(input, ct);
        return result.Group is null
            ? Conflict(APIResponse<StorageDimensionGroupDto>.Fail(result.Error!))
            : Ok(APIResponse<StorageDimensionGroupDto>.Ok(result.Group));
    }

    [HttpPut("{recId:long}")]
    public async Task<IActionResult> Update(long recId, StorageDimensionGroupInputDto input, CancellationToken ct)
    {
        var result = await _storageService.UpdateAsync(recId, input, ct);
        return result.Group is null
            ? BadRequest(APIResponse<StorageDimensionGroupDto>.Fail(result.Error!))
            : Ok(APIResponse<StorageDimensionGroupDto>.Ok(result.Group));
    }

    [HttpDelete("{id:long}")]
    public async Task<ActionResult<APIResponse<bool>>> Delete(long id, CancellationToken cancellationToken = default)
    {
        var result = await _storageService.DeleteAsync(id, cancellationToken);
        return result.Deleted
            ? Ok(APIResponse<bool>.Ok(true))
            : Conflict(APIResponse<bool>.Fail(result.Error!));
    }
}
