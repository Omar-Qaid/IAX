using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using System.Linq;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Identity.Permissions;

namespace IAX.IXApi.Modules.ERP.Foundation.LogisticsAddresses
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("Organization", "ElectronicAddresses")]
    public class LogisticsElectronicAddressController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILocationService _locationService;
        private readonly IElectronicAddressService _electronicAddressService;
        private readonly IPartyLocationService _partyLocationService;
        private readonly IGlobalAddressBookService _globalAddressBookService;

        public LogisticsElectronicAddressController(
            IUnitOfWork unitOfWork, 
            ILocationService locationService,
            IElectronicAddressService electronicAddressService,
            IPartyLocationService partyLocationService,
            IGlobalAddressBookService globalAddressBookService)
        {
            _unitOfWork = unitOfWork;
            _locationService = locationService;
            _electronicAddressService = electronicAddressService;
            _partyLocationService = partyLocationService;
            _globalAddressBookService = globalAddressBookService;
        }

        [HttpGet("Party/{partyId}")]
        public async Task<IActionResult> GetPartyContacts(long partyId)
        {
            var partyLocations = await _partyLocationService.GetPartyLocationsAsync(partyId);
            var electronicOnly = partyLocations.Where(x => x.IsPostalAddress == IAX.IXApi.Modules.ERP.Common.NoYes.No).ToList();
            var locationIds = electronicOnly.Select(x => x.Location).ToList();
            var electronicAddresses = await _electronicAddressService.GetContactsByLocationsAsync(locationIds);

            var dtos = electronicAddresses.Select(e => {
                var pLoc = electronicOnly.FirstOrDefault(l => l.Location == e.Location);
                return new ContactInfoDto
                {
                    Id = e.RecId.ToString(),
                    Location = e.Location,
                    Description = e.Description,
                    Type = e.Type.ToString(),
                    Number = e.Locator,
                    Extension = e.LocatorExtension,
                    Primary = e.IsPrimary == IAX.IXApi.Modules.ERP.Common.NoYes.Yes || (pLoc != null && pLoc.IsPrimary == IAX.IXApi.Modules.ERP.Common.NoYes.Yes)
                };
            }).ToList();
            return Ok(APIResponse<System.Collections.Generic.IEnumerable<ContactInfoDto>>.Ok(dtos));
        }

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
