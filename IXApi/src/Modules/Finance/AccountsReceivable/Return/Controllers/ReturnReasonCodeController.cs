using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

[ApiController]
[Route("api/v1/[controller]")]
[DomainPermission("AccountsReceivable", "ReturnOrders")]
public class ReturnReasonCodeController : BaseController<ReturnReasonCode, ReturnReasonCodeDto>
{
    public ReturnReasonCodeController(
        IReturnReasonCodeService service,
        ILogger<ReturnReasonCodeController> logger)
        : base(service, logger)
    {
    }
}
