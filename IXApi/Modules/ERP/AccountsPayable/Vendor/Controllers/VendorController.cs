using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Infrastructure.Persistence.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.ERP.AccountsPayable
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Route("api/[controller]")]
    [Route("api/v1/VendTable")]
    [Route("api/VendTable")]
    public class VendorController : BaseController<VendTable, VendorDto>
    {
        public VendorController(IBaseService<VendTable> service, ILogger<VendorController> logger)
            : base(service, logger)
        {
        }
    }

    [ApiController]
    [Route("api/v1/[controller]")]
    [Route("api/[controller]")]
    [Route("api/v1/VendGroup")]
    [Route("api/VendGroup")]
    public class VendorGroupController : BaseController<VendGroup, VendorGroupDto>
    {
        public VendorGroupController(IBaseService<VendGroup> service, ILogger<VendorGroupController> logger)
            : base(service, logger)
        {
        }
    }
}
