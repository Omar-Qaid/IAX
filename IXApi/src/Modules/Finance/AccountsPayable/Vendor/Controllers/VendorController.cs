using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Infrastructure.Persistence.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.AccountsPayable
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class VendorController : BaseController<VendTable, VendorDto>
    {
        public VendorController(IBaseService<VendTable> service, ILogger<VendorController> logger)
            : base(service, logger)
        {
        }
    }
}
