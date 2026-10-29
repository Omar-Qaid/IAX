using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed record ReservationResultDto(
    string InventTransId,
    string ItemId,
    decimal Demand,
    decimal Reserved,
    decimal AvailablePhysical);

public interface IInventoryReservationService : IBaseService<InventTrans>
{
    Task<(PickingResultDto? Result, string? Error)> SetPickedAsync(string inventTransId, decimal targetQuantity, CancellationToken ct);
    /// <summary>
    /// Sets a source's physical reservation to <paramref name="targetQuantity"/> (idempotent, D365
    /// "set reserved quantity" semantics). Reserves only physically available stock and never above
    /// remaining demand. Use <see cref="ReleaseAsync"/> to lower a reservation.
    /// </summary>
    Task<(ReservationResultDto? Result, string? Error)> ReserveAsync(
        string inventTransId, decimal targetQuantity, bool allowPartial, CancellationToken ct);

    /// <summary>Releases <paramref name="quantity"/> of a source's physical reservation back to open demand.</summary>
    Task<(ReservationResultDto? Result, string? Error)> ReleaseAsync(
        string inventTransId, decimal quantity, CancellationToken ct);
}

public sealed record PickingResultDto(string InventTransId, decimal Picked, decimal Reserved);
