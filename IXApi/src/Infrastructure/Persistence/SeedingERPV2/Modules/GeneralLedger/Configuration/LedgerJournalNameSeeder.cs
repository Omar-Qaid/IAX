using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Inventory.InventTable;
using IAX.IXApi.Modules.Finance.Inventory.ProductCategories.Entities;
using IAX.IXApi.Modules.Finance.Foundation.Dimensions.LedgerDimension;
using IAX.IXApi.Modules.Finance.Foundation.Dimensions.AccountStructure;
using Microsoft.EntityFrameworkCore;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
public sealed class LedgerJournalNameSeeder : IDataSeeder
{
    public string Id => "ERPV2.LedgerJournalNameSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.GeneralLedgerNumberSequenceSeeder", "ERPV2.CurrencySeeder"];
    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {

        foreach (var (code, type, seq) in new[] { ("APV2-INV", LedgerJournalType.Daily, "APV2-VOUCH"), ("APV2-PAY", LedgerJournalType.VendDisbursement, "APV2-PAY") })
        {
            var sequence = await c.RequireAsync<SysNumberSequence>(x => x.NumberSequence == seq, ct);
            await c.EnsureAsync<LedgerJournalName>(x => x.JournalName == code,
                new() { JournalName = code, Name = code, JournalType = type, NumberSequenceTable = sequence.RecId, CurrencyCode = "SAR" }, ct);
        }
    }
}
