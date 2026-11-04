using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Validation;

/// <summary>One posted-state check over a finished scenario. Each implementation owns the concern its name states.</summary>
public interface IAccountsPayableCycleCheck
{
    void Verify(AccountsPayableReconciliationSnapshot posted);
}

/// <summary>
/// Everything the cycle checks read, loaded once after a scenario finishes posting. Amounts come from the persisted
/// rows only; nothing here recomputes what a posting service already decided.
/// </summary>
public sealed class AccountsPayableReconciliationSnapshot
{
    public static IReadOnlyList<IAccountsPayableCycleCheck> Checks { get; } =
    [
        new OpenTransactionsValidator(), new VendorBalanceValidator(), new VendorToLedgerValidator(),
        new LedgerBalanceValidator(), new InventoryQuantityValidator(), new InventoryValueValidator(),
        new UninvoicedReceiptValidator(), new BankPostingValidator(), new TaxPostingValidator(),
        new SettlementValidator()
    ];

    public required string ScenarioId { get; init; }
    public required decimal ExpectedPhysicalQuantity { get; init; }
    public required decimal ExpectedOpenDebit { get; init; }
    public required decimal ExpectedOpenCredit { get; init; }

    public required IReadOnlyList<string> PurchaseOrders { get; init; }
    public required IReadOnlyList<VendTrans> VendorTransactions { get; init; }
    public required IReadOnlyList<VendTransOpen> Open { get; init; }
    public required IReadOnlyList<VendSettlement> SettlementRows { get; init; }
    public required IReadOnlyList<InventTrans> Inventory { get; init; }
    public required IReadOnlyList<PurchReceiptAccrual> Accruals { get; init; }
    public required IReadOnlyList<TaxTrans> Tax { get; init; }
    public required IReadOnlyList<BankAccountTrans> Bank { get; init; }
    public required IReadOnlyList<GeneralJournalAccountEntry> Entries { get; init; }
    public required IReadOnlyList<long> PostedVouchers { get; init; }
    public required int SourceDistributions { get; init; }

    /// <summary>Ledger dimensions per seeded main account; read through <see cref="LedgerAmount"/>, not directly.</summary>
    public required IReadOnlyDictionary<string, long[]> DimensionsByAccount { get; init; }

    public decimal OpenDebit => Open.Where(x => x.AmountCur > 0).Sum(x => x.AmountCur);
    public decimal OpenCredit => -Open.Where(x => x.AmountCur < 0).Sum(x => x.AmountCur);

    public decimal PhysicalQuantity => Inventory.Where(x => x.StatusReceipt is StatusReceipt.Received or StatusReceipt.Purchased
        || x.StatusIssue is StatusIssue.Deducted or StatusIssue.Sold).Sum(x => x.Qty);

    /// <summary>Accounting-currency rate of one vendor transaction, for amounts stored only in its own currency.</summary>
    public decimal AccountingRate(long vendTransRecId)
    {
        var transaction = VendorTransactions.Single(x => x.RecId == vendTransRecId);
        return transaction.AmountCur == 0m ? 1m : transaction.AmountMst / transaction.AmountCur;
    }

    /// <summary>
    /// Accounting-currency total posted to one main account of the demo chart. An unseeded code is an error rather
    /// than a zero, because a zero would let a renumbered chart satisfy the checks that expect a cleared account.
    /// </summary>
    public decimal LedgerAmount(string mainAccount)
    {
        if (!DimensionsByAccount.TryGetValue(mainAccount, out var dimensions))
            throw new InvalidOperationException($"AP cycle integrity: main account {mainAccount} is not in the seeded demo chart.");
        if (dimensions.Length == 0)
            throw new InvalidOperationException($"AP cycle integrity: main account {mainAccount} has no ledger dimension in this company.");
        return Entries.Where(x => dimensions.Contains(x.LedgerDimension)).Sum(x => x.AccountingCurrencyAmount);
    }

    public static void Check(bool valid, string message)
    {
        if (!valid) throw new InvalidOperationException("AP cycle integrity: " + message);
    }

    public static async Task<AccountsPayableReconciliationSnapshot> LoadAsync(ApplicationDbContext db, string key, string id,
        decimal expectedPhysical, decimal expectedDebit, decimal expectedCredit, CancellationToken ct)
    {
        var area = db.GetDataAreaId();
        var purchaseOrders = await db.Set<PurchTable>().Where(x => x.DataAreaId == area && (x.PurchName == key || x.ReturnItemNum == key + "-RET")).Select(x => x.PurchId).ToArrayAsync(ct);
        var invoiceRows = await db.Set<VendInvoiceJour>().Where(x => x.DataAreaId == area && x.InvoiceId.StartsWith(key + "-")).ToListAsync(ct);
        var paymentLines = await db.Set<LedgerJournalTrans>().Where(x => x.DataAreaId == area && x.PaymReference.StartsWith(key + "-")).ToListAsync(ct);
        var journals = paymentLines.Select(x => x.JournalNum).ToArray();
        var reversals = await db.Set<LedgerJournalTable>().Where(x => x.DataAreaId == area && journals.Contains(x.OriginalJournalNum)).Select(x => x.JournalNum).ToArrayAsync(ct);
        var reverseVouchers = await db.Set<LedgerJournalTrans>().Where(x => x.DataAreaId == area && reversals.Contains(x.JournalNum)).Select(x => x.Voucher).ToArrayAsync(ct);
        var vendorVouchers = invoiceRows.Select(x => x.LedgerVoucher).Concat(paymentLines.Select(x => x.Voucher)).Concat(reverseVouchers).ToArray();
        var vendorTransactions = await db.Set<VendTrans>().Where(x => x.DataAreaId == area && vendorVouchers.Contains(x.Voucher)).ToListAsync(ct);
        var ids = vendorTransactions.Select(x => x.RecId).ToArray();
        var open = await db.Set<VendTransOpen>().Where(x => x.DataAreaId == area && ids.Contains(x.RefRecId)).ToListAsync(ct);
        var origins = await db.Set<InventTransOrigin>().Where(x => x.DataAreaId == area && purchaseOrders.Contains(x.ReferenceId) && x.ReferenceCategory == InventRefType.PurchaseOrder).Select(x => x.RecId).ToArrayAsync(ct);
        var inventory = await db.Set<InventTrans>().Where(x => x.DataAreaId == area && origins.Contains(x.InventTransOrigin)).ToListAsync(ct);
        var receiptVouchers = await db.Set<VendPackingSlipJour>().Where(x => x.DataAreaId == area && purchaseOrders.Contains(x.PurchId)).Select(x => x.LedgerVoucher).ToArrayAsync(ct);
        var settlementRows = await db.Set<VendSettlement>().Where(x => x.DataAreaId == area && ids.Contains(x.InvoiceTransRecId)).ToListAsync(ct);
        var allVouchers = vendorVouchers.Concat(receiptVouchers).Concat(settlementRows.Select(x => x.Voucher)).Distinct().ToArray();
        var posted = await db.Set<GeneralJournalEntry>().Where(x => x.DataAreaId == area && allVouchers.Contains(x.SubledgerVoucher)).Select(x => x.RecId).ToArrayAsync(ct);
        var entries = await db.Set<GeneralJournalAccountEntry>().Where(x => x.DataAreaId == area && posted.Contains(x.GeneralJournalEntry)).ToListAsync(ct);
        var accruals = await (from accrual in db.Set<PurchReceiptAccrual>() join receipt in db.Set<VendPackingSlipTrans>() on accrual.PackingSlipTransRecId equals receipt.RecId
            where accrual.DataAreaId == area && receipt.DataAreaId == area && purchaseOrders.Contains(receipt.OrigPurchId) select accrual).ToListAsync(ct);
        var entryIds = entries.Select(x => x.RecId).ToArray();
        var links = await db.Set<SubledgerJournalAccountEntry>().Where(x => x.GeneralJournalAccountEntry != null && entryIds.Contains(x.GeneralJournalAccountEntry.Value)).CountAsync(ct);
        var dimensions = await db.Set<DimensionAttributeValueCombination>().Where(x => x.DataAreaId == area).ToListAsync(ct);
        return new AccountsPayableReconciliationSnapshot
        {
            ScenarioId = id,
            ExpectedPhysicalQuantity = expectedPhysical,
            ExpectedOpenDebit = expectedDebit,
            ExpectedOpenCredit = expectedCredit,
            PurchaseOrders = purchaseOrders,
            VendorTransactions = vendorTransactions,
            Open = open,
            SettlementRows = settlementRows,
            Inventory = inventory,
            Accruals = accruals,
            Tax = await db.Set<TaxTrans>().Where(x => x.DataAreaId == area && vendorVouchers.Contains(x.Voucher)).ToListAsync(ct),
            Bank = await db.Set<BankAccountTrans>().Where(x => x.DataAreaId == area && vendorVouchers.Contains(x.Voucher)).ToListAsync(ct),
            Entries = entries,
            PostedVouchers = posted,
            SourceDistributions = links,
            DimensionsByAccount = GeneralLedgerSeedExample.MainAccounts.ToDictionary(x => x.Code,
                x => dimensions.Where(d => d.MainAccountValue == x.Code).Select(d => d.RecId).ToArray())
        };
    }
}
