using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.LedgerAccount
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("GeneralLedger", "Accounts")]
    public class MainAccountCategoryController : BaseController<MainAccountCategory, MainAccountCategoryDto>
    {
        public MainAccountCategoryController(
            IMainAccountCategoryService service,
            ILogger<MainAccountCategoryController>? logger = null)
            : base(service, logger ?? NullLogger<MainAccountCategoryController>.Instance)
        {
        }
    }
}
