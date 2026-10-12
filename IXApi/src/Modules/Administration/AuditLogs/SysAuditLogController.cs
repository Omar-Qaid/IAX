using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Modules.Administration.Persistence;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Modules.Administration.AuditLogs.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Administration.AuditLogs
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
            if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(recordId))
                return Ok(APIResponse<IEnumerable<SysAuditLogDto>>.Ok(Array.Empty<SysAuditLogDto>()));

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 50;

            var (items, total) = await _auditLogs.GetByRecordAsync(
                tableName, recordId, pageNumber, pageSize, cancellationToken);

            var response = APIResponse<IEnumerable<SysAuditLogDto>>.Ok(items);
            response.Pagination = new PaginationMetadata(pageNumber, pageSize, total);
            return Ok(response);
        }

    }
}
