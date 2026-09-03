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
    [Route("api/v1/CustGroup")]
    [Route("api/CustGroup")]
    [DomainPermission("AccountsReceivable", "CustomerGroups")]
    public class CustomerGroupController : BaseController<CustGroup, CustGroupDto>
    {
        public CustomerGroupController(IBaseService<CustGroup> service, ILogger<CustomerGroupController> logger)
            : base(service, logger)
        {
        }
    }
}

