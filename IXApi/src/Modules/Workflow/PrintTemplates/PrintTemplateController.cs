using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Workflow.PrintTemplates;

[ApiController]
[Route("api/v1/print-templates")]
public sealed class PrintTemplateController : ControllerBase
{
    private readonly IPrintTemplateService _service;

    public PrintTemplateController(IPrintTemplateService service)
    {
        _service = service;
    }

    [HttpGet("process/{processId:long}")]
    [DomainPermission("Workflow", "PrintTemplates", "View")]
    public async Task<ActionResult<APIResponse<IReadOnlyList<PrintTemplateSummaryDto>>>> ListByProcess(
        long processId,
        CancellationToken cancellationToken)
    {
        var templates = await _service.ListByProcessAsync(processId, cancellationToken);
        return Ok(APIResponse<IReadOnlyList<PrintTemplateSummaryDto>>.Ok(templates));
    }

    [HttpGet("process/{processId:long}/published")]
    public async Task<ActionResult<APIResponse<IReadOnlyList<PrintTemplateSummaryDto>>>> ListPublishedByProcess(
        long processId,
        CancellationToken cancellationToken)
    {
        var templates = await _service.ListPublishedByProcessAsync(processId, cancellationToken);
        return Ok(APIResponse<IReadOnlyList<PrintTemplateSummaryDto>>.Ok(templates));
    }

    [HttpGet("request/{requestId:long}/template/{templateId:long}")]
    public async Task<ActionResult<APIResponse<PublishedPrintTemplateDto>>> GetPublishedForRequest(
        long requestId,
        long templateId,
        CancellationToken cancellationToken)
    {
        var template = await _service.GetPublishedForRequestAsync(requestId, templateId, cancellationToken);
        return template == null
            ? NotFound(APIResponse<PublishedPrintTemplateDto>.Fail("No active published template was found for this request."))
            : Ok(APIResponse<PublishedPrintTemplateDto>.Ok(template));
    }

    [HttpGet("process/{processId:long}/template/{templateId:long}/published")]
    public async Task<ActionResult<APIResponse<PublishedPrintTemplateDto>>> GetPublishedForProcess(
        long processId,
        long templateId,
        CancellationToken cancellationToken)
    {
        var template = await _service.GetPublishedForProcessAsync(processId, templateId, cancellationToken);
        return template == null
            ? NotFound(APIResponse<PublishedPrintTemplateDto>.Fail("No active published template was found for this process."))
            : Ok(APIResponse<PublishedPrintTemplateDto>.Ok(template));
    }

    [HttpGet("{templateId:long}")]
    [DomainPermission("Workflow", "PrintTemplates", "View")]
    public async Task<ActionResult<APIResponse<PrintTemplateDto>>> Get(long templateId, CancellationToken cancellationToken)
    {
        var template = await _service.GetAsync(templateId, cancellationToken);
        return template == null
            ? NotFound(APIResponse<PrintTemplateDto>.Fail("The print template was not found."))
            : Ok(APIResponse<PrintTemplateDto>.Ok(template));
    }

    [HttpPost]
    [DomainPermission("Workflow", "PrintTemplates", "Create")]
    public async Task<ActionResult<APIResponse<PrintTemplateDto>>> Create(
        [FromBody] CreatePrintTemplateDto input,
        CancellationToken cancellationToken)
    {
        try
        {
            var template = await _service.CreateAsync(input, cancellationToken);
            return CreatedAtAction(nameof(Get), new { templateId = template.TemplateId },
                APIResponse<PrintTemplateDto>.Ok(template, "Print template created."));
        }
        catch (PrintTemplateValidationException exception)
        {
            return BadRequest(APIResponse<PrintTemplateDto>.Fail(string.Join(" ", exception.Errors)));
        }
    }

    [HttpPut("{templateId:long}")]
    [DomainPermission("Workflow", "PrintTemplates", "Edit")]
    public async Task<ActionResult<APIResponse<PrintTemplateDto>>> Update(
        long templateId,
        [FromBody] UpdatePrintTemplateDto input,
        CancellationToken cancellationToken)
    {
        try
        {
            var template = await _service.UpdateAsync(templateId, input, cancellationToken);
            return template == null
                ? NotFound(APIResponse<PrintTemplateDto>.Fail("The print template was not found."))
                : Ok(APIResponse<PrintTemplateDto>.Ok(template, "Print template draft saved."));
        }
        catch (PrintTemplateValidationException exception)
        {
            return BadRequest(APIResponse<PrintTemplateDto>.Fail(string.Join(" ", exception.Errors)));
        }
    }

    [HttpPost("{templateId:long}/publish")]
    [DomainPermission("Workflow", "PrintTemplates", "Publish")]
    public async Task<ActionResult<APIResponse<PrintTemplateDto>>> Publish(
        long templateId,
        [FromBody] PublishPrintTemplateDto input,
        CancellationToken cancellationToken)
    {
        try
        {
            var template = await _service.PublishAsync(templateId, input.TemplateVersionId, cancellationToken);
            return template == null
                ? NotFound(APIResponse<PrintTemplateDto>.Fail("The print template was not found."))
                : Ok(APIResponse<PrintTemplateDto>.Ok(template, "Print template published."));
        }
        catch (PrintTemplateValidationException exception)
        {
            return BadRequest(APIResponse<PrintTemplateDto>.Fail(string.Join(" ", exception.Errors)));
        }
    }

    [HttpPost("{templateId:long}/archive")]
    [DomainPermission("Workflow", "PrintTemplates", "Archive")]
    public async Task<ActionResult<APIResponse<PrintTemplateDto>>> Archive(long templateId, CancellationToken cancellationToken)
    {
        var template = await _service.ArchiveAsync(templateId, cancellationToken);
        return template == null
            ? NotFound(APIResponse<PrintTemplateDto>.Fail("The print template was not found."))
            : Ok(APIResponse<PrintTemplateDto>.Ok(template, "Print template archived."));
    }

    [HttpDelete("{templateId:long}")]
    [DomainPermission("Workflow", "PrintTemplates", "Delete")]
    public async Task<ActionResult<APIResponse<bool>>> DeleteDraft(long templateId, CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await _service.DeleteDraftAsync(templateId, cancellationToken);
            return deleted
                ? Ok(APIResponse<bool>.Ok(true, "Draft print template deleted."))
                : NotFound(APIResponse<bool>.Fail("The print template was not found."));
        }
        catch (PrintTemplateValidationException exception)
        {
            return BadRequest(APIResponse<bool>.Fail(string.Join(" ", exception.Errors)));
        }
    }

    [HttpGet("{templateId:long}/validation")]
    [DomainPermission("Workflow", "PrintTemplates", "View")]
    public async Task<ActionResult<APIResponse<PrintTemplateValidationResultDto>>> Validate(long templateId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.ValidateAsync(templateId, cancellationToken);
            return result == null
                ? NotFound(APIResponse<PrintTemplateValidationResultDto>.Fail("The print template was not found."))
                : Ok(APIResponse<PrintTemplateValidationResultDto>.Ok(result));
        }
        catch (PrintTemplateValidationException exception)
        {
            return BadRequest(APIResponse<PrintTemplateValidationResultDto>.Fail(string.Join(" ", exception.Errors)));
        }
    }
}
