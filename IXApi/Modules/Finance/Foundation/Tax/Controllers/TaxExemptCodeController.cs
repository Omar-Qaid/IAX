using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Infrastructure.Persistence.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using IAX.IXApi.Modules.Finance.Shared.Features;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class TaxExemptCodeController : BaseController<TaxExemptCodeTable, TaxExemptCodeDto>
    {
        public TaxExemptCodeController(
            IBaseService<TaxExemptCodeTable> service, 
            ILogger<TaxExemptCodeController> logger) : base(service, logger)
        {
        }
    }
}

