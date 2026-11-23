using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.CashAndBank;

/// <summary>The bank group every bank account is filed under; the account carries its identifier.</summary>
public sealed class BankGroupSeeder : IDataSeeder
{
    public string Id => "ERPV2.BankGroupSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.CurrencySeeder"];

    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        await c.EnsureAsync<BankGroup>(x => x.BankGroupId == "APV2-BG", new()
        {
            BankGroupId = "APV2-BG", Name = "Demo clearing bank", RegistrationNum = "000000009000",
            BankCodeType = BankCodeType.Standard, CurrencyCode = "SAR"
        }, ct);
    }
}
