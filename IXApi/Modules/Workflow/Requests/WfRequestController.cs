using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IAX.IXApi.Modules.Workflow.Requests
{
    [ApiController]
    [Route("api/v1/[controller]")]
[DomainPermission("Workflow", "Requests")]
    public class WfRequestController : BaseController<WfRequest, WfRequestDto>
    {
        private readonly IValidationEngine _validationEngine;

        public WfRequestController(IWfRequestService service, ILogger<WfRequestController> logger, IValidationEngine validationEngine) : base(service, logger)
        {
            _validationEngine = validationEngine;
        }

        [HttpPost("validate")]
        public async Task<IActionResult> ValidateRequest([FromBody] ValidateRequestParams parameters)
        {
            var errors = await _validationEngine.ValidateRequestAsync(parameters.ProcessId, parameters.RequestId, parameters.Details);
            return Ok(new { success = errors.Count == 0, errors });
        }
    }

    public class ValidateRequestParams
    {
        public long ProcessId { get; set; }
        public long? RequestId { get; set; }
        public List<WfRequestDetail> Details { get; set; } = new();
    }
}
