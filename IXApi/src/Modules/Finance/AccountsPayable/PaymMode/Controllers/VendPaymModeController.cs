using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.AccountsPayable
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Route("api/[controller]")]
    [Route("api/v1/VendPaymModeTable")]
    [Route("api/VendPaymModeTable")]
    [DomainPermission("AccountsPayable", "PaymentMethods")]
    public class VendPaymModeController : BaseController<VendPaymModeTable, VendPaymModeDto>
    {
        public VendPaymModeController(IVendPaymModeService service, ILogger<VendPaymModeController> logger)
            : base(service, logger)
        {
        }
    }
}
