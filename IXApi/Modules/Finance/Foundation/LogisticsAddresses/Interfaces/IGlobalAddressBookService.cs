using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IAX.IXApi.Modules.Finance.Foundation.LogisticsAddresses
{
    public interface IGlobalAddressBookService
    {
        Task UpdateGlobalAddressBookAsync(long partyRecId, List<AddressInfoDto> addresses, List<ContactInfoDto> contacts, CancellationToken cancellationToken = default);
        Task<AddressInfoDto> CreatePartyAddressAsync(long partyId, AddressInfoDto dto, CancellationToken cancellationToken = default);
        Task<AddressInfoDto> UpdatePartyAddressAsync(long partyId, AddressInfoDto dto, CancellationToken cancellationToken = default);
        Task<bool> DeletePartyAddressAsync(long partyId, long locationId, CancellationToken cancellationToken = default);
        
        Task<ContactInfoDto> CreatePartyContactAsync(long partyId, ContactInfoDto dto, CancellationToken cancellationToken = default);
        Task<ContactInfoDto> UpdatePartyContactAsync(long partyId, ContactInfoDto dto, CancellationToken cancellationToken = default);
        Task<bool> DeletePartyContactAsync(long partyId, long locationId, CancellationToken cancellationToken = default);
    }
}

