using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/inventory-reconciliation/physical")]
[DomainPermission("Inventory", "Transactions", "View")]
public sealed class InventPhysicalReconciliationController(IInventPhysicalReconciliationService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<APIResponse<InventPhysicalReconciliationDto>>> Check(
        [FromQuery] string? itemId, [FromQuery] string? siteId, [FromQuery] string? warehouseId,
        CancellationToken ct)
    {
        var result = await service.CheckAsync(itemId, siteId, warehouseId, ct);
        if (result is null)
            return BadRequest(APIResponse<InventPhysicalReconciliationDto>.Fail("A company must be selected."));
        return Ok(APIResponse<InventPhysicalReconciliationDto>.Ok(result));
    }
}
