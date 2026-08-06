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

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Route("api/[controller]")]
    [Route("api/v1/MarkupTable")]
    [Route("api/MarkupTable")]
    [Route("api/v1/ChargesCode")]
    [Route("api/ChargesCode")]
    public class MarkupTableController : BaseController<MarkupTable, MarkupTableDto>
    {
        public MarkupTableController(IBaseService<MarkupTable> service, ILogger<MarkupTableController> logger)
            : base(service, logger)
        {
        }
    }
}

