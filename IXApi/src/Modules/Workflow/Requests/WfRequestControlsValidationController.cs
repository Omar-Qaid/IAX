using IAX.IXApi.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Workflow.Requests
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class WfRequestControlsValidationController : BaseController<WfRequestControlsValidation, WfRequestControlsValidationDto>
    {
        public WfRequestControlsValidationController(IWfRequestControlsValidationService service, ILogger<WfRequestControlsValidationController> logger) : base(service, logger)
        {
        }
    }
}
