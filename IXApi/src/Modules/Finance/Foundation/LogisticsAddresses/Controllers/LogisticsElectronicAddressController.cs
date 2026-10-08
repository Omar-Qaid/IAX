using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;
using System.Linq;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Identity.Permissions;

namespace IAX.IXApi.Modules.Finance.Foundation.LogisticsAddresses
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("Organization", "ElectronicAddresses")]
    public class LogisticsElectronicAddressController : ControllerBase
    {
        private readonly IAddressBookQueryService _queries;
        private readonly IPartyLocationService _partyLocationService;
        private readonly IGlobalAddressBookService _globalAddressBookService;

        public LogisticsElectronicAddressController(
            IAddressBookQueryService queries,
            IPartyLocationService partyLocationService,
            IGlobalAddressBookService globalAddressBookService)
        {
            _queries = queries;
            _partyLocationService = partyLocationService;
            _globalAddressBookService = globalAddressBookService;
        }

        [HttpGet("Party/{partyId}")]
        public async Task<IActionResult> GetPartyContacts(long partyId)
            => Ok(APIResponse<System.Collections.Generic.IEnumerable<ContactInfoDto>>.Ok(await _queries.GetPartyContactsAsync(partyId)));

        [HttpPost("Party/{partyId}")]
        public async Task<IActionResult> CreatePartyContact(long partyId, [FromBody] ContactInfoDto dto, CancellationToken cancellationToken)
        {
            var result = await _globalAddressBookService.CreatePartyContactAsync(partyId, dto, cancellationToken);
            return Ok(APIResponse<ContactInfoDto>.Ok(result, "Created successfully"));
        }

        [HttpPut("Party/{partyId}")]
        public async Task<IActionResult> UpdatePartyContact(long partyId, [FromBody] ContactInfoDto dto, CancellationToken cancellationToken)
        {
            var result = await _globalAddressBookService.UpdatePartyContactAsync(partyId, dto, cancellationToken);
            return Ok(APIResponse<ContactInfoDto>.Ok(result, "Updated successfully"));
        }

        [HttpDelete("Party/{partyId}/{locationId}")]
        public async Task<IActionResult> DeletePartyContact(long partyId, long locationId, CancellationToken cancellationToken)
        {
            var success = await _globalAddressBookService.DeletePartyContactAsync(partyId, locationId, cancellationToken);
            if (!success) return NotFound();

            return Ok(APIResponse<string>.Ok(null, "Deleted successfully"));
        }

        [HttpPost("Party/{partyId}/{locationId}/SetPrimary")]
        public async Task<IActionResult> SetPrimaryContact(long partyId, long locationId)
        {
            await _partyLocationService.UpdatePartyLocationPrimaryAsync(partyId, locationId, false, true);
            return Ok(APIResponse<string>.Ok(null, "Primary contact updated"));
        }
    }
}
