using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/inventory/reservations")]
[DomainPermission("Inventory", "Transactions", "View")]
public sealed class InventoryReservationController(IInventoryReservationService service) : ControllerBase
{
    public sealed record ReserveRequest(string InventTransId, decimal Quantity, bool AllowPartial = false);
    public sealed record ReleaseRequest(string InventTransId, decimal Quantity);

    [HttpPost("reserve")]
    [DomainPermission("Inventory", "Transactions", "Edit")]
    public async Task<IActionResult> Reserve([FromBody] ReserveRequest request, CancellationToken ct)
    {
        var (result, error) = await service.ReserveAsync(request.InventTransId, request.Quantity, request.AllowPartial, ct);
        return error is null
            ? Ok(APIResponse<ReservationResultDto>.Ok(result!, "Reservation updated."))
            : UnprocessableEntity(APIResponse<object>.Fail(error));
    }

    [HttpPost("release")]
    [DomainPermission("Inventory", "Transactions", "Edit")]
    public async Task<IActionResult> Release([FromBody] ReleaseRequest request, CancellationToken ct)
    {
        var (result, error) = await service.ReleaseAsync(request.InventTransId, request.Quantity, ct);
        return error is null
            ? Ok(APIResponse<ReservationResultDto>.Ok(result!, "Reservation released."))
            : UnprocessableEntity(APIResponse<object>.Fail(error));
    }
}
