using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Administration.AuditLogs.Dtos;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Interfaces;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Administration.AuditLogs.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("System", "AuditLog")]
    public class SysAuditLogController : BaseController<SysAuditLog, SysAuditLogDto>
    {
        private readonly ISysAuditLogService _auditLogs;

        public SysAuditLogController(ISysAuditLogService service, ILogger<SysAuditLogController> logger)
            : base(service, logger)
        {
            _auditLogs = service;
        }

        // Audit records are append-only infrastructure output. They must never be created,
        // edited, or deleted through the generic CRUD surface.
        [NonAction]
        public override Task<ActionResult<APIResponse<SysAuditLogDto>>> Create(
            SysAuditLogDto dto,
            CancellationToken cancellationToken = default) =>
            base.Create(dto, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<SysAuditLogDto>>> Update(
            string id,
            SysAuditLogDto dto,
            CancellationToken cancellationToken = default) =>
            base.Update(id, dto, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<bool>>> Delete(
            string id,
            CancellationToken cancellationToken = default) =>
            base.Delete(id, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<IEnumerable<SysAuditLogDto>>>> CreateRange(
            IEnumerable<SysAuditLogDto> dtos,
            CancellationToken cancellationToken = default) =>
            base.CreateRange(dtos, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<IEnumerable<SysAuditLogDto>>>> UpdateRange(
            IEnumerable<SysAuditLogDto> dtos,
            CancellationToken cancellationToken = default) =>
            base.UpdateRange(dtos, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<bool>>> DeleteRange(
            IEnumerable<string> ids,
            CancellationToken cancellationToken = default) =>
            base.DeleteRange(ids, cancellationToken);

        /// <summary>
        /// Audit trail for a single record. Accepts either the physical DB table name
        /// or a logical/entity name (e.g. "ARSalesOrder") and resolves it to the actual
        /// table via EF model metadata, so it matches the table name the audit
        /// interceptor stores (which is the physical table, often pluralized).
        /// </summary>
        [HttpGet("by-record")]
        public async Task<ActionResult<APIResponse<IEnumerable<SysAuditLogDto>>>> GetByRecord(
            [FromQuery] string tableName,
            [FromQuery] string recordId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                return BadRequest(APIResponse<IEnumerable<SysAuditLogDto>>.Fail("tableName is required."));
            if (string.IsNullOrWhiteSpace(recordId))
                return BadRequest(APIResponse<IEnumerable<SysAuditLogDto>>.Fail("recordId is required."));

            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 1, 200);

            var (items, total) = await _auditLogs.GetByRecordAsync(
                tableName, recordId, pageNumber, pageSize, cancellationToken);

            var response = APIResponse<IEnumerable<SysAuditLogDto>>.Ok(items);
            response.Pagination = new PaginationMetadata(pageNumber, pageSize, total);
            return Ok(response);
        }
    }
}
