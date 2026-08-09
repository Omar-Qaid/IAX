using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Application.Contracts;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Organization.Attachments
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class OrgAttachmentController : BaseController<OrgAttachment, OrgAttachmentDto>
    {
        private readonly IOrgAttachmentService _attachmentService;

        public OrgAttachmentController(IOrgAttachmentService service, ILogger<OrgAttachmentController> logger) : base(service, logger)
        {
            _attachmentService = service;
        }

        [HttpPost("{id}/upload")]
        public async Task<ActionResult<APIResponse<OrgAttachmentDetailDto>>> UploadFile(long id, IFormFile file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(APIResponse<OrgAttachmentDetailDto>.Fail("No file uploaded."));
            }

            _logger.LogInformation("Uploading file: {FileName} for Attachment ID: {Id}", file.FileName, id);

            using (var stream = file.OpenReadStream())
            {
                var detail = await _attachmentService.SaveFileAsync(id, file.FileName, file.ContentType, file.Length, stream, cancellationToken);
                var detailDto = detail.Adapt<OrgAttachmentDetailDto>();
                return Ok(APIResponse<OrgAttachmentDetailDto>.Ok(detailDto, "File uploaded successfully"));
            }
        }

        [HttpDelete("file/{fileId}")]
        public async Task<ActionResult<APIResponse<bool>>> DeleteFile(long fileId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting Attachment File with ID: {FileId}", fileId);
            await _attachmentService.DeleteFileAsync(fileId, cancellationToken);
            return Ok(APIResponse<bool>.Ok(true, "File deleted successfully"));
        }

        protected override string[]? GetDefaultIncludes()
        {
            return new[] { "Details" };
        }
    }
}
