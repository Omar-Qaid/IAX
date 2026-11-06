using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.AccountsPayable
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Route("api/[controller]")]
    [DomainPermission("AccountsPayable", "VendParameters")]
    public class VendParametersController : BaseController<VendParameters, VendParametersDto>
    {
        public VendParametersController(IVendParametersService service, ILogger<VendParametersController> logger)
            : base(service, logger)
        {
        }
    }
}
