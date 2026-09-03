using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Route("api/[controller]")]
    [Route("api/v1/CustPaymModeTable")]
    [Route("api/CustPaymModeTable")]
    [Route("api/v1/PaymMode")]
    [Route("api/PaymMode")]
    [DomainPermission("AccountsReceivable", "PaymentMethods")]
    public class CustPaymModeController : BaseController<CustPaymModeTable, CustPaymModeDto>
    {
        public CustPaymModeController(IBaseService<CustPaymModeTable> service, ILogger<CustPaymModeController> logger)
            : base(service, logger)
        {
        }
    }
}

