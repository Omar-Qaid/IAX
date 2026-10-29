using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;
using JournalHeader = IAX.IXApi.Modules.Finance.Entities.LedgerJournalTable;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable;

/// <summary>
/// Reverses a posted general journal. The reversal is itself a journal: a new
/// LedgerJournalTable whose OriginalJournalNum points at the reversed journal, with mirrored
/// LedgerJournalTrans lines, posted through the same posting path. Original, reversal and any
/// later replacement entry all stay in the audit trail; nothing posted is edited or deleted.
///
/// A journal is reversed once. The reversal takes its own voucher numbers and its own accounting
/// date, which must fall in an open period — the original period is never reopened implicitly.
/// </summary>
public sealed class LedgerJournalReversalService(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    ISysNumberSequenceService numbers) : ILedgerJournalReversalService
{
    private DbContext Db => unitOfWork.Context;

    public Task<(LedgerJournalPostResultDto? Result, string? Error)> ReverseAsync(long recId, DateTime? reversalDate, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => ReverseCoreAsync(recId, reversalDate, ct));

    private async Task<(LedgerJournalPostResultDto? Result, string? Error)> ReverseCoreAsync(long recId, DateTime? reversalDate, CancellationToken ct)
    {
        var area = currentUser.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(area)) return (null, "A company must be selected.");
        await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var original = await Db.Set<JournalHeader>().AsNoTracking()
                .SingleOrDefaultAsync(x => x.DataAreaId == area && x.RecId == recId, ct);
            if (original == null)
            {
                await unitOfWork.RollbackTransactionAsync(ct);
                return (null, null);
            }
            var reversal = await BuildReversalAsync(area, original, (reversalDate ?? DateTime.UtcNow).Date, ct);
            var result = await new LedgerJournalPosting(Db).PostAsync(area, reversal, ct);
            await unitOfWork.CompleteAsync(ct); // The posted lines must exist before they can be marked.
            await MarkCorrectionAsync(area, reversal.JournalNum, ct);
            await unitOfWork.CompleteAsync(ct);
            await unitOfWork.CommitTransactionAsync(ct);
            return (result, null);
        }
        catch (SubledgerPostingException ex)
        {
            await unitOfWork.RollbackTransactionAsync(ct);
            Db.ChangeTracker.Clear();
            return (null, ex.Message);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(ct);
            Db.ChangeTracker.Clear();
            throw;
        }
    }

    /// <summary>
    /// When the company's ledger parameters mark transaction reversals as corrections, the
    /// reversal's posted lines carry IsCorrection so reports can separate them from ordinary
    /// activity. Without that parameter the reversal posts as a normal entry.
    /// </summary>
    private async Task MarkCorrectionAsync(string area, string journalNum, CancellationToken ct)
    {
        var parameters = await LedgerParameters.LedgerParametersService.PeekAsync(Db, area, ct);
        if (parameters?.TransactionReversalCorrection != NoYes.Yes) return;
        var entryIds = await Db.Set<GeneralJournalEntry>().AsNoTracking()
            .Where(x => x.JournalNumber == journalNum).Select(x => x.RecId).ToListAsync(ct);
        var lines = await Db.Set<GeneralJournalAccountEntry>()
            .Where(x => entryIds.Contains(x.GeneralJournalEntry)).ToListAsync(ct);
        foreach (var line in lines) line.IsCorrection = NoYes.Yes;
    }

    private async Task<JournalHeader> BuildReversalAsync(string area, JournalHeader original, DateTime reversalDate, CancellationToken ct)
    {
        var support = new SubledgerPostingSupport(Db);
        await support.LockAsync($"GeneralJournal:{area}:{original.JournalNum}", ct);

        if (original.Posted != NoYes.Yes)
            throw new SubledgerPostingException("Only a posted journal can be reversed; delete or correct an unposted journal instead.");
        if (original.OriginalJournalNum.Length > 0)
            throw new SubledgerPostingException($"Journal '{original.JournalNum}' is itself a reversal and cannot be reversed again.");
        if (await Db.Set<JournalHeader>().AnyAsync(x => x.DataAreaId == area
                && x.OriginalJournalNum == original.JournalNum && x.Posted == NoYes.Yes, ct))
            throw new SubledgerPostingException($"Journal '{original.JournalNum}' has already been reversed.");

        var lines = await Db.Set<LedgerJournalTrans>().AsNoTracking()
            .Where(x => x.DataAreaId == area && x.JournalNum == original.JournalNum).ToListAsync(ct);
        if (lines.Count == 0) throw new SubledgerPostingException("The posted journal has no lines to reverse.");

        var header = new JournalHeader
        {
            DataAreaId = area,
            JournalNum = (await numbers.NextAsync("LedgerJournalTable", area, ct)).Code,
            JournalName = original.JournalName,
            Name = Truncate($"Reversal of {original.JournalNum}", FieldLengths.Name),
            JournalType = original.JournalType,
            CurrencyCode = original.CurrencyCode,
            OriginalJournalNum = original.JournalNum,
            OriginalCompany = area,
            NumOfLines = lines.Count,
            Posted = NoYes.No
        };
        Db.Add(header);

        // Each original voucher reverses under its own new voucher, on the reversal date.
        var replacements = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var voucher in lines.Select(x => x.Voucher).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal))
            replacements[voucher] = (await numbers.NextAsync("Voucher", area, ct)).Code;

        foreach (var line in lines.OrderBy(x => x.LineNum).ThenBy(x => x.RecId))
            Db.Add(Mirror(line, header.JournalNum, replacements[line.Voucher], reversalDate));

        await Db.SaveChangesAsync(ct); // The posting path reads the reversal journal back from the store.
        return header;
    }

    /// <summary>The mirrored line: debit and credit swap, so a line with an offset flips both sides.</summary>
    private static LedgerJournalTrans Mirror(LedgerJournalTrans source, string journalNum, string voucher, DateTime date) => new()
    {
        DataAreaId = source.DataAreaId,
        Company = source.Company,
        JournalNum = journalNum,
        LineNum = source.LineNum,
        Voucher = voucher,
        TransDate = date,
        DocumentDate = date,
        DocumentNum = source.DocumentNum,
        AccountType = source.AccountType,
        LedgerDimension = source.LedgerDimension,
        LedgerDimensionName = source.LedgerDimensionName,
        DefaultDimension = source.DefaultDimension,
        OffsetAccountType = source.OffsetAccountType,
        OffsetLedgerDimension = source.OffsetLedgerDimension,
        OffsetDefaultDimension = source.OffsetDefaultDimension,
        OffsetCompany = source.OffsetCompany,
        CurrencyCode = source.CurrencyCode,
        ExchRate = source.ExchRate,
        AmountCurDebit = source.AmountCurCredit,
        AmountCurCredit = source.AmountCurDebit,
        Txt = source.Txt,
        OffsetTxt = source.OffsetTxt
    };

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..max];
}
