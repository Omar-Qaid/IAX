using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Security.Cryptography;
using System.Text;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Invoice;

public sealed record SalesInvoiceResultDto(string InvoiceId, string SalesId, decimal Quantity,
    decimal CostOfGoodsSold, int InventoryTransactionsSold, int LedgerEntriesCreated);

public interface ISalesInventoryInvoiceService
{
    Task<(SalesInvoiceResultDto? Result, string? Error)> PostAsync(string salesId, DateTime? invoiceDate, CancellationToken ct);
}

/// <summary>
/// Sales invoice — inventory financial issue (basic scope). Transitions delivered (Deducted)
/// inventory issues to Sold, applies the provisional running-average COGS, reduces inventory value,
/// posts Dr COGS / Cr Inventory, and records the customer invoice document. Revenue/AR/tax posting
/// is owned by Accounts Receivable and is intentionally out of this inventory-cost scope. The FIFO
/// inventory close later settles these Sold issues to their actual FIFO cost.
/// </summary>
public sealed class SalesInventoryInvoiceService : ISalesInventoryInvoiceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICompanyExecutionContext _company;
    private readonly ICurrentUserService _user;
    private readonly ISysNumberSequenceService _numbers;
    private DbContext Db => _unitOfWork.Context;

    public SalesInventoryInvoiceService(IUnitOfWork unitOfWork, ICompanyExecutionContext company,
        ICurrentUserService user, ISysNumberSequenceService numbers)
    {
        _unitOfWork = unitOfWork;
        _company = company;
        _user = user;
        _numbers = numbers;
    }

    public Task<(SalesInvoiceResultDto? Result, string? Error)> PostAsync(string salesId, DateTime? invoiceDate, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => PostCoreAsync(salesId, invoiceDate, ct));

    private async Task<(SalesInvoiceResultDto? Result, string? Error)> PostCoreAsync(string salesId, DateTime? invoiceDate, CancellationToken ct)
    {
        var companyId = _company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(companyId)) return (null, "A company must be selected.");
        var date = (invoiceDate ?? DateTime.UtcNow).Date;
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            await LockPostingKeyAsync($"SalesInvoice:{companyId}:{salesId}", ct);
            var order = await Db.Set<SalesTable>().FirstOrDefaultAsync(x => x.DataAreaId == companyId && x.SalesId == salesId, ct);
            if (order == null) return await Fail("Sales order was not found in the selected company.", ct);

            var lines = await Db.Set<SalesLine>().Where(x => x.DataAreaId == companyId && x.SalesId == salesId).ToListAsync(ct);
            var lineTransIds = lines.Select(x => x.InventTransId).Where(x => x != string.Empty).ToList();
            var origins = await Db.Set<InventTransOrigin>()
                .Where(x => x.DataAreaId == companyId && lineTransIds.Contains(x.InventTransId)
                    && x.ReferenceId == salesId && x.ReferenceCategory == InventRefType.SalesTable)
                .ToListAsync(ct);
            if (origins.Count == 0) return await Fail("No delivered inventory issues are available to invoice.", ct);
            var originIds = origins.Select(x => x.RecId).ToList();
            var delivered = await Db.Set<InventTrans>()
                .Where(x => x.DataAreaId == companyId && originIds.Contains(x.InventTransOrigin)
                    && x.StatusIssue == StatusIssue.Deducted)
                .ToListAsync(ct);
            if (delivered.Count == 0) return await Fail("There is no delivered, uninvoiced quantity to invoice.", ct);

            var context = await LoadLedgerContextAsync(companyId, date, ct);
            var invoiceId = (await _numbers.NextAsync("CustInvoiceJour", companyId, ct)).Code;
            var itemGroups = (await Db.Set<InventItemGroupItem>().AsNoTracking()
                .Where(x => x.ItemDataAreaId == companyId).ToListAsync(ct))
                .GroupBy(x => x.ItemId).ToDictionary(x => x.Key, x => x.First().ItemGroupId);

            decimal totalQty = 0, totalCogs = 0;
            int sold = 0, ledgerEntries = 0;
            foreach (var issue in delivered)
            {
                var site = await Db.Set<InventDim>().AsNoTracking()
                    .Where(x => x.DataAreaId == companyId && x.InventDimId == issue.InventDimId)
                    .Select(x => x.InventSiteId).FirstOrDefaultAsync(ct) ?? string.Empty;
                var unitCost = await ReceiptRunningAverageAsync(companyId, issue.ItemId, site, ct);
                var qty = -issue.Qty;                 // positive issued quantity
                var cogs = unitCost * qty;            // positive cost of goods sold
                issue.StatusIssue = StatusIssue.Sold;
                issue.DateFinancial = date;
                issue.DateStatus = date;
                issue.CostAmountPosted = -cogs;       // issue value is negative on the transaction
                issue.InvoiceId = invoiceId;
                issue.Voucher = invoiceId;

                var summary = await Db.Set<InventSum>().SingleOrDefaultAsync(x => x.DataAreaId == companyId
                    && x.ItemId == issue.ItemId && x.InventDimId == issue.InventDimId, ct);
                if (summary != null)
                {
                    summary.Deducted -= qty;
                    summary.PostedValue -= cogs;
                    summary.LastUpdDatePhysical = date;
                }

                var itemGroupId = itemGroups.GetValueOrDefault(issue.ItemId, string.Empty);
                await CreateCogsLedgerAsync(companyId, invoiceId, issue, itemGroupId, cogs, context, ct);
                ledgerEntries++;

                Db.Set<CustInvoiceTrans>().Add(new CustInvoiceTrans
                {
                    DataAreaId = companyId, InvoiceId = invoiceId, InvoiceDate = date, SalesId = salesId,
                    ItemId = issue.ItemId, InventDimId = issue.InventDimId, InventTransId = origins.First(o => o.RecId == issue.InventTransOrigin).InventTransId,
                    Qty = qty, QtyPhysical = qty, CurrencyCode = order.CurrencyCode
                });

                totalQty += qty;
                totalCogs += cogs;
                sold++;
            }

            Db.Set<CustInvoiceJour>().Add(new CustInvoiceJour
            {
                DataAreaId = companyId, InvoiceId = invoiceId, InvoiceDate = date, SalesId = salesId,
                OrderAccount = order.CustAccount, InvoiceAccount = string.IsNullOrWhiteSpace(order.InvoiceAccount) ? order.CustAccount : order.InvoiceAccount,
                CurrencyCode = order.CurrencyCode, LedgerVoucher = invoiceId, Qty = totalQty
            });

            await _unitOfWork.CompleteAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
            return (new SalesInvoiceResultDto(invoiceId, salesId, totalQty, totalCogs, sold, ledgerEntries), null);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            Db.ChangeTracker.Clear();
            throw;
        }
    }

    // Provisional issue cost = weighted average of financially posted receipts in the item/site pool
    // (spec §7: 2,200 / 200 = 11). The FIFO close later adjusts each issue to its actual FIFO cost.
    private async Task<decimal> ReceiptRunningAverageAsync(string companyId, string itemId, string site, CancellationToken ct)
    {
        var dimIds = await Db.Set<InventDim>().AsNoTracking()
            .Where(x => x.DataAreaId == companyId && x.InventSiteId == site).Select(x => x.InventDimId).ToListAsync(ct);
        var receipts = await Db.Set<InventTrans>().AsNoTracking()
            .Where(x => x.DataAreaId == companyId && x.ItemId == itemId && dimIds.Contains(x.InventDimId)
                && x.StatusReceipt == StatusReceipt.Purchased && x.Qty > 0)
            .Select(x => new { x.Qty, x.CostAmountPosted, x.CostAmountAdjustment }).ToListAsync(ct);
        var qty = receipts.Sum(x => x.Qty);
        var value = receipts.Sum(x => x.CostAmountPosted + x.CostAmountAdjustment);
        return qty <= 0 ? 0m : value / qty;
    }

    private async Task CreateCogsLedgerAsync(string companyId, string voucher, InventTrans issue,
        string itemGroupId, decimal cogs, LedgerContext context, CancellationToken ct)
    {
        var inventory = await ResolvePostingDimensionAsync(companyId, issue.ItemId, itemGroupId, InventAccountType.InventReceipt, ct);
        var cogsAccount = await ResolvePostingDimensionAsync(companyId, issue.ItemId, itemGroupId, InventAccountType.InventIssue, ct);
        var entry = new GeneralJournalEntry
        {
            DataAreaId = companyId, JournalNumber = voucher, Ledger = context.Ledger.RecId,
            JournalCategory = LedgerJournalCategory.General, PostingLayer = CurrentToOperationsPostingLayer.Current,
            SubledgerVoucher = voucher, SubledgerVoucherDataAreaId = companyId, DocumentNumber = voucher,
            AccountingDate = context.AccountingDate, DocumentDate = context.AccountingDate,
            FiscalCalendarPeriod = context.Period.RecId, FiscalCalendarYear = context.Period.FiscalCalendarYear
        };
        Db.Add(entry);
        await _unitOfWork.CompleteAsync(ct);
        // Dr COGS, Cr Inventory for the cost of goods sold.
        Db.AddRange(
            AccountEntry(companyId, entry.RecId, cogsAccount, LedgerPostingType.InventIssue, cogs, issue, context.Ledger.AccountingCurrency),
            AccountEntry(companyId, entry.RecId, inventory, LedgerPostingType.InventReceipt, -cogs, issue, context.Ledger.AccountingCurrency));
        Db.Add(new SubledgerVoucherGeneralJournalEntry
        {
            DataAreaId = companyId, GeneralJournalEntry = entry.RecId, Voucher = voucher,
            VoucherDataAreaId = companyId, AccountingDate = context.AccountingDate
        });
    }

    private static GeneralJournalAccountEntry AccountEntry(string companyId, long entryId,
        DimensionAttributeValueCombination dimension, LedgerPostingType type, decimal amount,
        InventTrans issue, string currency) => new()
    {
        DataAreaId = companyId, GeneralJournalEntry = entryId, LedgerDimension = dimension.RecId,
        MainAccount = dimension.MainAccount,
        LedgerAccount = dimension.DisplayValue.Length == 0 ? dimension.MainAccountValue : dimension.DisplayValue,
        Text = $"Sales invoice COGS for item {issue.ItemId}", PostingType = type,
        TransactionCurrencyCode = currency, TransactionCurrencyAmount = amount,
        AccountingCurrencyAmount = amount, ReportingCurrencyAmount = amount,
        Quantity = issue.Qty, IsCredit = amount < 0 ? NoYes.Yes : NoYes.No, PaymentReference = string.Empty
    };

    private async Task<DimensionAttributeValueCombination> ResolvePostingDimensionAsync(string companyId,
        string itemId, string itemGroupId, InventAccountType accountType, CancellationToken ct)
    {
        var rules = await Db.Set<InventPosting>().AsNoTracking()
            .Where(x => x.DataAreaId == companyId && x.InventAccountType == accountType && x.CustVendCode == TableGroupAll.All)
            .ToListAsync(ct);
        var rule = rules.Where(x => x.ItemCode == TableGroupAll.Table && x.ItemRelation == itemId).OrderBy(x => x.RecId).FirstOrDefault()
            ?? rules.Where(x => x.ItemCode == TableGroupAll.Group && x.ItemRelation == itemGroupId).OrderBy(x => x.RecId).FirstOrDefault()
            ?? rules.Where(x => x.ItemCode == TableGroupAll.All).OrderBy(x => x.RecId).FirstOrDefault();
        if (rule == null || rule.LedgerDimension == 0)
            throw new PostingValidationException($"Inventory posting setup is missing for {accountType} and item '{itemId}'.");
        var dimension = await Db.Set<DimensionAttributeValueCombination>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.DataAreaId == companyId && x.RecId == rule.LedgerDimension, ct)
            ?? throw new PostingValidationException($"The {accountType} posting rule references an invalid ledger dimension.");
        var valid = dimension.MainAccount > 0
            ? await Db.Set<MainAccount>().AsNoTracking().AnyAsync(x => x.DataAreaId == companyId && x.RecId == dimension.MainAccount, ct)
            : await Db.Set<MainAccount>().AsNoTracking().AnyAsync(x => x.DataAreaId == companyId && x.MainAccountId == dimension.MainAccountValue, ct);
        if (!valid) throw new PostingValidationException($"Ledger dimension '{dimension.DisplayValue}' does not reference a valid main account.");
        return dimension;
    }

    private async Task<LedgerContext> LoadLedgerContextAsync(string companyId, DateTime date, CancellationToken ct)
    {
        var ledger = await Db.Set<Ledger>().AsNoTracking().FirstOrDefaultAsync(x => x.DataAreaId == companyId, ct)
            ?? throw new PostingValidationException("A ledger is required for the selected company.");
        if (string.IsNullOrWhiteSpace(ledger.AccountingCurrency))
            throw new PostingValidationException("The ledger accounting currency is required.");
        var period = await Db.Set<FiscalCalendarPeriod>().AsNoTracking().FirstOrDefaultAsync(
            x => x.FiscalCalendar == ledger.FiscalCalendar && x.StartDate <= date && x.EndDate >= date && x.Type == FiscalPeriodType.Operating, ct)
            ?? throw new PostingValidationException($"No operating fiscal period contains {date:d}.");
        var ledgerPeriod = await Db.Set<LedgerFiscalCalendarPeriod>().AsNoTracking().FirstOrDefaultAsync(
            x => x.Ledger == ledger.RecId && x.FiscalCalendarPeriod == period.RecId, ct);
        if (ledgerPeriod == null || ledgerPeriod.Status != FiscalPeriodStatus.Open)
            throw new PostingValidationException($"The fiscal period for {date:d} is not open.");
        return new LedgerContext(ledger, period, date);
    }

    private async Task LockPostingKeyAsync(string key, CancellationToken ct)
    {
        if (Db.Database.ProviderName?.Contains("SqlServer", StringComparison.OrdinalIgnoreCase) != true) return;
        var resource = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key)));
        await using var command = Db.Database.GetDbConnection().CreateCommand();
        command.Transaction = Db.Database.CurrentTransaction!.GetDbTransaction();
        command.CommandText = "DECLARE @result int; EXEC @result = sys.sp_getapplock @Resource = @resource, @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 10000; SELECT @result;";
        var parameter = command.CreateParameter();
        parameter.ParameterName = "@resource";
        parameter.Value = "SalesInvoice:" + resource;
        command.Parameters.Add(parameter);
        if (await command.ExecuteScalarAsync(ct) is not int code || code < 0)
            throw new PostingValidationException("Sales invoice posting could not acquire its concurrency lock.");
    }

    private async Task<(SalesInvoiceResultDto? Result, string? Error)> Fail(string message, CancellationToken ct)
    {
        await _unitOfWork.RollbackTransactionAsync(ct);
        Db.ChangeTracker.Clear();
        return (null, message);
    }

    private sealed record LedgerContext(Ledger Ledger, FiscalCalendarPeriod Period, DateTime AccountingDate);
    private sealed class PostingValidationException(string message) : Exception(message);
}
