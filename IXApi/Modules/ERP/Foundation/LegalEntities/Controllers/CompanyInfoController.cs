using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Shared.Application.Contracts;
using Mapster;

namespace IAX.IXApi.Modules.ERP.Foundation.LegalEntities
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("Organization", "LegalEntities")]
    public class CompanyInfoController : BaseController<CompanyInfo, CompanyInfoDto>
    {
        public CompanyInfoController(ICompanyInfoService service, ILogger<CompanyInfoController> logger) : base(service, logger)
        {
        }

        [HttpPut("{id}")]
        public override async Task<ActionResult<APIResponse<CompanyInfoDto>>> Update(string id, [FromBody] CompanyInfoDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{EntityName}] - Updating record with ID: {Id} via GAB Transaction Flow", _entityName, id);
            
            var companyInfoService = _service as ICompanyInfoService;
            if (companyInfoService == null)
            {
                return BadRequest(APIResponse<CompanyInfoDto>.Fail("Invalid service configuration"));
            }

            var updatedEntity = await companyInfoService.UpdateCompanyWithAddressBookAsync(id, dto, cancellationToken);
            var resultDto = (await ReloadWithDefaultsAsync(id, cancellationToken) ?? updatedEntity).Adapt<CompanyInfoDto>();

            // Query and populate the database-persisted lists (with correct RecIds and Locations)
            await companyInfoService.PopulateGlobalAddressBookAsync(new[] { resultDto }, cancellationToken);

            await OnAfterUpdateAsync(resultDto);
            return Ok(APIResponse<CompanyInfoDto>.Ok(resultDto, "Updated successfully"));
        }

        [HttpPost]
        public override async Task<ActionResult<APIResponse<CompanyInfoDto>>> Create([FromBody] CompanyInfoDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{EntityName}] - Creating record via GAB Transaction Flow", _entityName);

            var companyInfoService = _service as ICompanyInfoService;
            if (companyInfoService == null)
            {
                return BadRequest(APIResponse<CompanyInfoDto>.Fail("Invalid service configuration"));
            }

            var createdEntity = await companyInfoService.CreateCompanyWithAddressBookAsync(dto, cancellationToken);
            var resultDto = createdEntity.Adapt<CompanyInfoDto>();

            // Query and populate the database-persisted lists (with correct RecIds and Locations)
            await companyInfoService.PopulateGlobalAddressBookAsync(new[] { resultDto }, cancellationToken);

            return Ok(APIResponse<CompanyInfoDto>.Ok(resultDto, "Created successfully"));
        }

        [HttpGet("paged")]
        public override async Task<ActionResult<APIResponse<IEnumerable<CompanyInfoDto>>>> GetPaged([FromQuery] IAX.IXApi.Shared.Application.Contracts.QueryFilterDto filter, CancellationToken cancellationToken = default)
        {
            var result = await base.GetPaged(filter, cancellationToken);
            if (result.Result is OkObjectResult okResult && okResult.Value is APIResponse<IEnumerable<CompanyInfoDto>> apiResponse && apiResponse.Data != null)
            {
                var companyInfoService = _service as ICompanyInfoService;
                if (companyInfoService != null)
                {
                    await companyInfoService.PopulateGlobalAddressBookAsync(apiResponse.Data, cancellationToken);
                }
            }
            return result;
        }

        [HttpGet("{id}")]
        public override async Task<ActionResult<APIResponse<CompanyInfoDto>>> GetById(string id, CancellationToken cancellationToken = default)
        {
            var result = await base.GetById(id, cancellationToken);
            if (result.Result is OkObjectResult okResult && okResult.Value is APIResponse<CompanyInfoDto> apiResponse && apiResponse.Data != null)
            {
                var companyInfoService = _service as ICompanyInfoService;
                if (companyInfoService != null)
                {
                    await companyInfoService.PopulateGlobalAddressBookAsync(new[] { apiResponse.Data }, cancellationToken);
                }
            }
            return result;
        }
    }
}

