using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Infrastructure.Persistence.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.ERP.AccountsReceivable
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Route("api/[controller]")]
    [Route("api/v1/CustPaymModeTable")]
    [Route("api/CustPaymModeTable")]
    [Route("api/v1/PaymMode")]
    [Route("api/PaymMode")]
    public class CustPaymModeController : BaseController<CustPaymModeTable, CustPaymModeDto>
    {
        public CustPaymModeController(IBaseService<CustPaymModeTable> service, ILogger<CustPaymModeController> logger)
            : base(service, logger)
        {
        }
    }
}
