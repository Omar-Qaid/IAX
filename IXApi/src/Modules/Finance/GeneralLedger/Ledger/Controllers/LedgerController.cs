using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.Ledger
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("GeneralLedger", "Ledger")]
    public class LedgerController : BaseController<IAX.IXApi.Modules.Finance.Entities.Ledger, LedgerDto>
    {
        public LedgerController(
            ILedgerService service,
            ILogger<LedgerController>? logger = null)
            : base(service, logger ?? NullLogger<LedgerController>.Instance)
        {
        }
    }
}
