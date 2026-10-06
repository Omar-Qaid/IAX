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
    private readonly ILocationService _locationService;
    private readonly IPostalAddressService _postalAddressService;

    public SalesTableController(
        IFinanceDataContext dbContext,
        ISysNumberSequenceService numberSequences,
        ICompanyExecutionContext company,
        ISalesInventoryDemandService inventoryDemand,
        ILocationService locationService,
        IPostalAddressService postalAddressService)
    {
        _dbContext = dbContext;
        _numberSequences = numberSequences;
        _company = company;
        _inventoryDemand = inventoryDemand;
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
            return LineRecord(line, dimension?.InventSiteId, dimension?.InventLocationId,
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
        return await _dbContext.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable, cancellationToken);
            var area = _company.GetDataAreaId();
            var order = await _dbContext.Set<SalesTable>().FirstOrDefaultAsync(
                row => row.RecId == recId && row.DataAreaId == area, cancellationToken);
            if (order == null) return (IActionResult)NotFound(APIResponse<object>.Fail("Sales order was not found."));
            if (order.SalesStatus != SalesStatus.Backorder)
                return UnprocessableEntity(APIResponse<object>.Fail("Only open sales orders can be changed."));
            SalesLine? line = null;
            if (!string.IsNullOrWhiteSpace(input.LineId))
            {
                if (!long.TryParse(input.LineId, System.Globalization.NumberStyles.None,
                        System.Globalization.CultureInfo.InvariantCulture, out var lineId))
                    return BadRequest(APIResponse<object>.Fail("Sales line was not found."));
                line = await _dbContext.Set<SalesLine>().FirstOrDefaultAsync(row => row.RecId == lineId
                    && row.SalesId == order.SalesId && row.DataAreaId == order.DataAreaId, cancellationToken);
                if (line == null) return NotFound(APIResponse<object>.Fail("Sales line was not found."));
                if (line.SalesStatus != SalesStatus.Backorder || line.RemainSalesPhysical != line.SalesQty
                    || line.RemainSalesFinancial != line.SalesQty)
                    return UnprocessableEntity(APIResponse<object>.Fail("Processed sales lines cannot be changed."));
            }

            var location = await _locationService.CreateLocationAsync(input.Address.Description.Trim(), true, cancellationToken);
            var postalAddress = await _postalAddressService.CreatePostalAddressAsync(location.RecId, input.Address, cancellationToken);
            postalAddress.DataAreaId = order.DataAreaId;
            if (line == null) order.DeliveryPostalAddress = postalAddress.RecId;
            else line.DeliveryPostalAddress = postalAddress.RecId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return (IActionResult)Ok(APIResponse<object>.Ok(new
            {
                id = postalAddress.RecId.ToString(System.Globalization.CultureInfo.InvariantCulture),
                description = location.Description,
                address = postalAddress.Address
            }));
        });
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
        [System.ComponentModel.DataAnnotations.EnumDataType(typeof(InventRefType))]
        public InventRefType? ItemReferenceType { get; set; }
        [System.ComponentModel.DataAnnotations.EnumDataType(typeof(SalesLineSourcingOrigin))]
        public SalesLineSourcingOrigin? SourcingOrigin { get; set; }
        public bool? ExcludeFromMasterPlanning { get; set; }
        [System.ComponentModel.DataAnnotations.EnumDataType(typeof(SalesDeliveryType))]
        public SalesDeliveryType? LineDeliveryType { get; set; }
        [System.ComponentModel.DataAnnotations.EnumDataType(typeof(SalesDlvDateControlType))]
        public SalesDlvDateControlType? DeliveryDateControlType { get; set; }
        [System.ComponentModel.DataAnnotations.EnumDataType(typeof(ReqFullCTPStatus))]
        public ReqFullCTPStatus? MpsFullRunCtpStatus { get; set; }
        [System.ComponentModel.DataAnnotations.EnumDataType(typeof(WHSShipCarrierDlvType))]
        public WHSShipCarrierDlvType? ShipCarrierDlvType { get; set; }
        [System.ComponentModel.DataAnnotations.Range(typeof(decimal), "0", "1000000000")]
        public decimal? PlanningPriority { get; set; }
        [System.ComponentModel.DataAnnotations.EnumDataType(typeof(SalesIntercompanyOrigin))]
        public SalesIntercompanyOrigin? IntercompanyOrigin { get; set; }
        public bool? Stopped { get; set; }
        public bool? PreventPartialDelivery { get; set; }
        [System.ComponentModel.DataAnnotations.Range(typeof(long), "0", "9223372036854775807")]
        public long SalesCategory { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(10)]
        public string? InventSiteId { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(10)]
        public string? InventLocationId { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.InventBatchId)]
        public string? BatchNumber { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.InventSerialId)]
        public string? SerialNumber { get; set; }
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
        public string? DeliveryPostalAddress { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.InventTransId)]
        public string? ReturnLotId { get; set; }
        [System.ComponentModel.DataAnnotations.EnumDataType(typeof(SalesAutoReservation))]
        public SalesAutoReservation? Reservation { get; set; }
        public bool? AutoBatchReservation { get; set; }
        public bool? SameBatchSelection { get; set; }
        public bool? Scrap { get; set; }
        public long? LedgerDimension { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.SalesGroupId)]
        public string? SalesGroup { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.ReferenceId)]
        public string? CustomerReference { get; set; }
        [System.ComponentModel.DataAnnotations.Range(0, int.MaxValue)]
        public int? CustomerLineNumber { get; set; }
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
        [System.ComponentModel.DataAnnotations.Range(typeof(decimal), "0", "1000000000")]
        public decimal SalesMarkup { get; set; }
        public bool? ExcludeFromRebate { get; set; }
        public bool? ExcludeFromRebateManagement { get; set; }
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
            long deliveryAddressId = order.DeliveryPostalAddress;
            if (!string.IsNullOrWhiteSpace(input.DeliveryPostalAddress))
            {
                if (!long.TryParse(input.DeliveryPostalAddress, out deliveryAddressId)
                    || !await IsDeliveryAddressAvailableForOrderAsync(order, deliveryAddressId, cancellationToken))
                    return UnprocessableEntity(APIResponse<object>.Fail("The selected delivery address does not belong to this customer."));
            }
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
            if (!string.IsNullOrWhiteSpace(input.ReturnLotId))
            {
                var returnLotExists = await _dbContext.Set<InventTransOrigin>().AnyAsync(origin =>
                    origin.DataAreaId == order.DataAreaId && origin.ItemId == item.ItemId
                    && origin.InventTransId == input.ReturnLotId.Trim(), cancellationToken);
                if (!returnLotExists)
                    return UnprocessableEntity(APIResponse<object>.Fail("The selected return lot does not belong to this item."));
            }
            if (input.LedgerDimension.HasValue && input.LedgerDimension.Value != 0
                && !await _dbContext.Set<DimensionAttributeValueCombination>().AnyAsync(dimension =>
                    dimension.DataAreaId == order.DataAreaId
                    && dimension.RecId == input.LedgerDimension.Value, cancellationToken))
                return UnprocessableEntity(APIResponse<object>.Fail("The selected main account was not found."));
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
            var inventSiteId = string.IsNullOrWhiteSpace(input.InventSiteId) ? order.InventSiteId : input.InventSiteId;
            var inventLocationId = string.IsNullOrWhiteSpace(input.InventLocationId) ? order.InventLocationId : input.InventLocationId;
            await _inventoryDemand.CreateAsync(order, line,
                inventSiteId, inventLocationId, cancellationToken);
            _dbContext.Set<SalesLine>().Add(line);
            order.SmmSalesAmountTotal += line.LineAmount;
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Ok(APIResponse<object>.Ok(LineRecord(line, inventSiteId, inventLocationId,
                productName: item.NameAlias)));
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
                if (string.IsNullOrWhiteSpace(input.DeliveryPostalAddress))
                    line.DeliveryPostalAddress = 0;
                else if (!long.TryParse(input.DeliveryPostalAddress, out var deliveryAddressId)
                    || !await IsDeliveryAddressAvailableForOrderAsync(order, deliveryAddressId, cancellationToken))
                    return UnprocessableEntity(APIResponse<object>.Fail("The selected delivery address does not belong to this customer."));
                else
                    line.DeliveryPostalAddress = deliveryAddressId;
                if (!string.IsNullOrWhiteSpace(input.ReturnLotId))
                {
                    var returnLotId = input.ReturnLotId.Trim();
                    var returnLotExists = await _dbContext.Set<InventTransOrigin>().AnyAsync(origin =>
                        origin.DataAreaId == line.DataAreaId && origin.ItemId == line.ItemId
                        && origin.InventTransId == returnLotId, cancellationToken);
                    if (!returnLotExists)
                        return UnprocessableEntity(APIResponse<object>.Fail("The selected return lot does not belong to this item."));
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
                        return UnprocessableEntity(APIResponse<object>.Fail("The selected main account was not found."));
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
                line.PriceUnit = input.PriceUnit;
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
                var batchNumber = input.BatchNumber?.Trim();
                if (!string.IsNullOrEmpty(batchNumber)
                    && !await _dbContext.Set<InventBatch>().AnyAsync(batch => batch.DataAreaId == line.DataAreaId
                        && batch.ItemId == line.ItemId && batch.InventBatchId == batchNumber, cancellationToken))
                    return UnprocessableEntity(APIResponse<object>.Fail("The selected batch does not belong to this item."));
                var serialNumber = input.SerialNumber?.Trim();
                if (!string.IsNullOrEmpty(serialNumber)
                    && !await _dbContext.Set<InventSerial>().AnyAsync(serial => serial.DataAreaId == line.DataAreaId
                        && serial.ItemId == line.ItemId && serial.InventSerialId == serialNumber, cancellationToken))
                    return UnprocessableEntity(APIResponse<object>.Fail("The selected serial number does not belong to this item."));
                await _inventoryDemand.UpdateAsync(line, inventSiteId, inventLocationId,
                    cancellationToken, batchNumber, serialNumber);
                responseSiteId = inventSiteId;
                responseLocationId = inventLocationId;
            }
            order.SmmSalesAmountTotal += (input == null ? 0 : line.LineAmount) - previousAmount;
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Ok(APIResponse<object>.Ok(input == null
                ? new { deleted = true }
                : LineRecord(line, responseSiteId, responseLocationId,
                    inventoryDimension: await _dbContext.Set<InventDim>().AsNoTracking()
                        .FirstOrDefaultAsync(item => item.DataAreaId == line.DataAreaId
                            && item.InventDimId == line.InventDimId, cancellationToken),
                    productName: await _dbContext.Set<InventTable>().AsNoTracking()
                        .Where(item => item.DataAreaId == line.DataAreaId && item.ItemId == line.ItemId)
                        .Select(item => item.NameAlias).FirstOrDefaultAsync(cancellationToken))));
        });
    }

    private static object LineRecord(SalesLine line, string? inventSiteId = null,
        string? inventLocationId = null, string? ledgerDimensionDisplay = null,
        InventDim? inventoryDimension = null, string? productName = null) => new {
        id = line.RecId.ToString(), lineNumber = line.LineNum, itemNumber = line.ItemId,
        productName = productName ?? line.Name,
        lineType = (int)line.SalesType, deliveryType = (int)line.DeliveryType,
        lineDeliveryType = (int)line.LineDeliveryType,
        sourcingOrigin = (int)line.SourcingOrigin,
        excludeFromMasterPlanning = line.MpsExcludeSalesLine != 0,
        deliveryDateControlType = (int)line.DeliveryDateControlType,
        mpsFullRunCtpStatus = (int)line.MpsFullRunCtpStatus,
        shipCarrierDlvType = (int)line.ShipCarrierDlvType,
        planningPriority = line.PlanningPriority,
        salesCategory = line.SalesCategory,
        customerLineNumber = line.CustomerLineNum, intercompanyOrigin = (int)line.IntercompanyOrigin,
        stopped = line.Blocked != SalesLineBlocked.No, preventPartialDelivery = line.Complete != 0,
        description = line.Name, quantity = line.SalesQty, unit = line.SalesUnit,
        unitPrice = line.SalesPrice, lineTotal = line.LineAmount, deliveryDate = line.ReceiptDateRequested,
        line.InventTransId, line.InventDimId, line.CurrencyCode, salesStatus = line.SalesStatus.ToString(),
        line.PriceUnit, line.CostPrice, line.SalesMarkup,
        excludeFromRebate = line.PdsExcludeFromRebate != 0,
        excludeFromRebateManagement = line.TamRebateExcludeRebateManagement != 0,
        lineDiscount = line.LineDisc, lineDiscountPercent = line.LinePercent,
        multiLineDiscount = line.MultiLnDisc, multiLineDiscountPercent = line.MultiLnPercent,
        overDeliveryPercent = line.OverDeliveryPct, underDeliveryPercent = line.UnderDeliveryPct,
        line.RemainSalesPhysical, line.RemainSalesFinancial, line.SalesDeliverNow, line.InventDeliverNow,
        line.PackingUnit, packingUnitQuantity = line.PackingUnitQty, deliveryMode = line.DlvMode,
        deliveryTerms = line.DlvTerm,
        shippingDateRequested = line.ShippingDateRequested == default ? (DateTime?)null : line.ShippingDateRequested,
        shippingDateConfirmed = line.ShippingDateConfirmed == default ? (DateTime?)null : line.ShippingDateConfirmed,
        receiptDateConfirmed = line.ReceiptDateConfirmed == default ? (DateTime?)null : line.ReceiptDateConfirmed,
        customerReference = line.CustomerRef, line.DeliveryName,
        deliveryPostalAddress = line.DeliveryPostalAddress.ToString(System.Globalization.CultureInfo.InvariantCulture), line.TaxGroup, line.TaxItemGroup, line.LedgerDimension,
        batchNumber = inventoryDimension?.InventBatchId ?? string.Empty,
        serialNumber = inventoryDimension?.InventSerialId ?? string.Empty,
        location = inventoryDimension?.WmsLocationId ?? string.Empty,
        inventoryStatus = inventoryDimension?.InventStatusId ?? string.Empty,
        licensePlate = inventoryDimension?.LicensePlateId ?? string.Empty,
        itemReferenceNumber = line.InventRefId,
        itemReferenceType = (int)line.InventRefType,
        itemReferenceLot = line.InventRefTransId,
        line.DefaultDimension, financialTag = line.FinTag, line.IntrastatCommodity,
        returnLotId = line.InventTransIdReturn, reservation = (int)line.Reservation,
        autoBatchReservation = line.PdsBatchAttribAutoRes != 0,
        sameBatchSelection = line.PdsSameLot != 0, scrap = line.Scrap != 0,
        line.SalesGroup, line.CreatedAt, ledgerDimensionDisplay,
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

    public sealed class CreateSalesDeliveryAddressInput
    {
        public string? LineId { get; set; }
        public AddressInfoDto Address { get; set; } = new();
    }

    private async Task<bool> IsCustomerPostalAddressAsync(string customerAccount, string dataAreaId,
        long postalAddressId, CancellationToken cancellationToken)
    {
        var partyId = await _dbContext.Set<CustTable>().AsNoTracking()
            .Where(customer => customer.DataAreaId == dataAreaId && customer.AccountNum == customerAccount)
            .Select(customer => customer.Party).FirstOrDefaultAsync(cancellationToken);
        if (partyId == 0) return false;
        var addressLocation = await _dbContext.Set<LogisticsPostalAddress>().AsNoTracking()
            .Where(address => address.RecId == postalAddressId && address.DataAreaId == dataAreaId)
            .Select(address => address.Location).FirstOrDefaultAsync(cancellationToken);
        if (addressLocation == 0) return false;
        return await _dbContext.Set<DirPartyLocation>().AsNoTracking().AnyAsync(link =>
            link.Party == partyId && link.Location == addressLocation
            && link.IsPostalAddress == NoYes.Yes, cancellationToken);
    }

    private async Task<bool> IsDeliveryAddressAvailableForOrderAsync(SalesTable order, long postalAddressId,
        CancellationToken cancellationToken)
    {
        if (await IsCustomerPostalAddressAsync(order.CustAccount, order.DataAreaId, postalAddressId, cancellationToken))
            return true;
        if (order.DeliveryPostalAddress == postalAddressId) return true;
        return await _dbContext.Set<SalesLine>().AsNoTracking().AnyAsync(line =>
            line.SalesId == order.SalesId && line.DataAreaId == order.DataAreaId
            && line.DeliveryPostalAddress == postalAddressId, cancellationToken);
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
        public DateTime? ShippingDateRequested { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.Name)]
        public string DeliveryName { get; set; } = string.Empty;
        public string DeliveryPostalAddress { get; set; } = string.Empty;
        public DateTime? ShippingDateConfirmed { get; set; }
        public DateTime? ReceiptDateConfirmed { get; set; }
        [System.ComponentModel.DataAnnotations.EnumDataType(typeof(SalesDlvDateControlType))]
        public SalesDlvDateControlType DeliveryDateControlType { get; set; }
        [System.ComponentModel.DataAnnotations.EnumDataType(typeof(ReqFullCTPStatus))]
        public ReqFullCTPStatus MpsFullRunCtpStatus { get; set; }
        public bool BlindShipment { get; set; }
        public bool ResidentialDestination { get; set; }
        public bool ExcludeFromMasterPlanning { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.ReasonCodeId)]
        public string DeliveryReason { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.Code)]
        public string ExportReason { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.Name)]
        public string ShippingCarrier { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.Code)]
        public string CarrierId { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.MarkupGroup)]
        public string CarrierGroup { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.Code)]
        public string BrokerId { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.Code)]
        public string TransportMode { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.EnumDataType(typeof(WHSShipCarrierDlvType))]
        public WHSShipCarrierDlvType CarrierService { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.PaymModeId)]
        public string PaymentMethod { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.PaymentSched)]
        public string PaymentSchedule { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.PaymSpec)]
        public string PaymentSpecification { get; set; } = string.Empty;
        public DateTime? FixedDueDate { get; set; }
        public DateTime? PaymentTermsBaseDate { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.CashDisc)]
        public string CashDiscountCode { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Range(typeof(decimal), "0", "100")]
        public decimal DiscountPercent { get; set; }
        [System.ComponentModel.DataAnnotations.Range(typeof(decimal), "0", "100")]
        public decimal TotalDiscountPercent { get; set; }
        [System.ComponentModel.DataAnnotations.Range(typeof(decimal), "0", "1000000000")]
        public decimal FixedExchangeRate { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.PriceGroupId)]
        public string PriceGroup { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.LineDisc)]
        public string LineDiscountGroup { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.Code)]
        public string MultiLineDiscountGroup { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.EndDisc)]
        public string TotalDiscountGroup { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.MarkupGroup)]
        public string ChargesGroup { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.GroupId)]
        public string CustomerRebateGroup { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.GroupId)]
        public string CustomerTmaGroup { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.Num)]
        public string RebateReference { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.SalesPoolId)]
        public string SalesPool { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.Code)]
        public string CarrierCustomerAccount { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.Code)]
        public string FreightZone { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.StringLength(FieldLengths.Memo)]
        public string Notes { get; set; } = string.Empty;
        public bool IntercompanyAutoCreateOrders { get; set; }
        public bool IntercompanyDirectDelivery { get; set; }
        [System.ComponentModel.DataAnnotations.EnumDataType(typeof(SalesIntercompanyOrigin))]
        public SalesIntercompanyOrigin IntercompanyOrigin { get; set; }
        public bool IntercompanyAllowIndirectCreation { get; set; }
        [System.ComponentModel.DataAnnotations.EnumDataType(typeof(SalesAutoReservation))]
        public SalesAutoReservation Reservation { get; set; }
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
            if (!string.IsNullOrEmpty(taxGroupId)
                && !await _dbContext.Set<TaxGroupHeading>().AsNoTracking()
                    .AnyAsync(group => group.TaxGroup == taxGroupId, cancellationToken))
                return UnprocessableEntity(APIResponse<object>.Fail("Sales tax group was not found."));
            order.TaxGroupId = taxGroupId;
            order.InclTax = input.PricesIncludeSalesTax;
            order.SalesGroup = input.SalesGroup.Trim();
            order.LanguageId = input.LanguageId.Trim();
            order.DeliveryDate = input.DeliveryDate!.Value.Date;
            order.ReceiptDateRequested = order.DeliveryDate;
            order.ShippingDateRequested = input.ShippingDateRequested?.Date ?? default;
            var deliveryName = input.DeliveryName.Trim();
            var deliveryAddressId = 0L;
            if (!string.IsNullOrWhiteSpace(input.DeliveryPostalAddress)
                && input.DeliveryPostalAddress != "0"
                && (!long.TryParse(input.DeliveryPostalAddress, out deliveryAddressId)
                    || !await IsDeliveryAddressAvailableForOrderAsync(order, deliveryAddressId, cancellationToken)))
                return UnprocessableEntity(APIResponse<object>.Fail("The selected delivery address does not belong to this customer."));
            order.DeliveryName = deliveryName;
            order.DeliveryPostalAddress = deliveryAddressId;
            order.ShippingDateConfirmed = input.ShippingDateConfirmed?.Date ?? default;
            order.ReceiptDateConfirmed = input.ReceiptDateConfirmed?.Date ?? default;
            order.DeliveryDateControlType = input.DeliveryDateControlType;
            order.MpsFullRunCtpStatus = input.MpsFullRunCtpStatus;
            order.ShipCarrierBlindShipment = input.BlindShipment ? NoYes.Yes : NoYes.No;
            order.ShipCarrierResidential = input.ResidentialDestination;
            order.MpsExcludeSalesOrder = input.ExcludeFromMasterPlanning;
            order.DlvReason = input.DeliveryReason.Trim();
            order.ExportReason = input.ExportReason.Trim();
            order.ShipCarrierName = input.ShippingCarrier.Trim();
            order.ShipCarrierId = input.CarrierId.Trim();
            order.MarkupGroup = input.CarrierGroup.Trim();
            order.ShipCarrierAccountCode = input.BrokerId.Trim();
            order.Transport = input.TransportMode.Trim();
            order.ShipCarrierDlvType = input.CarrierService;
            order.PaymMode = input.PaymentMethod.Trim();
            order.PaymentSched = input.PaymentSchedule.Trim();
            order.PaymSpec = input.PaymentSpecification.Trim();
            order.FixedDueDate = input.FixedDueDate?.Date ?? default;
            order.CashDiscBaseDate = input.PaymentTermsBaseDate?.Date ?? default;
            order.CashDisc = input.CashDiscountCode.Trim();
            order.CashDiscPercent = input.DiscountPercent;
            order.DiscPercent = input.TotalDiscountPercent;
            order.FixedExchRate = input.FixedExchangeRate;
            order.PriceGroupId = input.PriceGroup.Trim();
            order.LineDisc = input.LineDiscountGroup.Trim();
            order.MultiLineDisc = input.MultiLineDiscountGroup.Trim();
            order.EndDisc = input.TotalDiscountGroup.Trim();
            order.MarkupGroup = input.ChargesGroup.Trim();
            order.PdsCustRebateGroupId = input.CustomerRebateGroup.Trim();
            order.PdsRebateProgramTmaGroup = input.CustomerTmaGroup.Trim();
            order.TamRebateReference = input.RebateReference.Trim();
            order.SalesPoolId = input.SalesPool.Trim();
            order.ShipCarrierAccount = input.CarrierCustomerAccount.Trim();
            order.FreightZone = input.FreightZone.Trim();
            order.Notes = input.Notes.Trim();
            order.IntercompanyAutoCreateOrders = input.IntercompanyAutoCreateOrders;
            order.IntercompanyDirectDelivery = input.IntercompanyDirectDelivery;
            order.IntercompanyOrigin = input.IntercompanyOrigin;
            order.IntercompanyAllowIndirectCreation = input.IntercompanyAllowIndirectCreation;
            order.Reservation = input.Reservation;
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
        var deliveryPostalAddress = input.DeliveryPostalAddress ?? 0;
        if (!string.IsNullOrWhiteSpace(input.DeliveryPostalAddressId)
            && (!long.TryParse(input.DeliveryPostalAddressId, out deliveryPostalAddress) || deliveryPostalAddress <= 0))
            return UnprocessableEntity(APIResponse<SalesOrderListDto>.Fail("The selected delivery address is invalid."));
        var taxGroupId = string.IsNullOrWhiteSpace(input.TaxGroupId) ? customer.TaxGroupId : input.TaxGroupId.Trim();
        if (!string.IsNullOrWhiteSpace(taxGroupId)
            && !await _dbContext.Set<TaxGroupHeading>().AsNoTracking()
                .AnyAsync(group => group.TaxGroup == taxGroupId, cancellationToken))
            return UnprocessableEntity(APIResponse<SalesOrderListDto>.Fail("The selected sales tax group was not found."));
        if (customer.Party > 0)
        {
            var customerAddresses = await (
                from link in _dbContext.Set<DirPartyLocation>().AsNoTracking()
                join address in _dbContext.Set<LogisticsPostalAddress>().AsNoTracking()
                    on link.Location equals address.Location
                where link.Party == customer.Party && link.IsPostalAddress == NoYes.Yes
                    && address.DataAreaId == customer.DataAreaId
                    && address.ValidFrom <= DateTime.UtcNow && address.ValidTo >= DateTime.UtcNow
                orderby link.IsPrimary descending, address.RecId
                select address.RecId
            ).ToListAsync(cancellationToken);
            if (deliveryPostalAddress != 0 && !customerAddresses.Contains(deliveryPostalAddress))
                return UnprocessableEntity(APIResponse<SalesOrderListDto>.Fail("The selected delivery address does not belong to this customer."));
            if (deliveryPostalAddress == 0)
                deliveryPostalAddress = customerAddresses.FirstOrDefault();
        }
        var customerContacts = customer.Party > 0
            ? await (
                from link in _dbContext.Set<DirPartyLocation>().AsNoTracking()
                join contact in _dbContext.Set<LogisticsElectronicAddress>().AsNoTracking()
                    on link.Location equals contact.Location
                where link.Party == customer.Party && link.IsPostalAddress == NoYes.No
                    && (contact.Type == ElectronicAddressType.Email || contact.Type == ElectronicAddressType.Phone)
                orderby link.IsPrimary descending, contact.IsPrimary descending, contact.RecId
                select new { contact.Type, contact.Locator }
            ).ToListAsync(cancellationToken)
            : [];
        var email = customerContacts.FirstOrDefault(contact => contact.Type == ElectronicAddressType.Email)?.Locator ?? string.Empty;
        var phone = customerContacts.FirstOrDefault(contact => contact.Type == ElectronicAddressType.Phone)?.Locator ?? string.Empty;
        var chosenContact = input.Contact?.Trim();
        if (!string.IsNullOrEmpty(chosenContact))
        {
            if (string.Equals(input.ContactType, "Phone", StringComparison.OrdinalIgnoreCase)) phone = chosenContact;
            else email = chosenContact;
        }
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
            TaxGroupId = taxGroupId,
            PaymTerm = string.IsNullOrWhiteSpace(input.PaymentTerms) ? customer.PaymTermId : input.PaymentTerms.Trim(),
            PaymMode = string.IsNullOrWhiteSpace(input.PaymentMethod) ? customer.PaymModeId : input.PaymentMethod.Trim(),
            DlvMode = string.IsNullOrWhiteSpace(input.DeliveryMode) ? customer.DlvModeId : input.DeliveryMode.Trim(),
            DlvTerm = input.DeliveryTerms?.Trim() ?? string.Empty,
            InventSiteId = string.IsNullOrWhiteSpace(input.InventSiteId) ? customer.InventSiteId : input.InventSiteId.Trim(),
            InventLocationId = string.IsNullOrWhiteSpace(input.InventLocationId) ? customer.InventLocationId : input.InventLocationId.Trim(),
            SalesGroup = input.SalesGroup?.Trim() ?? string.Empty,
            SalesPoolId = customer.SalesPoolId,
            CustRequisitionNum = input.CustomerRequisitionNumber?.Trim() ?? string.Empty,
            IntercompanyOrder = input.Intercompany,
            IntercompanyCompanyId = input.IntercompanyCompanyId?.Trim() ?? string.Empty,
            OneTimeCustomer = input.OneTimeCustomer ? NoYes.Yes : NoYes.No,
            DeliveryName = input.DeliveryName?.Trim() ?? party?.Name ?? account,
            DeliveryPostalAddress = deliveryPostalAddress,
            CustomerRef = input.CustomerReference?.Trim() ?? string.Empty,
            Email = email,
            Phone = phone,
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
            Notes = order.Notes,
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
