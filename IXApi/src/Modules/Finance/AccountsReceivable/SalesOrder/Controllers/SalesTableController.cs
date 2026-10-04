using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;
using IAX.IXApi.Modules.Finance.Persistence;
using IAX.IXApi.Modules.Finance.Foundation.LogisticsAddresses;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Shared.Application.Identity;
using IAX.IXApi.Modules.Finance.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

[ApiController]
[Route("api/v1/SalesTable")]
[DomainPermission("AccountsReceivable", "SalesOrders", "View")]
public sealed class SalesTableController : ControllerBase
{
    private const int SalesTableDocumentId = 2002;
    private const int SalesLineDocumentId = 2003;

    private readonly IFinanceDataContext _dbContext;
    private readonly ISysNumberSequenceService _numberSequences;
    private readonly ICompanyExecutionContext _company;
    private readonly ISalesInventoryDemandService _inventoryDemand;

    public SalesTableController(
        IFinanceDataContext dbContext,
        ISysNumberSequenceService numberSequences,
        ICompanyExecutionContext company,
        ISalesInventoryDemandService inventoryDemand)
    {
        _dbContext = dbContext;
        _numberSequences = numberSequences;
        _company = company;
        _inventoryDemand = inventoryDemand;
    }

    [HttpGet("units")]
    public async Task<IActionResult> Units(string? search = null, int pageNumber = 1, int pageSize = 25, CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = _dbContext.Set<UnitOfMeasure>().AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(unit => unit.Symbol.Contains(search));
        var totalRecords = await query.CountAsync(cancellationToken);
        var data = await query.OrderBy(unit => unit.Symbol).Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .Select(unit => new { symbol = unit.Symbol }).ToListAsync(cancellationToken);
        return Ok(APIResponse<object>.Ok(new { data, pageNumber, totalRecords, totalPages = (int)Math.Ceiling((double)totalRecords / pageSize) }));
    }

    [HttpGet("items")]
    public async Task<IActionResult> Items(string? search = null, int pageNumber = 1, int pageSize = 25, CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = _dbContext.Set<InventTable>().AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(item => item.ItemId.Contains(search) || item.NameAlias.Contains(search));
        var totalRecords = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(item => item.ItemId).Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);
        var itemIds = items.Select(item => item.ItemId).ToList();
        var modules = await _dbContext.Set<InventTableModule>().AsNoTracking()
            .Where(module => itemIds.Contains(module.ItemId) && (int)module.ModuleType == 2).ToListAsync(cancellationToken);
        var data = items.Select(item => {
            var module = modules.FirstOrDefault(row => row.ItemId == item.ItemId);
            return new { itemNumber = item.ItemId, name = item.NameAlias, itemType = item.ItemType.ToString(),
                unit = module?.UnitId ?? string.Empty, unitPrice = module == null ? 0 : module.Price / (module.PriceUnit > 0 ? module.PriceUnit : 1) };
        }).ToList();
        return Ok(APIResponse<object>.Ok(new { data, pageNumber, totalRecords, totalPages = (int)Math.Ceiling((double)totalRecords / pageSize) }));
    }

    [HttpGet("inventory-dimensions")]
    public async Task<IActionResult> InventoryDimensions(CancellationToken cancellationToken = default)
    {
        var sites = await _dbContext.Set<InventSite>().AsNoTracking()
            .OrderBy(site => site.SiteId)
            .Select(site => new { id = site.SiteId, code = site.SiteId, name = site.Name })
            .ToListAsync(cancellationToken);
        var warehouses = await _dbContext.Set<InventLocation>().AsNoTracking()
            .OrderBy(location => location.InventLocationId)
            .Select(location => new {
                id = location.InventLocationId,
                code = location.InventLocationId,
                name = location.Name,
                siteId = location.InventSiteId
            })
            .ToListAsync(cancellationToken);
        return Ok(APIResponse<object>.Ok(new { sites, warehouses }));
    }

    [HttpGet("tax-groups")]
    public async Task<IActionResult> TaxGroups(CancellationToken cancellationToken = default)
    {
        var salesTaxGroups = await _dbContext.TaxGroupHeadings.AsNoTracking()
            .OrderBy(group => group.TaxGroup)
            .Select(group => new {
                id = group.TaxGroup,
                code = group.TaxGroup,
                name = group.TaxGroupName
            })
            .ToListAsync(cancellationToken);
        var itemSalesTaxGroups = await _dbContext.Set<TaxItemGroupHeading>().AsNoTracking()
            .OrderBy(group => group.TaxItemGroup)
            .Select(group => new {
                id = group.TaxItemGroup,
                code = group.TaxItemGroup,
                name = group.Name
            })
            .ToListAsync(cancellationToken);
        return Ok(APIResponse<object>.Ok(new { salesTaxGroups, itemSalesTaxGroups }));
    }

    [HttpGet("{recId:long}/lines")]
    public async Task<IActionResult> Lines(long recId, CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.Set<SalesTable>().AsNoTracking().FirstOrDefaultAsync(row => row.RecId == recId, cancellationToken);
        if (order == null) return NotFound(APIResponse<object>.Fail("Sales order was not found."));
        var lines = await _dbContext.Set<SalesLine>().AsNoTracking()
            .Where(line => line.SalesId == order.SalesId && line.DataAreaId == order.DataAreaId)
            .OrderBy(line => line.LineNum).ToListAsync(cancellationToken);
        var dimensionIds = lines.Select(line => line.InventDimId).Where(id => id != string.Empty).Distinct().ToList();
        var dimensions = await _dbContext.Set<InventDim>().AsNoTracking()
            .Where(item => item.DataAreaId == order.DataAreaId && dimensionIds.Contains(item.InventDimId))
            .ToDictionaryAsync(item => item.InventDimId, cancellationToken);
        return Ok(APIResponse<object>.Ok(lines.Select(line => dimensions.TryGetValue(line.InventDimId, out var dimension)
            ? LineRecord(line, dimension.InventSiteId, dimension.InventLocationId)
            : LineRecord(line))));
    }

    [HttpGet("{recId:long}/totals")]
    public async Task<IActionResult> Totals(long recId, CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.Set<SalesTable>().AsNoTracking()
            .FirstOrDefaultAsync(row => row.RecId == recId, cancellationToken);
        if (order == null) return NotFound(APIResponse<object>.Fail("Sales order was not found."));

        var lines = await _dbContext.Set<SalesLine>().AsNoTracking()
            .Where(line => line.SalesId == order.SalesId && line.DataAreaId == order.DataAreaId)
            .ToListAsync(cancellationToken);
        var lineRecIds = lines.Select(line => line.RecId).ToList();
        var charges = await _dbContext.Set<MarkupTrans>().AsNoTracking()
            .Where(charge => charge.DataAreaId == order.DataAreaId
                && charge.IsDeleted != NoYes.Yes
                && (charge.ModuleType == MarkupModuleType.Customer || charge.ModuleType == MarkupModuleType.Sales)
                && ((charge.TransRecId == order.RecId
                        && (charge.TransTableId == SalesTableDocumentId || charge.TransTableId == 0))
                    || (lineRecIds.Contains(charge.TransRecId)
                        && charge.TransTableId == SalesLineDocumentId)))
            .ToListAsync(cancellationToken);
        var taxGroups = lines.Select(line => line.TaxGroup)
            .Concat(charges.Select(charge => charge.TaxGroup))
            .Where(value => value != string.Empty).Distinct().ToList();
        var itemTaxGroups = lines.Select(line => line.TaxItemGroup)
            .Concat(charges.Select(charge => charge.TaxItemGroup))
            .Where(value => value != string.Empty).Distinct().ToList();
        var groupRows = await _dbContext.Set<TaxGroupData>().AsNoTracking()
            .Where(row => row.DataAreaId == order.DataAreaId && taxGroups.Contains(row.TaxGroup))
            .ToListAsync(cancellationToken);
        var itemRows = await _dbContext.Set<TaxOnItem>().AsNoTracking()
            .Where(row => row.DataAreaId == order.DataAreaId && itemTaxGroups.Contains(row.TaxItemGroup))
            .ToListAsync(cancellationToken);
        var taxCodes = groupRows.Select(row => row.TaxCode).Intersect(itemRows.Select(row => row.TaxCode)).Distinct().ToList();
        var rates = await _dbContext.Set<TaxData>().AsNoTracking()
            .Where(row => row.DataAreaId == order.DataAreaId && taxCodes.Contains(row.TaxCode))
            .ToListAsync(cancellationToken);

        decimal grossAmount = 0;
        decimal lineDiscount = 0;
        decimal multiLineDiscount = 0;
        decimal salesTax = 0;
        decimal totalCharges = 0;
        foreach (var line in lines)
        {
            grossAmount += line.LineAmount;
            lineDiscount += line.LineDisc;
            multiLineDiscount += line.MultiLnDisc;
            var taxableAmount = Math.Max(0, line.LineAmount - line.LineDisc - line.MultiLnDisc);
            var applicableCodes = groupRows
                .Where(row => row.TaxGroup == line.TaxGroup && row.ExemptTax != NoYes.Yes)
                .Select(row => row.TaxCode)
                .Intersect(itemRows.Where(row => row.TaxItemGroup == line.TaxItemGroup).Select(row => row.TaxCode));
            var combinedRate = applicableCodes.Distinct().Sum(taxCode =>
            {
                var effectiveRate = rates
                    .Where(rate => rate.TaxCode == taxCode
                        && (rate.TaxFromDate == default || rate.TaxFromDate.Date <= order.OrderDate.Date)
                        && (rate.TaxToDate == default || rate.TaxToDate.Date >= order.OrderDate.Date))
                    .OrderByDescending(rate => rate.TaxFromDate)
                    .FirstOrDefault();
                return effectiveRate?.TaxValue ?? 0m;
            });
            salesTax += order.InclTax && combinedRate > 0
                ? taxableAmount * combinedRate / (100m + combinedRate)
                : taxableAmount * combinedRate / 100m;
        }

        foreach (var charge in charges)
        {
            var chargeAmount = charge.CalculatedAmount != 0m ? charge.CalculatedAmount : charge.Value;
            totalCharges += chargeAmount;
            var applicableCodes = groupRows
                .Where(row => row.TaxGroup == charge.TaxGroup && row.ExemptTax != NoYes.Yes)
                .Select(row => row.TaxCode)
                .Intersect(itemRows.Where(row => row.TaxItemGroup == charge.TaxItemGroup).Select(row => row.TaxCode));
            var combinedRate = applicableCodes.Distinct().Sum(taxCode =>
            {
                var effectiveRate = rates
                    .Where(rate => rate.TaxCode == taxCode
                        && (rate.TaxFromDate == default || rate.TaxFromDate.Date <= order.OrderDate.Date)
                        && (rate.TaxToDate == default || rate.TaxToDate.Date >= order.OrderDate.Date))
                    .OrderByDescending(rate => rate.TaxFromDate)
                    .FirstOrDefault();
                return effectiveRate?.TaxValue ?? 0m;
            });
            salesTax += order.InclTax && combinedRate > 0
                ? chargeAmount * combinedRate / (100m + combinedRate)
                : chargeAmount * combinedRate / 100m;
        }

        var totalDiscount = lineDiscount + multiLineDiscount;
        var subtotal = grossAmount - totalDiscount - (order.InclTax ? salesTax : 0m);
        return Ok(APIResponse<object>.Ok(new
        {
            currencyCode = order.CurrencyCode,
            grossAmount,
            lineDiscount,
            multiLineDiscount,
            totalDiscount,
            subtotal,
            totalCharges,
            salesTax,
            invoiceAmount = subtotal + totalCharges + salesTax,
            quantity = lines.Sum(line => line.SalesQty),
            costValue = lines.Sum(line => line.CostPrice * line.SalesQty)
        }));
    }

    public sealed class AddSalesLineInput
    {
        [System.ComponentModel.DataAnnotations.Required]
        public string ItemNumber { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Range(typeof(decimal), "0.000001", "1000000000")]
        public decimal Quantity { get; set; }
        [System.ComponentModel.DataAnnotations.Range(typeof(decimal), "0", "1000000000")]
        public decimal UnitPrice { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(100)]
        public string? Description { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(10)]
        public string? Unit { get; set; }
        public DateTime? DeliveryDate { get; set; }
        [System.ComponentModel.DataAnnotations.EnumDataType(typeof(SalesType))]
        public SalesType? LineType { get; set; }
        [System.ComponentModel.DataAnnotations.EnumDataType(typeof(SalesDeliveryType))]
        public SalesDeliveryType? DeliveryType { get; set; }
        [System.ComponentModel.DataAnnotations.Range(typeof(long), "0", "9223372036854775807")]
        public long SalesCategory { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(10)]
        public string? InventSiteId { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(10)]
        public string? InventLocationId { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.DlvModeId)]
        public string? DeliveryMode { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.DlvTermId)]
        public string? DeliveryTerms { get; set; }
        public DateTime? ShippingDateRequested { get; set; }
        public DateTime? ShippingDateConfirmed { get; set; }
        public DateTime? ReceiptDateConfirmed { get; set; }
        [System.ComponentModel.DataAnnotations.Range(typeof(decimal), "0", "100")]
        public decimal OverDeliveryPercent { get; set; }
        [System.ComponentModel.DataAnnotations.Range(typeof(decimal), "0", "100")]
        public decimal UnderDeliveryPercent { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.Name)]
        public string? DeliveryName { get; set; }
        public long DeliveryPostalAddress { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.ReferenceId)]
        public string? CustomerReference { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.UnitId)]
        public string? PackingUnit { get; set; }
        [System.ComponentModel.DataAnnotations.Range(typeof(decimal), "0", "1000000000")]
        public decimal PackingUnitQuantity { get; set; }
        [System.ComponentModel.DataAnnotations.Range(typeof(decimal), "0.000001", "1000000000")]
        public decimal PriceUnit { get; set; } = 1;
        [System.ComponentModel.DataAnnotations.Range(typeof(decimal), "0", "1000000000")]
        public decimal LineDiscount { get; set; }
        [System.ComponentModel.DataAnnotations.Range(typeof(decimal), "0", "100")]
        public decimal LineDiscountPercent { get; set; }
        [System.ComponentModel.DataAnnotations.Range(typeof(decimal), "0", "1000000000")]
        public decimal MultiLineDiscount { get; set; }
        [System.ComponentModel.DataAnnotations.Range(typeof(decimal), "0", "100")]
        public decimal MultiLineDiscountPercent { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.TaxGroup)]
        public string? TaxGroup { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.TaxItemGroup)]
        public string? TaxItemGroup { get; set; }
    }

    [HttpPost("{recId:long}/lines")]
    [DomainPermission("AccountsReceivable", "SalesOrders", "Edit")]
    public async Task<IActionResult> AddLine(long recId, [FromBody] AddSalesLineInput input, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
            var order = await _dbContext.Set<SalesTable>().FirstOrDefaultAsync(row => row.RecId == recId, cancellationToken);
            if (order == null) return (IActionResult)NotFound(APIResponse<object>.Fail("Sales order was not found."));
            if (order.SalesStatus != SalesStatus.Backorder)
                return UnprocessableEntity(APIResponse<object>.Fail("Lines can only be added to open sales orders."));
            var item = await _dbContext.Set<InventTable>().AsNoTracking()
                .FirstOrDefaultAsync(row => row.ItemId == input.ItemNumber && row.DataAreaId == order.DataAreaId, cancellationToken);
            if (item == null) return UnprocessableEntity(APIResponse<object>.Fail("Item was not found."));
            var module = await _dbContext.Set<InventTableModule>().AsNoTracking()
                .FirstOrDefaultAsync(row => row.ItemId == item.ItemId && row.DataAreaId == order.DataAreaId && (int)row.ModuleType == 2, cancellationToken);
            if (module == null || string.IsNullOrWhiteSpace(module.UnitId))
                return UnprocessableEntity(APIResponse<object>.Fail("The item must have a sales unit configured."));
            var discountValidationError = NormalizeAndValidateLineDiscount(input);
            if (discountValidationError != null)
                return UnprocessableEntity(APIResponse<object>.Fail(discountValidationError));
            var lineTaxGroup = string.IsNullOrWhiteSpace(input.TaxGroup) ? order.TaxGroupId : input.TaxGroup.Trim();
            var lineTaxItemGroup = string.IsNullOrWhiteSpace(input.TaxItemGroup) ? module.TaxItemGroupId : input.TaxItemGroup.Trim();
            var taxValidationError = await ValidateTaxSetupAsync(lineTaxGroup, lineTaxItemGroup, cancellationToken);
            if (taxValidationError != null)
                return UnprocessableEntity(APIResponse<object>.Fail(taxValidationError));
            var lastLine = await _dbContext.Set<SalesLine>()
                .Where(row => row.SalesId == order.SalesId && row.DataAreaId == order.DataAreaId)
                .MaxAsync(row => (decimal?)row.LineNum, cancellationToken) ?? 0;
            var line = new SalesLine {
                SalesId = order.SalesId, LineNum = lastLine + 1, ItemId = item.ItemId,
                Name = string.IsNullOrWhiteSpace(input.Description) ? item.NameAlias : input.Description.Trim(),
                CustAccount = order.CustAccount, CustGroupId = order.CustGroup, CurrencyCode = order.CurrencyCode,
                SalesQty = input.Quantity, QtyOrdered = input.Quantity, RemainSalesPhysical = input.Quantity,
                RemainSalesFinancial = input.Quantity, SalesUnit = string.IsNullOrWhiteSpace(input.Unit) ? module.UnitId : input.Unit.Trim(), PriceUnit = input.PriceUnit,
                SalesPrice = input.UnitPrice, LineAmount = input.Quantity * input.UnitPrice,
                SalesStatus = SalesStatus.Backorder, SalesType = input.LineType ?? order.SalesType ?? SalesType.Sales,
                DeliveryType = input.DeliveryType ?? SalesDeliveryType.None, SalesCategory = input.SalesCategory,
                ReceiptDateRequested = input.DeliveryDate?.Date ?? order.ReceiptDateRequested,
                ShippingDateRequested = input.ShippingDateRequested?.Date ?? order.ShippingDateRequested,
                ShippingDateConfirmed = input.ShippingDateConfirmed?.Date ?? default,
                ReceiptDateConfirmed = input.ReceiptDateConfirmed?.Date ?? default,
                DlvMode = input.DeliveryMode?.Trim() ?? order.DlvMode,
                DlvTerm = input.DeliveryTerms?.Trim() ?? order.DlvTerm,
                OverDeliveryPct = input.OverDeliveryPercent,
                UnderDeliveryPct = input.UnderDeliveryPercent,
                DeliveryName = input.DeliveryName?.Trim() ?? order.DeliveryName,
                DeliveryPostalAddress = input.DeliveryPostalAddress,
                CustomerRef = input.CustomerReference?.Trim() ?? string.Empty,
                PackingUnit = input.PackingUnit?.Trim() ?? string.Empty,
                PackingUnitQty = input.PackingUnitQuantity,
                LineDisc = input.LineDiscount,
                LinePercent = input.LineDiscountPercent,
                MultiLnDisc = input.MultiLineDiscount,
                MultiLnPercent = input.MultiLineDiscountPercent,
                TaxGroup = lineTaxGroup,
                TaxItemGroup = lineTaxItemGroup,
                DataAreaId = order.DataAreaId,
            };
            var inventSiteId = string.IsNullOrWhiteSpace(input.InventSiteId) ? order.InventSiteId : input.InventSiteId;
            var inventLocationId = string.IsNullOrWhiteSpace(input.InventLocationId) ? order.InventLocationId : input.InventLocationId;
            await _inventoryDemand.CreateAsync(order, line,
                inventSiteId, inventLocationId, cancellationToken);
            _dbContext.Set<SalesLine>().Add(line);
            order.SmmSalesAmountTotal += line.LineAmount;
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Ok(APIResponse<object>.Ok(LineRecord(line, inventSiteId, inventLocationId)));
        });
    }


    [HttpPut("{recId:long}/lines/{lineId:long}")]
    [DomainPermission("AccountsReceivable", "SalesOrders", "Edit")]
    public Task<IActionResult> UpdateLine(long recId, long lineId, [FromBody] AddSalesLineInput input, CancellationToken cancellationToken = default)
        => ChangeLine(recId, lineId, input, cancellationToken);

    [HttpDelete("{recId:long}/lines/{lineId:long}")]
    [DomainPermission("AccountsReceivable", "SalesOrders", "Edit")]
    public Task<IActionResult> RemoveLine(long recId, long lineId, CancellationToken cancellationToken = default)
        => ChangeLine(recId, lineId, null, cancellationToken);

    [HttpPost("{recId:long}/cancel")]
    [DomainPermission("AccountsReceivable", "SalesOrders", "Edit")]
    public async Task<IActionResult> Cancel(long recId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable, cancellationToken);
            var order = await _dbContext.Set<SalesTable>().FirstOrDefaultAsync(
                row => row.RecId == recId, cancellationToken);
            if (order == null) return (IActionResult)NotFound(APIResponse<object>.Fail("Sales order was not found."));
            if (order.SalesStatus != SalesStatus.Backorder)
                return UnprocessableEntity(APIResponse<object>.Fail("Only an open sales order can be cancelled."));
            var lines = await _dbContext.Set<SalesLine>()
                .Where(row => row.SalesId == order.SalesId && row.DataAreaId == order.DataAreaId)
                .ToListAsync(cancellationToken);
            if (lines.Any(line => line.SalesStatus != SalesStatus.Backorder
                || line.RemainSalesPhysical != line.SalesQty
                || line.RemainSalesFinancial != line.SalesQty))
                return UnprocessableEntity(APIResponse<object>.Fail(
                    "This cancellation currently supports only completely open, unprocessed sales orders."));
            foreach (var line in lines)
                await _inventoryDemand.CancelRemainingAsync(line, cancellationToken);
            order.SalesStatus = SalesStatus.Canceled;
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Ok(APIResponse<object>.Ok(new { cancelled = true }));
        });
    }

    private async Task<IActionResult> ChangeLine(long recId, long lineId, AddSalesLineInput? input, CancellationToken cancellationToken)
    {
        return await _dbContext.Database.CreateExecutionStrategy().ExecuteAsync(async () => {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
            var order = await _dbContext.Set<SalesTable>().FirstOrDefaultAsync(row => row.RecId == recId, cancellationToken);
            if (order == null) return (IActionResult)NotFound(APIResponse<object>.Fail("Sales order was not found."));
            if (order.SalesStatus != SalesStatus.Backorder)
                return UnprocessableEntity(APIResponse<object>.Fail("Only open sales orders can be changed."));
            var line = await _dbContext.Set<SalesLine>().FirstOrDefaultAsync(row => row.RecId == lineId && row.SalesId == order.SalesId && row.DataAreaId == order.DataAreaId, cancellationToken);
            if (line == null) return NotFound(APIResponse<object>.Fail("Sales line was not found."));
            if (line.SalesStatus != SalesStatus.Backorder || line.RemainSalesPhysical != line.SalesQty || line.RemainSalesFinancial != line.SalesQty)
                return UnprocessableEntity(APIResponse<object>.Fail("Processed sales lines cannot be changed."));
            if (input != null && input.ItemNumber != line.ItemId)
                return UnprocessableEntity(APIResponse<object>.Fail("The item number cannot be changed."));
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
                    return UnprocessableEntity(APIResponse<object>.Fail(discountValidationError));
                var lineTaxGroup = string.IsNullOrWhiteSpace(input.TaxGroup) ? order.TaxGroupId : input.TaxGroup.Trim();
                var lineTaxItemGroup = input.TaxItemGroup?.Trim() ?? string.Empty;
                var taxValidationError = await ValidateTaxSetupAsync(lineTaxGroup, lineTaxItemGroup, cancellationToken);
                if (taxValidationError != null)
                    return UnprocessableEntity(APIResponse<object>.Fail(taxValidationError));
                // Item identity and name remain unchanged after item selection.
                line.SalesType = input.LineType ?? line.SalesType;
                line.DeliveryType = input.DeliveryType ?? line.DeliveryType;
                line.SalesCategory = input.SalesCategory;
                if (!string.IsNullOrWhiteSpace(input.Description)) line.Name = input.Description.Trim();
                line.SalesQty = input.Quantity;
                line.QtyOrdered = input.Quantity;
                line.RemainSalesPhysical = input.Quantity;
                line.RemainSalesFinancial = input.Quantity;
                line.SalesPrice = input.UnitPrice;
                line.PriceUnit = 1;
                line.LineAmount = input.Quantity * input.UnitPrice;
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
                line.DeliveryPostalAddress = input.DeliveryPostalAddress;
                line.CustomerRef = input.CustomerReference?.Trim() ?? string.Empty;
                line.PackingUnit = input.PackingUnit?.Trim() ?? string.Empty;
                line.PackingUnitQty = input.PackingUnitQuantity;
                line.PriceUnit = input.PriceUnit;
                line.LineDisc = input.LineDiscount;
                line.LinePercent = input.LineDiscountPercent;
                line.MultiLnDisc = input.MultiLineDiscount;
                line.MultiLnPercent = input.MultiLineDiscountPercent;
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
                await _inventoryDemand.UpdateAsync(line, inventSiteId, inventLocationId, cancellationToken);
                responseSiteId = inventSiteId;
                responseLocationId = inventLocationId;
            }
            order.SmmSalesAmountTotal += (input == null ? 0 : line.LineAmount) - previousAmount;
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Ok(APIResponse<object>.Ok(input == null
                ? new { deleted = true }
                : LineRecord(line, responseSiteId, responseLocationId)));
        });
    }

    private static object LineRecord(SalesLine line, string? inventSiteId = null, string? inventLocationId = null) => new {
        id = line.RecId.ToString(), lineNumber = line.LineNum, itemNumber = line.ItemId,
        lineType = (int)line.SalesType, deliveryType = (int)line.DeliveryType, salesCategory = line.SalesCategory,
        description = line.Name, quantity = line.SalesQty, unit = line.SalesUnit,
        unitPrice = line.SalesPrice, lineTotal = line.LineAmount, deliveryDate = line.ReceiptDateRequested,
        line.InventTransId, line.InventDimId, line.CurrencyCode, salesStatus = line.SalesStatus.ToString(),
        line.PriceUnit, line.CostPrice, lineDiscount = line.LineDisc, lineDiscountPercent = line.LinePercent,
        multiLineDiscount = line.MultiLnDisc, multiLineDiscountPercent = line.MultiLnPercent,
        overDeliveryPercent = line.OverDeliveryPct, underDeliveryPercent = line.UnderDeliveryPct,
        line.RemainSalesPhysical, line.RemainSalesFinancial, line.SalesDeliverNow, line.InventDeliverNow,
        line.PackingUnit, packingUnitQuantity = line.PackingUnitQty, deliveryMode = line.DlvMode,
        deliveryTerms = line.DlvTerm,
        shippingDateRequested = line.ShippingDateRequested == default ? (DateTime?)null : line.ShippingDateRequested,
        shippingDateConfirmed = line.ShippingDateConfirmed == default ? (DateTime?)null : line.ShippingDateConfirmed,
        receiptDateConfirmed = line.ReceiptDateConfirmed == default ? (DateTime?)null : line.ReceiptDateConfirmed,
        customerReference = line.CustomerRef, line.DeliveryName,
        line.DeliveryPostalAddress, line.TaxGroup, line.TaxItemGroup, line.LedgerDimension,
        line.DefaultDimension, financialTag = line.FinTag, line.IntrastatCommodity,
        site = inventSiteId, warehouse = inventLocationId,
    };

    private static string? NormalizeAndValidateLineDiscount(AddSalesLineInput input)
    {
        var grossAmount = input.Quantity * input.UnitPrice;
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

    private async Task<string?> ValidateTaxSetupAsync(
        string taxGroup,
        string taxItemGroup,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(taxGroup))
            return "A sales tax group is required for the sales line.";
        if (string.IsNullOrWhiteSpace(taxItemGroup))
            return "An item sales tax group is required for the sales line.";

        var salesGroupExists = await _dbContext.Set<TaxGroupHeading>().AsNoTracking()
            .AnyAsync(group => group.TaxGroup == taxGroup, cancellationToken);
        if (!salesGroupExists)
            return $"Sales tax group '{taxGroup}' was not found.";

        var itemGroupExists = await _dbContext.Set<TaxItemGroupHeading>().AsNoTracking()
            .AnyAsync(group => group.TaxItemGroup == taxItemGroup, cancellationToken);
        if (!itemGroupExists)
            return $"Item sales tax group '{taxItemGroup}' was not found.";

        var salesTaxCodes = _dbContext.Set<TaxGroupData>().AsNoTracking()
            .Where(row => row.TaxGroup == taxGroup)
            .Select(row => row.TaxCode);
        var hasCommonTaxCode = await _dbContext.Set<TaxOnItem>().AsNoTracking()
            .AnyAsync(row => row.TaxItemGroup == taxItemGroup && salesTaxCodes.Contains(row.TaxCode), cancellationToken);
        return hasCommonTaxCode
            ? null
            : $"Sales tax group '{taxGroup}' and item sales tax group '{taxItemGroup}' do not share a sales tax code.";
    }

    [HttpGet("list")]
    public async Task<ActionResult<APIResponse<IEnumerable<SalesOrderListDto>>>> GetList(
        CancellationToken cancellationToken = default)
    {
        var orders = await _dbContext.Set<SalesTable>()
            .AsNoTracking()
            .OrderByDescending(order => order.RecId)
            .ToListAsync(cancellationToken);

        var customerAccounts = orders.Select(order => order.CustAccount).Where(account => account != string.Empty).Distinct().ToList();
        var customers = await _dbContext.Set<CustTable>()
            .AsNoTracking()
            .Where(customer => customerAccounts.Contains(customer.AccountNum))
            .Select(customer => new { customer.AccountNum, customer.Party })
            .ToListAsync(cancellationToken);
        var partyIds = customers.Select(customer => customer.Party).Where(id => id > 0).Distinct().ToList();
        var parties = await _dbContext.Set<DirPartyTable>()
            .AsNoTracking()
            .Where(party => partyIds.Contains(party.RecId))
            .ToDictionaryAsync(party => party.RecId, party => party.Name, cancellationToken);
        var customerNames = customers.ToDictionary(
            customer => customer.AccountNum,
            customer => parties.GetValueOrDefault(customer.Party) ?? customer.AccountNum);

        var result = orders.Select(order => new SalesOrderListDto
        {
            RecId = order.RecId,
            SalesId = order.SalesId,
            CustomerAccount = order.CustAccount,
            CustomerName = customerNames.GetValueOrDefault(order.CustAccount) ?? order.SalesName,
            InvoiceAccount = order.InvoiceAccount,
            CustomerGroup = order.CustGroup,
            CurrencyCode = order.CurrencyCode,
            SalesStatus = order.SalesStatus.ToString(),
            DocumentStatus = order.DocumentStatus.ToString(),
            DeliveryDate = order.DeliveryDate,
            ShippingDateRequested = order.ShippingDateRequested,
            OrderTotal = order.SmmSalesAmountTotal,
            CustomerReference = order.CustomerRef,
            DeliveryMode = order.DlvMode,
            DeliveryTerms = order.DlvTerm,
            OrderDate = order.OrderDate == default ? order.CreatedAt?.Date ?? default : order.OrderDate,
            InventSiteId = order.InventSiteId,
            InventLocationId = order.InventLocationId,
            SalesNameAlias = order.SalesNameAlias,
            SalesType = (int)(order.SalesType ?? SalesType.Sales),
            OneTimeCustomer = order.OneTimeCustomer == NoYes.Yes,
            Email = order.Email,
            Phone = order.Phone,
            Deadline = order.Deadline == default ? null : order.Deadline,
            CustomerRequisitionNumber = order.CustRequisitionNum,
            CampaignId = order.SmmCampaignId,
            TaxGroupId = order.TaxGroupId,
            PricesIncludeSalesTax = order.InclTax,
            SalesGroup = order.SalesGroup,
            LanguageId = order.LanguageId,
            PaymentTerms = order.PaymTerm
        }).ToList();

        return Ok(APIResponse<IEnumerable<SalesOrderListDto>>.Ok(result));
    }

    public sealed class UpdateSalesHeaderInput
    {
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.InvoiceAccount)]
        public string InvoiceAccount { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.CurrencyCode)]
        public string CurrencyCode { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.ReferenceId)]
        public string CustomerReference { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.PaymTermId)]
        public string PaymentTerms { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.DlvModeId)]
        public string DeliveryMode { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.DlvTermId)]
        public string DeliveryTerms { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.InventSiteId)]
        public string InventSiteId { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.InventLocationId)]
        public string InventLocationId { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Required]
        public DateTime? OrderDate { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.NameAlias)]
        public string SalesNameAlias { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.EnumDataType(typeof(SalesType))]
        public SalesType SalesType { get; set; } = SalesType.Sales;
        public bool OneTimeCustomer { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.Email)]
        public string Email { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.Phone)]
        public string Phone { get; set; } = string.Empty;
        public DateTime? Deadline { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.Description)]
        public string CustomerRequisitionNumber { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.CampaignId)]
        public string CampaignId { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.TaxGroupId)]
        public string TaxGroupId { get; set; } = string.Empty;
        public bool PricesIncludeSalesTax { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.SalesGroupId)]
        public string SalesGroup { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.LanguageId)]
        public string LanguageId { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Required]
        public DateTime? DeliveryDate { get; set; }
    }

    [HttpPut("{recId:long}/header")]
    [DomainPermission("AccountsReceivable", "SalesOrders", "Edit")]
    public async Task<IActionResult> UpdateHeader(long recId, [FromBody] UpdateSalesHeaderInput input, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
            var order = await _dbContext.Set<SalesTable>().FirstOrDefaultAsync(row => row.RecId == recId, cancellationToken);
            if (order == null) return (IActionResult)NotFound(APIResponse<object>.Fail("Sales order was not found."));
            if (order.SalesStatus != SalesStatus.Backorder)
                return UnprocessableEntity(APIResponse<object>.Fail("Only open sales orders can be changed."));
            var invoiceAccount = input.InvoiceAccount.Trim();
            if (!await _dbContext.Set<CustTable>().AnyAsync(row => row.AccountNum == invoiceAccount && row.DataAreaId == order.DataAreaId, cancellationToken))
                return UnprocessableEntity(APIResponse<object>.Fail("Invoice account was not found."));
            var currency = input.CurrencyCode.Trim();
            if (currency != order.CurrencyCode && await _dbContext.Set<SalesLine>().AnyAsync(row => row.SalesId == order.SalesId && row.DataAreaId == order.DataAreaId, cancellationToken))
                return UnprocessableEntity(APIResponse<object>.Fail("Currency cannot be changed after sales lines have been added."));
            order.InvoiceAccount = invoiceAccount;
            order.CurrencyCode = currency;
            order.CustomerRef = input.CustomerReference?.Trim() ?? string.Empty;
            order.PaymTerm = input.PaymentTerms?.Trim() ?? string.Empty;
            order.DlvMode = input.DeliveryMode?.Trim() ?? string.Empty;
            order.DlvTerm = input.DeliveryTerms?.Trim() ?? string.Empty;
            var inventSiteId = input.InventSiteId.Trim();
            var inventLocationId = input.InventLocationId.Trim();
            if (!string.IsNullOrEmpty(inventSiteId) &&
                !await _dbContext.Set<InventSite>().AnyAsync(row => row.SiteId == inventSiteId, cancellationToken))
                return UnprocessableEntity(APIResponse<object>.Fail("Site was not found."));
            if (!string.IsNullOrEmpty(inventLocationId) &&
                !await _dbContext.Set<InventLocation>().AnyAsync(row => row.InventLocationId == inventLocationId && row.InventSiteId == inventSiteId, cancellationToken))
                return UnprocessableEntity(APIResponse<object>.Fail("Warehouse was not found in the selected site."));
            order.InventSiteId = inventSiteId;
            order.InventLocationId = inventLocationId;
            order.OrderDate = input.OrderDate!.Value.Date;
            order.SalesNameAlias = input.SalesNameAlias.Trim();
            order.SalesType = input.SalesType;
            order.OneTimeCustomer = input.OneTimeCustomer ? NoYes.Yes : NoYes.No;
            order.Email = input.Email.Trim();
            order.Phone = input.Phone.Trim();
            order.Deadline = input.Deadline?.Date ?? default;
            order.CustRequisitionNum = input.CustomerRequisitionNumber.Trim();
            order.SmmCampaignId = input.CampaignId.Trim();
            var taxGroupId = input.TaxGroupId.Trim();
            if (!await _dbContext.Set<TaxGroupHeading>().AsNoTracking()
                    .AnyAsync(group => group.TaxGroup == taxGroupId, cancellationToken))
                return UnprocessableEntity(APIResponse<object>.Fail("Sales tax group was not found."));
            order.TaxGroupId = taxGroupId;
            order.InclTax = input.PricesIncludeSalesTax;
            order.SalesGroup = input.SalesGroup.Trim();
            order.LanguageId = input.LanguageId.Trim();
            order.DeliveryDate = input.DeliveryDate!.Value.Date;
            order.ReceiptDateRequested = order.DeliveryDate;
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Ok(APIResponse<object>.Ok(new { saved = true }));
        });
    }

    [HttpPost("quick-create")]
    [DomainPermission("AccountsReceivable", "SalesOrders", "Create")]
    public async Task<ActionResult<APIResponse<SalesOrderListDto>>> QuickCreate(
        [FromBody] SalesOrderQuickCreateDto input,
        CancellationToken cancellationToken = default)
    {
        var account = input.CustomerAccount.Trim();
        var customer = await _dbContext.Set<CustTable>()
            .AsNoTracking()
            .FirstOrDefaultAsync(candidate => candidate.AccountNum == account, cancellationToken);
        if (customer == null)
            return UnprocessableEntity(APIResponse<SalesOrderListDto>.Fail("Customer account was not found."));

        var party = customer.Party > 0
            ? await _dbContext.Set<DirPartyTable>()
                .AsNoTracking()
                .FirstOrDefaultAsync(candidate => candidate.RecId == customer.Party, cancellationToken)
            : null;
        var salesId = await NextAvailableSalesIdAsync(cancellationToken);
        var today = DateTime.UtcNow.Date;
        var order = new SalesTable
        {
            SalesId = salesId,
            SalesName = string.IsNullOrWhiteSpace(input.SalesName) ? party?.Name ?? account : input.SalesName.Trim(),
            SalesNameAlias = party?.NameAlias ?? string.Empty,
            SalesStatus = SalesStatus.Backorder,
            DocumentStatus = DocumentStatus.None,
            SalesType = SalesType.Sales,
            CustAccount = account,
            InvoiceAccount = string.IsNullOrWhiteSpace(input.InvoiceAccount)
                ? string.IsNullOrWhiteSpace(customer.InvoiceAccount) ? account : customer.InvoiceAccount
                : input.InvoiceAccount.Trim(),
            CustGroup = customer.CustGroupId,
            CurrencyCode = string.IsNullOrWhiteSpace(input.CurrencyCode) ? customer.CurrencyCode : input.CurrencyCode.Trim(),
            TaxGroupId = customer.TaxGroupId,
            PaymTerm = input.PaymentTerms?.Trim() ?? customer.PaymTermId,
            PaymMode = input.PaymentMethod?.Trim() ?? customer.PaymModeId,
            DlvMode = string.IsNullOrWhiteSpace(input.DeliveryMode) ? customer.DlvModeId : input.DeliveryMode.Trim(),
            DlvTerm = input.DeliveryTerms?.Trim() ?? string.Empty,
            InventSiteId = string.IsNullOrWhiteSpace(input.InventSiteId) ? customer.InventSiteId : input.InventSiteId.Trim(),
            InventLocationId = string.IsNullOrWhiteSpace(input.InventLocationId) ? customer.InventLocationId : input.InventLocationId.Trim(),
            SalesGroup = input.SalesGroup?.Trim() ?? string.Empty,
            CustRequisitionNum = input.CustomerRequisitionNumber?.Trim() ?? string.Empty,
            IntercompanyOrder = input.Intercompany,
            IntercompanyCompanyId = input.IntercompanyCompanyId?.Trim() ?? string.Empty,
            OneTimeCustomer = input.OneTimeCustomer ? NoYes.Yes : NoYes.No,
            DeliveryName = input.DeliveryName?.Trim() ?? party?.Name ?? account,
            DeliveryPostalAddress = input.DeliveryPostalAddress ?? 0,
            CustomerRef = input.CustomerReference?.Trim() ?? string.Empty,
            Email = input.Contact?.Trim() ?? string.Empty,
            OrderDate = today,
            DeliveryDate = input.RequestedReceiptDate?.Date ?? today,
            ReceiptDateRequested = input.RequestedReceiptDate?.Date ?? today,
            ShippingDateRequested = input.RequestedShipDate?.Date ?? today,
            ReceiptDateConfirmed = input.ConfirmDates ? input.RequestedReceiptDate?.Date ?? today : default,
            ShippingDateConfirmed = input.ConfirmDates ? input.RequestedShipDate?.Date ?? today : default,
            DeliveryDateControlType = Enum.IsDefined(typeof(SalesDlvDateControlType), input.DeliveryDateControlType)
                ? (SalesDlvDateControlType)input.DeliveryDateControlType
                : default,
            DataAreaId = _company.GetDataAreaId() ?? "dat"
        };

        _dbContext.Set<SalesTable>().Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(APIResponse<SalesOrderListDto>.Ok(new SalesOrderListDto
        {
            RecId = order.RecId,
            SalesId = order.SalesId,
            CustomerAccount = order.CustAccount,
            CustomerName = party?.Name ?? order.SalesName,
            InvoiceAccount = order.InvoiceAccount,
            CustomerGroup = order.CustGroup,
            CurrencyCode = order.CurrencyCode,
            SalesStatus = order.SalesStatus.ToString(),
            DocumentStatus = order.DocumentStatus.ToString(),
            DeliveryDate = order.DeliveryDate,
            ShippingDateRequested = order.ShippingDateRequested,
            OrderTotal = order.SmmSalesAmountTotal,
            CustomerReference = order.CustomerRef,
            DeliveryMode = order.DlvMode,
            DeliveryTerms = order.DlvTerm,
            OrderDate = order.OrderDate,
            InventSiteId = order.InventSiteId,
            InventLocationId = order.InventLocationId,
            SalesNameAlias = order.SalesNameAlias,
            SalesType = (int)(order.SalesType ?? SalesType.Sales),
            OneTimeCustomer = order.OneTimeCustomer == NoYes.Yes,
            Email = order.Email,
            Phone = order.Phone,
            Deadline = order.Deadline == default ? null : order.Deadline,
            CustomerRequisitionNumber = order.CustRequisitionNum,
            CampaignId = order.SmmCampaignId,
            TaxGroupId = order.TaxGroupId,
            PricesIncludeSalesTax = order.InclTax,
            SalesGroup = order.SalesGroup,
            LanguageId = order.LanguageId,
            PaymentTerms = order.PaymTerm
        }, "Created successfully"));
    }

    private async Task<string> NextAvailableSalesIdAsync(CancellationToken cancellationToken)
    {
        const int maximumCollisionAttempts = 10_000;
        for (var attempt = 0; attempt < maximumCollisionAttempts; attempt++)
        {
            var candidate = (await _numberSequences.NextAsync(
                "SalesTable", cancellationToken: cancellationToken)).Code;
            var exists = await _dbContext.Set<SalesTable>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .AnyAsync(order => order.SalesId == candidate, cancellationToken);
            if (!exists) return candidate;
        }

        throw new InvalidOperationException(
            "The sales order number sequence could not produce an unused SalesId. " +
            "Review the SalesTable number sequence NextRec value.");
    }
}
