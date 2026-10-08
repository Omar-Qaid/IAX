using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using IAX.IXApi.Infrastructure.Persistence;
using System.Linq;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Identity.Permissions;

namespace IAX.IXApi.Modules.Finance.Foundation.LogisticsAddresses
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("Organization", "PostalAddresses")]
    public class LogisticsPostalAddressController : ControllerBase
    {
        private readonly IAddressBookQueryService _queries;
        private readonly IPartyLocationService _partyLocationService;
        private readonly IGlobalAddressBookService _globalAddressBookService;

        public LogisticsPostalAddressController(
            IAddressBookQueryService queries,
            IPartyLocationService partyLocationService,
            IGlobalAddressBookService globalAddressBookService)
        {
            _queries = queries;
            _partyLocationService = partyLocationService;
            _globalAddressBookService = globalAddressBookService;
        }

        [HttpGet("CountryRegions")]
        public async Task<IActionResult> GetCountryRegions()
            => Ok(APIResponse<object>.Ok(await _queries.GetCountryRegionsAsync()));

        [HttpGet("States/{countryRegionId}")]
        public async Task<IActionResult> GetStates(string countryRegionId)
            => Ok(APIResponse<object>.Ok(await _queries.GetStatesAsync(countryRegionId)));

        [HttpGet("Cities/{stateId}")]
        public async Task<IActionResult> GetCities(string stateId)
            => Ok(APIResponse<object>.Ok(await _queries.GetCitiesAsync(stateId)));

        [HttpGet("Cities/{countryRegionId}/{stateId}")]
        public async Task<IActionResult> GetCities(string countryRegionId, string stateId)
            => Ok(APIResponse<object>.Ok(await _queries.GetCitiesAsync(countryRegionId, stateId)));

        [HttpGet("Counties/{stateId}")]
        public async Task<IActionResult> GetCounties(string stateId)
            => Ok(APIResponse<object>.Ok(await _queries.GetCountiesAsync(stateId)));

        [HttpGet("Counties/{countryRegionId}/{stateId}")]
        public async Task<IActionResult> GetCounties(string countryRegionId, string stateId)
            => Ok(APIResponse<object>.Ok(await _queries.GetCountiesAsync(countryRegionId, stateId)));

        [HttpGet("Party/{partyId}")]
        public async Task<IActionResult> GetPartyAddresses(long partyId)
            => Ok(APIResponse<System.Collections.Generic.IEnumerable<AddressInfoDto>>.Ok(await _queries.GetPartyAddressesAsync(partyId)));

        [HttpPost("Party/{partyId}")]
        public async Task<IActionResult> CreatePartyAddress(long partyId, [FromBody] AddressInfoDto dto, CancellationToken cancellationToken)
        {
            var result = await _globalAddressBookService.CreatePartyAddressAsync(partyId, dto, cancellationToken);
            return Ok(APIResponse<AddressInfoDto>.Ok(result, "Created successfully"));
        }

        [HttpPut("Party/{partyId}")]
        public async Task<IActionResult> UpdatePartyAddress(long partyId, [FromBody] AddressInfoDto dto, CancellationToken cancellationToken)
        {
            var result = await _globalAddressBookService.UpdatePartyAddressAsync(partyId, dto, cancellationToken);
            return Ok(APIResponse<AddressInfoDto>.Ok(result, "Updated successfully"));
        }

        [HttpDelete("Party/{partyId}/{locationId}")]
        public async Task<IActionResult> DeletePartyAddress(long partyId, long locationId, CancellationToken cancellationToken)
        {
            var success = await _globalAddressBookService.DeletePartyAddressAsync(partyId, locationId, cancellationToken);
            if (!success) return NotFound();

            return Ok(APIResponse<string>.Ok(null, "Deleted successfully"));
        }

        [HttpPost("Party/{partyId}/{locationId}/SetPrimary")]
        public async Task<IActionResult> SetPrimaryAddress(long partyId, long locationId)
        {
            await _partyLocationService.UpdatePartyLocationPrimaryAsync(partyId, locationId, true, true);
            return Ok(APIResponse<string>.Ok(null, "Primary address updated"));
        }
    }
}
