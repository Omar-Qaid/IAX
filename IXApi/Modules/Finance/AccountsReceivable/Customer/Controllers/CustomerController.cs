using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Infrastructure.Persistence.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Route("api/[controller]")]
    [Route("api/v1/CustTable")]
    [Route("api/CustTable")]
    public class CustomerController : BaseController<CustTable, CustomerDto>
    {
        public CustomerController(IBaseService<CustTable> service, ILogger<CustomerController> logger)
            : base(service, logger)
        {
        }
    }
}

