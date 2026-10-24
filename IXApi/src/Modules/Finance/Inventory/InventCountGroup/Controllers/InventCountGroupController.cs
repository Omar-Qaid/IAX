using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/InventCountGroup")]
[DomainPermission("Inventory", "Transactions")]
public sealed class InventCountGroupController(IInventCountGroupService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) =>
        Ok(APIResponse<IReadOnlyList<InventCountGroupRecordDto>>.Ok(await service.ListAsync(ct)));

    [HttpGet("{recId:long}")]
    public async Task<IActionResult> Get(long recId, CancellationToken ct)
    {
        var group = await service.GetAsync(recId, ct);
        return group is null ? NotFound(APIResponse<object>.Fail("Count group was not found."))
            : Ok(APIResponse<InventCountGroupRecordDto>.Ok(group));
    }

    [HttpPost]
    public async Task<IActionResult> Create(InventCountGroupInputDto input, CancellationToken ct)
    {
        var result = await service.CreateAsync(input, ct);
        return result.Group is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!))
            : Ok(APIResponse<InventCountGroupRecordDto>.Ok(result.Group));
    }

    [HttpPut("{recId:long}")]
    public async Task<IActionResult> Update(long recId, InventCountGroupInputDto input, CancellationToken ct)
    {
        var result = await service.UpdateAsync(recId, input, ct);
        if (result.Error == "Count group was not found.") return NotFound(APIResponse<object>.Fail(result.Error));
        return result.Group is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!))
            : Ok(APIResponse<InventCountGroupRecordDto>.Ok(result.Group));
    }

    [HttpDelete("{recId:long}")]
    public async Task<IActionResult> Delete(long recId, CancellationToken ct)
    {
        var error = await service.DeleteAsync(recId, ct);
        if (error == "Count group was not found.") return NotFound(APIResponse<object>.Fail(error));
        return error is null ? Ok(APIResponse<bool>.Ok(true)) : Conflict(APIResponse<object>.Fail(error));
    }
}
