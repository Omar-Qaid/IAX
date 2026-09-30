using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Services;

public sealed class SalesInventoryNumberService : ISalesInventoryNumberService
{
    private readonly ISysNumberSequenceService _numberSequences;

    public SalesInventoryNumberService(ISysNumberSequenceService numberSequences)
        => _numberSequences = numberSequences;

    public async Task<string> NextInventDimIdAsync(CancellationToken cancellationToken = default)
        => (await _numberSequences.NextAsync("InventDim", cancellationToken: cancellationToken)).Code;

    public async Task<string> NextInventTransIdAsync(CancellationToken cancellationToken = default)
        => (await _numberSequences.NextAsync("InventTransId", cancellationToken: cancellationToken)).Code;
}
