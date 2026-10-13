using Mapster;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public sealed class SalesOrderReadService : ISalesOrderReadService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICompanyExecutionContext _company;

    public SalesOrderReadService(IUnitOfWork unitOfWork, ICompanyExecutionContext company)
    {
        _unitOfWork = unitOfWork;
        _company = company;
    }

    public async Task<object?> LinesAsync(long recId, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Context.Set<SalesTable>().AsNoTracking().FirstOrDefaultAsync(row => row.RecId == recId, cancellationToken);
        if (order == null) return null;
        var lines = await _unitOfWork.Context.Set<SalesLine>().AsNoTracking()
            .Where(line => line.SalesId == order.SalesId && line.DataAreaId == order.DataAreaId)
            .OrderBy(line => line.LineNum).ToListAsync(cancellationToken);
        var dimensionIds = lines.Select(line => line.InventDimId).Where(id => id != string.Empty).Distinct().ToList();
        var dimensions = await _unitOfWork.Context.Set<InventDim>().AsNoTracking()
            .Where(item => item.DataAreaId == order.DataAreaId && dimensionIds.Contains(item.InventDimId))
            .ToDictionaryAsync(item => item.InventDimId, cancellationToken);
        var ledgerDimensionIds = lines.Select(line => line.LedgerDimension).Where(id => id != 0).Distinct().ToList();
        var ledgerDimensions = await _unitOfWork.Context.Set<DimensionAttributeValueCombination>().AsNoTracking()
            .Where(row => row.DataAreaId == order.DataAreaId && ledgerDimensionIds.Contains(row.RecId))
            .ToDictionaryAsync(row => row.RecId, row => row.DisplayValue, cancellationToken);
        var itemIds = lines.Select(line => line.ItemId).Distinct().ToList();
        var productNames = await _unitOfWork.Context.Set<InventTable>().AsNoTracking()
            .Where(item => item.DataAreaId == order.DataAreaId && itemIds.Contains(item.ItemId))
            .ToDictionaryAsync(item => item.ItemId, item => item.NameAlias, cancellationToken);
        return (lines.Select(line =>
        {
            dimensions.TryGetValue(line.InventDimId, out var dimension);
            ledgerDimensions.TryGetValue(line.LedgerDimension, out var ledgerDisplay);
            productNames.TryGetValue(line.ItemId, out var productName);
            return SalesOrderLinePresenter.Record(line, dimension?.InventSiteId, dimension?.InventLocationId,
                ledgerDisplay, dimension, productName);
        }));
    }

    public async Task<object?> DeliveryAddressesAsync(long recId, CancellationToken cancellationToken = default)
    {
        var area = _company.GetDataAreaId();
        var order = await _unitOfWork.Context.Set<SalesTable>().AsNoTracking()
            .FirstOrDefaultAsync(row => row.RecId == recId && row.DataAreaId == area, cancellationToken);
        if (order == null) return null;
        var partyId = await _unitOfWork.Context.Set<CustTable>().AsNoTracking()
            .Where(customer => customer.DataAreaId == area && customer.AccountNum == order.CustAccount)
            .Select(customer => customer.Party).FirstOrDefaultAsync(cancellationToken);
        var addresses = await (
            from partyLocation in _unitOfWork.Context.Set<DirPartyLocation>().AsNoTracking()
            join address in _unitOfWork.Context.Set<LogisticsPostalAddress>().AsNoTracking()
                on partyLocation.Location equals address.Location
            join location in _unitOfWork.Context.Set<LogisticsLocation>().AsNoTracking()
                on partyLocation.Location equals location.RecId
            where partyId > 0 && partyLocation.Party == partyId && partyLocation.IsPostalAddress == NoYes.Yes
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
        var currentIds = await _unitOfWork.Context.Set<SalesLine>().AsNoTracking()
            .Where(line => line.SalesId == order.SalesId && line.DataAreaId == area && line.DeliveryPostalAddress > 0)
            .Select(line => line.DeliveryPostalAddress).Distinct().ToListAsync(cancellationToken);
        if (order.DeliveryPostalAddress > 0) currentIds.Add(order.DeliveryPostalAddress);
        var existingValues = addresses.Select(row => row.value).ToHashSet(StringComparer.Ordinal);
        var orderAddresses = await (
            from address in _unitOfWork.Context.Set<LogisticsPostalAddress>().AsNoTracking()
            join location in _unitOfWork.Context.Set<LogisticsLocation>().AsNoTracking() on address.Location equals location.RecId
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
        return (addresses);
    }

    public async Task<object?> TotalsAsync(long recId, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Context.Set<SalesTable>().AsNoTracking()
            .FirstOrDefaultAsync(row => row.RecId == recId, cancellationToken);
        if (order == null) return null;

        var lines = await _unitOfWork.Context.Set<SalesLine>().AsNoTracking()
            .Where(line => line.SalesId == order.SalesId && line.DataAreaId == order.DataAreaId)
            .ToListAsync(cancellationToken);
        var lineRecIds = lines.Select(line => line.RecId).ToList();
        var charges = await _unitOfWork.Context.Set<MarkupTrans>().AsNoTracking()
            .Where(charge => charge.DataAreaId == order.DataAreaId
                && charge.IsDeleted != NoYes.Yes
                && (charge.ModuleType == MarkupModuleType.Customer || charge.ModuleType == MarkupModuleType.Sales)
                && ((charge.TransRecId == order.RecId
                        && (charge.TransTableId == FinanceTableNumbers.SalesTable || charge.TransTableId == FinanceTableNumbers.Unspecified))
                    || (lineRecIds.Contains(charge.TransRecId)
                        && charge.TransTableId == FinanceTableNumbers.SalesLine)))
            .ToListAsync(cancellationToken);
        var taxGroups = lines.Select(line => line.TaxGroup)
            .Concat(charges.Select(charge => charge.TaxGroup))
            .Where(value => value != string.Empty).Distinct().ToList();
        var itemTaxGroups = lines.Select(line => line.TaxItemGroup)
            .Concat(charges.Select(charge => charge.TaxItemGroup))
            .Where(value => value != string.Empty).Distinct().ToList();
        var groupRows = await _unitOfWork.Context.Set<TaxGroupData>().AsNoTracking()
            .Where(row => row.DataAreaId == order.DataAreaId && taxGroups.Contains(row.TaxGroup))
            .ToListAsync(cancellationToken);
        var itemRows = await _unitOfWork.Context.Set<TaxOnItem>().AsNoTracking()
            .Where(row => row.DataAreaId == order.DataAreaId && itemTaxGroups.Contains(row.TaxItemGroup))
            .ToListAsync(cancellationToken);
        var taxCodes = groupRows.Select(row => row.TaxCode).Intersect(itemRows.Select(row => row.TaxCode)).Distinct().ToList();
        var rates = await _unitOfWork.Context.Set<TaxData>().AsNoTracking()
            .Where(row => row.DataAreaId == order.DataAreaId && taxCodes.Contains(row.TaxCode))
            .ToListAsync(cancellationToken);

        decimal grossAmount = 0;
        decimal lineDiscount = 0;
        decimal multiLineDiscount = 0;
        decimal salesTax = 0;
        decimal totalCharges = 0;
        var salesModules = await _unitOfWork.Context.Set<InventTableModule>().AsNoTracking()
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
            var chargeAmount = charge.MarkupCategory == MarkupCategory.Fixed && charge.CalculatedAmount == 0m
                ? charge.Value : charge.CalculatedAmount;
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
        return (new
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
        });
    }
    public async Task<IEnumerable<SalesOrderListDto>> GetListAsync(
        CancellationToken cancellationToken = default)
    {
        var orders = await _unitOfWork.Context.Set<SalesTable>()
            .AsNoTracking()
            .OrderByDescending(order => order.RecId)
            .ToListAsync(cancellationToken);

        var customerAccounts = orders.Select(order => order.CustAccount).Where(account => account != string.Empty).Distinct().ToList();
        var customers = await _unitOfWork.Context.Set<CustTable>()
            .AsNoTracking()
            .Where(customer => customerAccounts.Contains(customer.AccountNum))
            .Select(customer => new { customer.AccountNum, customer.Party })
            .ToListAsync(cancellationToken);
        var partyIds = customers.Select(customer => customer.Party).Where(id => id > 0).Distinct().ToList();
        var parties = await _unitOfWork.Context.Set<DirPartyTable>()
            .AsNoTracking()
            .Where(party => partyIds.Contains(party.RecId))
            .ToDictionaryAsync(party => party.RecId, party => party.Name, cancellationToken);
        var customerNames = customers.ToDictionary(
            customer => customer.AccountNum,
            customer => parties.GetValueOrDefault(customer.Party) ?? customer.AccountNum);
        var postalAddressIds = orders.Select(order => order.DeliveryPostalAddress).Where(id => id > 0).Distinct().ToList();
        var postalAddresses = await _unitOfWork.Context.Set<LogisticsPostalAddress>().AsNoTracking()
            .Where(address => postalAddressIds.Contains(address.RecId))
            .ToDictionaryAsync(address => address.RecId, address => address.Address, cancellationToken);

        var result = orders.Select(order => new SalesOrderListMappingSource(
            order,
            customerNames.GetValueOrDefault(order.CustAccount) ?? order.SalesName,
            postalAddresses.GetValueOrDefault(order.DeliveryPostalAddress) ?? string.Empty)
            .Adapt<SalesOrderListDto>()).ToList();

        return (result);
    }
}
