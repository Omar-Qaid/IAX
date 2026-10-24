using IAX.IXApi.Modules.Administration.NumberSequences;

namespace IAX.IXApi.Modules.Finance.Inventory;

internal static class InventoryNumberSequencePolicy
{
    public static bool CanAllocate(SysNumberSequence sequence)
    {
        var lowest = sequence.Lowest ?? 1;
        var highest = sequence.Highest ?? int.MaxValue;
        var next = sequence.NextRec ?? lowest;
        var format = string.IsNullOrWhiteSpace(sequence.AnnotatedFormat)
            ? sequence.Format : sequence.AnnotatedFormat;
        return !sequence.IsDeleted && sequence.IsActive && sequence.InUse is > 0 &&
            sequence.Blocked != 1 && sequence.Manual != 1 && sequence.NoIncrement != 1 &&
            lowest <= highest && next >= lowest && next <= highest &&
            !string.IsNullOrWhiteSpace(format) && format.Contains("{SEQ}", StringComparison.Ordinal);
    }
}
