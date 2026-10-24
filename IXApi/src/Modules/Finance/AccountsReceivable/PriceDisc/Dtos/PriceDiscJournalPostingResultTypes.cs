namespace IAX.IXApi.Modules.Finance.AccountsReceivable.PriceDisc;

public enum PriceDiscJournalPostStatus { NotFound, AlreadyPosted, Invalid, Posted }
public sealed record PriceDiscJournalPostResult(PriceDiscJournalPostStatus Status, List<string>? Errors = null, int PublishedRules = 0);
