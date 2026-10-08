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
    private readonly SalesOrderQuickCreateService _quickCreate;
    private readonly ICompanyExecutionContext _company;
    private readonly ISalesInventoryDemandService _inventoryDemand;
    private readonly SalesOrderCancellationService _cancellation;
    private readonly SalesOrderDeliveryAddressService _deliveryAddresses;
    private readonly SalesOrderValidationService _validation;
    private readonly SalesOrderHeaderService _header;
    private readonly SalesOrderDiscountService _discounts;
    private readonly SalesOrderLineService _lineWrites;
    private readonly ILocationService _locationService;
    private readonly IPostalAddressService _postalAddressService;

    public SalesTableController(
        IFinanceDataContext dbContext,
        SalesOrderQuickCreateService quickCreate,
        ICompanyExecutionContext company,
        ISalesInventoryDemandService inventoryDemand,
        SalesOrderCancellationService cancellation,
        SalesOrderDeliveryAddressService deliveryAddresses,
        SalesOrderValidationService validation,
        SalesOrderHeaderService header,
        SalesOrderDiscountService discounts,
        SalesOrderLineService lineWrites,
        ILocationService locationService,
        IPostalAddressService postalAddressService)
    {
        _dbContext = dbContext;
        _quickCreate = quickCreate;
        _company = company;
        _inventoryDemand = inventoryDemand;
        _cancellation = cancellation;
        _deliveryAddresses = deliveryAddresses;
        _validation = validation;
        _header = header;
        _discounts = discounts;
        _lineWrites = lineWrites;
        _locationService = locationService;
        _postalAddressService = postalAddressService;
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
            .Where(module => itemIds.Contains(module.ItemId) && module.ModuleType == ModuleInventPurchSales.Sales).ToListAsync(cancellationToken);
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
        var dataAreaId = _company.GetDataAreaId();
        var sites = await _dbContext.Set<InventSite>().AsNoTracking()
            .Where(site => site.DataAreaId == dataAreaId)
            .OrderBy(site => site.SiteId)
            .Select(site => new { id = site.SiteId, code = site.SiteId, name = site.Name })
            .ToListAsync(cancellationToken);
        var warehouses = await _dbContext.Set<InventLocation>().AsNoTracking()
            .Where(location => location.DataAreaId == dataAreaId)
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

    [HttpGet("ledger-dimensions")]
    public async Task<IActionResult> LedgerDimensions(string? search = null, int pageNumber = 1,
        int pageSize = 50, CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var dataAreaId = _company.GetDataAreaId();
        var query = _dbContext.Set<DimensionAttributeValueCombination>().AsNoTracking()
            .Where(row => row.DataAreaId == dataAreaId && row.MainAccountValue != string.Empty);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(row => row.MainAccountValue.Contains(term) || row.DisplayValue.Contains(term));
        }
        var totalRecords = await query.CountAsync(cancellationToken);
        var dimensions = await query
            .OrderBy(row => row.MainAccountValue)
            .ThenBy(row => row.RecId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(row => new { id = row.RecId, code = row.MainAccountValue, name = row.DisplayValue })
            .ToListAsync(cancellationToken);
        return Ok(APIResponse<object>.Ok(new
        {
            data = dimensions,
            pageNumber,
            totalRecords,
            totalPages = (int)Math.Ceiling((double)totalRecords / pageSize)
        }));
    }

    [HttpGet("return-lots")]
    public async Task<IActionResult> ReturnLots(string itemNumber, string? search = null,
        int pageNumber = 1, int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(itemNumber))
            return BadRequest(APIResponse<object>.Fail("An item number is required to find return lots."));

        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var dataAreaId = _company.GetDataAreaId();
        var query = _dbContext.Set<InventTransOrigin>().AsNoTracking()
            .Where(origin => origin.DataAreaId == dataAreaId && origin.ItemId == itemNumber
                && _dbContext.Set<InventTrans>().Any(transaction =>
                    transaction.DataAreaId == dataAreaId && transaction.InventTransOrigin == origin.RecId));
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(origin => origin.InventTransId.Contains(term)
                || origin.ReferenceId.Contains(term));
        }

        var totalRecords = await query.CountAsync(cancellationToken);
        var lots = await query.OrderByDescending(origin => origin.RecId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(origin => new { id = origin.InventTransId, code = origin.InventTransId,
                name = origin.ReferenceId })
            .ToListAsync(cancellationToken);
        return Ok(APIResponse<object>.Ok(new
        {
            data = lots,
            pageNumber,
            totalRecords,
            totalPages = (int)Math.Ceiling((double)totalRecords / pageSize)
        }));
    }

    [HttpGet("batch-numbers")]
    public async Task<IActionResult> BatchNumbers(string itemNumber, string? search = null,
        int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(itemNumber))
            return BadRequest(APIResponse<object>.Fail("An item number is required to find batches."));
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var dataAreaId = _company.GetDataAreaId();
        var query = _dbContext.Set<InventBatch>().AsNoTracking()
            .Where(batch => batch.DataAreaId == dataAreaId && batch.ItemId == itemNumber);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(batch => batch.InventBatchId.Contains(term));
        }
        var totalRecords = await query.CountAsync(cancellationToken);
        var rows = await query.OrderBy(batch => batch.InventBatchId)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .Select(batch => new { id = batch.InventBatchId, code = batch.InventBatchId, name = batch.InventBatchId })
            .ToListAsync(cancellationToken);
        return Ok(APIResponse<object>.Ok(new { data = rows, pageNumber, totalRecords,
            totalPages = (int)Math.Ceiling((double)totalRecords / pageSize) }));
    }

    [HttpGet("serial-numbers")]
    public async Task<IActionResult> SerialNumbers(string itemNumber, string? search = null,
        int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(itemNumber))
            return BadRequest(APIResponse<object>.Fail("An item number is required to find serial numbers."));
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var dataAreaId = _company.GetDataAreaId();
        var query = _dbContext.Set<InventSerial>().AsNoTracking()
            .Where(serial => serial.DataAreaId == dataAreaId && serial.ItemId == itemNumber);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(serial => serial.InventSerialId.Contains(term));
        }
        var totalRecords = await query.CountAsync(cancellationToken);
        var rows = await query.OrderBy(serial => serial.InventSerialId)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .Select(serial => new { id = serial.InventSerialId, code = serial.InventSerialId, name = serial.InventSerialId })
            .ToListAsync(cancellationToken);
        return Ok(APIResponse<object>.Ok(new { data = rows, pageNumber, totalRecords,
            totalPages = (int)Math.Ceiling((double)totalRecords / pageSize) }));
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
        var ledgerDimensionIds = lines.Select(line => line.LedgerDimension).Where(id => id != 0).Distinct().ToList();
        var ledgerDimensions = await _dbContext.Set<DimensionAttributeValueCombination>().AsNoTracking()
            .Where(row => row.DataAreaId == order.DataAreaId && ledgerDimensionIds.Contains(row.RecId))
            .ToDictionaryAsync(row => row.RecId, row => row.DisplayValue, cancellationToken);
        var itemIds = lines.Select(line => line.ItemId).Distinct().ToList();
        var productNames = await _dbContext.Set<InventTable>().AsNoTracking()
            .Where(item => item.DataAreaId == order.DataAreaId && itemIds.Contains(item.ItemId))
            .ToDictionaryAsync(item => item.ItemId, item => item.NameAlias, cancellationToken);
        return Ok(APIResponse<object>.Ok(lines.Select(line =>
        {
            dimensions.TryGetValue(line.InventDimId, out var dimension);
            ledgerDimensions.TryGetValue(line.LedgerDimension, out var ledgerDisplay);
            productNames.TryGetValue(line.ItemId, out var productName);
            return SalesOrderLinePresenter.Record(line, dimension?.InventSiteId, dimension?.InventLocationId,
                ledgerDisplay, dimension, productName);
        })));
    }

    [HttpGet("{recId:long}/delivery-addresses")]
    public async Task<IActionResult> DeliveryAddresses(long recId, CancellationToken cancellationToken = default)
    {
        var area = _company.GetDataAreaId();
        var order = await _dbContext.Set<SalesTable>().AsNoTracking()
            .FirstOrDefaultAsync(row => row.RecId == recId && row.DataAreaId == area, cancellationToken);
        if (order == null) return NotFound(APIResponse<object>.Fail("Sales order was not found."));
        var partyId = await _dbContext.Set<CustTable>().AsNoTracking()
            .Where(customer => customer.DataAreaId == area && customer.AccountNum == order.CustAccount)
            .Select(customer => customer.Party).FirstOrDefaultAsync(cancellationToken);
        var addresses = await (
            from partyLocation in _dbContext.Set<DirPartyLocation>().AsNoTracking()
            join address in _dbContext.Set<LogisticsPostalAddress>().AsNoTracking()
                on partyLocation.Location equals address.Location
            join location in _dbContext.Set<LogisticsLocation>().AsNoTracking()
                on partyLocation.Location equals location.RecId
            where partyLocation.Party == partyId && partyLocation.IsPostalAddress == NoYes.Yes
                && address.DataAreaId == area && address.ValidFrom <= DateTime.UtcNow
                && address.ValidTo >= DateTime.UtcNow
            orderby partyLocation.IsPrimary descending, location.Description
            select new { value = address.RecId.ToString(System.Globalization.CultureInfo.InvariantCulture), label = location.Description,
                address = address.Address, isPrimary = partyLocation.IsPrimary == NoYes.Yes,
                postalAddress = new { locationId = location.LocationId, description = location.Description,
                    street = address.Street, building = address.StreetNumber, buildingComplement = address.BuildingCompliment,
                    postBox = address.PostBox, city = address.City, state = address.State, zipCode = address.ZipCode,
                    county = address.County, countryRegionId = address.CountryRegionId, district = address.DistrictName,
                    validFrom = address.ValidFrom, validTo = address.ValidTo, primary = partyLocation.IsPrimary == NoYes.Yes } }
        ).ToListAsync(cancellationToken);
        var currentIds = await _dbContext.Set<SalesLine>().AsNoTracking()
            .Where(line => line.SalesId == order.SalesId && line.DataAreaId == area && line.DeliveryPostalAddress > 0)
            .Select(line => line.DeliveryPostalAddress).Distinct().ToListAsync(cancellationToken);
        if (order.DeliveryPostalAddress > 0) currentIds.Add(order.DeliveryPostalAddress);
        var existingValues = addresses.Select(row => row.value).ToHashSet(StringComparer.Ordinal);
        var orderAddresses = await (
            from address in _dbContext.Set<LogisticsPostalAddress>().AsNoTracking()
            join location in _dbContext.Set<LogisticsLocation>().AsNoTracking() on address.Location equals location.RecId
            where currentIds.Contains(address.RecId) && address.DataAreaId == area
            select new { value = address.RecId.ToString(System.Globalization.CultureInfo.InvariantCulture),
                label = location.Description, address = address.Address, isPrimary = false,
                postalAddress = new { locationId = location.LocationId, description = location.Description,
                    street = address.Street, building = address.StreetNumber, buildingComplement = address.BuildingCompliment,
                    postBox = address.PostBox, city = address.City, state = address.State, zipCode = address.ZipCode,
                    county = address.County, countryRegionId = address.CountryRegionId, district = address.DistrictName,
                    validFrom = address.ValidFrom, validTo = address.ValidTo, primary = false } }
        ).ToListAsync(cancellationToken);
        addresses.AddRange(orderAddresses.Where(row => existingValues.Add(row.value)));
        return Ok(APIResponse<object>.Ok(addresses));
    }

    [HttpPost("{recId:long}/delivery-addresses")]
    [DomainPermission("AccountsReceivable", "SalesOrders", "Edit")]
    public async Task<IActionResult> CreateDeliveryAddress(long recId, [FromBody] CreateSalesDeliveryAddressInput input,
        CancellationToken cancellationToken = default)
    {
        var result = await _deliveryAddresses.CreateAsync(recId, input.LineId, input.Address, cancellationToken);
        return result.Status switch
        {
            SalesDeliveryAddressStatus.OrderNotFound => NotFound(APIResponse<object>.Fail("Sales order was not found.")),
            SalesDeliveryAddressStatus.NotOpen => UnprocessableEntity(APIResponse<object>.Fail("Only open sales orders can be changed.")),
            SalesDeliveryAddressStatus.BadLineId => BadRequest(APIResponse<object>.Fail("Sales line was not found.")),
            SalesDeliveryAddressStatus.LineNotFound => NotFound(APIResponse<object>.Fail("Sales line was not found.")),
            SalesDeliveryAddressStatus.ProcessedLine => UnprocessableEntity(APIResponse<object>.Fail("Processed sales lines cannot be changed.")),
            _ => Ok(APIResponse<object>.Ok(new
            {
                id = result.Id.ToString(System.Globalization.CultureInfo.InvariantCulture),
                description = result.Description,
                address = result.Address
            }))
        };
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
        var salesModules = await _dbContext.Set<InventTableModule>().AsNoTracking()
            .Where(module => module.DataAreaId == order.DataAreaId
                && module.ModuleType == ModuleInventPurchSales.Sales
                && lines.Select(line => line.ItemId).Contains(module.ItemId))
            .ToListAsync(cancellationToken);
        var totalDiscountEligibleLineIds = lines
            .Where(line => salesModules.Any(module => module.ItemId == line.ItemId && module.EndDisc == NoYes.Yes))
            .Select(line => line.RecId)
            .ToHashSet();
        var eligibleNetAmount = lines
            .Where(line => totalDiscountEligibleLineIds.Contains(line.RecId))
            .Sum(line => Math.Max(0m, line.LineAmount - line.LineDisc - line.MultiLnDisc));
        var orderDiscount = eligibleNetAmount * order.DiscPercent / 100m;
        foreach (var line in lines)
        {
            grossAmount += line.LineAmount;
            lineDiscount += line.LineDisc;
            multiLineDiscount += line.MultiLnDisc;
            var lineNetAmount = Math.Max(0m, line.LineAmount - line.LineDisc - line.MultiLnDisc);
            var allocatedOrderDiscount = totalDiscountEligibleLineIds.Contains(line.RecId) && eligibleNetAmount > 0
                ? orderDiscount * lineNetAmount / eligibleNetAmount
                : 0m;
            var taxableAmount = Math.Max(0m, lineNetAmount - allocatedOrderDiscount);
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

        var totalDiscount = lineDiscount + multiLineDiscount + orderDiscount;
        var subtotal = grossAmount - totalDiscount - (order.InclTax ? salesTax : 0m);
        return Ok(APIResponse<object>.Ok(new
        {
            currencyCode = order.CurrencyCode,
            grossAmount,
            lineDiscount,
            multiLineDiscount,
            orderDiscount,
            totalDiscount,
            subtotal,
            totalCharges,
            salesTax,
            invoiceAmount = subtotal + totalCharges + salesTax,
            quantity = lines.Sum(line => line.SalesQty),
            costValue = lines.Sum(line => line.CostPrice * line.SalesQty)
        }));
    }

    [HttpPost("{recId:long}/recalculate-discounts")]
    public async Task<IActionResult> RecalculateDiscounts(long recId, CancellationToken cancellationToken = default)
    {
        var result = await _discounts.RecalculateAsync(recId, cancellationToken);
        return result.Status switch
        {
            SalesDiscountRecalculationStatus.NotFound => NotFound(APIResponse<object>.Fail("Sales order was not found in the selected company.")),
            SalesDiscountRecalculationStatus.NotOpen => UnprocessableEntity(APIResponse<object>.Fail("Only open sales orders can recalculate discounts.")),
            _ => Ok(APIResponse<object>.Ok(new
            {
                updatedLines = result.UpdatedLines,
                lineDiscount = result.LineDiscount,
                multiLineDiscount = result.MultiLineDiscount,
                totalDiscountPercent = result.TotalDiscountPercent
            }))
        };
    }

    public sealed class AddSalesLineInput : SalesLineInputDto { }

    [HttpPost("{recId:long}/lines")]
    [DomainPermission("AccountsReceivable", "SalesOrders", "Edit")]
    public async Task<IActionResult> AddLine(long recId, [FromBody] AddSalesLineInput input, CancellationToken cancellationToken = default)
        => PresentLineWrite(await _lineWrites.AddAsync(recId, input, cancellationToken));

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
        var status = await _cancellation.CancelAsync(recId, cancellationToken);
        return status switch
        {
            SalesOrderCancelStatus.NotFound => NotFound(APIResponse<object>.Fail("Sales order was not found.")),
            SalesOrderCancelStatus.NotOpen => UnprocessableEntity(APIResponse<object>.Fail("Only an open sales order can be cancelled.")),
            SalesOrderCancelStatus.Processed => UnprocessableEntity(APIResponse<object>.Fail(
                "This cancellation currently supports only completely open, unprocessed sales orders.")),
            _ => Ok(APIResponse<object>.Ok(new { cancelled = true }))
        };
    }

    private async Task<IActionResult> ChangeLine(long recId, long lineId, AddSalesLineInput? input, CancellationToken cancellationToken)
        => PresentLineWrite(await _lineWrites.ChangeAsync(recId, lineId, input, cancellationToken));

    private IActionResult PresentLineWrite(SalesLineWriteResult result) => result.Status switch
    {
        SalesLineWriteStatus.NotFound => NotFound(APIResponse<object>.Fail(result.Error!)),
        SalesLineWriteStatus.Invalid => UnprocessableEntity(APIResponse<object>.Fail(result.Error!)),
        _ => Ok(APIResponse<object>.Ok(result.Data!))
    };

    public sealed class CreateSalesDeliveryAddressInput
    {
        public string? LineId { get; set; }
        public AddressInfoDto Address { get; set; } = new();
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
        var postalAddressIds = orders.Select(order => order.DeliveryPostalAddress).Where(id => id > 0).Distinct().ToList();
        var postalAddresses = await _dbContext.Set<LogisticsPostalAddress>().AsNoTracking()
            .Where(address => postalAddressIds.Contains(address.RecId))
            .ToDictionaryAsync(address => address.RecId, address => address.Address, cancellationToken);

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
            PaymentTerms = order.PaymTerm,
            DeliveryName = order.DeliveryName,
            DeliveryPostalAddress = order.DeliveryPostalAddress == 0
                ? string.Empty
                : order.DeliveryPostalAddress.ToString(System.Globalization.CultureInfo.InvariantCulture),
            DeliveryAddress = postalAddresses.GetValueOrDefault(order.DeliveryPostalAddress) ?? string.Empty,
            ShippingDateConfirmed = order.ShippingDateConfirmed == default ? null : order.ShippingDateConfirmed,
            ReceiptDateConfirmed = order.ReceiptDateConfirmed == default ? null : order.ReceiptDateConfirmed,
            DeliveryDateControlType = (int)order.DeliveryDateControlType,
            MpsFullRunCtpStatus = (int)order.MpsFullRunCtpStatus,
            BlindShipment = order.ShipCarrierBlindShipment == NoYes.Yes,
            ResidentialDestination = order.ShipCarrierResidential,
            ExcludeFromMasterPlanning = order.MpsExcludeSalesOrder,
            DeliveryReason = order.DlvReason,
            ExportReason = order.ExportReason,
            ShippingCarrier = order.ShipCarrierName,
            CarrierId = order.ShipCarrierId,
            CarrierGroup = order.MarkupGroup,
            BrokerId = order.ShipCarrierAccountCode,
            TransportMode = order.Transport,
            CarrierService = (int)order.ShipCarrierDlvType,
            PaymentMethod = order.PaymMode,
            PaymentSchedule = order.PaymentSched,
            PaymentSpecification = order.PaymSpec,
            FixedDueDate = order.FixedDueDate == default ? null : order.FixedDueDate,
            PaymentTermsBaseDate = order.CashDiscBaseDate == default ? null : order.CashDiscBaseDate,
            CashDiscountCode = order.CashDisc,
            DiscountPercent = order.CashDiscPercent,
            TotalDiscountPercent = order.DiscPercent,
            FixedExchangeRate = order.FixedExchRate,
            ReportingCurrencyFixedExchangeRate = order.ReportingCurrencyFixedExchRate,
            PriceGroup = order.PriceGroupId,
            LineDiscountGroup = order.LineDisc,
            MultiLineDiscountGroup = order.MultiLineDisc,
            TotalDiscountGroup = order.EndDisc,
            ChargesGroup = order.MarkupGroup,
            CustomerRebateGroup = order.PdsCustRebateGroupId,
            CustomerTmaGroup = order.PdsRebateProgramTmaGroup,
            RebateReference = order.TamRebateReference,
            SalesPool = order.SalesPoolId,
            CarrierCustomerAccount = order.ShipCarrierAccount,
            FreightZone = order.FreightZone,
            Notes = order.Notes,
            IntercompanyAutoCreateOrders = order.IntercompanyAutoCreateOrders,
            IntercompanyDirectDelivery = order.IntercompanyDirectDelivery,
            IntercompanyOrigin = (int)order.IntercompanyOrigin,
            IntercompanyAllowIndirectCreation = order.IntercompanyAllowIndirectCreation,
            ReleaseStatus = order.ReleaseStatus.ToString(),
            Reservation = (int)order.Reservation
        }).ToList();

        return Ok(APIResponse<IEnumerable<SalesOrderListDto>>.Ok(result));
    }

    public sealed class UpdateSalesHeaderInput : UpdateSalesHeaderInputDto { }

    [HttpPut("{recId:long}/header")]
    [DomainPermission("AccountsReceivable", "SalesOrders", "Edit")]
    public async Task<IActionResult> UpdateHeader(long recId, [FromBody] UpdateSalesHeaderInput input, CancellationToken cancellationToken = default)
    {
        var result = await _header.UpdateAsync(recId, input, cancellationToken);
        return result.Status switch
        {
            SalesHeaderUpdateStatus.NotFound => NotFound(APIResponse<object>.Fail(result.Error!)),
            SalesHeaderUpdateStatus.Invalid => UnprocessableEntity(APIResponse<object>.Fail(result.Error!)),
            _ => Ok(APIResponse<object>.Ok(new { saved = true }))
        };
    }

    [HttpPost("quick-create")]
    [DomainPermission("AccountsReceivable", "SalesOrders", "Create")]
    public async Task<ActionResult<APIResponse<SalesOrderListDto>>> QuickCreate(
        [FromBody] SalesOrderQuickCreateDto input, CancellationToken cancellationToken = default)
    {
        var result = await _quickCreate.CreateAsync(input, cancellationToken);
        return result.Error != null
            ? UnprocessableEntity(APIResponse<SalesOrderListDto>.Fail(result.Error))
            : Ok(APIResponse<SalesOrderListDto>.Ok(result.Order!, "Created successfully"));
    }

}
