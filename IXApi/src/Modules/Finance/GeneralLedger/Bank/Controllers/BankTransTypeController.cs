using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.Bank
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("GeneralLedger", "Bank")]
    public class BankTransTypeController : BaseController<BankTransType, BankTransTypeDto>
    {
        public BankTransTypeController(
            IBankTransTypeService service,
            ILogger<BankTransTypeController>? logger = null)
            : base(service, logger ?? NullLogger<BankTransTypeController>.Instance)
        {
        }
    }
}
