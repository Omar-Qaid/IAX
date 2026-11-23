using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.CashAndBank.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.CashAndBank.Reports;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.CashAndBank;

/// <summary>
/// Bank movement is only ever a consequence of a payment, so this runs the vendor payment and the
/// customer collection through their owning services and then reads the resulting bank transactions
/// back. Nothing is inserted into the bank tables here.
/// </summary>
public sealed class CashAndBankTransactionExamples(
    ApplicationDbContext db,
    CashAndBankCycleSeeder preparation,
    AccountsPayableTransactionExamples payments,
    AccountsReceivableTransactionExamples collections)
{
    /// <summary>The full purchase-to-payment and sale-to-collection scenario; both settle through the bank.</summary>
    private static readonly string[] ScenarioIds = ["01"];

    public async Task<CashAndBankTransactionReport> RunAsync(CashAndBankExampleOptions options, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        var prepared = await preparation.PrepareAsync(options, ct);
        db.ChangeTracker.Clear();
        var paid = await payments.RunAsync(new AccountsPayableTransactionOptions(options.FiscalYear, options.Actor, ScenarioIds), ct);
        var collected = await collections.RunAsync(new AccountsReceivableTransactionOptions(options.FiscalYear, options.Actor, ScenarioIds), ct);

        var area = db.GetDataAreaId();
        var accounts = await db.Set<BankAccountTable>().AsNoTracking()
            .Where(x => x.DataAreaId == area).OrderBy(x => x.AccountId).ToListAsync(ct);
        var transactions = await db.Set<BankAccountTrans>().AsNoTracking()
            .Where(x => x.DataAreaId == area).ToListAsync(ct);
        if (transactions.Count == 0)
            throw new InvalidOperationException("The payment examples posted no bank transaction; the cash and bank cycle is not demonstrated.");
        var movements = accounts.Select(account =>
        {
            var posted = transactions.Where(x => x.AccountId == account.AccountId).ToList();
            return new BankAccountMovement(account.AccountId, account.CurrencyCode, posted.Count,
                posted.Where(x => x.AmountCur > 0m).Sum(x => x.AmountCur),
                -posted.Where(x => x.AmountCur < 0m).Sum(x => x.AmountCur));
        }).ToArray();
        return new(prepared.Company, options.FiscalYear, prepared, paid.Scenarios, collected.Scenarios, movements);
    }
}
