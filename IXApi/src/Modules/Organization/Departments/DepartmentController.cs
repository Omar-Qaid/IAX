using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Organization.Departments
{
    [ApiController]
    [Route("api/v1/[controller]")]
[DomainPermission("Organization", "Departments")]
    public class DepartmentController : BaseController<Department, DepartmentDto>
    {
        public DepartmentController(IDepartmentService service, ILogger<DepartmentController> logger) : base(service, logger)
        {
        }
    }
}
