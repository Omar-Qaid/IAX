using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;

/// <summary>
/// The inventory journal fixtures, owned beside <see cref="InventJournalNameSeeder"/>. The four names are not a
/// free choice: <c>InventParametersService.GetReadiness</c> reports a company unready unless a Movement,
/// Adjustment, Transfer and Counting name each exist with an allocatable voucher sequence.
/// </summary>
public static class InventJournalSeedExample
{
    /// <summary>The voucher sequence every example journal name draws from.</summary>
    public const string VoucherSequence = "IV2-JVOUCH";

    public const string CountGroup = "IV2-COUNT";

    public static IReadOnlyList<(string JournalNameId, string Description, InventJournalType JournalType)> Names { get; } =
    [
        ("IV2-MOVE", "Inventory movement (demo)", InventJournalType.Movement),
        ("IV2-ADJ", "Inventory adjustment (demo)", InventJournalType.Adjustment),
        ("IV2-TRANS", "Inventory transfer (demo)", InventJournalType.Transfer),
        ("IV2-COUNT", "Inventory counting (demo)", InventJournalType.Counting)
    ];

    public static string Name(InventJournalType type) => Names.Single(x => x.JournalType == type).JournalNameId;
}
