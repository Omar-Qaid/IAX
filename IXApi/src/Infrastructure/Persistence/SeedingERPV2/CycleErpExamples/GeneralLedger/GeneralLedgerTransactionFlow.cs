using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.GeneralLedger.Data;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.GeneralLedger;

public sealed class GeneralLedgerTransactionFlow(
    ApplicationDbContext db, ILedgerJournalTableService journals,
    ILedgerJournalTransService lines, ILedgerJournalReversalService reversals)
{
    public async Task<(string Journal, string Reversal)> RunAsync(int fiscalYear, string actor, CancellationToken ct = default)
    {
        if (db.ChangeTracker.HasChanges())
            throw new InvalidOperationException("Run GL examples in a clean DI scope.");
        db.ChangeTracker.Clear();
        await using var seedLock = await new SqlServerSeederLock(db).AcquireLockAsync(120, ct);

        var company = db.GetDataAreaId();
        var key = $"GLV2-{fiscalYear}-01";
        var date = new DateTime(fiscalYear, 1, 15);
        var debit = await DimensionAsync(GeneralLedgerDemoData.DebitAccount);
        var credit = await DimensionAsync(GeneralLedgerDemoData.CreditAccount);
        const decimal amount = GeneralLedgerDemoData.ReclassificationAmount;
        var header = await db.Set<LedgerJournalTable>().SingleOrDefaultAsync(x => x.DataAreaId == company && x.Name == key, ct)
            ?? await journals.AddAsync(new LedgerJournalTable
            {
                DataAreaId = company, JournalName = GeneralLedgerExampleSetupSeeder.JournalName,
                Name = key, CurrencyCode = "SAR"
            }, ct);

        if (header.JournalName != GeneralLedgerExampleSetupSeeder.JournalName || header.CurrencyCode != "SAR")
            throw new InvalidOperationException($"Conflicting GL example journal {key}.");
        var entered = await db.Set<LedgerJournalTrans>()
            .SingleOrDefaultAsync(x => x.DataAreaId == company && x.JournalNum == header.JournalNum, ct);
        if (entered == null && header.Posted != NoYes.Yes)
            entered = await lines.AddAsync(new LedgerJournalTrans
            {
                DataAreaId = company, JournalNum = header.JournalNum, TransDate = date,
                AccountType = LedgerJournalACType.Ledger, LedgerDimension = debit,
                OffsetAccountType = LedgerJournalACType.Ledger, OffsetLedgerDimension = credit,
                CurrencyCode = "SAR", AmountCurDebit = amount, DocumentNum = key, Txt = "GL expense reclassification example"
            }, ct);
        if (entered == null || entered.AccountType != LedgerJournalACType.Ledger
            || entered.OffsetAccountType != LedgerJournalACType.Ledger || entered.LedgerDimension != debit
            || entered.OffsetLedgerDimension != credit || entered.AmountCurDebit != amount
            || entered.AmountCurCredit != 0 || entered.TransDate != date || entered.CurrencyCode != "SAR"
            || entered.DocumentNum != key)
            throw new InvalidOperationException($"Conflicting GL example line {key}.");

        if (header.Posted != NoYes.Yes)
        {
            var validation = await journals.ValidateAsync(header.RecId, ct);
            if (validation == null || !validation.Valid)
                throw new InvalidOperationException("GL example validation failed.");
            var entryCount = await db.Set<GeneralJournalEntry>().CountAsync(x => x.DataAreaId == company, ct);
            var (simulation, simulationError) = await journals.SimulateAsync(header.RecId, ct);
            if (simulation == null || !simulation.WouldPost || simulation.TotalDebit != amount
                || simulation.TotalCredit != amount)
                throw new InvalidOperationException($"GL example simulation failed: {simulationError ?? simulation?.Refusal}");
            if (await db.Set<GeneralJournalEntry>().CountAsync(x => x.DataAreaId == company, ct) != entryCount
                || await db.Set<LedgerJournalTable>().AnyAsync(x => x.RecId == header.RecId && x.Posted == NoYes.Yes, ct))
                throw new InvalidOperationException("GL simulation left persisted accounting.");
            var (posted, error) = await journals.PostAsync(header.RecId, ct);
            if (posted == null) throw new InvalidOperationException($"GL example posting failed: {error}");
        }
        await VerifyVoucherAsync(header.JournalNum, amount, date);
        var reversal = await FindReversalAsync();
        if (reversal == null)
        {
            var (posted, error) = await reversals.ReverseAsync(header.RecId, date.AddDays(1), ct);
            if (posted == null) throw new InvalidOperationException($"GL example reversal failed: {error}");
            reversal = await FindReversalAsync() ?? throw new InvalidOperationException("GL example reversal journal is missing.");
        }
        await VerifyVoucherAsync(reversal.JournalNum, -amount, date.AddDays(1));
        await new SeedExecutionHistory().RecordAsync(db, $"ERPV2.GL.{fiscalYear}.01", company, ct);
        db.ChangeTracker.Clear();
        return (header.JournalNum, reversal.JournalNum);

        Task<long> DimensionAsync(string code) => db.Set<DimensionAttributeValueCombination>()
            .Where(x => x.DataAreaId == company && x.MainAccountValue == code)
            .Select(x => x.RecId).SingleAsync(ct);

        Task<LedgerJournalTable?> FindReversalAsync() => db.Set<LedgerJournalTable>().SingleOrDefaultAsync(
            x => x.DataAreaId == company && x.OriginalJournalNum == header.JournalNum && x.Posted == NoYes.Yes, ct);

        async Task VerifyVoucherAsync(string journal, decimal signedAmount, DateTime accountingDate)
        {
            var voucher = await db.Set<LedgerJournalTrans>()
                .Where(x => x.DataAreaId == company && x.JournalNum == journal)
                .Select(x => x.Voucher).Distinct().SingleAsync(ct);
            var entry = await db.Set<SubledgerVoucherGeneralJournalEntry>()
                .Where(x => x.Voucher == voucher && x.VoucherDataAreaId == company && x.AccountingDate == accountingDate)
                .Select(x => x.GeneralJournalEntry).SingleAsync(ct);
            var postings = await db.Set<GeneralJournalAccountEntry>().AsNoTracking()
                .Where(x => x.GeneralJournalEntry == entry).ToListAsync(ct);
            if (postings.Count != 2 || postings.Sum(x => x.AccountingCurrencyAmount) != 0
                || postings.Single(x => x.LedgerDimension == debit).AccountingCurrencyAmount != signedAmount
                || postings.Single(x => x.LedgerDimension == credit).AccountingCurrencyAmount != -signedAmount)
                throw new InvalidOperationException($"GL example voucher {voucher} did not reconcile.");
        }
    }
}

