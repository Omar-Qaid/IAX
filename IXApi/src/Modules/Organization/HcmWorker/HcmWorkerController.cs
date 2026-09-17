using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Organization.HcmWorkers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("Organization", "Employees")]
    public class HcmWorkerController : BaseController<HcmWorker, HcmWorkerDto>
    {
        public HcmWorkerController(IHcmWorkerService service, ILogger<HcmWorkerController> logger) : base(service, logger)
        {
        }

        protected override string[]? GetDefaultIncludes() => new[] { "Gender", "Nationality" };
    }
}

