using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/inventory-costing")]
[DomainPermission("Inventory", "Transactions", "View")]
public sealed class FifoCostPreviewController(IFifoCostPreviewService service) : ControllerBase
{
    [HttpGet("fifo-preview")]
    public async Task<IActionResult> Preview([FromQuery] DateTime cutoff, CancellationToken ct)
    {
        if (cutoff == default)
            return BadRequest(APIResponse<object>.Fail("A cutoff date is required."));
        return Ok(APIResponse<object>.Ok(await service.PreviewAsync(cutoff, ct)));
    }
}
