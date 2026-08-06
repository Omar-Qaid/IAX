using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Infrastructure.Persistence.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Route("api/[controller]")]
    public class CustLedgerController : BaseController<CustLedger, CustLedgerDto>
    {
        public CustLedgerController(IBaseService<CustLedger> service, ILogger<CustLedgerController> logger)
            : base(service, logger)
        {
        }
    }
}

