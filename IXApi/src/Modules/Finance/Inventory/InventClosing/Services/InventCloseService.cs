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
using ItemModelGroup = IAX.IXApi.Modules.Finance.Entities.InventModelGroup;

namespace IAX.IXApi.Modules.Finance.Inventory;

/// <summary>
/// Executes a basic FIFO inventory close: deterministic FIFO settlement of financially posted
/// issues against financially posted receipts within an item/financial-site pool, cost-adjustment
/// of each issue to its FIFO cost, settlement history, a ledger correction, and a close boundary.
/// Scope mirrors <see cref="FifoCostPreviewService"/>: journal-sourced FIFO items only. Recalculation,
/// close reversal, and non-journal source documents are deferred.
/// </summary>
public sealed class InventCloseService : IInventCloseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICompanyExecutionContext _company;
    private readonly ICurrentUserService _user;
    private readonly ISysNumberSequenceService _numbers;
    private DbContext Db => _unitOfWork.Context;

    public InventCloseService(IUnitOfWork unitOfWork, ICompanyExecutionContext company,
        ICurrentUserService user, ISysNumberSequenceService numbers)
    {
        _unitOfWork = unitOfWork;
        _company = company;
        _user = user;
        _numbers = numbers;
    }

    public Task<(InventCloseResultDto? Result, string? Error)> CloseAsync(DateTime cutoff, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => CloseCoreAsync(cutoff, ct));

    private async Task<(InventCloseResultDto? Result, string? Error)> CloseCoreAsync(DateTime cutoff, CancellationToken ct)
    {
        var companyId = _company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(companyId)) return (null, "A company must be selected.");
        var cutoffDate = cutoff.Date;
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            await LockPostingKeyAsync($"InventClose:{companyId}", ct);

            // Idempotency and close-boundary enforcement.
            var closings = await Db.Set<InventClosing>()
                .Where(x => x.DataAreaId == companyId && x.AdjustmentType == InventAdjustmentType.Closing)
                .ToListAsync(ct);
            var existing = closings
                .Where(x => x.TransDate.Date == cutoffDate && x.InventCostStatus == InventCostStatus.Finished)
                .OrderByDescending(x => x.RunNum).FirstOrDefault();
            if (existing != null)
            {
                await _unitOfWork.CommitTransactionAsync(ct);
                return (new InventCloseResultDto(existing.RecId, existing.Voucher, cutoffDate,
                    existing.RunNum, 0, 0, 0, 0m), null);
            }
            var lastRun = closings.Where(x => x.InventCostStatus == InventCostStatus.Finished).ToList();
            if (lastRun.Any(x => x.TransDate.Date >= cutoffDate))
                throw new PostingValidationException(
                    "A later or equal inventory close already exists; backdated close is not supported in the basic scope.");
            var runNum = (lastRun.Count == 0 ? 0 : lastRun.Max(x => x.RunNum)) + 1;

            var context = await LoadLedgerContextAsync(companyId, cutoffDate, ct);
            var pools = await LoadFifoPoolsAsync(companyId, cutoffDate, ct);

            var voucher = (await _numbers.NextAsync("InventClosing", companyId, ct)).Code;
            var settlementsCreated = 0;
            var issuesSettled = 0;
            var ledgerEntriesCreated = 0;
            var totalAdjustment = 0m;

            foreach (var pool in pools)
            {
                // FIFO layers from financially posted receipts, oldest first.
                var layers = new LinkedList<Layer>();
                foreach (var row in pool.Rows)
                {
                    var trans = row.Transaction;
                    var inventTransId = row.InventTransId;
                    if (trans.Qty > 0 && trans.StatusReceipt == StatusReceipt.Purchased)
                    {
                        var receiptValue = trans.CostAmountPosted + trans.CostAmountAdjustment;
                        if (receiptValue <= 0)
                            throw new PostingValidationException(
                                $"Receipt transaction {trans.RecId} has no positive financial cost.");
                        layers.AddLast(new Layer { Receipt = trans, Remaining = trans.Qty, UnitCost = receiptValue / trans.Qty });
                        continue;
                    }
                    if (trans.Qty >= 0 || trans.StatusIssue != StatusIssue.Sold)
                        throw new PostingValidationException(
                            $"Transaction {trans.RecId} has an unsupported financial quantity/status combination for close.");

                    var needed = -trans.Qty;
                    var fifoCost = 0m;
                    while (needed > 0 && layers.Count > 0)
                    {
                        var layer = layers.First!.Value;
                        var quantity = Math.Min(needed, layer.Remaining);
                        var layerCost = quantity * layer.UnitCost;
                        fifoCost += layerCost;
                        needed -= quantity;
                        layer.Receipt.QtySettled += quantity;
                        layer.Remaining -= quantity;
                        if (layer.Remaining <= 0)
                        {
                            if (layer.Receipt.Qty - layer.Receipt.QtySettled <= 0)
                                layer.Receipt.ValueOpen = InventTransOpen.No;
                            layers.RemoveFirst();
                        }
                        settlementsCreated++;
                        Db.Add(new InventSettlement
                        {
                            DataAreaId = companyId, SettleTransId = (await _numbers.NextAsync("SettleTransId", companyId, ct)).Code,
                            InventTransId = inventTransId, ItemId = trans.ItemId, ItemGroupId = pool.ItemGroupId,
                            Voucher = voucher, TransDate = cutoffDate, TransRecId = trans.RecId,
                            QtySettled = -quantity, CostAmountSettled = -layerCost, CostAmountAdjustment = 0m,
                            SettleType = InventSettleType.FinancialClose, SettleModel = InventModel.FIFO,
                            Posted = NoYes.Yes, TransBeginTime = DateTime.UtcNow
                        });
                    }
                    if (needed > 0)
                        throw new PostingValidationException(
                            $"Issue transaction {trans.RecId} lacks {needed} financially received units in its FIFO pool.");

                    // target is the negative FIFO cost of the issued quantity.
                    var target = -fifoCost;
                    var recognized = trans.CostAmountPosted + trans.CostAmountAdjustment;
                    var adjustment = target - recognized;
                    trans.QtySettled = trans.Qty;
                    trans.CostAmountAdjustment += adjustment;
                    trans.CostAmountSettled = target;
                    trans.ValueOpen = InventTransOpen.No;
                    trans.DateClosed = cutoffDate;
                    issuesSettled++;
                    totalAdjustment += adjustment;

                    if (adjustment != 0m)
                    {
                        // FIFO close moves value between COGS and remaining inventory.
                        // adjustment d>0 => provisional COGS overstated: credit COGS, debit inventory.
                        await AdjustInventorySummaryAsync(companyId, trans.ItemId, trans.InventDimId, adjustment, ct);
                        await CreateLedgerCorrectionAsync(companyId, voucher, trans, inventTransId, pool.ItemGroupId, adjustment, context, ct);
                        ledgerEntriesCreated++;
                    }
                }
            }

            var closing = new InventClosing
            {
                DataAreaId = companyId, Voucher = voucher, TransDate = cutoffDate,
                PeriodCode = Truncate(context.Period.Name, FieldLengths.PeriodCode),
                RunNum = runNum, NextRunNum = runNum + 1,
                AdjustmentType = InventAdjustmentType.Closing, AdjustmentSpec = InventAdjustmentSpec.Total,
                InventCostStatus = InventCostStatus.Finished, Executed = DateTime.UtcNow,
                Active = NoYes.Yes, Ledger = NoYes.Yes, Start_ = NoYes.Yes, End_ = NoYes.Yes,
                Notes = $"FIFO close run {runNum} by {_user.GetCurrentUserId()}"
            };
            Db.Add(closing);
            await _unitOfWork.CompleteAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
            return (new InventCloseResultDto(closing.RecId, voucher, cutoffDate, runNum,
                issuesSettled, settlementsCreated, ledgerEntriesCreated, totalAdjustment), null);
        }
        catch (PostingValidationException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            Db.ChangeTracker.Clear();
            return (null, ex.Message);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            Db.ChangeTracker.Clear();
            throw;
        }
    }

    private async Task<List<Pool>> LoadFifoPoolsAsync(string companyId, DateTime cutoffDate, CancellationToken ct)
    {
        var end = cutoffDate.AddDays(1);
        var transactions = (await Db.Set<InventTrans>()
            .Where(x => x.DataAreaId == companyId && x.DateFinancial < end &&
                (x.StatusReceipt == StatusReceipt.Purchased || x.StatusIssue == StatusIssue.Sold))
            .ToListAsync(ct))
            .OrderBy(x => x.DateFinancial).ThenBy(x => x.RecId).ToList();
        if (transactions.Count == 0) return [];

        var dimensionIds = transactions.Select(x => x.InventDimId).Distinct().ToList();
        var dimensions = await Db.Set<InventDim>().AsNoTracking()
            .Where(x => x.DataAreaId == companyId && dimensionIds.Contains(x.InventDimId))
            .ToDictionaryAsync(x => x.InventDimId, ct);
        var originIds = transactions.Select(x => x.InventTransOrigin).Distinct().ToList();
        var origins = await Db.Set<InventTransOrigin>().AsNoTracking()
            .Where(x => x.DataAreaId == companyId && originIds.Contains(x.RecId))
            .ToDictionaryAsync(x => x.RecId, ct);
        var transferSet = (await Db.Set<InventJournalTrans>().AsNoTracking()
            .Where(x => x.DataAreaId == companyId && x.JournalType == InventJournalType.Transfer)
            .Select(x => new { x.InventTransId, x.ToInventTransId }).ToListAsync(ct))
            .SelectMany(x => new[] { x.InventTransId, x.ToInventTransId }).ToHashSet(StringComparer.Ordinal);
        var assignments = await Db.Set<InventModelGroupItem>().AsNoTracking()
            .Where(x => x.ItemDataAreaId == companyId).ToListAsync(ct);
        var models = await Db.Set<ItemModelGroup>().AsNoTracking()
            .Where(x => x.DataAreaId == companyId).ToDictionaryAsync(x => x.ModelGroupId, ct);
        var itemGroups = (await Db.Set<InventItemGroupItem>().AsNoTracking()
            .Where(x => x.ItemDataAreaId == companyId).ToListAsync(ct))
            .GroupBy(x => x.ItemId).ToDictionary(x => x.Key, x => x.First().ItemGroupId);

        var rows = new List<(InventTrans Transaction, string Site, string ItemGroupId, string InventTransId)>();
        foreach (var transaction in transactions)
        {
            if (!dimensions.TryGetValue(transaction.InventDimId, out var dim) || string.IsNullOrWhiteSpace(dim.InventSiteId))
                throw new PostingValidationException($"Transaction {transaction.RecId} has no valid financial site dimension.");
            if (!origins.TryGetValue(transaction.InventTransOrigin, out var origin) ||
                origin.ReferenceCategory != InventRefType.InventoryJournal || transferSet.Contains(origin.InventTransId))
                throw new PostingValidationException(
                    $"Transaction {transaction.RecId} needs a verified journal source or transfer cost relationship before close.");
            var assignment = assignments.FirstOrDefault(x => x.ItemId == transaction.ItemId);
            if (assignment is null || !models.TryGetValue(assignment.ModelGroupId, out var model) || model.InventModel != InventModel.FIFO)
                throw new PostingValidationException($"Item {transaction.ItemId} does not have a verified FIFO model group.");
            rows.Add((transaction, dim.InventSiteId,
                itemGroups.GetValueOrDefault(transaction.ItemId, string.Empty), origin.InventTransId));
        }

        return rows.GroupBy(x => (x.Transaction.ItemId, x.Site))
            .Select(g => new Pool(g.Key.ItemId, g.Key.Site, g.First().ItemGroupId,
                g.OrderBy(x => x.Transaction.DateFinancial).ThenBy(x => x.Transaction.RecId)
                    .Select(x => (x.Transaction, x.InventTransId)).ToList()))
            .ToList();
    }

    private async Task AdjustInventorySummaryAsync(string companyId, string itemId, string dimId, decimal value, CancellationToken ct)
    {
        var sum = await Db.Set<InventSum>().FirstOrDefaultAsync(
            x => x.DataAreaId == companyId && x.ItemId == itemId && x.InventDimId == dimId, ct);
        if (sum == null) return;
        sum.PostedValue += value;
        sum.LastUpdDatePhysical = DateTime.UtcNow;
    }

    private async Task CreateLedgerCorrectionAsync(string companyId, string voucher, InventTrans issue,
        string inventTransId, string itemGroupId, decimal adjustment, LedgerContext context, CancellationToken ct)
    {
        var inventory = await ResolvePostingDimensionAsync(companyId, issue.ItemId, itemGroupId, InventAccountType.InventReceipt, ct);
        var cogs = await ResolvePostingDimensionAsync(companyId, issue.ItemId, itemGroupId, InventAccountType.InventIssue, ct);
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
        Db.AddRange(
            AccountEntry(companyId, entry.RecId, inventory, LedgerPostingType.InventReceipt, adjustment, issue, inventTransId, context.Ledger.AccountingCurrency),
            AccountEntry(companyId, entry.RecId, cogs, LedgerPostingType.InventIssue, -adjustment, issue, inventTransId, context.Ledger.AccountingCurrency));
        Db.Add(new SubledgerVoucherGeneralJournalEntry
        {
            DataAreaId = companyId, GeneralJournalEntry = entry.RecId, Voucher = voucher,
            VoucherDataAreaId = companyId, AccountingDate = context.AccountingDate
        });
    }

    private static GeneralJournalAccountEntry AccountEntry(string companyId, long entryId,
        DimensionAttributeValueCombination dimension, LedgerPostingType type, decimal amount,
        InventTrans issue, string inventTransId, string currency) => new()
    {
        DataAreaId = companyId, GeneralJournalEntry = entryId, LedgerDimension = dimension.RecId,
        MainAccount = dimension.MainAccount,
        LedgerAccount = dimension.DisplayValue.Length == 0 ? dimension.MainAccountValue : dimension.DisplayValue,
        Text = $"FIFO close adjustment for issue {inventTransId}", PostingType = type,
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
        if (!string.IsNullOrWhiteSpace(ledger.ReportingCurrency) &&
            !string.Equals(ledger.ReportingCurrency, ledger.AccountingCurrency, StringComparison.OrdinalIgnoreCase))
            throw new PostingValidationException("Inventory close with a different reporting currency requires exchange-rate processing, which is not enabled in the basic inventory scope.");
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
        parameter.Value = "InventClose:" + resource;
        command.Parameters.Add(parameter);
        if (await command.ExecuteScalarAsync(ct) is not int code || code < 0)
            throw new PostingValidationException("Inventory close could not acquire its concurrency lock.");
    }

    private static string Truncate(string value, int max) =>
        string.IsNullOrEmpty(value) || value.Length <= max ? value : value[..max];

    private sealed class Layer
    {
        public required InventTrans Receipt { get; init; }
        public decimal Remaining { get; set; }
        public decimal UnitCost { get; init; }
    }

    private sealed record Pool(string ItemId, string Site, string ItemGroupId,
        List<(InventTrans Transaction, string InventTransId)> Rows);

    private sealed record LedgerContext(Ledger Ledger, FiscalCalendarPeriod Period, DateTime AccountingDate);
    private sealed class PostingValidationException(string message) : Exception(message);
}
