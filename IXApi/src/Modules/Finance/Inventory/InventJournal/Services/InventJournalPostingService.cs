using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;
using ReleasedItem = IAX.IXApi.Modules.Finance.Entities.InventTable;
using ItemModelGroup = IAX.IXApi.Modules.Finance.Entities.InventModelGroup;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventJournalPostingService : IInventJournalPostingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICompanyExecutionContext _company;
    private readonly ICurrentUserService _user;
    private readonly ISysNumberSequenceService _numbers;
    private readonly ISalesInventoryNumberService _inventoryNumbers;
    private DbContext Db => _unitOfWork.Context;

    public InventJournalPostingService(IUnitOfWork unitOfWork, ICompanyExecutionContext company,
        ICurrentUserService user, ISysNumberSequenceService numbers, ISalesInventoryNumberService inventoryNumbers)
    {
        _unitOfWork = unitOfWork;
        _company = company;
        _user = user;
        _numbers = numbers;
        _inventoryNumbers = inventoryNumbers;
    }

    public async Task<(InventJournalPostResultDto? Result, string? Error)> PostAsync(string journalId, CancellationToken ct)
    {
        var companyId = _company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(companyId)) return (null, "A company must be selected.");
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var header = await Db.Set<InventJournalTable>().FirstOrDefaultAsync(x => x.DataAreaId == companyId && x.JournalId == Normalize(journalId), ct)
                ?? throw new PostingValidationException("Inventory journal was not found.");
            if (header.Posted == NoYes.Yes) throw new PostingValidationException("The inventory journal has already been posted.");
            var lines = (await Db.Set<InventJournalTrans>().Where(x => x.DataAreaId == companyId && x.JournalId == header.JournalId).ToListAsync(ct))
                .OrderBy(x => x.LineNum).ToList();
            if (lines.Count == 0) throw new PostingValidationException("The inventory journal has no lines.");
            if (lines.Any(x => x.JournalType != header.JournalType)) throw new PostingValidationException("A journal line type does not match the journal header.");

            var name = await Db.Set<InventJournalName>().AsNoTracking().FirstOrDefaultAsync(x => x.DataAreaId == companyId && x.JournalNameId == header.JournalNameId, ct)
                ?? throw new PostingValidationException("The journal name setup no longer exists.");
            if (name.JournalType != header.JournalType) throw new PostingValidationException("The journal type no longer matches its journal name setup.");
            var voucherSequence = await Db.Set<SysNumberSequence>().IgnoreQueryFilters().AsNoTracking()
                .FirstOrDefaultAsync(x => x.RecId == header.VoucherNumberSequenceTable && !x.IsDeleted && x.IsActive && x.InUse != 0 &&
                    (x.DataAreaId == companyId || (x.DataAreaId == string.Empty && x.NumberSequenceScope == null)), ct)
                ?? throw new PostingValidationException("A valid voucher number sequence is required before posting.");

            var requiresLedger = header.JournalType != InventJournalType.Transfer;
            var ledgerContext = requiresLedger ? await LoadLedgerContextAsync(companyId, lines, ct) : null;
            var inventoryCurrency = ledgerContext?.Ledger.AccountingCurrency ?? await Db.Set<Ledger>().AsNoTracking()
                .Where(x => x.DataAreaId == companyId).Select(x => x.AccountingCurrency).FirstOrDefaultAsync(ct) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(inventoryCurrency))
                throw new PostingValidationException("The ledger accounting currency is required.");
            var result = new InventJournalPostResultDto { JournalId = header.JournalId, PostedDateTime = DateTime.UtcNow };

            foreach (var line in lines)
            {
                var item = await LoadReadyItemAsync(companyId, line.ItemId, ct);
                var fromDim = await Db.Set<InventDim>().FirstOrDefaultAsync(x => x.DataAreaId == companyId && x.InventDimId == line.InventDimId, ct)
                    ?? throw new PostingValidationException($"Inventory dimension '{line.InventDimId}' was not found for line {line.LineNum}.");
                var inventoryQty = await ConvertToInventoryUnitAsync(item.Product, item.ItemId, line.Unit, line.Qty, ct);
                var fromSum = await GetOrCreateSummaryAsync(companyId, item.ItemId, fromDim, ct);
                decimal signedQty;
                if (header.JournalType == InventJournalType.Counting)
                {
                    var counted = await ConvertToInventoryUnitAsync(item.Product, item.ItemId, line.Unit, line.Counted, ct);
                    line.InventOnHand = fromSum.PhysicalInvent;
                    signedQty = counted - fromSum.PhysicalInvent;
                    line.Qty = signedQty;
                }
                else signedQty = header.JournalType == InventJournalType.Transfer ? -Math.Abs(inventoryQty) : inventoryQty;

                await ValidateTrackingDimensionsAsync(item, fromDim, signedQty, ct);

                if (signedQty < 0 && fromSum.PhysicalInvent + signedQty < 0 && item.Model.NegativePhysical != NoYes.Yes)
                    throw new PostingValidationException($"Line {line.LineNum} would create negative physical inventory for item '{item.ItemId}'.");

                if (signedQty == 0)
                {
                    line.Voucher = string.Empty;
                    continue;
                }
                var unitCost = header.JournalType == InventJournalType.Transfer || signedQty < 0 || header.JournalType == InventJournalType.Counting
                    ? GetRunningAverageCost(fromSum)
                    : line.CostPrice;
                if (unitCost <= 0 && signedQty < 0 && item.Model.NegativePhysical == NoYes.Yes)
                    unitCost = await GetDefaultItemCostAsync(item.ItemId, line.TransDate, ct);
                if (unitCost <= 0 && signedQty > 0 && header.JournalType == InventJournalType.Counting)
                    unitCost = await GetDefaultItemCostAsync(item.ItemId, line.TransDate, ct);
                if (unitCost < 0 || (signedQty > 0 && unitCost == 0 && header.JournalType != InventJournalType.Transfer))
                    throw new PostingValidationException($"A valid receipt cost is required for item '{item.ItemId}' on line {line.LineNum}.");
                var voucher = (await _numbers.NextAsync(voucherSequence.NumberSequence, companyId, ct)).Code;
                result.Vouchers.Add(voucher);

                if (header.JournalType == InventJournalType.Transfer)
                {
                    if (string.IsNullOrWhiteSpace(line.ToInventDimId)) throw new PostingValidationException($"A destination dimension is required on transfer line {line.LineNum}.");
                    var toDim = await Db.Set<InventDim>().FirstOrDefaultAsync(x => x.DataAreaId == companyId && x.InventDimId == line.ToInventDimId, ct)
                        ?? throw new PostingValidationException($"Destination inventory dimension '{line.ToInventDimId}' was not found.");
                    if (toDim.InventDimId == fromDim.InventDimId) throw new PostingValidationException("Transfer source and destination dimensions must be different.");
                    await ValidateTrackingDimensionsAsync(item, toDim, Math.Abs(inventoryQty), ct);
                    var toSum = await GetOrCreateSummaryAsync(companyId, item.ItemId, toDim, ct);
                    var transferQty = Math.Abs(inventoryQty);
                    await CreateInventoryTransactionAsync(companyId, header, line, item.ItemId, fromDim.InventDimId, -transferQty, unitCost, inventoryCurrency, voucher, ct);
                    await CreateInventoryTransactionAsync(companyId, header, line, item.ItemId, toDim.InventDimId, transferQty, unitCost, inventoryCurrency, voucher, ct);
                    ApplySummary(fromSum, -transferQty, unitCost);
                    ApplySummary(toSum, transferQty, unitCost);
                    line.CostAmount = transferQty * unitCost;
                    result.InventoryTransactionsCreated += 2;
                }
                else if (signedQty != 0)
                {
                    var costAmount = signedQty * unitCost;
                    await CreateInventoryTransactionAsync(companyId, header, line, item.ItemId, fromDim.InventDimId, signedQty, unitCost, inventoryCurrency, voucher, ct);
                    ApplySummary(fromSum, signedQty, unitCost);
                    line.CostAmount = costAmount;
                    var accounts = await ResolveAccountsAsync(companyId, item.ItemId, signedQty, line.LedgerDimension, header.JournalType, ct);
                    await CreateLedgerEntryAsync(companyId, header, line, voucher, costAmount, accounts, ledgerContext!, ct);
                    result.InventoryTransactionsCreated++;
                    result.LedgerEntriesCreated++;
                }
                line.Voucher = voucher;
            }

            header.Posted = NoYes.Yes;
            header.PostedDateTime = result.PostedDateTime;
            header.PostedUserId = _user.GetCurrentUserId();
            await _unitOfWork.CompleteAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
            result.Posted = true;
            return (result, null);
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

    private async Task<ReadyItem> LoadReadyItemAsync(string companyId, string itemId, CancellationToken ct)
    {
        var item = await Db.Set<ReleasedItem>().AsNoTracking().FirstOrDefaultAsync(x => x.DataAreaId == companyId && x.ItemId == itemId, ct)
            ?? throw new PostingValidationException($"Released item '{itemId}' was not found.");
        var assignment = await Db.Set<InventModelGroupItem>().AsNoTracking().FirstOrDefaultAsync(x => x.ItemDataAreaId == companyId && x.ItemId == itemId, ct)
            ?? throw new PostingValidationException($"Item model group is required for item '{itemId}'.");
        var model = await Db.Set<ItemModelGroup>().AsNoTracking().FirstOrDefaultAsync(x => x.DataAreaId == companyId && x.ModelGroupId == assignment.ModelGroupId, ct)
            ?? throw new PostingValidationException($"Item model group '{assignment.ModelGroupId}' was not found.");
        if (model.StockedProduct != NoYes.Yes) throw new PostingValidationException($"Item '{itemId}' is not configured as a stocked product.");
        var tracking = await Db.Set<EcoResTrackingDimensionGroupItem>().AsNoTracking().FirstOrDefaultAsync(x => x.ItemDataAreaId == companyId && x.ItemId == itemId, ct)
            ?? throw new PostingValidationException($"A tracking dimension group is required for item '{itemId}'.");
        return new ReadyItem(item.ItemId, item.Product, model, tracking.TrackingDimensionGroup);
    }

    private async Task ValidateTrackingDimensionsAsync(ReadyItem item, InventDim dim, decimal signedQty, CancellationToken ct)
    {
        var setup = await Db.Set<EcoResTrackingDimensionGroupFldSetup>().AsNoTracking()
            .Where(x => x.TrackingDimensionGroup == item.TrackingDimensionGroup &&
                (x.DimensionFieldId == InventDimensionFields.BatchNumber || x.DimensionFieldId == InventDimensionFields.SerialNumber))
            .ToListAsync(ct);
        var batch = setup.FirstOrDefault(x => x.DimensionFieldId == InventDimensionFields.BatchNumber);
        var serial = setup.FirstOrDefault(x => x.DimensionFieldId == InventDimensionFields.SerialNumber);
        var batchId = dim.InventBatchId.Trim();
        var serialId = dim.InventSerialId.Trim();
        var isIssue = signedQty < 0;
        if (batch?.IsPhysicalInventoryEnabled == true && batchId.Length == 0 &&
            !(isIssue ? batch.IsAllowBlankIssueEnabled : batch.IsAllowBlankReceiptEnabled))
            throw new PostingValidationException($"A batch number is required for item '{item.ItemId}'.");
        if (serial?.IsPhysicalInventoryEnabled == true && serialId.Length == 0 &&
            !(isIssue ? serial.IsAllowBlankIssueEnabled : serial.IsAllowBlankReceiptEnabled))
            throw new PostingValidationException($"A serial number is required for item '{item.ItemId}'.");
        if (batchId.Length > 0 && !await Db.Set<InventBatch>().AnyAsync(x => x.ItemId == item.ItemId && x.InventBatchId == batchId, ct))
            throw new PostingValidationException($"Batch '{batchId}' is not defined for item '{item.ItemId}'.");
        if (serialId.Length > 0 && !await Db.Set<InventSerial>().AnyAsync(x => x.ItemId == item.ItemId && x.InventSerialId == serialId, ct))
            throw new PostingValidationException($"Serial number '{serialId}' is not defined for item '{item.ItemId}'.");
        if (serial?.IsPhysicalInventoryEnabled == true && signedQty != 0 &&
            (serialId.Length == 0 || Math.Abs(signedQty) != 1m))
            throw new PostingValidationException($"Each journal line for serial-tracked item '{item.ItemId}' must identify one serial-numbered unit.");
    }

    private async Task<decimal> ConvertToInventoryUnitAsync(long product, string itemId, string fromSymbol, decimal qty, CancellationToken ct)
    {
        var inventoryUnit = await Db.Set<InventTableModule>().AsNoTracking().Where(x => x.ItemId == itemId && x.ModuleType == ModuleInventPurchSales.Inventory)
            .Select(x => x.UnitId).FirstOrDefaultAsync(ct);
        if (string.IsNullOrWhiteSpace(inventoryUnit)) throw new PostingValidationException($"Inventory unit is required for item '{itemId}'.");
        if (string.Equals(fromSymbol, inventoryUnit, StringComparison.OrdinalIgnoreCase)) return qty;
        var symbols = await Db.Set<UnitOfMeasure>().IgnoreQueryFilters().AsNoTracking()
            .Where(x => x.Symbol == fromSymbol || x.Symbol == inventoryUnit).ToListAsync(ct);
        var from = symbols.FirstOrDefault(x => x.Symbol == fromSymbol);
        var to = symbols.FirstOrDefault(x => x.Symbol == inventoryUnit);
        if (from == null || to == null) throw new PostingValidationException($"Unit conversion from '{fromSymbol}' to '{inventoryUnit}' is not configured.");
        var direct = await Db.Set<UnitOfMeasureConversion>().AsNoTracking()
            .Where(x => (x.Product == product || x.Product == 0) && x.FromUnitOfMeasure == from.RecId && x.ToUnitOfMeasure == to.RecId)
            .OrderByDescending(x => x.Product == product).ThenByDescending(x => x.RecId).FirstOrDefaultAsync(ct);
        var reverse = direct == null ? await Db.Set<UnitOfMeasureConversion>().AsNoTracking()
            .Where(x => (x.Product == product || x.Product == 0) && x.FromUnitOfMeasure == to.RecId && x.ToUnitOfMeasure == from.RecId)
            .OrderByDescending(x => x.Product == product).ThenByDescending(x => x.RecId).FirstOrDefaultAsync(ct) : null;
        if (direct == null && reverse == null) throw new PostingValidationException($"Unit conversion from '{fromSymbol}' to '{inventoryUnit}' is not configured.");
        decimal converted;
        UnitOfMeasureRounding rounding;
        if (direct != null)
        {
            var factor = EffectiveFactor(direct);
            converted = (qty + direct.InnerOffset) * factor + direct.OuterOffset;
            rounding = direct.Rounding;
        }
        else
        {
            var factor = EffectiveFactor(reverse!);
            if (factor == 0) throw new PostingValidationException("The reverse unit conversion factor cannot be zero.");
            converted = ((qty - reverse!.OuterOffset) / factor) - reverse.InnerOffset;
            rounding = reverse.Rounding;
        }
        return Round(converted, to.DecimalPrecision, rounding);
    }

    private async Task<InventSum> GetOrCreateSummaryAsync(string companyId, string itemId, InventDim dim, CancellationToken ct)
    {
        var sum = await Db.Set<InventSum>().FirstOrDefaultAsync(x => x.DataAreaId == companyId && x.ItemId == itemId && x.InventDimId == dim.InventDimId, ct);
        if (sum != null) return sum;
        sum = new InventSum
        {
            DataAreaId = companyId, ItemId = itemId, InventDimId = dim.InventDimId,
            InventSiteId = dim.InventSiteId, InventLocationId = dim.InventLocationId,
            WmsLocationId = dim.WmsLocationId, LicensePlateId = dim.LicensePlateId,
            ConfigId = dim.ConfigId, InventSizeId = dim.InventSizeId, InventColorId = dim.InventColorId,
            InventStyleId = dim.InventStyleId, InventVersionId = dim.InventVersionId,
            InventBatchId = dim.InventBatchId, InventSerialId = dim.InventSerialId, InventStatusId = dim.InventStatusId
        };
        Db.Add(sum);
        return sum;
    }

    private async Task CreateInventoryTransactionAsync(string companyId, InventJournalTable header, InventJournalTrans line,
        string itemId, string dimId, decimal qty, decimal unitCost, string currency, string voucher, CancellationToken ct)
    {
        var transId = await _inventoryNumbers.NextInventTransIdAsync(ct);
        var origin = new InventTransOrigin
        {
            DataAreaId = companyId, InventTransId = transId, ItemId = itemId, ItemInventDimId = dimId,
            ReferenceId = header.JournalId, ReferenceCategory = line.InventRefType
        };
        Db.Add(origin);
        await _unitOfWork.CompleteAsync(ct);
        Db.Add(new InventTrans
        {
            DataAreaId = companyId, ItemId = itemId, InventDimId = dimId, InventTransOrigin = origin.RecId,
            Qty = qty, QtySettled = 0, CostAmountPosted = qty * unitCost, CurrencyCode = currency,
            Voucher = voucher, VoucherPhysical = voucher, InvoiceId = string.Empty, PackingSlipId = string.Empty,
            StatusReceipt = qty > 0 ? StatusReceipt.Purchased : StatusReceipt.None,
            StatusIssue = qty < 0 ? StatusIssue.Sold : StatusIssue.None,
            ValueOpen = InventTransOpen.Yes, DateStatus = line.TransDate, DatePhysical = line.TransDate,
            DateFinancial = line.TransDate, DateInvent = line.TransDate, ProjId = string.Empty,
            ProjCategoryId = string.Empty, ProjAdjustRefId = string.Empty, PickingRouteId = string.Empty,
            TransChildRefId = string.Empty
        });
        if (string.IsNullOrEmpty(line.InventTransId)) line.InventTransId = transId; else line.ToInventTransId = transId;
    }

    private static decimal GetRunningAverageCost(InventSum sum) =>
        sum.PhysicalInvent == 0 ? 0m : Math.Abs((sum.PostedValue + sum.PhysicalValue) / sum.PhysicalInvent);

    private async Task<decimal> GetDefaultItemCostAsync(string itemId, DateTime transDate, CancellationToken ct) =>
        await Db.Set<InventItemPrice>().AsNoTracking()
            .Where(x => x.ItemId == itemId && x.PriceType == PreferredPriceType.CostPrice &&
                x.CostingType == CostingType.StandardCost && x.ActivationDate <= transDate)
            .OrderByDescending(x => x.ActivationDate).Select(x => (decimal?)x.Price).FirstOrDefaultAsync(ct) ?? 0m;

    private static void ApplySummary(InventSum sum, decimal qty, decimal unitCost)
    {
        sum.PostedQty += qty;
        sum.PhysicalInvent += qty;
        sum.AvailPhysical = sum.PhysicalInvent - sum.ReservPhysical;
        sum.AvailOrdered = sum.AvailPhysical + sum.Ordered - sum.OnOrder - sum.ReservOrdered;
        sum.PostedValue += qty * unitCost;
        sum.LastUpdDatePhysical = DateTime.UtcNow;
        sum.Closed = sum.PhysicalInvent == 0 ? NoYes.Yes : NoYes.No;
        sum.ClosedQty = sum.PhysicalInvent == 0 ? NoYes.Yes : NoYes.No;
    }

    private async Task<LedgerContext> LoadLedgerContextAsync(string companyId, IReadOnlyCollection<InventJournalTrans> lines, CancellationToken ct)
    {
        var date = lines.Min(x => x.TransDate == default ? DateTime.Today : x.TransDate.Date);
        if (lines.Any(x => (x.TransDate == default ? DateTime.Today : x.TransDate.Date) != date))
            throw new PostingValidationException("All lines must use the same transaction date for basic journal posting.");
        var ledger = await Db.Set<Ledger>().AsNoTracking().FirstOrDefaultAsync(x => x.DataAreaId == companyId, ct)
            ?? throw new PostingValidationException("A ledger is required for the selected company.");
        if (string.IsNullOrWhiteSpace(ledger.AccountingCurrency))
            throw new PostingValidationException("The ledger accounting currency is required.");
        if (!string.IsNullOrWhiteSpace(ledger.ReportingCurrency) &&
            !string.Equals(ledger.ReportingCurrency, ledger.AccountingCurrency, StringComparison.OrdinalIgnoreCase))
            throw new PostingValidationException("Inventory journal posting with a different reporting currency requires exchange-rate processing, which is not enabled in the basic inventory scope.");
        var period = await Db.Set<FiscalCalendarPeriod>().AsNoTracking().FirstOrDefaultAsync(x => x.FiscalCalendar == ledger.FiscalCalendar && x.StartDate <= date && x.EndDate >= date && x.Type == FiscalPeriodType.Operating, ct)
            ?? throw new PostingValidationException($"No operating fiscal period contains {date:d}.");
        var ledgerPeriod = await Db.Set<LedgerFiscalCalendarPeriod>().AsNoTracking().FirstOrDefaultAsync(x => x.Ledger == ledger.RecId && x.FiscalCalendarPeriod == period.RecId, ct);
        if (ledgerPeriod == null || ledgerPeriod.Status != FiscalPeriodStatus.Open)
            throw new PostingValidationException($"The fiscal period for {date:d} is not open.");
        return new LedgerContext(ledger, period);
    }

    private async Task<PostingAccounts> ResolveAccountsAsync(string companyId, string itemId, decimal qty,
        long movementOffset, InventJournalType journalType, CancellationToken ct)
    {
        var inventoryType = qty > 0 ? InventAccountType.InventReceipt : InventAccountType.InventIssue;
        var inventory = await ResolvePostingDimensionAsync(companyId, itemId, inventoryType, ct);
        DimensionAttributeValueCombination offset;
        LedgerPostingType offsetType;
        if (journalType == InventJournalType.Movement)
        {
            offset = await Db.Set<DimensionAttributeValueCombination>().AsNoTracking().FirstOrDefaultAsync(x => x.DataAreaId == companyId && x.RecId == movementOffset, ct)
                ?? throw new PostingValidationException("The movement offset ledger account is invalid for the selected company.");
            await ValidateMainAccountAsync(companyId, offset, ct);
            offsetType = LedgerPostingType.LedgerJournal;
        }
        else
        {
            var type = qty > 0 ? InventAccountType.InventProfit : InventAccountType.InventLoss;
            offset = await ResolvePostingDimensionAsync(companyId, itemId, type, ct);
            offsetType = qty > 0 ? LedgerPostingType.InventProfit : LedgerPostingType.InventLoss;
        }
        return new PostingAccounts(inventory, offset,
            qty > 0 ? LedgerPostingType.InventReceipt : LedgerPostingType.InventIssue, offsetType);
    }

    private async Task<DimensionAttributeValueCombination> ResolvePostingDimensionAsync(string companyId, string itemId, InventAccountType accountType, CancellationToken ct)
    {
        var itemGroup = await Db.Set<InventItemGroupItem>().AsNoTracking().Where(x => x.ItemDataAreaId == companyId && x.ItemId == itemId)
            .Select(x => x.ItemGroupId).FirstOrDefaultAsync(ct) ?? string.Empty;
        var rules = await Db.Set<InventPosting>().AsNoTracking().Where(x => x.DataAreaId == companyId && x.InventAccountType == accountType && x.CustVendCode == TableGroupAll.All).ToListAsync(ct);
        var rule = rules.Where(x => x.ItemCode == TableGroupAll.Table && x.ItemRelation == itemId).OrderBy(x => x.RecId).FirstOrDefault()
            ?? rules.Where(x => x.ItemCode == TableGroupAll.Group && x.ItemRelation == itemGroup).OrderBy(x => x.RecId).FirstOrDefault()
            ?? rules.Where(x => x.ItemCode == TableGroupAll.All).OrderBy(x => x.RecId).FirstOrDefault();
        if (rule == null || rule.LedgerDimension == 0)
            throw new PostingValidationException($"Inventory posting setup is missing for {accountType} and item '{itemId}'.");
        var dimension = await Db.Set<DimensionAttributeValueCombination>().AsNoTracking().FirstOrDefaultAsync(x => x.DataAreaId == companyId && x.RecId == rule.LedgerDimension, ct)
            ?? throw new PostingValidationException($"The {accountType} posting rule references an invalid ledger dimension.");
        await ValidateMainAccountAsync(companyId, dimension, ct);
        return dimension;
    }

    private async Task ValidateMainAccountAsync(string companyId, DimensionAttributeValueCombination dimension, CancellationToken ct)
    {
        var valid = dimension.MainAccount > 0
            ? await Db.Set<MainAccount>().AsNoTracking().AnyAsync(x => x.DataAreaId == companyId && x.RecId == dimension.MainAccount, ct)
            : await Db.Set<MainAccount>().AsNoTracking().AnyAsync(x => x.DataAreaId == companyId && x.MainAccountId == dimension.MainAccountValue, ct);
        if (!valid) throw new PostingValidationException($"Ledger dimension '{dimension.DisplayValue}' does not reference a valid main account.");
    }

    private async Task CreateLedgerEntryAsync(string companyId, InventJournalTable header, InventJournalTrans line, string voucher,
        decimal amount, PostingAccounts accounts, LedgerContext context, CancellationToken ct)
    {
        var entry = new GeneralJournalEntry
        {
            DataAreaId = companyId, JournalNumber = header.JournalId, Ledger = context.Ledger.RecId,
            JournalCategory = LedgerJournalCategory.General, PostingLayer = CurrentToOperationsPostingLayer.Current,
            SubledgerVoucher = voucher, SubledgerVoucherDataAreaId = companyId, DocumentNumber = header.JournalId,
            AccountingDate = line.TransDate, DocumentDate = line.TransDate,
            FiscalCalendarPeriod = context.Period.RecId, FiscalCalendarYear = context.Period.FiscalCalendarYear
        };
        Db.Add(entry);
        await _unitOfWork.CompleteAsync(ct);
        Db.AddRange(
            AccountEntry(companyId, entry.RecId, accounts.Inventory, accounts.InventoryPostingType, amount, line, context.Ledger.AccountingCurrency),
            AccountEntry(companyId, entry.RecId, accounts.Offset, accounts.OffsetPostingType, -amount, line, context.Ledger.AccountingCurrency));
        Db.Add(new SubledgerVoucherGeneralJournalEntry
        {
            DataAreaId = companyId, GeneralJournalEntry = entry.RecId, Voucher = voucher,
            VoucherDataAreaId = companyId, AccountingDate = line.TransDate
        });
    }

    private static GeneralJournalAccountEntry AccountEntry(string companyId, long entryId,
        DimensionAttributeValueCombination dimension, LedgerPostingType type, decimal amount,
        InventJournalTrans line, string currency) => new()
    {
        DataAreaId = companyId, GeneralJournalEntry = entryId, LedgerDimension = dimension.RecId,
        MainAccount = dimension.MainAccount, LedgerAccount = dimension.DisplayValue.Length == 0 ? dimension.MainAccountValue : dimension.DisplayValue,
        Text = $"Inventory journal {line.JournalId} line {line.LineNum}", PostingType = type,
        TransactionCurrencyCode = currency, TransactionCurrencyAmount = amount,
        AccountingCurrencyAmount = amount, ReportingCurrencyAmount = amount,
        Quantity = line.Qty, IsCredit = amount < 0 ? NoYes.Yes : NoYes.No, PaymentReference = string.Empty
    };

    private static decimal EffectiveFactor(UnitOfMeasureConversion conversion) =>
        conversion.Factor * (conversion.Numerator > 0 && conversion.Denominator > 0
            ? (decimal)conversion.Numerator / conversion.Denominator : 1m);

    private static decimal Round(decimal value, int precision, UnitOfMeasureRounding rounding)
    {
        var scale = (decimal)Math.Pow(10, precision);
        return rounding switch
        {
            UnitOfMeasureRounding.Up => Math.Ceiling(value * scale) / scale,
            UnitOfMeasureRounding.Down => Math.Floor(value * scale) / scale,
            _ => Math.Round(value, precision, MidpointRounding.AwayFromZero)
        };
    }

    private static string Normalize(string value) => value.Trim().ToUpperInvariant();
    private sealed record ReadyItem(string ItemId, long Product, ItemModelGroup Model, long TrackingDimensionGroup);
    private sealed record LedgerContext(Ledger Ledger, FiscalCalendarPeriod Period);
    private sealed record PostingAccounts(DimensionAttributeValueCombination Inventory,
        DimensionAttributeValueCombination Offset, LedgerPostingType InventoryPostingType, LedgerPostingType OffsetPostingType);
    private sealed class PostingValidationException(string message) : Exception(message);
}
