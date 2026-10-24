namespace IAX.IXApi.Modules.Finance.AccountsReceivable.PriceDisc;

public interface IPriceDiscJournalPostingService
{
    Task<List<string>?> ValidateAsync(long recId, CancellationToken cancellationToken);
    Task<PriceDiscJournalPostResult> PostAsync(long recId, CancellationToken cancellationToken);
}
