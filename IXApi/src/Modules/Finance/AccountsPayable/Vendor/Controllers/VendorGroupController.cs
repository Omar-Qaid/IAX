using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.AccountsPayable
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("AccountsPayable", "VendorGroups")]
    public class VendorGroupController : BaseController<VendGroup, VendorGroupDto>
    {
        public VendorGroupController(
            IVendorGroupService service,
            ILogger<VendorGroupController> logger)
            : base(service, logger)
        {
        }
    }
}
