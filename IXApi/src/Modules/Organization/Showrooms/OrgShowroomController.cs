using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Organization.Showrooms
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("Organization", "Showrooms")]
    public class OrgShowroomController : BaseController<OrgShowroom, OrgShowroomDto>
    {
        private readonly IOrgShowroomService _showroomService;

        public OrgShowroomController(IOrgShowroomService service, ILogger<OrgShowroomController> logger) : base(service, logger)
        {
            _showroomService = service;
        }

        protected override string[]? GetDefaultIncludes() => new[] { "Department" };

        /// <summary>Lists the sellers assigned to a showroom (for the showroom sellers page).</summary>
        [HttpGet("{id:long}/sellers")]
        public async Task<ActionResult<APIResponse<IEnumerable<ShowroomSellerDto>>>> GetSellers(long id, CancellationToken cancellationToken = default)
        {
            var sellers = await _showroomService.GetSellersAsync(id, cancellationToken);
            return Ok(APIResponse<IEnumerable<ShowroomSellerDto>>.Ok(sellers));
        }

        /// <summary>Replaces the full set of sellers assigned to a showroom.</summary>
        [HttpPut("{id:long}/sellers")]
        public async Task<ActionResult<APIResponse<IEnumerable<ShowroomSellerDto>>>> SetSellers(long id, [FromBody] IEnumerable<long> employeeIds, CancellationToken cancellationToken = default)
        {
            var sellers = await _showroomService.SetSellersAsync(id, employeeIds ?? Enumerable.Empty<long>(), cancellationToken);
            return Ok(APIResponse<IEnumerable<ShowroomSellerDto>>.Ok(sellers, "Saved successfully"));
        }
    }
}

