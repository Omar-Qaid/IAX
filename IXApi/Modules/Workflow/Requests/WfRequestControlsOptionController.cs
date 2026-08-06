using IAX.IXApi.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Workflow.Requests
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class WfRequestControlsOptionController : BaseController<WfRequestControlsOption, WfRequestControlsOptionDto>
    {
        public WfRequestControlsOptionController(IWfRequestControlsOptionService service, ILogger<WfRequestControlsOptionController> logger) : base(service, logger)
        {
        }
    }
}
