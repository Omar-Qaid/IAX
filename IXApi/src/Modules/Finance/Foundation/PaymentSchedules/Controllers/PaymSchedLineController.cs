using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.Foundation.PaymentSchedules
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("AccountsReceivable", "PaymSched")]
    public class PaymSchedLineController : BaseController<PaymSchedLine, PaymSchedLineDto>
    {
        public PaymSchedLineController(IPaymSchedLineService service, ILogger<PaymSchedLineController> logger)
            : base(service, logger)
        {
        }
    }
}
