using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Modules.Organization.DocumentManagement.Dtos;
using IAX.IXApi.Modules.Organization.DocumentManagement.Interfaces;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Organization.DocumentManagement.Controllers;

[ApiController]
[Route("api/v1/documents")]
[DomainPermission("System", "Documents")]
public sealed class DocumentController : ControllerBase
{
    private readonly IDocumentService _service;
    private readonly ILogger<DocumentController> _logger;

    public DocumentController(IDocumentService service, ILogger<DocumentController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("types")]
    public async Task<ActionResult<APIResponse<IReadOnlyList<DocuTypeDto>>>> GetTypes(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching document types");
        var types = await _service.GetTypesAsync(cancellationToken);
        return Ok(APIResponse<IReadOnlyList<DocuTypeDto>>.Ok(types));
    }

    [HttpGet("record/{refTableId:int}/{refRecId:long}")]
    public async Task<ActionResult<APIResponse<DocumentPageDto>>> GetForRecord(
        int refTableId,
        long refRecId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching documents for RefTableId {RefTableId}, RefRecId {RefRecId}", refTableId, refRecId);
        var page = await _service.GetForRecordAsync(refTableId, refRecId, pageNumber, pageSize, cancellationToken);
        return Ok(APIResponse<DocumentPageDto>.Ok(page));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<APIResponse<DocumentDto>>> Get(long id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching document with Id {Id}", id);
        var value = await _service.GetAsync(id, cancellationToken);
        return value == null
            ? NotFound(APIResponse<DocumentDto>.Fail("Document not found."))
            : Ok(APIResponse<DocumentDto>.Ok(value));
    }

    [HttpPost("record/{refTableId:int}/{refRecId:long}")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(104_857_600)]
    public async Task<ActionResult<APIResponse<DocumentDto>>> Create(
        int refTableId,
        long refRecId,
        [FromForm] CreateDocumentForm form,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Attaching document to RefTableId {RefTableId}, RefRecId {RefRecId}", refTableId, refRecId);
        await using var stream = form.File?.OpenReadStream();
        var value = await _service.CreateAsync(new CreateDocumentCommand
        {
            RefTableId = refTableId,
            RefRecId = refRecId,
            TypeId = form.TypeId,
            Name = form.Name,
            Notes = form.Notes,
            Url = form.Url,
            FileName = form.File?.FileName,
            MimeType = form.File?.ContentType,
            FileSize = form.File?.Length ?? 0,
            Content = stream,
        }, cancellationToken);
        return Ok(APIResponse<DocumentDto>.Ok(value, "Document attached successfully."));
    }

    [HttpGet("{id:long}/download")]
    public async Task<IActionResult> Download(long id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Downloading document with Id {Id}", id);
        var content = await _service.OpenContentAsync(id, cancellationToken);
        return content == null
            ? NotFound()
            : File(content.Stream, content.MimeType, content.FileName, enableRangeProcessing: true);
    }

    [HttpGet("{id:long}/preview")]
    public async Task<IActionResult> Preview(long id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Previewing document with Id {Id}", id);
        var content = await _service.OpenContentAsync(id, cancellationToken);
        return content == null
            ? NotFound()
            : File(content.Stream, content.MimeType, enableRangeProcessing: true);
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<APIResponse<DocumentDto>>> Update(
        long id,
        [FromBody] UpdateDocumentRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating document with Id {Id}", id);
        var value = await _service.UpdateAsync(id, request, cancellationToken);
        return value == null
            ? NotFound(APIResponse<DocumentDto>.Fail("Document not found."))
            : Ok(APIResponse<DocumentDto>.Ok(value, "Document updated successfully."));
    }

    [HttpDelete("{id:long}")]
    public async Task<ActionResult<APIResponse<bool>>> Delete(long id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting document with Id {Id}", id);
        return await _service.DeleteAsync(id, cancellationToken)
            ? Ok(APIResponse<bool>.Ok(true, "Document deleted successfully."))
            : NotFound(APIResponse<bool>.Fail("Document not found."));
    }
}
