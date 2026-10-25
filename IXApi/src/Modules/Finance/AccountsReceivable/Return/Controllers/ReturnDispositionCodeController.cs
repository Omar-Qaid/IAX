using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

[ApiController]
[Route("api/v1/[controller]")]
[DomainPermission("AccountsReceivable", "ReturnOrders")]
public class ReturnDispositionCodeController : BaseController<ReturnDispositionCode, ReturnDispositionCodeDto>
{
    public ReturnDispositionCodeController(
        IReturnDispositionCodeService service,
        ILogger<ReturnDispositionCodeController> logger)
        : base(service, logger)
    {
    }
}
