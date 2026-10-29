using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/inventory/reservations")]
[DomainPermission("Inventory", "Transactions", "View")]
public sealed class InventoryReservationController
    : BaseController<InventTrans, ReservationResultDto>
{
    private readonly IInventoryReservationService _reservationService;

    public InventoryReservationController(
        IInventoryReservationService service,
        ILogger<InventoryReservationController> logger)
        : base(service, logger)
    {
        _reservationService = service;
    }

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<ReservationResultDto>>>> GetAll(CancellationToken cancellationToken = default) =>
        base.GetAll(cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<ReservationResultDto>>> GetById(string id, CancellationToken cancellationToken = default) =>
        base.GetById(id, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<ReservationResultDto>>> Create(ReservationResultDto dto, CancellationToken cancellationToken = default) =>
        base.Create(dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<ReservationResultDto>>> Update(string id, ReservationResultDto dto, CancellationToken cancellationToken = default) =>
        base.Update(id, dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default) =>
        base.Delete(id, cancellationToken);

    public sealed record ReserveRequest(string InventTransId, decimal Quantity, bool AllowPartial = false);
    public sealed record ReleaseRequest(string InventTransId, decimal Quantity);
    public sealed record PickingRequest(string InventTransId, decimal TargetQuantity);

    [HttpPost("picking")]
    [DomainPermission("Inventory", "Transactions", "Edit")]
    public async Task<IActionResult> SetPicked([FromBody] PickingRequest request, CancellationToken ct)
    {
        var (result, error) = await _reservationService.SetPickedAsync(request.InventTransId, request.TargetQuantity, ct);
        return error is null ? Ok(APIResponse<PickingResultDto>.Ok(result!, "Picking updated."))
            : UnprocessableEntity(APIResponse<object>.Fail(error));
    }

    [HttpPost("reserve")]
    [DomainPermission("Inventory", "Transactions", "Edit")]
    public async Task<IActionResult> Reserve([FromBody] ReserveRequest request, CancellationToken ct)
    {
        var (result, error) = await _reservationService.ReserveAsync(request.InventTransId, request.Quantity, request.AllowPartial, ct);
        return error is null
            ? Ok(APIResponse<ReservationResultDto>.Ok(result!, "Reservation updated."))
            : UnprocessableEntity(APIResponse<object>.Fail(error));
    }

    [HttpPost("release")]
    [DomainPermission("Inventory", "Transactions", "Edit")]
    public async Task<IActionResult> Release([FromBody] ReleaseRequest request, CancellationToken ct)
    {
        var (result, error) = await _reservationService.ReleaseAsync(request.InventTransId, request.Quantity, ct);
        return error is null
            ? Ok(APIResponse<ReservationResultDto>.Ok(result!, "Reservation released."))
            : UnprocessableEntity(APIResponse<object>.Fail(error));
    }
}
