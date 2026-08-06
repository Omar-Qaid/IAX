using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Organization.Employees
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("Organization", "Employees")]
    public class HcmWorkerController : BaseController<IAX.IXApi.Modules.Organization.Employees.Entities.HcmWorker, HcmWorkerDto>
    {
        public HcmWorkerController(IHcmWorkerService service, ILogger<HcmWorkerController> logger) : base(service, logger)
        {
        }

        protected override string[]? GetDefaultIncludes() => new[] { "Department", "Occupation", "Gender", "Nationality", "Showroom" };
    }
}

