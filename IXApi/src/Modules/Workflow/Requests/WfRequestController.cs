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

        [HttpGet]
        public override async Task<ActionResult<APIResponse<IEnumerable<WfRequestDto>>>> GetAll(CancellationToken cancellationToken = default)
        {
            var requests = await _requestService.GetRequestListAsync(cancellationToken);
            return Ok(APIResponse<IEnumerable<WfRequestDto>>.Ok(requests));
        }

        [HttpGet("form-definition/{processId:long}")]
        public async Task<ActionResult<APIResponse<DynamicRequestFormDto>>> GetFormDefinition(long processId, CancellationToken cancellationToken)
        {
            var definition = await _requestService.GetFormDefinitionAsync(processId, cancellationToken);
            return definition == null
                ? NotFound(APIResponse<DynamicRequestFormDto>.Fail("The workflow process was not found or is inactive."))
                : Ok(APIResponse<DynamicRequestFormDto>.Ok(definition));
        }

        [HttpGet("{requestId:long}/mail-details")]
        public async Task<ActionResult<APIResponse<MailRequestDetailsDto>>> GetMailDetails(long requestId, CancellationToken cancellationToken)
        {
            var details = await _requestService.GetMailDetailsAsync(requestId, cancellationToken);
            return details == null
                ? NotFound(APIResponse<MailRequestDetailsDto>.Fail("The workflow request was not found."))
                : Ok(APIResponse<MailRequestDetailsDto>.Ok(details));
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
