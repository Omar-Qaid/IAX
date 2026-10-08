using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Persistence;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public enum SalesLineWriteStatus { NotFound, Invalid, Success }
public sealed record SalesLineWriteResult(SalesLineWriteStatus Status, string? Error = null, object? Data = null);

public sealed class SalesOrderLineService
{
    private readonly IFinanceDataContext _dbContext;
    private readonly ICompanyExecutionContext _company;
    private readonly SalesOrderValidationService _validation;
    private readonly ISalesInventoryDemandService _inventoryDemand;
    private readonly SalesUnitConversionService _units;

    public SalesOrderLineService(IFinanceDataContext dbContext, ICompanyExecutionContext company,
        SalesOrderValidationService validation, ISalesInventoryDemandService inventoryDemand,
        SalesUnitConversionService units)
    {
        _dbContext = dbContext;
        _company = company;
        _validation = validation;
        _inventoryDemand = inventoryDemand;
        _units = units;
    }

    public async Task<SalesLineWriteResult> AddAsync(long recId, SalesLineInputDto input, CancellationToken cancellationToken)
    {
        return await _dbContext.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
            var order = await _dbContext.Set<SalesTable>().FirstOrDefaultAsync(row => row.RecId == recId, cancellationToken);
            if (order == null) return new SalesLineWriteResult(SalesLineWriteStatus.NotFound, "Sales order was not found.");
            if (order.SalesStatus != SalesStatus.Backorder)
                return new SalesLineWriteResult(SalesLineWriteStatus.Invalid, "Lines can only be added to open sales orders.");
            var dataAreaId = _company.GetDataAreaId();
            if (!string.Equals(order.DataAreaId, dataAreaId, StringComparison.OrdinalIgnoreCase))
                return new SalesLineWriteResult(SalesLineWriteStatus.NotFound, "Sales order was not found in the selected company.");
            long deliveryAddressId = order.DeliveryPostalAddress;
            if (!string.IsNullOrWhiteSpace(input.DeliveryPostalAddress))
            {
                if (!long.TryParse(input.DeliveryPostalAddress, out deliveryAddressId)
                    || !await _validation.IsDeliveryAddressAvailableForOrderAsync(order, deliveryAddressId, cancellationToken))
                    return new SalesLineWriteResult(SalesLineWriteStatus.Invalid, "The selected delivery address does not belong to this customer.");
            }
            var item = await _dbContext.Set<InventTable>().AsNoTracking()
                .FirstOrDefaultAsync(row => row.ItemId == input.ItemNumber
                    && row.DataAreaId == order.DataAreaId, cancellationToken);
            if (item == null) return new SalesLineWriteResult(SalesLineWriteStatus.Invalid, "Item was not found.");
            var module = await _dbContext.Set<InventTableModule>().AsNoTracking()
                .FirstOrDefaultAsync(row => row.ItemId == item.ItemId && row.DataAreaId == order.DataAreaId && row.ModuleType == ModuleInventPurchSales.Sales, cancellationToken);
            if (module == null || string.IsNullOrWhiteSpace(module.UnitId))
                return new SalesLineWriteResult(SalesLineWriteStatus.Invalid, "The item must have a sales unit configured.");
            var salesUnit = string.IsNullOrWhiteSpace(input.Unit) ? module.UnitId : input.Unit.Trim();
            var inventoryQuantity = await _units.ToInventoryQuantityAsync(item, salesUnit, input.Quantity, cancellationToken);
            if (inventoryQuantity == null)
                return new SalesLineWriteResult(SalesLineWriteStatus.Invalid, "A conversion from the sales unit to the item's inventory unit is required.");
            var priceUnit = input.PriceUnit.GetValueOrDefault() > 0 ? input.PriceUnit!.Value : 1m;
            var enteredPrice = input.UnitPrice;
            var usePriceAgreement = input.UsePriceAgreement;
            var listPrice = usePriceAgreement
                ? await FindSalesPriceAsync(order, item.ItemId, input.Quantity,
                    string.IsNullOrWhiteSpace(input.Unit) ? module.UnitId : input.Unit.Trim(),
                    module.PriceUnit > 0 ? module.PriceUnit : 1m, null, cancellationToken)
                : null;
            var effectivePrice = !usePriceAgreement
                ? enteredPrice
                : listPrice?.Amount ?? (string.Equals(salesUnit, module.UnitId, StringComparison.OrdinalIgnoreCase)
                    ? module.Price : 0m);
            if (usePriceAgreement)
                priceUnit = listPrice?.PriceUnit ?? (module.PriceUnit > 0 ? module.PriceUnit : 1m);
            input.UnitPrice = effectivePrice;
            input.PriceUnit = priceUnit;
            var discountValidationError = NormalizeAndValidateLineDiscount(input);
            if (discountValidationError != null)
                return new SalesLineWriteResult(SalesLineWriteStatus.Invalid, discountValidationError);
            var lineTaxGroup = string.IsNullOrWhiteSpace(input.TaxGroup) ? order.TaxGroupId : input.TaxGroup.Trim();
            var lineTaxItemGroup = string.IsNullOrWhiteSpace(input.TaxItemGroup) ? module.TaxItemGroupId : input.TaxItemGroup.Trim();
            var taxValidationError = await _validation.ValidateTaxSetupAsync(lineTaxGroup, lineTaxItemGroup, cancellationToken);
            if (taxValidationError != null)
                return new SalesLineWriteResult(SalesLineWriteStatus.Invalid, taxValidationError);
            if (!string.IsNullOrWhiteSpace(input.ReturnLotId))
            {
                var returnLotExists = await _dbContext.Set<InventTransOrigin>().AnyAsync(origin =>
                    origin.DataAreaId == order.DataAreaId && origin.ItemId == item.ItemId
                    && origin.InventTransId == input.ReturnLotId.Trim(), cancellationToken);
                if (!returnLotExists)
                    return new SalesLineWriteResult(SalesLineWriteStatus.Invalid, "The selected return lot does not belong to this item.");
            }
            if (input.LedgerDimension.HasValue && input.LedgerDimension.Value != 0
                && !await _dbContext.Set<DimensionAttributeValueCombination>().AnyAsync(dimension =>
                    dimension.DataAreaId == order.DataAreaId
                    && dimension.RecId == input.LedgerDimension.Value, cancellationToken))
                return new SalesLineWriteResult(SalesLineWriteStatus.Invalid, "The selected main account was not found.");
            var lastLine = await _dbContext.Set<SalesLine>()
                .Where(row => row.SalesId == order.SalesId && row.DataAreaId == order.DataAreaId)
                .MaxAsync(row => (decimal?)row.LineNum, cancellationToken) ?? 0;
            var line = new SalesLine {
                SalesId = order.SalesId, LineNum = lastLine + 1, ItemId = item.ItemId,
                Name = string.IsNullOrWhiteSpace(input.Description) ? item.NameAlias : input.Description.Trim(),
                CustAccount = order.CustAccount, CustGroupId = order.CustGroup, CurrencyCode = order.CurrencyCode,
                SalesQty = input.Quantity, QtyOrdered = inventoryQuantity.Value, RemainSalesPhysical = input.Quantity,
                RemainSalesFinancial = input.Quantity, SalesUnit = salesUnit, PriceUnit = input.PriceUnit.GetValueOrDefault(1m),
                SalesPrice = input.UnitPrice, LineAmount = CalculateGrossLineAmount(input),
                ManualEntryChangePolicy = input.UsePriceAgreement ? 0L : 1L,
                SalesStatus = SalesStatus.Backorder, SalesType = input.LineType ?? order.SalesType ?? SalesType.Sales,
                DeliveryType = input.DeliveryType ?? SalesDeliveryType.None, SalesCategory = input.SalesCategory,
                InventRefType = input.ItemReferenceType ?? InventRefType.None,
                LineDeliveryType = input.LineDeliveryType ?? SalesDeliveryType.None,
                SourcingOrigin = input.SourcingOrigin ?? SalesLineSourcingOrigin.None,
                MpsExcludeSalesLine = input.ExcludeFromMasterPlanning == true ? 1 : 0,
                DeliveryDateControlType = input.DeliveryDateControlType ?? SalesDlvDateControlType.None,
                MpsFullRunCtpStatus = input.MpsFullRunCtpStatus ?? ReqFullCTPStatus.None,
                ShipCarrierDlvType = input.ShipCarrierDlvType ?? WHSShipCarrierDlvType.None,
                PlanningPriority = input.PlanningPriority ?? 0,
                IntercompanyOrigin = input.IntercompanyOrigin ?? SalesIntercompanyOrigin.None,
                Blocked = input.Stopped == true ? SalesLineBlocked.Yes : SalesLineBlocked.No,
                Complete = input.PreventPartialDelivery == true ? 1 : 0,
                ReceiptDateRequested = input.DeliveryDate?.Date ?? order.ReceiptDateRequested,
                ShippingDateRequested = input.ShippingDateRequested?.Date ?? order.ShippingDateRequested,
                ShippingDateConfirmed = input.ShippingDateConfirmed?.Date ?? default,
                ReceiptDateConfirmed = input.ReceiptDateConfirmed?.Date ?? default,
                DlvMode = input.DeliveryMode?.Trim() ?? order.DlvMode,
                DlvTerm = input.DeliveryTerms?.Trim() ?? order.DlvTerm,
                OverDeliveryPct = input.OverDeliveryPercent,
                UnderDeliveryPct = input.UnderDeliveryPercent,
                DeliveryName = input.DeliveryName?.Trim() ?? order.DeliveryName,
                DeliveryPostalAddress = deliveryAddressId,
                InventTransIdReturn = input.ReturnLotId?.Trim() ?? string.Empty,
                Reservation = input.Reservation ?? SalesAutoReservation.None,
                PdsBatchAttribAutoRes = input.AutoBatchReservation == true ? 1 : 0,
                PdsSameLot = input.SameBatchSelection == true ? 1 : 0,
                Scrap = input.Scrap == true ? 1 : 0,
                LedgerDimension = input.LedgerDimension ?? 0,
                SalesGroup = input.SalesGroup?.Trim() ?? string.Empty,
                CustomerRef = input.CustomerReference?.Trim() ?? string.Empty,
                CustomerLineNum = input.CustomerLineNumber ?? 0,
                PackingUnit = input.PackingUnit?.Trim() ?? string.Empty,
                PackingUnitQty = input.PackingUnitQuantity,
                LineDisc = input.LineDiscount,
                LinePercent = input.LineDiscountPercent,
                MultiLnDisc = input.MultiLineDiscount,
                MultiLnPercent = input.MultiLineDiscountPercent,
                SalesMarkup = input.SalesMarkup,
                PdsExcludeFromRebate = input.ExcludeFromRebate == true ? 1 : 0,
                TamRebateExcludeRebateManagement = input.ExcludeFromRebateManagement == true ? 1 : 0,
                TaxGroup = lineTaxGroup,
                TaxItemGroup = lineTaxItemGroup,
                DataAreaId = order.DataAreaId,
            };
            var inventSiteId = string.IsNullOrWhiteSpace(input.InventSiteId) ? order.InventSiteId : input.InventSiteId.Trim();
            var inventLocationId = string.IsNullOrWhiteSpace(input.InventLocationId) ? order.InventLocationId : input.InventLocationId.Trim();
            var dimensionError = await _validation.ValidateSiteWarehouseAsync(inventSiteId, inventLocationId, cancellationToken);
            if (dimensionError != null) return new SalesLineWriteResult(SalesLineWriteStatus.Invalid, dimensionError);
            if (!string.IsNullOrWhiteSpace(input.BatchNumber)
                && !await _dbContext.Set<InventBatch>().AnyAsync(batch => batch.DataAreaId == order.DataAreaId
                    && batch.ItemId == line.ItemId && batch.InventBatchId == input.BatchNumber.Trim(), cancellationToken))
                return new SalesLineWriteResult(SalesLineWriteStatus.Invalid, "The selected batch does not belong to this item.");
            if (!string.IsNullOrWhiteSpace(input.SerialNumber)
                && !await _dbContext.Set<InventSerial>().AnyAsync(serial => serial.DataAreaId == order.DataAreaId
                    && serial.ItemId == line.ItemId && serial.InventSerialId == input.SerialNumber.Trim(), cancellationToken))
                return new SalesLineWriteResult(SalesLineWriteStatus.Invalid, "The selected serial number does not belong to this item.");
            var dimensionTemplate = new InventDim
            {
                ConfigId = input.ConfigId?.Trim() ?? string.Empty,
                InventSizeId = input.InventSizeId?.Trim() ?? string.Empty,
                InventColorId = input.InventColorId?.Trim() ?? string.Empty,
                InventStyleId = input.InventStyleId?.Trim() ?? string.Empty,
                InventVersionId = input.InventVersionId?.Trim() ?? string.Empty,
                InventBatchId = input.BatchNumber?.Trim() ?? string.Empty,
                InventSerialId = input.SerialNumber?.Trim() ?? string.Empty
            };
            await _inventoryDemand.CreateAsync(order, line,
                inventSiteId, inventLocationId, cancellationToken, dimensionTemplate);
            _dbContext.Set<SalesLine>().Add(line);
            order.SmmSalesAmountTotal += line.LineAmount;
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new SalesLineWriteResult(SalesLineWriteStatus.Success,
                Data: SalesOrderLinePresenter.Record(line, inventSiteId, inventLocationId,
                    productName: item.NameAlias));
        });
    }

    public async Task<SalesLineWriteResult> ChangeAsync(long recId, long lineId, SalesLineInputDto? input, CancellationToken cancellationToken)
    {
        return await _dbContext.Database.CreateExecutionStrategy().ExecuteAsync(async () => {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
            var order = await _dbContext.Set<SalesTable>().FirstOrDefaultAsync(row => row.RecId == recId, cancellationToken);
            if (order == null) return new SalesLineWriteResult(SalesLineWriteStatus.NotFound, "Sales order was not found.");
            if (!string.Equals(order.DataAreaId, _company.GetDataAreaId(), StringComparison.OrdinalIgnoreCase))
                return new SalesLineWriteResult(SalesLineWriteStatus.NotFound, "Sales order was not found in the selected company.");
            if (order.SalesStatus != SalesStatus.Backorder)
                return new SalesLineWriteResult(SalesLineWriteStatus.Invalid, "Only open sales orders can be changed.");
            var line = await _dbContext.Set<SalesLine>().FirstOrDefaultAsync(row => row.RecId == lineId && row.SalesId == order.SalesId && row.DataAreaId == order.DataAreaId, cancellationToken);
            if (line == null) return new SalesLineWriteResult(SalesLineWriteStatus.NotFound, "Sales line was not found.");
            if (line.SalesStatus != SalesStatus.Backorder || line.RemainSalesPhysical != line.SalesQty || line.RemainSalesFinancial != line.SalesQty)
                return new SalesLineWriteResult(SalesLineWriteStatus.Invalid, "Processed sales lines cannot be changed.");
            if (input != null && input.ItemNumber != line.ItemId)
                return new SalesLineWriteResult(SalesLineWriteStatus.Invalid, "The item number cannot be changed.");
            var previousAmount = line.LineAmount;
            string? responseSiteId = null;
            string? responseLocationId = null;
            if (input == null) {
                await _inventoryDemand.DeleteAsync(line, cancellationToken);
                _dbContext.Set<SalesLine>().Remove(line);
            }
            else {
                var discountValidationError = NormalizeAndValidateLineDiscount(input);
                if (discountValidationError != null)
                    return new SalesLineWriteResult(SalesLineWriteStatus.Invalid, discountValidationError);
                var lineTaxGroup = string.IsNullOrWhiteSpace(input.TaxGroup) ? order.TaxGroupId : input.TaxGroup.Trim();
                var lineTaxItemGroup = input.TaxItemGroup?.Trim() ?? string.Empty;
                var taxValidationError = await _validation.ValidateTaxSetupAsync(lineTaxGroup, lineTaxItemGroup, cancellationToken);
                if (taxValidationError != null)
                    return new SalesLineWriteResult(SalesLineWriteStatus.Invalid, taxValidationError);
                // Item identity and name remain unchanged after item selection.
                line.SalesType = input.LineType ?? line.SalesType;
                line.DeliveryType = input.DeliveryType ?? line.DeliveryType;
                if (input.ItemReferenceType.HasValue) line.InventRefType = input.ItemReferenceType.Value;
                if (input.SourcingOrigin.HasValue) line.SourcingOrigin = input.SourcingOrigin.Value;
                if (input.ExcludeFromMasterPlanning.HasValue)
                    line.MpsExcludeSalesLine = input.ExcludeFromMasterPlanning.Value ? 1 : 0;
                if (input.LineDeliveryType.HasValue) line.LineDeliveryType = input.LineDeliveryType.Value;
                if (input.DeliveryDateControlType.HasValue) line.DeliveryDateControlType = input.DeliveryDateControlType.Value;
                if (input.MpsFullRunCtpStatus.HasValue) line.MpsFullRunCtpStatus = input.MpsFullRunCtpStatus.Value;
                if (input.ShipCarrierDlvType.HasValue) line.ShipCarrierDlvType = input.ShipCarrierDlvType.Value;
                if (input.PlanningPriority.HasValue) line.PlanningPriority = input.PlanningPriority.Value;
                line.SalesCategory = input.SalesCategory;
                if (input.IntercompanyOrigin.HasValue) line.IntercompanyOrigin = input.IntercompanyOrigin.Value;
                if (input.Stopped.HasValue)
                    line.Blocked = input.Stopped.Value ? SalesLineBlocked.Yes : SalesLineBlocked.No;
                if (input.PreventPartialDelivery.HasValue)
                    line.Complete = input.PreventPartialDelivery.Value ? 1 : 0;
                if (!string.IsNullOrWhiteSpace(input.Description)) line.Name = input.Description.Trim();
                var salesModule = await _dbContext.Set<InventTableModule>().AsNoTracking()
                    .FirstOrDefaultAsync(row => row.ItemId == line.ItemId
                        && row.DataAreaId == order.DataAreaId && row.ModuleType == ModuleInventPurchSales.Sales, cancellationToken);
                if (salesModule == null)
                    return new SalesLineWriteResult(SalesLineWriteStatus.Invalid, "The item must have a sales unit configured.");
                var requestedUnit = string.IsNullOrWhiteSpace(input.Unit) ? line.SalesUnit : input.Unit.Trim();
                var item = await _dbContext.Set<InventTable>().AsNoTracking()
                    .FirstOrDefaultAsync(row => row.ItemId == line.ItemId && row.DataAreaId == order.DataAreaId, cancellationToken);
                if (item == null) return new SalesLineWriteResult(SalesLineWriteStatus.Invalid, "Item was not found.");
                var inventoryQuantity = await _units.ToInventoryQuantityAsync(item, requestedUnit, input.Quantity, cancellationToken);
                if (inventoryQuantity == null)
                    return new SalesLineWriteResult(SalesLineWriteStatus.Invalid, "A conversion from the sales unit to the item's inventory unit is required.");
                var requestedPriceUnit = input.PriceUnit.GetValueOrDefault() > 0 ? input.PriceUnit!.Value :
                    line.PriceUnit > 0 ? line.PriceUnit : 1m;
                var priceWasChanged = input.UnitPrice != line.SalesPrice;
                var priceUnitWasChanged = input.PriceUnit.HasValue && input.PriceUnit.Value != line.PriceUnit;
                var manualPriceWasEdited = priceWasChanged || priceUnitWasChanged;
                var unitWasChanged = !string.Equals(requestedUnit, line.SalesUnit, StringComparison.OrdinalIgnoreCase);
                var repriceByAgreement = input.UsePriceAgreement
                    || (unitWasChanged && !manualPriceWasEdited && line.ManualEntryChangePolicy != 1L)
                    || (!manualPriceWasEdited && input.Quantity != line.SalesQty
                        && line.ManualEntryChangePolicy != 1L);
                var requestedPrice = priceWasChanged ? input.UnitPrice : line.SalesPrice;
                if (repriceByAgreement)
                {
                    var currentDimensionId = line.InventDimId;
                    var applicablePrice = await FindSalesPriceAsync(order, line.ItemId,
                        input.Quantity, requestedUnit, requestedPriceUnit,
                        currentDimensionId, cancellationToken);
                    if (applicablePrice != null)
                    {
                        requestedPrice = applicablePrice.Value.Amount;
                        requestedPriceUnit = applicablePrice.Value.PriceUnit;
                        line.ManualEntryChangePolicy = 0L;
                    }
                    else if (unitWasChanged && !string.Equals(requestedUnit, salesModule.UnitId, StringComparison.OrdinalIgnoreCase)
                        && line.ManualEntryChangePolicy != 1L)
                    {
                        requestedPrice = 0m;
                        requestedPriceUnit = 1m;
                    }
                    else if (input.Quantity != line.SalesQty && line.ManualEntryChangePolicy != 1L)
                    {
                        requestedPrice = salesModule.Price;
                        requestedPriceUnit = salesModule.PriceUnit > 0 ? salesModule.PriceUnit : 1m;
                    }
                }
                if (manualPriceWasEdited) line.ManualEntryChangePolicy = 1L;
                input.UnitPrice = requestedPrice;
                input.PriceUnit = requestedPriceUnit;
                line.SalesPrice = requestedPrice;
                line.PriceUnit = requestedPriceUnit;
                var updatedDiscountValidationError = NormalizeAndValidateLineDiscount(input);
                if (updatedDiscountValidationError != null)
                    return new SalesLineWriteResult(SalesLineWriteStatus.Invalid, updatedDiscountValidationError);
                var processedSalesQuantity = line.SalesQty - line.RemainSalesPhysical;
                var processedFinancialQuantity = line.SalesQty - line.RemainSalesFinancial;
                line.SalesQty = input.Quantity;
                line.QtyOrdered = inventoryQuantity.Value;
                line.RemainSalesPhysical = Math.Max(0m, input.Quantity - processedSalesQuantity);
                line.RemainSalesFinancial = Math.Max(0m, input.Quantity - processedFinancialQuantity);
                line.RemainInventPhysical = inventoryQuantity.Value;
                line.RemainInventFinancial = inventoryQuantity.Value;
                line.LineAmount = CalculateGrossLineAmount(input);
                if (!string.IsNullOrWhiteSpace(input.Unit)) line.SalesUnit = input.Unit.Trim();
                if (input.DeliveryDate.HasValue) line.ReceiptDateRequested = input.DeliveryDate.Value.Date;
                line.DlvMode = input.DeliveryMode?.Trim() ?? string.Empty;
                line.DlvTerm = input.DeliveryTerms?.Trim() ?? string.Empty;
                line.ShippingDateRequested = input.ShippingDateRequested?.Date ?? default;
                line.ShippingDateConfirmed = input.ShippingDateConfirmed?.Date ?? default;
                line.ReceiptDateConfirmed = input.ReceiptDateConfirmed?.Date ?? default;
                line.OverDeliveryPct = input.OverDeliveryPercent;
                line.UnderDeliveryPct = input.UnderDeliveryPercent;
                line.DeliveryName = input.DeliveryName?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(input.DeliveryPostalAddress))
                    line.DeliveryPostalAddress = 0;
                else if (!long.TryParse(input.DeliveryPostalAddress, out var deliveryAddressId)
                    || !await _validation.IsDeliveryAddressAvailableForOrderAsync(order, deliveryAddressId, cancellationToken))
                    return new SalesLineWriteResult(SalesLineWriteStatus.Invalid, "The selected delivery address does not belong to this customer.");
                else
                    line.DeliveryPostalAddress = deliveryAddressId;
                if (!string.IsNullOrWhiteSpace(input.ReturnLotId))
                {
                    var returnLotId = input.ReturnLotId.Trim();
                    var returnLotExists = await _dbContext.Set<InventTransOrigin>().AnyAsync(origin =>
                        origin.DataAreaId == line.DataAreaId && origin.ItemId == line.ItemId
                        && origin.InventTransId == returnLotId, cancellationToken);
                    if (!returnLotExists)
                        return new SalesLineWriteResult(SalesLineWriteStatus.Invalid, "The selected return lot does not belong to this item.");
                    line.InventTransIdReturn = returnLotId;
                }
                else if (input.ReturnLotId != null)
                {
                    line.InventTransIdReturn = string.Empty;
                }
                if (input.LedgerDimension.HasValue)
                {
                    var ledgerDimensionExists = input.LedgerDimension.Value == 0 ||
                        await _dbContext.Set<DimensionAttributeValueCombination>().AnyAsync(dimension =>
                            dimension.DataAreaId == line.DataAreaId
                            && dimension.RecId == input.LedgerDimension.Value, cancellationToken);
                    if (!ledgerDimensionExists)
                        return new SalesLineWriteResult(SalesLineWriteStatus.Invalid, "The selected main account was not found.");
                    line.LedgerDimension = input.LedgerDimension.Value;
                }
                if (input.Reservation.HasValue) line.Reservation = input.Reservation.Value;
                if (input.AutoBatchReservation.HasValue)
                    line.PdsBatchAttribAutoRes = input.AutoBatchReservation.Value ? 1 : 0;
                if (input.SameBatchSelection.HasValue)
                    line.PdsSameLot = input.SameBatchSelection.Value ? 1 : 0;
                if (input.Scrap.HasValue) line.Scrap = input.Scrap.Value ? 1 : 0;
                if (input.SalesGroup != null) line.SalesGroup = input.SalesGroup.Trim();
                line.CustomerRef = input.CustomerReference?.Trim() ?? string.Empty;
                if (input.CustomerLineNumber.HasValue) line.CustomerLineNum = input.CustomerLineNumber.Value;
                line.PackingUnit = input.PackingUnit?.Trim() ?? string.Empty;
                line.PackingUnitQty = input.PackingUnitQuantity;
                line.PriceUnit = requestedPriceUnit;
                line.LineDisc = input.LineDiscount;
                line.LinePercent = input.LineDiscountPercent;
                line.MultiLnDisc = input.MultiLineDiscount;
                line.MultiLnPercent = input.MultiLineDiscountPercent;
                line.SalesMarkup = input.SalesMarkup;
                if (input.ExcludeFromRebate.HasValue)
                    line.PdsExcludeFromRebate = input.ExcludeFromRebate.Value ? 1 : 0;
                if (input.ExcludeFromRebateManagement.HasValue)
                    line.TamRebateExcludeRebateManagement = input.ExcludeFromRebateManagement.Value ? 1 : 0;
                line.TaxGroup = lineTaxGroup;
                line.TaxItemGroup = lineTaxItemGroup;
                var currentDimension = await _dbContext.Set<InventDim>().AsNoTracking()
                    .FirstOrDefaultAsync(item => item.DataAreaId == line.DataAreaId
                        && item.InventDimId == line.InventDimId, cancellationToken);
                var inventSiteId = string.IsNullOrWhiteSpace(input.InventSiteId)
                    ? currentDimension?.InventSiteId ?? order.InventSiteId
                    : input.InventSiteId.Trim();
                var inventLocationId = string.IsNullOrWhiteSpace(input.InventLocationId)
                    ? currentDimension?.InventLocationId ?? order.InventLocationId
                    : input.InventLocationId.Trim();
                var dimensionError = await _validation.ValidateSiteWarehouseAsync(inventSiteId, inventLocationId, cancellationToken);
                if (dimensionError != null)
                    return new SalesLineWriteResult(SalesLineWriteStatus.Invalid, dimensionError);
                var batchNumber = input.BatchNumber?.Trim();
                if (!string.IsNullOrEmpty(batchNumber)
                    && !await _dbContext.Set<InventBatch>().AnyAsync(batch => batch.DataAreaId == line.DataAreaId
                        && batch.ItemId == line.ItemId && batch.InventBatchId == batchNumber, cancellationToken))
                    return new SalesLineWriteResult(SalesLineWriteStatus.Invalid, "The selected batch does not belong to this item.");
                var serialNumber = input.SerialNumber?.Trim();
                if (!string.IsNullOrEmpty(serialNumber)
                    && !await _dbContext.Set<InventSerial>().AnyAsync(serial => serial.DataAreaId == line.DataAreaId
                        && serial.ItemId == line.ItemId && serial.InventSerialId == serialNumber, cancellationToken))
                    return new SalesLineWriteResult(SalesLineWriteStatus.Invalid, "The selected serial number does not belong to this item.");
                var dimensionTemplate = new InventDim
                {
                    ConfigId = input.ConfigId == null ? currentDimension?.ConfigId ?? string.Empty : input.ConfigId.Trim(),
                    InventSizeId = input.InventSizeId == null ? currentDimension?.InventSizeId ?? string.Empty : input.InventSizeId.Trim(),
                    InventColorId = input.InventColorId == null ? currentDimension?.InventColorId ?? string.Empty : input.InventColorId.Trim(),
                    InventStyleId = input.InventStyleId == null ? currentDimension?.InventStyleId ?? string.Empty : input.InventStyleId.Trim(),
                    InventVersionId = input.InventVersionId == null ? currentDimension?.InventVersionId ?? string.Empty : input.InventVersionId.Trim(),
                    InventBatchId = batchNumber ?? currentDimension?.InventBatchId ?? string.Empty,
                    InventSerialId = serialNumber ?? currentDimension?.InventSerialId ?? string.Empty,
                    InventStatusId = currentDimension?.InventStatusId ?? string.Empty,
                    WmsLocationId = currentDimension?.WmsLocationId ?? string.Empty,
                    LicensePlateId = currentDimension?.LicensePlateId ?? string.Empty,
                    InventDimension10 = currentDimension?.InventDimension10 ?? 0m,
                    InventDimension9 = currentDimension?.InventDimension9 ?? default,
                    InventDimension9TzId = currentDimension?.InventDimension9TzId ?? 0
                };
                await _inventoryDemand.UpdateAsync(line, inventSiteId, inventLocationId,
                    cancellationToken, batchNumber, serialNumber, dimensionTemplate);
                responseSiteId = inventSiteId;
                responseLocationId = inventLocationId;
            }
            order.SmmSalesAmountTotal += (input == null ? 0 : line.LineAmount) - previousAmount;
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new SalesLineWriteResult(SalesLineWriteStatus.Success,
                Data: input == null
                    ? new { deleted = true }
                    : SalesOrderLinePresenter.Record(line, responseSiteId, responseLocationId,
                        inventoryDimension: await _dbContext.Set<InventDim>().AsNoTracking()
                            .FirstOrDefaultAsync(item => item.DataAreaId == line.DataAreaId
                                && item.InventDimId == line.InventDimId, cancellationToken),
                        productName: await _dbContext.Set<InventTable>().AsNoTracking()
                            .Where(item => item.DataAreaId == line.DataAreaId && item.ItemId == line.ItemId)
                            .Select(item => item.NameAlias).FirstOrDefaultAsync(cancellationToken)));
        });
    }


    private static string? NormalizeAndValidateLineDiscount(SalesLineInputDto input)
    {
        var grossAmount = input.Quantity * input.UnitPrice / (input.PriceUnit.GetValueOrDefault() > 0 ? input.PriceUnit!.Value : 1m);
        if (input.LineDiscount < 0 || input.LineDiscountPercent < 0 || input.LineDiscountPercent > 100)
            return "The line discount must be nonnegative and the discount percentage must be between 0 and 100.";

        if (input.LineDiscountPercent > 0 && input.LineDiscount == 0)
            input.LineDiscount = decimal.Round(
                grossAmount * input.LineDiscountPercent / 100m,
                2,
                MidpointRounding.AwayFromZero);
        else if (input.LineDiscount > 0 && input.LineDiscountPercent == 0 && grossAmount > 0)
            input.LineDiscountPercent = decimal.Round(
                input.LineDiscount / grossAmount * 100m,
                4,
                MidpointRounding.AwayFromZero);

        if (input.LineDiscount > grossAmount)
            return "The line discount cannot exceed the gross line amount.";

        var expectedAmount = decimal.Round(
            grossAmount * input.LineDiscountPercent / 100m,
            2,
            MidpointRounding.AwayFromZero);
        if (Math.Abs(input.LineDiscount - expectedAmount) > 0.01m)
            return "The line discount amount does not match the discount percentage.";

        return null;
    }

    private async Task<(decimal Amount, decimal PriceUnit)?> FindSalesPriceAsync(SalesTable order,
        string itemId, decimal quantity, string unitId, decimal fallbackPriceUnit,
        string? inventDimId, CancellationToken cancellationToken)
    {
        var custParameters = await _dbContext.Set<CustParameters>().AsNoTracking()
            .Where(row => row.DataAreaId == order.DataAreaId)
            .OrderBy(row => row.Key)
            .FirstOrDefaultAsync(cancellationToken);
        if (custParameters?.PriceDiscSearchPrice == NoYes.No)
            return null;

        var today = DateTime.UtcNow.Date;
        var agreements = await _dbContext.Set<PriceDiscTable>().AsNoTracking()
            .Where(row => row.DataAreaId == order.DataAreaId
                && row.Module == ModuleInventCustVend.Cust
                && row.Relation == PriceType.PriceSales
                && row.Currency == order.CurrencyCode
                && (row.UnitId == unitId || row.UnitAppliesToAll != 0)
                && (string.IsNullOrEmpty(row.PriceGroup)
                    || (!string.IsNullOrEmpty(order.PriceGroupId)
                        && row.PriceGroup == order.PriceGroupId))
                && (string.IsNullOrEmpty(row.InventDimId)
                    || row.InventDimId == inventDimId)
                && (row.FromDate == default || row.FromDate <= today)
                && (row.ToDate == default || row.ToDate >= today)
                && (row.QuantityAmountFrom <= 0 || row.QuantityAmountFrom <= quantity)
                && (row.QuantityAmountTo <= 0 || row.QuantityAmountTo >= quantity)
                && ((row.ItemCode == PriceDiscProductCodeType.Table && row.ItemRelation == itemId)
                    || row.ItemCode == PriceDiscProductCodeType.All)
                && ((row.AccountCode == PriceDiscPartyCodeType.Table && row.AccountRelation == order.CustAccount)
                    || row.AccountCode == PriceDiscPartyCodeType.All))
            .ToListAsync(cancellationToken);

        var agreement = agreements
            .OrderByDescending(row => row.AccountCode == PriceDiscPartyCodeType.Table)
            .ThenByDescending(row => row.ItemCode == PriceDiscProductCodeType.Table)
            .ThenByDescending(row => !string.IsNullOrEmpty(row.PriceGroup))
            .ThenByDescending(row => !string.IsNullOrEmpty(row.InventDimId))
            .ThenByDescending(row => row.QuantityAmountFrom)
            .ThenByDescending(row => row.FromDate)
            .FirstOrDefault();
        if (agreement == null) return null;

        var agreementPriceUnit = agreement.PriceUnit > 0 ? agreement.PriceUnit :
            fallbackPriceUnit > 0 ? fallbackPriceUnit : 1m;
        return (agreement.Amount, agreementPriceUnit);
    }

    private static decimal CalculateGrossLineAmount(SalesLineInputDto input)
    {
        var priceUnit = input.PriceUnit.GetValueOrDefault() > 0 ? input.PriceUnit!.Value : 1m;
        return input.Quantity * input.UnitPrice / priceUnit;
    }

}
