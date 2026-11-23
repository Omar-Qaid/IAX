using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.CashAndBank;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.CashAndBank.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Foundation;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Foundation.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.GeneralLedger;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.GeneralLedger.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Tax;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Tax.Data;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AllModules;

/// <summary>One stage of the full cycle and what it posted.</summary>
public sealed record AllModulesStageReport(int Order, string Module, string Cycle, int Scenarios);

/// <summary>A subledger balance compared with the general ledger accounts that carry it.</summary>
public sealed record AllModulesReconciliation(string Area, decimal Subledger, decimal GeneralLedger)
{
    public decimal Difference => Subledger - GeneralLedger;
}

public sealed record AllModulesTransactionReport(string Company, int FiscalYear,
    IReadOnlyList<AllModulesStageReport> Stages, IReadOnlyList<AllModulesReconciliation> Reconciliations,
    decimal TrialBalanceNet, IReadOnlyList<string> Skipped);

/// <summary>
/// The full basic D365 cycle across every module, on the one all-modules seed: each module's own
/// transaction examples run in dependency order through the business services that own the
/// documents, then the subledgers are reconciled to the general ledger.
/// <list type="number">
/// <item>Procure to pay (AP): purchase, receipt, vendor invoice, payment, settlement, returns.</item>
/// <item>Inventory: movement, adjustment, transfer and counting journals, reservation, close.</item>
/// <item>Order to cash (AR): sales, delivery, invoice, prepayment, collection, settlement, returns.</item>
/// <item>Cash and bank: bank movements reconciled to the ledger.</item>
/// <item>General ledger: journal posting and reversal.</item>
/// <item>Tax: posted tax reconciled to its source, settlement of the period.</item>
/// <item>Foundation: taxed journals and charges.</item>
/// </list>
/// Each stage resolves the documents it would post before posting, so a rerun verifies and resumes.
/// </summary>
public sealed class AllModulesTransactionExamples(ApplicationDbContext db,
    AccountsPayableTransactionExamples payables, InventoryTransactionExamples inventory,
    AccountsReceivableTransactionExamples receivables, CashAndBankTransactionExamples bank,
    GeneralLedgerTransactionExamples ledger, TaxTransactionExamples tax, FoundationTransactionExamples foundation)
{
    /// <summary>
    /// Steps of the reference cycle the services cannot perform yet. They are skipped, not approximated.
    /// </summary>
    public static IReadOnlyList<string> Skipped { get; } =
    [
        "Opening capital journal (bank to equity): the seeded chart has no equity account yet.",
        "Expense journal with input VAT on a ledger line: the journal engine calculates tax only on an inclusive vendor line, and refuses it elsewhere by name.",
        "Bank reconciliation against a bank statement (out of basic scope).",
        "Ledger period close and module access lock: verified by the GL service tests, not run here, so the example company stays postable.",
        "Customer aging snapshot and write-off (Credit and collections, out of basic scope)."
    ];

    public async Task<AllModulesTransactionReport> RunAsync(int fiscalYear, string actor, CancellationToken ct = default)
    {
        var stages = new List<AllModulesStageReport>();
        // Every example prepares the shared seed through the runner, which needs an empty change tracker.
        async Task Stage(string module, string cycle, Func<Task<int>> run)
        {
            db.ChangeTracker.Clear();
            stages.Add(new(stages.Count + 1, module, cycle, await run()));
        }

        await Stage("AccountsPayable", "Procure to pay", async () =>
            (await payables.RunAsync(new AccountsPayableTransactionOptions(fiscalYear, actor), ct)).Scenarios.Count);
        await Stage("Inventory", "Inventory journals, transfer and close", async () =>
            (await inventory.RunAsync(new InventoryTransactionOptions(fiscalYear, actor), ct)).Scenarios.Count);
        await Stage("AccountsReceivable", "Order to cash", async () =>
            (await receivables.RunAsync(new AccountsReceivableTransactionOptions(fiscalYear, actor), ct)).Scenarios.Count);
        await Stage("CashAndBank", "Bank movements", async () =>
        {
            await bank.RunAsync(new CashAndBankExampleOptions(fiscalYear, actor), ct);
            return 1;
        });
        await Stage("GeneralLedger", "General journal and reversal", async () =>
        {
            await ledger.RunAsync(new GeneralLedgerTransactionOptions(fiscalYear, actor), ct);
            return 1;
        });
        await Stage("Tax", "Posted tax and settlement", async () =>
        {
            await tax.RunAsync(new TaxExampleOptions(fiscalYear, actor), ct);
            return 1;
        });
        await Stage("Foundation", "Taxed journal and charges", async () =>
            (await foundation.RunAsync(new FoundationExampleOptions(fiscalYear, actor), ct)).Scenarios.Count);

        db.ChangeTracker.Clear();
        var area = db.GetDataAreaId();
        var reconciliations = new List<AllModulesReconciliation>
        {
            new("Accounts receivable",
                await db.Set<CustTrans>().AsNoTracking().Where(x => x.DataAreaId == area).SumAsync(x => x.AmountMst, ct),
                await LedgerAsync(area, await db.Set<CustLedgerAccounts>().AsNoTracking()
                    .Where(x => x.DataAreaId == area).Select(x => x.SummaryLedgerDimension).ToListAsync(ct), ct)),
            new("Accounts payable",
                await db.Set<VendTrans>().AsNoTracking().Where(x => x.DataAreaId == area).SumAsync(x => x.AmountMst, ct),
                await LedgerAsync(area, await db.Set<VendLedgerAccounts>().AsNoTracking()
                    .Where(x => x.DataAreaId == area).Select(x => x.SummaryLedgerDimension).ToListAsync(ct), ct)),
            new("Bank",
                await db.Set<BankAccountTrans>().AsNoTracking().Where(x => x.DataAreaId == area).SumAsync(x => x.AmountMst, ct),
                await LedgerAsync(area, await db.Set<BankAccountTable>().AsNoTracking()
                    .Where(x => x.DataAreaId == area).Select(x => x.LedgerDimension).ToListAsync(ct), ct)),
        };
        var trialBalance = await db.Set<GeneralJournalAccountEntry>().AsNoTracking()
            .Where(x => x.DataAreaId == area).SumAsync(x => x.AccountingCurrencyAmount, ct);

        if (trialBalance != 0m)
            throw new InvalidOperationException($"Full cycle: the trial balance of {area} is off by {trialBalance}.");
        foreach (var check in reconciliations.Where(x => x.Difference != 0m))
            throw new InvalidOperationException(
                $"Full cycle: {check.Area} subledger {check.Subledger} differs from its ledger accounts {check.GeneralLedger}.");
        return new(area, fiscalYear, stages, reconciliations, trialBalance, Skipped);
    }

    /// <summary>
    /// The posted balance of the main accounts behind the given ledger dimensions. A control account
    /// is compared by main account, so every dimension combination of it counts.
    /// </summary>
    private async Task<decimal> LedgerAsync(string area, List<long> dimensions, CancellationToken ct)
    {
        var mainAccounts = await db.Set<DimensionAttributeValueCombination>().AsNoTracking()
            .Where(x => x.DataAreaId == area && dimensions.Contains(x.RecId)).Select(x => x.MainAccount)
            .Distinct().ToListAsync(ct);
        return await db.Set<GeneralJournalAccountEntry>().AsNoTracking()
            .Where(x => x.DataAreaId == area && mainAccounts.Contains(x.MainAccount))
            .SumAsync(x => x.AccountingCurrencyAmount, ct);
    }
}
