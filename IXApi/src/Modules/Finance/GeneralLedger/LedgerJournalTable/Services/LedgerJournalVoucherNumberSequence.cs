using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using JournalHeader = IAX.IXApi.Modules.Finance.Entities.LedgerJournalTable;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable;

internal static class LedgerJournalVoucherNumberSequence
{
    public static async Task AllocateForPostingAsync(DbContext db, string area,
        JournalHeader header, LedgerJournalName name, List<LedgerJournalTrans> lines,
        CancellationToken ct)
    {
        if (header.VoucherAllocatedAtPosting == NoYes.Yes)
        {
            if (lines.Any(x => string.IsNullOrWhiteSpace(x.Voucher)))
                throw new SubledgerPostingException(
                    "A temporary voucher is required on every line when voucher numbers are allocated at posting.");

            EnsureConfigured(name);

            var replacements = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var temporaryVoucher in lines.Select(x => x.Voucher)
                         .Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal))
                replacements[temporaryVoucher] = await NextAsync(db, area, name.NumberSequenceTable, ct);
            foreach (var line in lines) line.Voucher = replacements[line.Voucher];
            return;
        }

        if (lines.All(x => !string.IsNullOrWhiteSpace(x.Voucher))) return;
        EnsureConfigured(name);
        string? shared = null;
        foreach (var line in lines.Where(x => string.IsNullOrWhiteSpace(x.Voucher)))
        {
            if (name.NewVoucher == LedgerJournalVoucherChange.None && shared != null) line.Voucher = shared;
            else
            {
                line.Voucher = await NextAsync(db, area, name.NumberSequenceTable, ct);
                shared ??= line.Voucher;
            }
        }
    }

    private static void EnsureConfigured(LedgerJournalName name)
    {
        if (name.NumberSequenceTable == 0)
            throw new SubledgerPostingException($"Journal name '{name.JournalName}' has no voucher number sequence.");
    }

    public static async Task<string> NextAsync(DbContext db, string area, long sequenceRecId, CancellationToken ct)
    {
        // Different journals can allocate from the same sequence in concurrent transactions.
        await new SubledgerPostingSupport(db).LockAsync($"LedgerJournalVoucher:{area}:{sequenceRecId}", ct);
        var sequence = await db.Set<SysNumberSequence>().IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.RecId == sequenceRecId && x.DataAreaId == area && !x.IsDeleted, ct)
            ?? throw new SubledgerPostingException("The journal voucher number sequence was not found in the selected company.");
        // Preserve increments staged by this transaction, but refresh an earlier read snapshot.
        if (db.Entry(sequence).State == EntityState.Unchanged)
            await db.Entry(sequence).ReloadAsync(ct);
        if (db.Entry(sequence).State == EntityState.Detached || sequence.IsDeleted || sequence.DataAreaId != area)
            throw new SubledgerPostingException("The journal voucher number sequence was not found in the selected company.");
        if (!sequence.IsActive || sequence.Blocked == 1 || sequence.InUse == 0 || sequence.Manual == 1 || sequence.NoIncrement == 1)
            throw new SubledgerPostingException($"Voucher number sequence '{sequence.NumberSequence}' is unavailable for automatic allocation.");
        var next = sequence.NextRec ?? sequence.Lowest ?? 1;
        if (next < (sequence.Lowest ?? 1))
            throw new SubledgerPostingException($"Voucher number sequence '{sequence.NumberSequence}' is below its configured minimum.");
        if (next > (sequence.Highest ?? int.MaxValue) || next == int.MaxValue)
            throw new SubledgerPostingException($"Voucher number sequence '{sequence.NumberSequence}' is exhausted.");
        sequence.NextRec = next + 1;
        return SysNumberSequenceService.FormatCode(sequence, next);
    }
}
