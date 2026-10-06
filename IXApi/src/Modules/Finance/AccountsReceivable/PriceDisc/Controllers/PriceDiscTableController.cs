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
    [DomainPermission("AccountsReceivable", "PriceDiscTables")]
    public class PriceDiscTableController : BaseController<PriceDiscTable, PriceDiscTableDto>
    {
        public PriceDiscTableController(IBaseService<PriceDiscTable> service, ILogger<PriceDiscTableController> logger)
            : base(service, logger)
        {
        }
    }
}
