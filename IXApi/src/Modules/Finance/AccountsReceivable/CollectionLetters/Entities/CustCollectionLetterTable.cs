using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.CollectionLetters;

public sealed class CustCollectionLetterTable : Entity<long>
{
    public string CollectionLetterCourse { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? PaymTermId { get; set; }
}
