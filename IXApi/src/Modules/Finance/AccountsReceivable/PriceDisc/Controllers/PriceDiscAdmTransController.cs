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
    [DomainPermission("AccountsReceivable", "PriceDiscAdmTrans")]
    public class PriceDiscAdmTransController : BaseController<PriceDiscAdmTrans, PriceDiscAdmTransDto>
    {
        public PriceDiscAdmTransController(IBaseService<PriceDiscAdmTrans> service, ILogger<PriceDiscAdmTransController> logger)
            : base(service, logger)
        {
        }
    }
}
