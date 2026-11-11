using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.GeneralLedger.Data;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.GeneralLedger.Validation;

public sealed class GeneralLedgerCycleValidator : ISeedValidator
{
    public async Task ValidateAsync(SeedContext context, CancellationToken ct)
    {
        var definition = GeneralLedgerExampleCatalog.Read();
        var journal = await context.RequireAsync<LedgerJournalName>(
            x => x.JournalName == GeneralLedgerExampleSetupSeeder.JournalName, ct);
        var sequence = await context.RequireAsync<SysNumberSequence>(
            x => x.RecId == journal.NumberSequenceTable, ct);
        if (journal.JournalType != LedgerJournalType.Daily || journal.CurrencyCode != "SAR"
            || sequence.Blocked != 0 || sequence.Manual != 0 || sequence.InUse != 1
            || sequence.NextRec < sequence.Lowest || sequence.NextRec > sequence.Highest)
            throw new InvalidOperationException("GL preparation has invalid journal or voucher sequence configuration.");
        foreach (var code in definition.Accounts)
        {
            var dimension = await context.RequireAsync<DimensionAttributeValueCombination>(
                x => x.MainAccountValue == code, ct);
            if (dimension.RecId <= 0)
                throw new InvalidOperationException("GL preparation has an invalid ledger dimension.");
        }
    }
}
