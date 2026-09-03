using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Workflow.Variables
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("Workflow", "DataTypes")]
    public class WfDataTypeController : BaseController<WfDataType, WfDataTypeDto>
    {
        public WfDataTypeController(IWfDataTypeService service, ILogger<WfDataTypeController> logger) : base(service, logger)
        {
        }
    }
}
