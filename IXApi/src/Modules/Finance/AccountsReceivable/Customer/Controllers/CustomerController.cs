using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Foundation.LogisticsAddresses;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Infrastructure.Identity;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Customer
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Route("api/[controller]")]
    [Route("api/v1/CustTable")]
    [Route("api/CustTable")]
    [DomainPermission("AccountsReceivable", "Customers")]
    public class CustomerController : BaseController<CustTable, CustomerDto>
    {
        private readonly ICustomerService _customer;
        private readonly ICustomerQuickService _quick;

        public CustomerController(
            ICustomerService service,
            ILogger<CustomerController> logger,
            ICustomerQuickService quick)
            : base(service, logger)
        {
            _customer = service;
            _quick = quick;
        }

        [HttpGet("country-regions")]
        public async Task<IActionResult> GetCountryRegions(CancellationToken cancellationToken = default)
            => Ok(APIResponse<object>.Ok(await _customer.GetCountryRegionsAsync(cancellationToken)));

        [HttpGet("list")]
        public async Task<ActionResult<APIResponse<IEnumerable<CustomerListDto>>>> GetCustomerList(
            CancellationToken cancellationToken = default)
            => Ok(APIResponse<IEnumerable<CustomerListDto>>.Ok(
                await _customer.GetCustomerListAsync(cancellationToken)));

        [HttpGet("{accountNumber}/sales-order-defaults")]
        public async Task<ActionResult<APIResponse<CustomerSalesOrderDefaultsDto>>> GetSalesOrderDefaults(
            string accountNumber, CancellationToken cancellationToken = default)
        {
            var defaults = await _customer.GetSalesOrderDefaultsAsync(accountNumber, cancellationToken);
            return defaults == null
                ? NotFound(APIResponse<CustomerSalesOrderDefaultsDto>.Fail("Customer account was not found."))
                : Ok(APIResponse<CustomerSalesOrderDefaultsDto>.Ok(defaults));
        }

        [HttpPost("quick-create")]
        public async Task<ActionResult<APIResponse<CustomerListDto>>> QuickCreate(
            [FromBody] CustomerQuickCreateDto input, CancellationToken cancellationToken = default)
        {
            var result = await _quick.QuickCreateAsync(input, cancellationToken);
            return Ok(APIResponse<CustomerListDto>.Ok(_customer.ToListDto(result.Customer, result.Party), "Created successfully"));
        }

        [HttpPut("quick-update/{id:long}")]
        public async Task<ActionResult<APIResponse<CustomerListDto>>> QuickUpdate(
            long id, [FromBody] CustomerQuickCreateDto input, CancellationToken cancellationToken = default)
        {
            var result = await _quick.QuickUpdateAsync(id, input, cancellationToken);
            return result.Status switch
            {
                CustomerQuickUpdateStatus.CustomerNotFound => NotFound(APIResponse<CustomerListDto>.Fail("Customer not found")),
                CustomerQuickUpdateStatus.PartyNotFound => NotFound(APIResponse<CustomerListDto>.Fail("Customer party not found")),
                _ => Ok(APIResponse<CustomerListDto>.Ok(_customer.ToListDto(result.Customer!, result.Party), "Updated successfully"))
            };
        }


    }
}

