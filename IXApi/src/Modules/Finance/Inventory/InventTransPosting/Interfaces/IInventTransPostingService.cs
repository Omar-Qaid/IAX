namespace IAX.IXApi.Modules.Finance.Inventory.InventTransPosting;

public interface IInventTransPostingService
{
    /// <summary>Inventory posting records for one inventory transaction identifier.</summary>
    Task<(IReadOnlyList<InventTransPostingDto>? Items, string? Error)> ByInventTransIdAsync(string inventTransId, CancellationToken ct);

    /// <summary>Inventory posting records carried by one voucher.</summary>
    Task<(IReadOnlyList<InventTransPostingDto>? Items, string? Error)> ByVoucherAsync(string voucher, CancellationToken ct);
}
