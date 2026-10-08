using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.PriceDisc
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Route("api/[controller]")]
    [DomainPermission("AccountsReceivable", "PriceDiscAdmNames")]
    public class PriceDiscAdmNameController : BaseController<PriceDiscAdmName, PriceDiscAdmNameDto>
    {
        public PriceDiscAdmNameController(IPriceDiscAdmNameService service, ILogger<PriceDiscAdmNameController> logger)
            : base(service, logger)
        {
        }
    }
}
