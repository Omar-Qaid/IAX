using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Finance.AccountsPayable.CurrencyRevaluation;

/// <summary>D365 VendExchRateAdjustment: one foreign-currency revaluation run on the vendor side.</summary>
public sealed class VendExchRateAdjustment : Entity<long>
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
