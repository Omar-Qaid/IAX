using IAX.IXApi.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Organization.Companies
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class OrgCompanyController : BaseController<OrgCompany, OrgCompanyDto>
    {
        public OrgCompanyController(IOrgCompanyService service, ILogger<OrgCompanyController> logger) : base(service, logger)
        {
        }
    }
}
