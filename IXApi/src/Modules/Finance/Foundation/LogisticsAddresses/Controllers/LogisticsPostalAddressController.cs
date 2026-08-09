using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using IAX.IXApi.Infrastructure.Persistence;
using System.Linq;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILocationService _locationService;
        private readonly IPostalAddressService _postalAddressService;
        private readonly IPartyLocationService _partyLocationService;
        private readonly IGlobalAddressBookService _globalAddressBookService;

        public LogisticsPostalAddressController(
            IUnitOfWork unitOfWork, 
            ILocationService locationService,
            IPostalAddressService postalAddressService,
            IPartyLocationService partyLocationService,
            IGlobalAddressBookService globalAddressBookService)
        {
            _unitOfWork = unitOfWork;
            _locationService = locationService;
            _postalAddressService = postalAddressService;
            _partyLocationService = partyLocationService;
            _globalAddressBookService = globalAddressBookService;
        }

        [HttpGet("CountryRegions")]
        public async Task<IActionResult> GetCountryRegions()
        {
            var data = await _unitOfWork.Context.Set<LogisticsAddressCountryRegion>().AsNoTracking().Select(x => new { x.CountryRegionId, x.IsoCode }).ToListAsync();
            return Ok(APIResponse<object>.Ok(data));
        }

        [HttpGet("States/{countryRegionId}")]
        public async Task<IActionResult> GetStates(string countryRegionId)
        {
            var data = await _unitOfWork.Context.Set<LogisticsAddressState>()
                .AsNoTracking()
                .Where(x => x.CountryRegionId == countryRegionId)
                .Select(x => new { x.StateId, x.Name })
                .ToListAsync();
            return Ok(APIResponse<object>.Ok(data));
        }

        [HttpGet("Cities/{stateId}")]
        public async Task<IActionResult> GetCities(string stateId)
        {
            var data = await _unitOfWork.Context.Set<LogisticsAddressCity>()
                .AsNoTracking()
                .Where(x => x.StateId == stateId)
                .Select(x => new { x.CityKey, x.Name })
                .ToListAsync();
            return Ok(APIResponse<object>.Ok(data));
        }

        [HttpGet("Counties/{stateId}")]
        public async Task<IActionResult> GetCounties(string stateId)
        {
            var data = await _unitOfWork.Context.Set<LogisticsAddressCounty>()
                .AsNoTracking()
                .Where(x => x.StateId == stateId)
                .Select(x => new { x.CountyId, x.Name })
                .ToListAsync();
            return Ok(APIResponse<object>.Ok(data));
        }

        [HttpGet("Party/{partyId}")]
        public async Task<IActionResult> GetPartyAddresses(long partyId)
        {
            var partyLocations = await _partyLocationService.GetPartyLocationsAsync(partyId);
            var postalOnly = partyLocations.Where(x => x.IsPostalAddress == IAX.IXApi.Modules.Finance.Common.NoYes.Yes).ToList();
            var locationIds = postalOnly.Select(x => x.Location).ToList();
            var postalAddresses = await _postalAddressService.GetAddressesByLocationsAsync(locationIds);

            var dtos = postalAddresses.Select(p => {
                var pLoc = postalOnly.FirstOrDefault(l => l.Location == p.Location);
                return new AddressInfoDto
                {
                    Id = p.RecId.ToString(),
                    Location = p.Location,
                    LocationId = p.LogisticsLocationTable?.LocationId ?? string.Empty,
                    Description = p.LogisticsLocationTable?.Description ?? string.Empty,
                    Address = p.Address,
                    Primary = pLoc?.IsPrimary == IAX.IXApi.Modules.Finance.Common.NoYes.Yes,
                    Street = p.Street,
                    City = p.City,
                    State = p.State,
                    ZipCode = p.ZipCode,
                    County = p.County,
                    CountryRegionId = p.CountryRegionId,
                    ValidFrom = p.ValidFrom,
                    ValidTo = p.ValidTo
                };
            }).ToList();
            return Ok(APIResponse<System.Collections.Generic.IEnumerable<AddressInfoDto>>.Ok(dtos));
        }

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

