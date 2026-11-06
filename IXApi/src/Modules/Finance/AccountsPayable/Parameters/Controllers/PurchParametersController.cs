using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.AccountsPayable
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Route("api/[controller]")]
    [Route("api/v1/PurchParameters")]
    [Route("api/PurchParameters")]
    [DomainPermission("AccountsPayable", "VendParameters")]
    public class PurchParametersController : BaseController<PurchParameters, PurchParametersDto>
    {
        public PurchParametersController(IPurchParametersService service, ILogger<PurchParametersController> logger)
            : base(service, logger)
        {
        }
    }
}
