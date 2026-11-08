using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.CurrencyRevaluation;

public sealed class CustExchRateAdjustment : Entity<long>
{
    public DateTime? ConsideredDate { get; set; }
    public DateTime? DateOfRate { get; set; }
    public int? Dimensions { get; set; }
    public int? ExchPrinciple { get; set; }
    public DateTime? ExecutedDate { get; set; }
    public string? Notes { get; set; }
    public string? PostingProfile { get; set; }
    public int? Reviewed { get; set; }
    public int? UseProfile { get; set; }
}
