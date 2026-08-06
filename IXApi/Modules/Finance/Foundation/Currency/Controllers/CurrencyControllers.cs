using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("GeneralLedger", "Currencies")]
    public class CurrencyController : BaseController<Currency, CurrencyDto>
    {
        public CurrencyController(ICurrencyService service, ILogger<CurrencyController> logger)
            : base(service, logger)
        {
        }
    }

    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("GeneralLedger", "ExchangeRates")]
    public class ExchangeRateController : BaseController<ExchangeRate, ExchangeRateDto>
    {
        public ExchangeRateController(IExchangeRateService service, ILogger<ExchangeRateController> logger)
            : base(service, logger)
        {
        }
    }

    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("GeneralLedger", "ExchangeRateCurrencyPairs")]
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

    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("GeneralLedger", "ExchangeRateTypes")]
    public class ExchangeRateTypeController : BaseController<ExchangeRateType, ExchangeRateTypeDto>
    {
        public ExchangeRateTypeController(IExchangeRateTypeService service, ILogger<ExchangeRateTypeController> logger)
            : base(service, logger)
        {
        }
    }
}

