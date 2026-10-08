using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Mapster;

namespace IAX.IXApi.Modules.Finance.Foundation.LogisticsAddresses;

public sealed class AddressBookQueryService(IUnitOfWork unitOfWork, IPartyLocationService partyLocations,
    IPostalAddressService postalAddresses, IElectronicAddressService electronicAddresses) : IAddressBookQueryService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IPartyLocationService _partyLocationService = partyLocations;
    private readonly IPostalAddressService _postalAddressService = postalAddresses;
    private readonly IElectronicAddressService _electronicAddressService = electronicAddresses;
    public async Task<List<ContactInfoDto>> GetPartyContactsAsync(long partyId)
    {
            var partyLocations = await _partyLocationService.GetPartyLocationsAsync(partyId);
            var electronicOnly = partyLocations.Where(x => x.IsPostalAddress == IAX.IXApi.Modules.Finance.Common.NoYes.No).ToList();
            var locationIds = electronicOnly.Select(x => x.Location).ToList();
            var electronicAddresses = await _electronicAddressService.GetContactsByLocationsAsync(locationIds);

            var dtos = electronicAddresses.Select(e => {
                var pLoc = electronicOnly.FirstOrDefault(l => l.Location == e.Location);
                return new ElectronicAddressReadSource(e, pLoc).Adapt<ContactInfoDto>();
            }).ToList();
            return dtos;
    }

    public async Task<object> GetCountryRegionsAsync()
    {
            var data = await _unitOfWork.Context.Set<LogisticsAddressCountryRegion>()
                .AsNoTracking()
                .OrderBy(x => x.CountryRegionId)
                .Select(x => new { x.CountryRegionId, x.IsoCode })
                .ToListAsync();
            return data;
    }

    public async Task<object> GetStatesAsync(string countryRegionId)
    {
            var data = await _unitOfWork.Context.Set<LogisticsAddressState>()
                .AsNoTracking()
                .Where(x => x.CountryRegionId == countryRegionId)
                .OrderBy(x => x.Name)
                .Select(x => new { x.StateId, x.Name })
                .ToListAsync();
            return data;
    }

    public async Task<object> GetCitiesAsync(string stateId)
    {
            var data = await _unitOfWork.Context.Set<LogisticsAddressCity>()
                .AsNoTracking()
                .Where(x => x.StateId == stateId)
                .Select(x => new { x.CityKey, x.Name })
                .ToListAsync();
            return data;
    }

    public async Task<object> GetCitiesAsync(string countryRegionId, string stateId)
    {
            var data = await _unitOfWork.Context.Set<LogisticsAddressCity>()
                .AsNoTracking()
                .Where(x => x.CountryRegionId == countryRegionId && x.StateId == stateId)
                .OrderBy(x => x.Name)
                .Select(x => new { x.CityKey, x.Name, x.CountryRegionId, x.StateId })
                .ToListAsync();
            return data;
    }

    public async Task<object> GetCountiesAsync(string stateId)
    {
            var data = await _unitOfWork.Context.Set<LogisticsAddressCounty>()
                .AsNoTracking()
                .Where(x => x.StateId == stateId)
                .Select(x => new { x.CountyId, x.Name })
                .ToListAsync();
            return data;
    }

    public async Task<object> GetCountiesAsync(string countryRegionId, string stateId)
    {
            var data = await _unitOfWork.Context.Set<LogisticsAddressCounty>()
                .AsNoTracking()
                .Where(x => x.CountryRegionId == countryRegionId && x.StateId == stateId)
                .OrderBy(x => x.Name)
                .Select(x => new { x.CountyId, x.Name, x.CountryRegionId, x.StateId })
                .ToListAsync();
            return data;
    }

    public async Task<List<AddressInfoDto>> GetPartyAddressesAsync(long partyId)
    {
            var partyLocations = await _partyLocationService.GetPartyLocationsAsync(partyId);
            var postalOnly = partyLocations.Where(x => x.IsPostalAddress == IAX.IXApi.Modules.Finance.Common.NoYes.Yes).ToList();
            var locationIds = postalOnly.Select(x => x.Location).ToList();
            var postalAddresses = await _postalAddressService.GetAddressesByLocationsAsync(locationIds);

            var dtos = postalAddresses.Select(p => {
                var pLoc = postalOnly.FirstOrDefault(l => l.Location == p.Location);
                return new PostalAddressReadSource(p, pLoc).Adapt<AddressInfoDto>();
            }).ToList();
            return dtos;
    }
}
