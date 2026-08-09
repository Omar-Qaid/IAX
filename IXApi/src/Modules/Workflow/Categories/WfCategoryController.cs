using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Workflow.Categories
{
    [ApiController]
    [Route("api/v1/[controller]")]
[DomainPermission("Workflow", "Categories")]
    public class WfCategoryController : BaseController<WfCategory, WfCategoryDto>
    {
        public WfCategoryController(IWfCategoryService service, ILogger<WfCategoryController> logger) : base(service, logger)
        {
        }
    }
}





