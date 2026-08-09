using IAX.IXApi.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Workflow.Activities
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class WfActivityMappingVariableController : BaseController<WfActivityMappingVariable, WfActivityMappingVariableDto>
    {
        public WfActivityMappingVariableController(IWfActivityMappingVariableService service, ILogger<WfActivityMappingVariableController> logger) : base(service, logger)
        {
        }
    }
}
