using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Administration.NumberSequences;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;

/// <summary>Numbers consumed by inventory close and its cost-settlement history.</summary>
public sealed class InventoryNumberSequenceSeeder : IDataSeeder
{
    public string Id => "ERPV2.InventoryNumberSequenceSeeder";
    public IReadOnlyCollection<string> Dependencies => [];

    public async Task SeedAsync(SeedContext c,CancellationToken ct)
    {
        foreach(var (key,prefix) in new[] { ("InventClosing","IV2CL"),("SettleTransId","IV2ST") })
        {
            var sequence = await c.EnsureAsync<SysNumberSequence>(x => x.NumberSequence == key,
                new() { NumberSequence = key,Txt = "Inventory cost close",Format = prefix + "-######",AnnotatedFormat = prefix + "-{SEQ}",
                    Lowest = 1,Highest = 999999,NextRec = 1,InUse = 1,Blocked = 0,Manual = 0,NoIncrement = 0 },ct,
                nameof(SysNumberSequence.Format),nameof(SysNumberSequence.AnnotatedFormat),nameof(SysNumberSequence.Lowest),
                nameof(SysNumberSequence.Highest),nameof(SysNumberSequence.InUse),nameof(SysNumberSequence.Blocked),
                nameof(SysNumberSequence.Manual),nameof(SysNumberSequence.NoIncrement));
            if((sequence.NextRec ?? 0) < Math.Max(1,sequence.Lowest ?? 1) || sequence.NextRec > sequence.Highest)
                throw new InvalidOperationException($"Inventory sequence {key} is invalid or exhausted in {c.Company}.");
        }
    }
}
