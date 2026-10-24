using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/inventory-costing")]
[DomainPermission("Inventory", "Transactions", "View")]
public sealed class InventCloseController(IInventCloseService service) : ControllerBase
{
    public sealed record CloseRequest(DateTime Cutoff);

    [HttpPost("close")]
    [DomainPermission("Inventory", "Transactions", "Edit")]
    public async Task<IActionResult> Close([FromBody] CloseRequest request, CancellationToken ct)
    {
        if (request.Cutoff == default)
            return BadRequest(APIResponse<object>.Fail("A cutoff date is required."));
        var (result, error) = await service.CloseAsync(request.Cutoff, ct);
        return error is null
            ? Ok(APIResponse<InventCloseResultDto>.Ok(result!, "Inventory close completed."))
            : UnprocessableEntity(APIResponse<object>.Fail(error));
    }
}
