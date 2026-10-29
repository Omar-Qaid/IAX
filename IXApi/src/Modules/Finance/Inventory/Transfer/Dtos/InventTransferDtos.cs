using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Inventory.Transfer;

/// <summary>A transfer order to create, with its lines.</summary>
public sealed record InventTransferCreateDto(
    string TransferId,
    string InventSiteId,
    string InventLocationIdFrom,
    string InventLocationIdTransit,
    string InventLocationIdTo,
    DateTime? ShipDate,
    DateTime? ReceiveDate,
    IReadOnlyList<InventTransferLineInputDto> Lines);

public sealed record InventTransferLineInputDto(string ItemId, string UnitId, decimal QtyTransfer);

/// <summary>Quantities to ship or receive on one request, by line number.</summary>
public sealed record InventTransferPostDto(
    string RequestId,
    DateTime? TransDate,
    IReadOnlyDictionary<decimal, decimal> Quantities);

public sealed record InventTransferLineDto(
    decimal LineNum,
    string ItemId,
    string UnitId,
    decimal QtyTransfer,
    decimal QtyShipped,
    decimal QtyRemainShip,
    decimal QtyReceived,
    decimal QtyRemainReceive,
    InventTransferRemainStatus RemainStatus,
    string InventTransId,
    string InventTransIdTransitTo,
    string InventTransIdTransitFrom,
    string InventTransIdReceive);

public sealed record InventTransferOrderDto(
    string TransferId,
    InventTransferStatus TransferStatus,
    string InventLocationIdFrom,
    string InventLocationIdTransit,
    string InventLocationIdTo,
    DateTime ShipDate,
    DateTime ReceiveDate,
    IReadOnlyList<InventTransferLineDto> Lines);
