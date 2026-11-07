using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.AccountsReceivable;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.AccountsReceivable;

/// <summary>
/// Return handling for the sales example: one reason the goods came back, and one disposition that
/// credits the customer and puts the units back on hand. Credit-only and scrap dispositions are
/// deliberately left out, because this example never needs to decide against a physical return.
/// </summary>
public sealed class ReturnDispositionSeeder : IDataSeeder
{
    public string Id => "ERPV2.ReturnDispositionSeeder";
    public IReadOnlyCollection<string> Dependencies => [];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        await c.EnsureAsync<ReturnReasonCode>(x => x.ReasonCodeId == "ARV2-WRONG",
            new() { ReasonCodeId = "ARV2-WRONG", Description = "Wrong item delivered (demo)" }, ct);
        await c.EnsureAsync<ReturnDispositionCode>(x => x.DispositionCodeId == "ARV2-RET",
            new() { DispositionCodeId = "ARV2-RET", Description = "Credit the customer and return to inventory (demo)",
                DispositionAction = DispositionAction.Credit }, ct);
    }
}
