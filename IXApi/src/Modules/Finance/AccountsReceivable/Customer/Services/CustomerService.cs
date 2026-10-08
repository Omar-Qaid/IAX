using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using Microsoft.EntityFrameworkCore;
using Mapster;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Customer;

public sealed class CustomerService : BaseService<CustTable>, ICustomerService
{
    private readonly IUnitOfWork _data;
    private readonly ICurrentUserService _currentUser;

    public CustomerService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        : base(unitOfWork, currentUser)
    {
        _data = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<object> GetCountryRegionsAsync(CancellationToken cancellationToken)
        => await _data.Context.Set<LogisticsAddressCountryRegion>().AsNoTracking()
            .OrderBy(country => country.CountryRegionId)
            .Select(country => new { country.CountryRegionId, country.IsoCode })
            .ToListAsync(cancellationToken);

    public async Task<List<CustomerListDto>> GetCustomerListAsync(CancellationToken cancellationToken)
    {
        var customers = (await GetAllAsync(cancellationToken: cancellationToken)).ToList();
        var partyIds = customers.Select(customer => customer.Party).Where(id => id != 0).Distinct().ToList();
        var parties = await _data.Context.Set<DirPartyTable>().AsNoTracking()
            .Where(party => partyIds.Contains(party.RecId))
            .ToDictionaryAsync(party => party.RecId, cancellationToken);
        return customers.Select(customer => ToListDto(customer, parties.GetValueOrDefault(customer.Party))).ToList();
    }

    public async Task<CustomerSalesOrderDefaultsDto?> GetSalesOrderDefaultsAsync(string accountNumber, CancellationToken cancellationToken)
    {
        var area = _currentUser.GetDataAreaId() ?? "dat";
        var partyId = await _data.Context.Set<CustTable>().AsNoTracking()
            .Where(customer => customer.AccountNum == accountNumber && customer.DataAreaId == area)
            .Select(customer => customer.Party)
            .FirstOrDefaultAsync(cancellationToken);
        if (partyId == 0) return null;

        var now = DateTime.UtcNow;
        var addresses = await (
            from link in _data.Context.Set<DirPartyLocation>().AsNoTracking()
            join address in _data.Context.Set<LogisticsPostalAddress>().AsNoTracking()
                on link.Location equals address.Location
            where link.Party == partyId && link.IsPostalAddress == NoYes.Yes
                && address.DataAreaId == area && address.ValidFrom <= now && address.ValidTo >= now
            orderby link.IsPrimary descending, address.RecId
            select new { address.RecId, address.Address, Primary = link.IsPrimary == NoYes.Yes }
        ).ToListAsync(cancellationToken);
        var contacts = await (
            from link in _data.Context.Set<DirPartyLocation>().AsNoTracking()
            join contact in _data.Context.Set<LogisticsElectronicAddress>().AsNoTracking()
                on link.Location equals contact.Location
            where link.Party == partyId && link.IsPostalAddress == NoYes.No
                && (contact.Type == ElectronicAddressType.Email || contact.Type == ElectronicAddressType.Phone)
            orderby link.IsPrimary descending, contact.IsPrimary descending, contact.RecId
            select new { contact.Type, contact.Locator, Primary = link.IsPrimary == NoYes.Yes || contact.IsPrimary == NoYes.Yes }
        ).ToListAsync(cancellationToken);
        return new CustomerSalesOrderDefaultsDto
        {
            Address = addresses.FirstOrDefault()?.Address ?? string.Empty,
            Addresses = addresses.Select(address => new CustomerSalesOrderAddressDto
            {
                Id = address.RecId.ToString(System.Globalization.CultureInfo.InvariantCulture),
                Address = address.Address, Primary = address.Primary
            }).ToList(),
            Contacts = contacts.Select(contact => new CustomerSalesOrderContactDto
            {
                Type = contact.Type.ToString(), Number = contact.Locator, Primary = contact.Primary
            }).ToList()
        };
    }

    public CustomerListDto ToListDto(CustTable customer, DirPartyTable? party)
        => new CustomerMappingSource(customer, party).Adapt<CustomerListDto>();
}
