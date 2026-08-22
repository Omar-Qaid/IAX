using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Workflow.Requests
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class WfRequestController : BaseController<WfRequest, WfRequestDto>
    {
        private readonly IValidationEngine _validationEngine;
        private readonly IWfRequestService _requestService;

        public WfRequestController(IWfRequestService service, ILogger<WfRequestController> logger, IValidationEngine validationEngine) : base(service, logger)
        {
            _validationEngine = validationEngine;
            _requestService = service;
        }

        [HttpGet("form-definition/{processId:long}")]
        public async Task<ActionResult<APIResponse<DynamicRequestFormDto>>> GetFormDefinition(long processId, CancellationToken cancellationToken)
        {
            var definition = await _requestService.GetFormDefinitionAsync(processId, cancellationToken);
            return definition == null
                ? NotFound(APIResponse<DynamicRequestFormDto>.Fail("The workflow process was not found or is inactive."))
                : Ok(APIResponse<DynamicRequestFormDto>.Ok(definition));
        }

        [HttpPost("submit")]
        public async Task<ActionResult<APIResponse<SubmitDynamicRequestResultDto>>> SubmitDynamic([FromBody] SubmitDynamicRequestDto submission, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _requestService.SubmitDynamicAsync(submission, cancellationToken);
                return Ok(APIResponse<SubmitDynamicRequestResultDto>.Ok(result, "Request submitted successfully."));
            }
            catch (DynamicRequestValidationException exception)
            {
                return BadRequest(new APIResponse<SubmitDynamicRequestResultDto>
                {
                    Success = false,
                    Message = string.Join(" ", exception.Errors.Select(error => $"{error.ControlName}: {error.ErrorMessage}"))
                });
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(APIResponse<SubmitDynamicRequestResultDto>.Fail(exception.Message));
            }
        }

        [HttpPost("validate")]
        public async Task<IActionResult> ValidateRequest([FromBody] ValidateRequestParams parameters)
        {
            var errors = await _validationEngine.ValidateRequestAsync(parameters.ProcessId, parameters.RequestId, parameters.Details);
            return Ok(new { success = errors.Count == 0, errors });
        }
    }
}
