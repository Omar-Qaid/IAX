using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Route("api/[controller]")]
    [Route("api/v1/SalesParameters")]
    [Route("api/SalesParameters")]
    [DomainPermission("AccountsReceivable", "CustParameters")]
    public class SalesParametersController : BaseController<SalesParameters, SalesParametersDto>
    {
        public SalesParametersController(ISalesParametersService service, ILogger<SalesParametersController> logger)
            : base(service, logger)
        {
        }
    }
}
