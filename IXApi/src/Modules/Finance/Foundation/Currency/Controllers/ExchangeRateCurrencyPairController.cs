using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ExchangeRateCurrencyPairController : BaseController<ExchangeRateCurrencyPair, ExchangeRateCurrencyPairDto>
    {
        private readonly IExchangeRateCurrencyPairService _pairService;
        public ExchangeRateCurrencyPairController(IExchangeRateCurrencyPairService service, ILogger<ExchangeRateCurrencyPairController> logger)
            : base(service, logger)
        {
            _pairService = service;
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> BulkSave([FromBody] BulkExchangeRatePairDto dto)
        {
            var result = await _pairService.BulkSaveAsync(dto);
            return Ok(APIResponse<BulkExchangeRatePairDto>.Ok(result));
        }
    }
}
