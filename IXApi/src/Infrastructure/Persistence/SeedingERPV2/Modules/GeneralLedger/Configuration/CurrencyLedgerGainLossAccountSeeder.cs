using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using Microsoft.EntityFrameworkCore;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
public sealed class CurrencyLedgerGainLossAccountSeeder : IDataSeeder
{
    public string Id => "ERPV2.CurrencyLedgerGainLossAccountSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.DimensionAttributeValueCombinationSeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var ledger = await c.RequireAsync<Ledger>(x => x.Name == "APV2", ct);
        foreach (var (type, code) in new[] { (CurrencyGainLossAccountType.RealizedGain, "481002"), (CurrencyGainLossAccountType.RealizedLoss, "681002") })
        {
            var account = await c.RequireAsync<MainAccount>(x => x.MainAccountId == code && x.LedgerChartOfAccounts == ledger.ChartOfAccounts, ct);
            var dimension = await c.RequireAsync<DimensionAttributeValueCombination>(x => x.MainAccount == account.RecId, ct);
            await c.EnsureAsync<CurrencyLedgerGainLossAccount>(x => x.Ledger == ledger.RecId && x.CurrencyCode == "USD" && x.AccountType == type,
                new() { Ledger = ledger.RecId, CurrencyCode = "USD", AccountType = type, LedgerDimension = dimension.RecId }, ct);
        }
    }
}
