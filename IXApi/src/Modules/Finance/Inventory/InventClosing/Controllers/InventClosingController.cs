using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/inventory-costing/closings")]
[DomainPermission("Inventory", "Transactions", "View")]
public sealed class InventClosingController(IInventClosingQueryService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) =>
        Ok(APIResponse<IReadOnlyList<InventClosingRecordDto>>.Ok(await service.ListAsync(ct)));

    [HttpGet("{recId:long}")]
    public async Task<IActionResult> Get(long recId, CancellationToken ct)
    {
        var closing = await service.GetAsync(recId, ct);
        return closing is null
            ? NotFound(APIResponse<object>.Fail("Inventory closing was not found."))
            : Ok(APIResponse<InventClosingRecordDto>.Ok(closing));
    }
}
