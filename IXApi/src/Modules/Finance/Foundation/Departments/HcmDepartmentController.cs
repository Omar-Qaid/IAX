using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.Foundation.Departments
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("Organization", "Departments")]
    public class HcmDepartmentController : BaseController<HcmDepartment, HcmDepartmentDto>
    {
        public HcmDepartmentController(IHcmDepartmentService service, ILogger<HcmDepartmentController> logger) : base(service, logger)
        {
        }
    }

    [ApiController]
    [Route("api/v1/Department")]
    [DomainPermission("Organization", "Departments")]
    public class LegacyDepartmentController : BaseController<HcmDepartment, HcmDepartmentDto>
    {
        public LegacyDepartmentController(IHcmDepartmentService service, ILogger<LegacyDepartmentController> logger) : base(service, logger)
        {
        }
    }
}
