using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/InventOnHand")]
[DomainPermission("Inventory", "Transactions", "View")]
public sealed class InventOnHandController(IInventOnHandService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<APIResponse<IEnumerable<InventOnHandDto>>>> List(CancellationToken ct)
    {
        var rows = await service.ListAsync(ct);
        if (rows is null)
            return BadRequest(APIResponse<IEnumerable<InventOnHandDto>>.Fail(
                "A company must be selected to view on-hand inventory."));
        return Ok(APIResponse<IEnumerable<InventOnHandDto>>.Ok(rows));
    }
}
