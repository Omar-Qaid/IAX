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
    public class CurrencyController : BaseController<Currency, CurrencyDto>
    {
        public CurrencyController(ICurrencyService service, ILogger<CurrencyController> logger)
            : base(service, logger)
        {
        }
    }
}
