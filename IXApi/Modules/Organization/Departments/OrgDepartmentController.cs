using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Organization.Departments
{
    [ApiController]
    [Route("api/v1/[controller]")]
[DomainPermission("Organization", "Departments")]
    public class OrgDepartmentController : BaseController<OrgDepartment, OrgDepartmentDto>
    {
        public OrgDepartmentController(IOrgDepartmentService service, ILogger<OrgDepartmentController> logger) : base(service, logger)
        {
        }
    }
}
