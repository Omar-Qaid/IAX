using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("AccountsReceivable", "PaymTerm")]
    public class PaymTermController : BaseController<PaymTerm, PaymTermDto>
    {
        public PaymTermController(IPaymTermService service, ILogger<PaymTermController> logger)
            : base(service, logger)
        {
        }
    }
}

