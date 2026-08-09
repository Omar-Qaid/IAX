using IAX.IXApi.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Workflow.Variables
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class WfDataTypeController : BaseController<WfDataType, WfDataTypeDto>
    {
        public WfDataTypeController(IWfDataTypeService service, ILogger<WfDataTypeController> logger) : base(service, logger)
        {
        }
    }
}
