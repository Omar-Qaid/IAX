namespace IAX.IXApi.Modules.Finance.GeneralLedger.Settlement;

public interface ILedgerSettlementService
{
    /// <summary>Settles the selected posted ledger entries against each other under one settlement ID.</summary>
    Task<(LedgerSettlementResultDto? Result, string? Error)> SettleAsync(LedgerSettlementRequestDto input, CancellationToken ct);

    /// <summary>Reverses a standing settlement, returning its entries to the open items of the account.</summary>
    Task<(LedgerSettlementResultDto? Result, string? Error)> ReverseAsync(string settleId, CancellationToken ct);

    /// <summary>Posted entries on a main account that no standing settlement covers.</summary>
    Task<(IReadOnlyList<LedgerOpenItemDto>? Items, string? Error)> OpenItemsAsync(string mainAccountId, CancellationToken ct);
}
