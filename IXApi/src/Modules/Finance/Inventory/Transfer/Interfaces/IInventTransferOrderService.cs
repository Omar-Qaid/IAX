namespace IAX.IXApi.Modules.Finance.Inventory.Transfer;

public interface IInventTransferOrderService
{
    Task<(InventTransferOrderDto? Order, string? Error)> CreateAsync(InventTransferCreateDto input, CancellationToken ct);

    /// <summary>Ships the requested quantities: stock leaves the source warehouse into transit.</summary>
    Task<(InventTransferOrderDto? Order, string? Error)> ShipAsync(string transferId, InventTransferPostDto input, CancellationToken ct);

    /// <summary>Receives the requested quantities: stock leaves transit into the destination.</summary>
    Task<(InventTransferOrderDto? Order, string? Error)> ReceiveAsync(string transferId, InventTransferPostDto input, CancellationToken ct);

    Task<(InventTransferOrderDto? Order, string? Error)> GetAsync(string transferId, CancellationToken ct);
}
