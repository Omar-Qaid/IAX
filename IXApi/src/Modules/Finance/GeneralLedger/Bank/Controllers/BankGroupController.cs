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
    public class BankGroupController : BaseController<BankGroup, BankGroupDto>
    {
        public BankGroupController(
            IBankGroupService service,
            ILogger<BankGroupController>? logger = null)
            : base(service, logger ?? NullLogger<BankGroupController>.Instance)
        {
        }
    }
}
