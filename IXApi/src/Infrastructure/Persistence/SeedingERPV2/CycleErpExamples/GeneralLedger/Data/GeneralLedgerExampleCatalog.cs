using System.Text.Json;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.GeneralLedger.Data;

internal sealed record GeneralLedgerPreparationDefinition(string JournalName, string Currency, string[] Accounts);

internal static class GeneralLedgerExampleCatalog
{
    public static GeneralLedgerPreparationDefinition Read()
    {
        using var stream = typeof(GeneralLedgerExampleCatalog).Assembly
            .GetManifestResourceStream("SeedingERPV2.GeneralLedger.preparation.json")
            ?? throw new InvalidOperationException("GL preparation manifest is missing.");
        var definition = JsonSerializer.Deserialize<GeneralLedgerPreparationDefinition>(stream)
            ?? throw new InvalidOperationException("GL preparation manifest is empty.");
        if (definition.JournalName != GeneralLedgerExampleSetupSeeder.JournalName
            || definition.Currency != "SAR" || definition.Accounts == null
            || !definition.Accounts.SequenceEqual(new[] { GeneralLedgerDemoData.DebitAccount, GeneralLedgerDemoData.CreditAccount }))
            throw new InvalidOperationException("GL preparation manifest differs from its seed definitions.");
        return definition;
    }
}
