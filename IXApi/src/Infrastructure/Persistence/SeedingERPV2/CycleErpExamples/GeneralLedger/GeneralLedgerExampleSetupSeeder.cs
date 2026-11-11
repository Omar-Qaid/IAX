using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.GeneralLedger;

public sealed class GeneralLedgerExampleSetupSeeder : IDataSeeder
{
    internal const string JournalName = "GLV2-GEN";
    public string Id => "ERPV2.GL.ExampleSetup";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.LedgerJournalNameSeeder", "ERPV2.DimensionAttributeValueCombinationSeeder"];

    public async Task SeedAsync(SeedContext context, CancellationToken ct)
    {
        const string code = "GLV2-VOUCH";
        var sequence = await context.EnsureAsync<SysNumberSequence>(x => x.NumberSequence == code,
            new() { NumberSequence = code, Txt = "GL example vouchers", Format = code + "-######",
                AnnotatedFormat = code + "-{SEQ}", Lowest = 1, Highest = 999999, NextRec = 1, InUse = 1 }, ct,
            nameof(SysNumberSequence.Format), nameof(SysNumberSequence.AnnotatedFormat));
        await context.EnsureAsync<LedgerJournalName>(x => x.JournalName == JournalName,
            new() { JournalName = JournalName, Name = "GL example daily journal", JournalType = LedgerJournalType.Daily,
                CurrencyCode = "SAR", NumberSequenceTable = sequence.RecId }, ct);
    }
}
