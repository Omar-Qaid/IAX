using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Persistence;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public sealed class SalesOrderValidationService
{
    private readonly IFinanceDataContext _dbContext;
    private readonly ICompanyExecutionContext _company;

    public SalesOrderValidationService(IFinanceDataContext dbContext, ICompanyExecutionContext company)
    {
        _dbContext = dbContext;
        _company = company;
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

    public async Task<bool> IsDeliveryAddressAvailableForOrderAsync(SalesTable order, long postalAddressId,
        CancellationToken cancellationToken)
    {
        if (await IsCustomerPostalAddressAsync(order.CustAccount, order.DataAreaId, postalAddressId, cancellationToken))
            return true;
        if (order.DeliveryPostalAddress == postalAddressId) return true;
        return await _dbContext.Set<SalesLine>().AsNoTracking().AnyAsync(line =>
            line.SalesId == order.SalesId && line.DataAreaId == order.DataAreaId
            && line.DeliveryPostalAddress == postalAddressId, cancellationToken);
    }

    public async Task<string?> ValidateSiteWarehouseAsync(string? siteId, string? warehouseId,
        CancellationToken cancellationToken)
    {
        siteId = siteId?.Trim() ?? string.Empty;
        warehouseId = warehouseId?.Trim() ?? string.Empty;
        var dataAreaId = _company.GetDataAreaId();
        if (!string.IsNullOrEmpty(siteId)
            && !await _dbContext.Set<InventSite>().AnyAsync(site => site.DataAreaId == dataAreaId
                && site.SiteId == siteId, cancellationToken))
            return "Site was not found in the selected company.";
        if (!string.IsNullOrEmpty(warehouseId)
            && (string.IsNullOrEmpty(siteId)
                || !await _dbContext.Set<InventLocation>().AnyAsync(location =>
                    location.DataAreaId == dataAreaId && location.InventLocationId == warehouseId
                    && location.InventSiteId == siteId, cancellationToken)))
            return "Warehouse was not found in the selected site.";
        return null;
    }

    public async Task<string?> ValidateTaxSetupAsync(
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

}
