using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/InventItemGroupItem")]
[DomainPermission("Inventory", "Transactions")]
public sealed class ItemGroupAssignmentController(IItemGroupAssignmentService service) : ControllerBase
{
    [HttpPut("{itemId}")]
    public async Task<IActionResult> Assign(string itemId, ItemGroupAssignmentInputDto input, CancellationToken ct)
    {
        var result = await service.AssignAsync(itemId, input, ct);
        return result.Assignment is null
            ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!))
            : Ok(APIResponse<ItemGroupAssignmentRecordDto>.Ok(result.Assignment));
    }
}
