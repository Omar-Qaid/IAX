namespace IAX.IXApi.Modules.Finance.Foundation.LogisticsAddresses;

public interface IAddressBookQueryService
{
    Task<List<ContactInfoDto>> GetPartyContactsAsync(long partyId);
    Task<object> GetCountryRegionsAsync();
    Task<object> GetStatesAsync(string countryRegionId);
    Task<object> GetCitiesAsync(string stateId);
    Task<object> GetCitiesAsync(string countryRegionId, string stateId);
    Task<object> GetCountiesAsync(string stateId);
    Task<object> GetCountiesAsync(string countryRegionId, string stateId);
    Task<List<AddressInfoDto>> GetPartyAddressesAsync(long partyId);
}
