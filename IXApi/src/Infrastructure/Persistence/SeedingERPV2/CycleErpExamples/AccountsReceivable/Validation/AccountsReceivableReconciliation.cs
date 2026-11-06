using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable.Validation;

/// <summary>One posted-state check over a finished scenario. Each implementation owns the concern its name states.</summary>
public interface IAccountsReceivableCycleCheck
{
    void Verify(AccountsReceivableReconciliationSnapshot posted);
}

/// <summary>
/// Everything the cycle checks read, loaded once after a scenario finishes posting. Amounts come from the persisted
/// rows only; nothing here recomputes what a posting service already decided.
/// </summary>
public sealed class AccountsReceivableReconciliationSnapshot
{
    public static IReadOnlyList<IAccountsReceivableCycleCheck> Checks { get; } =
    [
        new OpenTransactionsValidator(), new CustomerBalanceValidator(), new CustomerToLedgerValidator(),
        new LedgerBalanceValidator(), new InventoryQuantityValidator(), new InventoryValueValidator(),
        new UndeliveredRevenueValidator(), new BankPostingValidator(), new TaxPostingValidator(),
        new SettlementValidator(), new RevenueRecognitionValidator()
    ];

    public required string ScenarioId { get; init; }
    public required decimal ExpectedPhysicalQuantity { get; init; }
    public required decimal ExpectedOpenDebit { get; init; }
    public required decimal ExpectedOpenCredit { get; init; }

    public required IReadOnlyList<string> SalesOrders { get; init; }
    public required IReadOnlyList<CustTrans> CustomerTransactions { get; init; }
    public required IReadOnlyList<CustTransOpen> Open { get; init; }
    public required IReadOnlyList<CustSettlement> SettlementRows { get; init; }
    public required IReadOnlyList<InventTrans> Inventory { get; init; }
    public required IReadOnlyList<CustInvoiceTrans> InvoiceLines { get; init; }
    public required IReadOnlyList<TaxTrans> Tax { get; init; }
    public required IReadOnlyList<BankAccountTrans> Bank { get; init; }
    public required IReadOnlyList<GeneralJournalAccountEntry> Entries { get; init; }
    public required IReadOnlyList<long> PostedVouchers { get; init; }
    public required int SourceDistributions { get; init; }

    /// <summary>Ledger dimensions per seeded main account; read through <see cref="LedgerAmount"/>, not directly.</summary>
    public required IReadOnlyDictionary<string, long[]> DimensionsByAccount { get; init; }

    public decimal OpenDebit => Open.Where(x => x.AmountCur > 0).Sum(x => x.AmountCur);
    public decimal OpenCredit => -Open.Where(x => x.AmountCur < 0).Sum(x => x.AmountCur);

    /// <summary>
    /// Quantity that left (or came back into) inventory for the scenario. Sales issues are negative, so a
    /// delivered order reports a negative movement and a received return nets it back up.
    /// </summary>
    public decimal PhysicalQuantity => Inventory.Where(x => x.StatusIssue is StatusIssue.Deducted or StatusIssue.Sold
        || x.StatusReceipt is StatusReceipt.Received or StatusReceipt.Purchased).Sum(x => x.Qty);

    /// <summary>Accounting-currency rate of one customer transaction, for amounts stored only in its own currency.</summary>
    public decimal AccountingRate(long custTransRecId)
    {
        var transaction = CustomerTransactions.Single(x => x.RecId == custTransRecId);
        return transaction.AmountCur == 0m ? 1m : transaction.AmountMst / transaction.AmountCur;
    }

    /// <summary>
    /// Accounting-currency total posted to one main account of the demo chart. An unseeded code is an error rather
    /// than a zero, because a zero would let a renumbered chart satisfy the checks that expect a cleared account.
    /// </summary>
    public decimal LedgerAmount(string mainAccount)
    {
        if (!DimensionsByAccount.TryGetValue(mainAccount, out var dimensions))
            throw new InvalidOperationException($"AR cycle integrity: main account {mainAccount} is not in the seeded demo chart.");
        if (dimensions.Length == 0)
            throw new InvalidOperationException($"AR cycle integrity: main account {mainAccount} has no ledger dimension in this company.");
        return Entries.Where(x => dimensions.Contains(x.LedgerDimension)).Sum(x => x.AccountingCurrencyAmount);
    }

    public static void Check(bool valid, string message)
    {
        if (!valid) throw new InvalidOperationException("AR cycle integrity: " + message);
    }

    public static async Task<AccountsReceivableReconciliationSnapshot> LoadAsync(ApplicationDbContext db, string key, string id,
        decimal expectedPhysical, decimal expectedDebit, decimal expectedCredit, CancellationToken ct)
    {
        var area = db.GetDataAreaId();
        var salesOrders = await db.Set<SalesTable>().Where(x => x.DataAreaId == area && (x.SalesName == key || x.ReturnItemNum == key + "-RET"))
            .Select(x => x.SalesId).ToArrayAsync(ct);
        var invoiceRows = await db.Set<CustInvoiceJour>().Where(x => x.DataAreaId == area && x.InvoiceId.StartsWith(key + "-")).ToListAsync(ct);
        var paymentLines = await db.Set<LedgerJournalTrans>().Where(x => x.DataAreaId == area && x.PaymReference.StartsWith(key + "-")).ToListAsync(ct);
        var journals = paymentLines.Select(x => x.JournalNum).ToArray();
        var reversals = await db.Set<LedgerJournalTable>().Where(x => x.DataAreaId == area && journals.Contains(x.OriginalJournalNum)).Select(x => x.JournalNum).ToArrayAsync(ct);
        var reverseVouchers = await db.Set<LedgerJournalTrans>().Where(x => x.DataAreaId == area && reversals.Contains(x.JournalNum)).Select(x => x.Voucher).ToArrayAsync(ct);
        var customerVouchers = invoiceRows.Select(x => x.LedgerVoucher).Concat(paymentLines.Select(x => x.Voucher)).Concat(reverseVouchers).ToArray();
        var customerTransactions = await db.Set<CustTrans>().Where(x => x.DataAreaId == area && customerVouchers.Contains(x.Voucher)).ToListAsync(ct);
        var ids = customerTransactions.Select(x => x.RecId).ToArray();
        var open = await db.Set<CustTransOpen>().Where(x => x.DataAreaId == area && ids.Contains(x.RefRecId)).ToListAsync(ct);
        var origins = await db.Set<InventTransOrigin>().Where(x => x.DataAreaId == area && salesOrders.Contains(x.ReferenceId)
            && x.ReferenceCategory == InventRefType.SalesTable).Select(x => x.RecId).ToArrayAsync(ct);
        var inventory = await db.Set<InventTrans>().Where(x => x.DataAreaId == area && origins.Contains(x.InventTransOrigin)).ToListAsync(ct);
        var deliveryVouchers = await db.Set<CustPackingSlipJour>().Where(x => x.DataAreaId == area && salesOrders.Contains(x.SalesId))
            .Select(x => x.LedgerVoucher).ToArrayAsync(ct);
        var settlementRows = await db.Set<CustSettlement>().Where(x => x.DataAreaId == area && ids.Contains(x.TransRecId)).ToListAsync(ct);
        var allVouchers = customerVouchers.Concat(deliveryVouchers).Concat(settlementRows.Select(x => x.SettlementVoucher)).Distinct().ToArray();
        var posted = await db.Set<GeneralJournalEntry>().Where(x => x.DataAreaId == area && allVouchers.Contains(x.SubledgerVoucher)).Select(x => x.RecId).ToArrayAsync(ct);
        var entries = await db.Set<GeneralJournalAccountEntry>().Where(x => x.DataAreaId == area && posted.Contains(x.GeneralJournalEntry)).ToListAsync(ct);
        var invoiceIds = invoiceRows.Select(x => x.InvoiceId).ToArray();
        var invoiceLines = await db.Set<CustInvoiceTrans>().Where(x => x.DataAreaId == area && invoiceIds.Contains(x.InvoiceId)).ToListAsync(ct);
        var entryIds = entries.Select(x => x.RecId).ToArray();
        var links = await db.Set<SubledgerJournalAccountEntry>().Where(x => x.GeneralJournalAccountEntry != null && entryIds.Contains(x.GeneralJournalAccountEntry.Value)).CountAsync(ct);
        var dimensions = await db.Set<DimensionAttributeValueCombination>().Where(x => x.DataAreaId == area).ToListAsync(ct);
        return new AccountsReceivableReconciliationSnapshot
        {
            ScenarioId = id,
            ExpectedPhysicalQuantity = expectedPhysical,
            ExpectedOpenDebit = expectedDebit,
            ExpectedOpenCredit = expectedCredit,
            SalesOrders = salesOrders,
            CustomerTransactions = customerTransactions,
            Open = open,
            SettlementRows = settlementRows,
            Inventory = inventory,
            InvoiceLines = invoiceLines,
            Tax = await db.Set<TaxTrans>().Where(x => x.DataAreaId == area && customerVouchers.Contains(x.Voucher)).ToListAsync(ct),
            Bank = await db.Set<BankAccountTrans>().Where(x => x.DataAreaId == area && customerVouchers.Contains(x.Voucher)).ToListAsync(ct),
            Entries = entries,
            PostedVouchers = posted,
            SourceDistributions = links,
            DimensionsByAccount = GeneralLedgerSeedExample.MainAccounts.ToDictionary(x => x.Code,
                x => dimensions.Where(d => d.MainAccountValue == x.Code).Select(d => d.RecId).ToArray())
        };
    }
}
