using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using IAX.IXApi.Modules.Finance.Inventory.InventoryStatus.Entities;
using IAX.IXApi.Modules.Finance.Inventory.ProductLifecycle;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;
using ReleasedItem = IAX.IXApi.Modules.Finance.Entities.InventTable;
using ItemModelGroup = IAX.IXApi.Modules.Finance.Entities.InventModelGroup;
using PostingValidationException = IAX.IXApi.Modules.Finance.Common.SubledgerPostingException;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed partial class InventJournalPostingService : IInventJournalPostingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICompanyExecutionContext _company;
    private readonly ICurrentUserService _user;
    private readonly ISysNumberSequenceService _numbers;
    private readonly ISalesInventoryNumberService _inventoryNumbers;
    private DbContext Db => _unitOfWork.Context;
    private SubledgerPostingSupport Support => new(Db);
    private InventoryMovement Movement => new(_unitOfWork, _inventoryNumbers);

    public InventJournalPostingService(IUnitOfWork unitOfWork, ICompanyExecutionContext company,
        ICurrentUserService user, ISysNumberSequenceService numbers, ISalesInventoryNumberService inventoryNumbers)
    {
        _unitOfWork = unitOfWork;
        _company = company;
        _user = user;
        _numbers = numbers;
        _inventoryNumbers = inventoryNumbers;
    }

    public Task<(InventJournalPostResultDto? Result, string? Error)> PostAsync(string journalId, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => PostCoreAsync(journalId, ct));

    private async Task<(InventJournalPostResultDto? Result, string? Error)> PostCoreAsync(string journalId, CancellationToken ct)
    {
        var companyId = _company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(companyId)) return (null, "A company must be selected.");
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            await Support.LockAsync($"InventJournal:{companyId}:{Normalize(journalId)}", ct);
            var header = await Db.Set<InventJournalTable>().FirstOrDefaultAsync(x => x.DataAreaId == companyId && x.JournalId == Normalize(journalId), ct)
                ?? throw new PostingValidationException("Inventory journal was not found.");
            if (header.Posted == NoYes.Yes)
            {
                var priorVouchers = await Db.Set<InventJournalTrans>().AsNoTracking()
                    .Where(x => x.DataAreaId == companyId && x.JournalId == header.JournalId && x.Voucher != string.Empty)
                    .ToListAsync(ct);
                await _unitOfWork.CommitTransactionAsync(ct);
                return (new InventJournalPostResultDto
                {
                    JournalId = header.JournalId, Posted = true, PostedDateTime = header.PostedDateTime,
                    Vouchers = priorVouchers.OrderBy(x => x.LineNum).Select(x => x.Voucher).ToList()
                }, null);
            }
            if (header.JournalType == InventJournalType.Counting && (!header.CountingFrozen || !header.CountingConfirmed))
                throw new PostingValidationException(
                    "Counting posting requires a confirmed count and durable movement freeze. The current journal has neither verified control, so posting is blocked.");
            var lines = (await Db.Set<InventJournalTrans>().Where(x => x.DataAreaId == companyId && x.JournalId == header.JournalId).ToListAsync(ct))
                .OrderBy(x => x.LineNum).ToList();
            if (lines.Count == 0) throw new PostingValidationException("The inventory journal has no lines.");
            if (lines.Any(x => x.JournalType != header.JournalType)) throw new PostingValidationException("A journal line type does not match the journal header.");
            foreach (var line in lines.OrderBy(x => x.ItemId).ThenBy(x => x.InventDimId))
            {
                var boundaryError = await InventoryFinancialBoundary.CheckAsync(Db, companyId, line.ItemId, line.TransDate, ct);
                if (boundaryError != null) throw new PostingValidationException(boundaryError);
            }
            foreach (var key in lines.Select(x => new { x.ItemId, Dimension = x.InventDimId })
                         .Concat(lines.Where(x => x.ToInventDimId.Length > 0)
                             .Select(x => new { x.ItemId, Dimension = x.ToInventDimId }))
                         .Distinct().OrderBy(x => x.ItemId, StringComparer.Ordinal).ThenBy(x => x.Dimension, StringComparer.Ordinal))
            {
                var freezeError = await InventoryCountProtection.CheckAsync(Db, companyId, key.ItemId, key.Dimension, ct, header.JournalId);
                if (freezeError != null) throw new PostingValidationException(freezeError);
            }
            if (header.JournalType == InventJournalType.Transfer)
            {
                foreach (var group in lines.GroupBy(x => new { x.ItemId, x.InventDimId }))
                {
                    if (group.Any(x => x.Qty <= 0))
                        throw new PostingValidationException("Transfer quantities must be positive.");
                    var inventoryUnit = await Db.Set<InventTableModule>().AsNoTracking()
                        .Where(x => x.DataAreaId == companyId && x.ItemId == group.Key.ItemId && x.ModuleType == ModuleInventPurchSales.Inventory)
                        .Select(x => x.UnitId).FirstOrDefaultAsync(ct);
                    if (string.IsNullOrWhiteSpace(inventoryUnit) || group.Any(x => !string.Equals(x.Unit, inventoryUnit, StringComparison.OrdinalIgnoreCase)))
                        throw new PostingValidationException($"Transfer lines for item '{group.Key.ItemId}' must use the inventory unit.");
                    var initial = await Db.Set<InventSum>().AsNoTracking().FirstOrDefaultAsync(x =>
                        x.DataAreaId == companyId && x.ItemId == group.Key.ItemId && x.InventDimId == group.Key.InventDimId, ct);
                    var available = (initial?.PhysicalInvent ?? 0m) - (initial?.ReservPhysical ?? 0m);
                    if (group.Sum(x => x.Qty) > available)
                        throw new PostingValidationException($"Transfer lines exceed available stock for item '{group.Key.ItemId}' and dimension '{group.Key.InventDimId}'.");
                }
            }

            var name = await Db.Set<InventJournalName>().AsNoTracking().FirstOrDefaultAsync(x => x.DataAreaId == companyId && x.JournalNameId == header.JournalNameId, ct)
                ?? throw new PostingValidationException("The journal name setup no longer exists.");
            if (name.JournalType != header.JournalType) throw new PostingValidationException("The journal type no longer matches its journal name setup.");
            var voucherSequence = await Db.Set<SysNumberSequence>().IgnoreQueryFilters().AsNoTracking()
                .FirstOrDefaultAsync(x => x.RecId == header.VoucherNumberSequenceTable && !x.IsDeleted && x.IsActive && x.InUse != 0 &&
                    (x.DataAreaId == companyId || (x.DataAreaId == string.Empty && x.NumberSequenceScope == null)), ct)
                ?? throw new PostingValidationException("A valid voucher number sequence is required before posting.");

            var ledgerContext = await LoadLedgerContextAsync(companyId, lines, ct);
            var inventoryCurrency = ledgerContext.Ledger.AccountingCurrency;
            if (string.IsNullOrWhiteSpace(inventoryCurrency))
                throw new PostingValidationException("The ledger accounting currency is required.");
            var result = new InventJournalPostResultDto { JournalId = header.JournalId, PostedDateTime = DateTime.UtcNow };

            foreach (var line in lines)
            {
                var lifecycleError = await ProductLifecyclePolicy.ValidateAsync(Db, companyId, line.ItemId,
                    ProductLifecycleProcess.Inventory, ct);
                if (lifecycleError is not null) throw new PostingValidationException(lifecycleError);
                var item = await LoadReadyItemAsync(companyId, line.ItemId, ct);
                var fromDim = await Db.Set<InventDim>().FirstOrDefaultAsync(x => x.DataAreaId == companyId && x.InventDimId == line.InventDimId, ct)
                    ?? throw new PostingValidationException($"Inventory dimension '{line.InventDimId}' was not found for line {line.LineNum}.");
                var inventoryQty = await ConvertToInventoryUnitAsync(companyId, item.Product, item.ItemId, line.Unit, line.Qty, ct);
                var fromSum = await Movement.GetOrCreateSummaryAsync(companyId, item.ItemId, fromDim, ct);
                decimal signedQty;
                if (header.JournalType == InventJournalType.Counting)
                {
                    var counted = await ConvertToInventoryUnitAsync(companyId, item.Product, item.ItemId, line.Unit, line.Counted, ct);
                    if (line.InventOnHand != fromSum.PhysicalInvent)
                        throw new PostingValidationException("Inventory changed after the count was frozen; cancel and recount.");
                    signedQty = counted - fromSum.PhysicalInvent;
                    line.Qty = signedQty;
                }
                else signedQty = header.JournalType == InventJournalType.Transfer ? -Math.Abs(inventoryQty) : inventoryQty;

                if (fromDim.InventStatusId.Length > 0)
                {
                    var status = await Db.Set<WHSInventStatus>().AsNoTracking().FirstOrDefaultAsync(
                        x => x.DataAreaId == companyId && x.InventStatusId == fromDim.InventStatusId, ct)
                        ?? throw new PostingValidationException($"Inventory status '{fromDim.InventStatusId}' no longer exists.");
                    if (status.InventStatusBlocking == NoYes.Yes && signedQty < 0)
                        throw new PostingValidationException($"Inventory status '{fromDim.InventStatusId}' is blocked for issue.");
                }

                await ValidateTrackingDimensionsAsync(item, fromDim, signedQty, ct);

                if (signedQty < 0 && fromSum.PhysicalInvent - fromSum.ReservPhysical - fromSum.Picked + signedQty < 0)
                    throw new PostingValidationException($"Line {line.LineNum} exceeds available physical inventory for item '{item.ItemId}'.");

                if (signedQty == 0)
                {
                    line.Voucher = string.Empty;
                    continue;
                }
                var unitCost = await ResolveUnitCostAsync(companyId, item, fromSum, line, header.JournalType, signedQty, ct);
                if (unitCost <= 0 && signedQty < 0 && item.Model.NegativePhysical == NoYes.Yes)
                    unitCost = await GetDefaultItemCostAsync(companyId, item.ItemId, line.InventDimId, line.TransDate, ct);
                if (unitCost <= 0 && signedQty > 0 && header.JournalType == InventJournalType.Counting)
                    unitCost = await GetDefaultItemCostAsync(companyId, item.ItemId, line.InventDimId, line.TransDate, ct);
                if (unitCost <= 0 && header.JournalType != InventJournalType.Transfer)
                    throw new PostingValidationException($"A positive inventory cost is required for item '{item.ItemId}' on line {line.LineNum}.");
                var voucher = (await _numbers.NextAsync(voucherSequence.NumberSequence, companyId, ct)).Code;
                result.Vouchers.Add(voucher);

                if (header.JournalType == InventJournalType.Transfer)
                {
                    if (string.IsNullOrWhiteSpace(line.ToInventDimId)) throw new PostingValidationException($"A destination dimension is required on transfer line {line.LineNum}.");
                    var toDim = await Db.Set<InventDim>().FirstOrDefaultAsync(x => x.DataAreaId == companyId && x.InventDimId == line.ToInventDimId, ct)
                        ?? throw new PostingValidationException($"Destination inventory dimension '{line.ToInventDimId}' was not found.");
                    if (toDim.InventDimId == fromDim.InventDimId) throw new PostingValidationException("Transfer source and destination dimensions must be different.");
                    if (toDim.InventSiteId != fromDim.InventSiteId)
                        throw new PostingValidationException("Cross-site transfer requires financial reclassification and is not available yet.");
                    if (!IsBasicTransferDimension(fromDim) || !IsBasicTransferDimension(toDim))
                        throw new PostingValidationException("The basic transfer journal supports site and warehouse dimensions only.");
                    if (toDim.InventStatusId.Length > 0 && !await Db.Set<WHSInventStatus>().AnyAsync(x =>
                            x.DataAreaId == companyId && x.InventStatusId == toDim.InventStatusId, ct))
                        throw new PostingValidationException($"Destination inventory status '{toDim.InventStatusId}' no longer exists.");
                    await ValidateTrackingDimensionsAsync(item, toDim, Math.Abs(inventoryQty), ct);
                    if (unitCost <= 0)
                        throw new PostingValidationException($"A positive source inventory cost is required for transfer line {line.LineNum}.");
                    var toSum = await Movement.GetOrCreateSummaryAsync(companyId, item.ItemId, toDim, ct);
                    var transferQty = Math.Abs(inventoryQty);
                    await CreateLineTransactionAsync(companyId, header, line, item.ItemId, fromDim.InventDimId, -transferQty, unitCost, inventoryCurrency, voucher, ct);
                    await CreateLineTransactionAsync(companyId, header, line, item.ItemId, toDim.InventDimId, transferQty, unitCost, inventoryCurrency, voucher, ct);
                    InventoryMovement.ApplySummary(fromSum, -transferQty, unitCost);
                    InventoryMovement.ApplySummary(toSum, transferQty, unitCost);
                    line.CostAmount = transferQty * unitCost;
                    result.InventoryTransactionsCreated += 2;
                }
                else if (signedQty != 0)
                {
                    var costAmount = signedQty * unitCost;
                    await CreateLineTransactionAsync(companyId, header, line, item.ItemId, fromDim.InventDimId, signedQty, unitCost, inventoryCurrency, voucher, ct);
                    InventoryMovement.ApplySummary(fromSum, signedQty, unitCost);
                    line.CostAmount = costAmount;
                    var accounts = await ResolveAccountsAsync(companyId, item.ItemId, signedQty, line.LedgerDimension, header.JournalType, ct);
                    await CreateLedgerEntryAsync(companyId, header, line, voucher, costAmount, accounts, ledgerContext!, ct);
                    result.InventoryTransactionsCreated++;
                    result.LedgerEntriesCreated++;
                }
                line.Voucher = voucher;
            }

            header.Posted = NoYes.Yes;
            header.CountingFrozen = false;
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

    private async Task<decimal> ConvertToInventoryUnitAsync(string companyId, long product, string itemId, string fromSymbol, decimal qty, CancellationToken ct)
    {
        var inventoryUnit = await Db.Set<InventTableModule>().AsNoTracking().Where(x => x.DataAreaId == companyId && x.ItemId == itemId && x.ModuleType == ModuleInventPurchSales.Inventory)
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
            if (factor <= 0 || direct.InnerOffset != 0 || direct.OuterOffset != 0)
                throw new PostingValidationException("Only positive linear unit conversions are supported for basic inventory.");
            converted = qty * factor;
            rounding = direct.Rounding;
        }
        else
        {
            var factor = EffectiveFactor(reverse!);
            if (factor <= 0 || reverse!.InnerOffset != 0 || reverse.OuterOffset != 0)
                throw new PostingValidationException("Only positive linear unit conversions are supported for basic inventory.");
            converted = qty / factor;
            rounding = reverse.Rounding;
        }
        return Round(converted, to.DecimalPrecision, rounding);
    }

    /// <summary>Posts one journal line's movement and keeps the line's inventory references.</summary>
    private async Task CreateLineTransactionAsync(string companyId, InventJournalTable header, InventJournalTrans line,
        string itemId, string dimId, decimal qty, decimal unitCost, string currency, string voucher, CancellationToken ct)
    {
        var transId = await Movement.CreateTransactionAsync(companyId, itemId, dimId, qty, unitCost, currency,
            voucher, line.TransDate, line.InventRefType, header.JournalId, ct);
        if (string.IsNullOrEmpty(line.InventTransId)) line.InventTransId = transId; else line.ToInventTransId = transId;
    }

    private static bool IsBasicTransferDimension(InventDim dim) =>
        dim.InventSiteId.Length > 0 && dim.InventLocationId.Length > 0 &&
        dim.ConfigId.Length == 0 && dim.InventSizeId.Length == 0 &&
        dim.InventColorId.Length == 0 && dim.InventStyleId.Length == 0 &&
        dim.InventVersionId.Length == 0 && dim.InventBatchId.Length == 0 &&
        dim.InventSerialId.Length == 0 && dim.InventStatusId.Length == 0 &&
        dim.WmsLocationId.Length == 0 && dim.LicensePlateId.Length == 0;

    private async Task<decimal> ResolveUnitCostAsync(string companyId, ReadyItem item, InventSum summary, InventJournalTrans line,
        InventJournalType journalType, decimal signedQty, CancellationToken ct)
    {
        if (item.Model.InventModel is InventModel.WeightedAvg or InventModel.WeightedAvg_Date)
            throw new PostingValidationException(
                $"Inventory model '{item.Model.InventModel}' requires inventory close/settlement, which is not implemented in this basic journal engine. Select Moving average or defer posting until costing close is supported.");

        if (item.Model.InventModel == InventModel.StandardCost)
        {
            var standardCost = await GetDefaultItemCostAsync(companyId, item.ItemId, line.InventDimId, line.TransDate, ct);
            if (standardCost <= 0)
                throw new PostingValidationException($"An active standard cost is required for item '{item.ItemId}' on {line.TransDate:d}.");
            if (signedQty > 0 && line.CostPrice > 0 && line.CostPrice != standardCost)
                throw new PostingValidationException(
                    $"Receipt price differs from active standard cost for item '{item.ItemId}'. Standard-cost variance posting is not implemented.");
            return standardCost;
        }

        if (journalType == InventJournalType.Transfer || signedQty < 0 || journalType == InventJournalType.Counting)
        {
            if (item.Model.InventModel != InventModel.FIFO) return InventoryMovement.RunningAverageCost(summary);
            var site = await Db.Set<InventDim>().AsNoTracking()
                .Where(x => x.DataAreaId == companyId && x.InventDimId == line.InventDimId)
                .Select(x => x.InventSiteId).FirstOrDefaultAsync(ct);
            if (string.IsNullOrEmpty(site)) throw new PostingValidationException("The inventory site is required for FIFO cost estimation.");
            var balances = await Db.Set<InventSum>().AsNoTracking()
                .Where(x => x.DataAreaId == companyId && x.ItemId == item.ItemId && x.InventSiteId == site)
                .Select(x => new { x.PhysicalInvent, x.PostedValue, x.PhysicalValue }).ToListAsync(ct);
            var quantity = balances.Sum(x => x.PhysicalInvent);
            var value = balances.Sum(x => x.PostedValue + x.PhysicalValue);
            return quantity <= 0 ? 0m : Math.Abs(value / quantity);
        }
        return line.CostPrice;
    }

    private async Task<decimal> GetDefaultItemCostAsync(string companyId, string itemId, string inventDimId,
        DateTime transDate, CancellationToken ct)
    {
        var price = await Db.Set<InventItemPrice>().AsNoTracking()
            .Where(x => x.DataAreaId == companyId && x.ItemId == itemId &&
                x.PriceType == PreferredPriceType.CostPrice &&
                x.CostingType == CostingType.StandardCost && x.ActivationDate <= transDate &&
                (x.InventDimId == inventDimId || x.InventDimId == string.Empty))
            .OrderByDescending(x => x.InventDimId == inventDimId)
            .ThenByDescending(x => x.ActivationDate)
            .ThenByDescending(x => x.RecId)
            .Select(x => new { x.Price, x.PriceUnit })
            .FirstOrDefaultAsync(ct);
        return price is null ? 0m : price.Price / (price.PriceUnit > 0 ? price.PriceUnit : 1m);
    }

    private async Task<SubledgerLedgerContext> LoadLedgerContextAsync(string companyId, IReadOnlyCollection<InventJournalTrans> lines, CancellationToken ct)
    {
        var date = lines.Min(x => x.TransDate == default ? DateTime.Today : x.TransDate.Date);
        if (lines.Any(x => (x.TransDate == default ? DateTime.Today : x.TransDate.Date) != date))
            throw new PostingValidationException("All lines must use the same transaction date for basic journal posting.");
        var context = await Support.LoadLedgerContextAsync(companyId, date, ct);
        if (!string.IsNullOrWhiteSpace(context.Ledger.ReportingCurrency) &&
            !string.Equals(context.Ledger.ReportingCurrency, context.Ledger.AccountingCurrency, StringComparison.OrdinalIgnoreCase))
            throw new PostingValidationException("Inventory journal posting with a different reporting currency requires exchange-rate processing, which is not enabled in the basic inventory scope.");
        return context;
    }

    private async Task<PostingAccounts> ResolveAccountsAsync(string companyId, string itemId, decimal qty,
        long movementOffset, InventJournalType journalType, CancellationToken ct)
    {
        var inventoryType = qty > 0 ? InventAccountType.InventReceipt : InventAccountType.InventIssue;
        var inventory = await Support.ResolveInventPostingAsync(companyId, itemId, inventoryType, ct);
        DimensionAttributeValueCombination offset;
        LedgerPostingType offsetType;
        if (journalType == InventJournalType.Movement)
        {
            offset = await Support.LoadDimensionAsync(companyId, movementOffset, "movement offset", ct);
            offsetType = LedgerPostingType.LedgerJournal;
        }
        else
        {
            var type = qty > 0 ? InventAccountType.InventProfit : InventAccountType.InventLoss;
            offset = await Support.ResolveInventPostingAsync(companyId, itemId, type, ct);
            offsetType = qty > 0 ? LedgerPostingType.InventProfit : LedgerPostingType.InventLoss;
        }
        return new PostingAccounts(inventory, offset,
            qty > 0 ? LedgerPostingType.InventReceipt : LedgerPostingType.InventIssue, offsetType);
    }

    private async Task CreateLedgerEntryAsync(string companyId, InventJournalTable header, InventJournalTrans line, string voucher,
        decimal amount, PostingAccounts accounts, SubledgerLedgerContext context, CancellationToken ct)
    {
        var entry = Support.NewEntry(companyId, header.JournalId, voucher, line.TransDate, context);
        Db.Add(entry);
        await _unitOfWork.CompleteAsync(ct);
        var text = $"Inventory journal {line.JournalId} line {line.LineNum}";
        var currency = context.Ledger.AccountingCurrency;
        Db.AddRange(
            SubledgerPostingSupport.AccountEntry(companyId, entry.RecId, accounts.Inventory, accounts.InventoryPostingType, amount, text, line.Qty, currency),
            SubledgerPostingSupport.AccountEntry(companyId, entry.RecId, accounts.Offset, accounts.OffsetPostingType, -amount, text, line.Qty, currency));
        Db.Add(new SubledgerVoucherGeneralJournalEntry
        {
            DataAreaId = companyId, GeneralJournalEntry = entry.RecId, Voucher = voucher,
            VoucherDataAreaId = companyId, AccountingDate = line.TransDate
        });
        // An inventory journal updates physically and financially in one operation, so the
        // posting record covers the financial update.
        var origin = await Db.Set<InventTransOrigin>().AsNoTracking()
            .Where(x => x.DataAreaId == companyId && x.InventTransId == line.InventTransId)
            .Select(x => x.RecId).FirstOrDefaultAsync(ct);
        if (origin != 0)
            Db.Add(IAX.IXApi.Modules.Finance.Inventory.InventTransPosting.InventTransPostingService.Record(companyId, origin, line.ItemId,
                InventTransPostingType.Financial, line.TransDate, voucher,
                accounts.Inventory, accounts.InventoryPostingType, accounts.Offset, accounts.OffsetPostingType));
    }

    private static decimal EffectiveFactor(UnitOfMeasureConversion conversion) =>
        conversion.Factor > 0 ? conversion.Factor :
        conversion.Numerator > 0 && conversion.Denominator > 0
            ? (decimal)conversion.Numerator / conversion.Denominator : 0m;

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
    private sealed record PostingAccounts(DimensionAttributeValueCombination Inventory,
        DimensionAttributeValueCombination Offset, LedgerPostingType InventoryPostingType, LedgerPostingType OffsetPostingType);
}
