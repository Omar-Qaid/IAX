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
    public class ExchangeRateTypeController : BaseController<ExchangeRateType, ExchangeRateTypeDto>
    {
        public ExchangeRateTypeController(IExchangeRateTypeService service, ILogger<ExchangeRateTypeController> logger)
            : base(service, logger)
        {
        }
    }
}
