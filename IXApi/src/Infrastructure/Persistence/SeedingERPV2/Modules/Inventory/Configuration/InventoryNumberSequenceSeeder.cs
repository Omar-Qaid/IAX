using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;

/// <summary>Numbers consumed by inventory close, its cost-settlement history and the inventory journals.</summary>
public sealed class InventoryNumberSequenceSeeder : IDataSeeder
{
    public string Id => "ERPV2.InventoryNumberSequenceSeeder";
    public IReadOnlyCollection<string> Dependencies => [];

    public async Task SeedAsync(SeedContext c,CancellationToken ct)
    {
        foreach(var (key,prefix) in new[] { ("InventClosing","IV2CL"),("SettleTransId","IV2ST"),
            // The journal id and its voucher sequence. InventDim and InventTransId are deliberately
            // absent: AccountsPayableNumberSequenceSeeder owns those, and seeding a second format
            // against the same key would be refused as a conflict.
            (nameof(InventJournalTable),"IV2JR"),(InventJournalSeedExample.VoucherSequence,"IV2JV"),
            (nameof(InventTransferTable),"IV2TO"),(nameof(InventTransferJour),"IV2TJ") })
        {
            var sequence = await c.EnsureAsync<SysNumberSequence>(x => x.NumberSequence == key,
                new() { NumberSequence = key,Txt = "Inventory cycle sequence",Format = prefix + "-######",AnnotatedFormat = prefix + "-{SEQ}",
                    Lowest = 1,Highest = 999999,NextRec = 1,InUse = 1,Blocked = 0,Manual = 0,NoIncrement = 0 },ct,
                nameof(SysNumberSequence.Format),nameof(SysNumberSequence.AnnotatedFormat),nameof(SysNumberSequence.Lowest),
                nameof(SysNumberSequence.Highest),nameof(SysNumberSequence.InUse),nameof(SysNumberSequence.Blocked),
                nameof(SysNumberSequence.Manual),nameof(SysNumberSequence.NoIncrement));
            if((sequence.NextRec ?? 0) < Math.Max(1,sequence.Lowest ?? 1) || sequence.NextRec > sequence.Highest)
                throw new InvalidOperationException($"Inventory sequence {key} is invalid or exhausted in {c.Company}.");
        }
    }
}
