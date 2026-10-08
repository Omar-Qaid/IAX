using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("Tax", "ExemptCodes")]
    public class TaxExemptCodeController : BaseController<TaxExemptCodeTable, TaxExemptCodeDto>
    {
        public TaxExemptCodeController(
            ITaxExemptCodeService service,
            ILogger<TaxExemptCodeController> logger) : base(service, logger)
        {
        }
    }
}

