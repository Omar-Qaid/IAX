using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
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
        private readonly ApplicationDbContext _db;

        public SysAuditLogController(ISysAuditLogService service, ILogger<SysAuditLogController> logger, ApplicationDbContext db)
            : base(service, logger)
        {
            _db = db;
        }

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

            var resolved = ResolveTableName(tableName);

            var baseQuery = _db.SysAuditLogs.AsNoTracking()
                .Where(a => a.TableName == resolved && a.RecordId == recordId);

            var total = await baseQuery.CountAsync(cancellationToken);

            var items = await baseQuery
                .OrderByDescending(a => a.ChangedOn)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new SysAuditLogDto
                {
                    RecId = a.RecId,
                    TableName = a.TableName,
                    RecordId = a.RecordId,
                    ColumnName = a.ColumnName,
                    OldValue = a.OldValue,
                    NewValue = a.NewValue,
                    Operation = a.Operation,
                    ChangedBy = a.ChangedBy,
                    ChangedOn = a.ChangedOn,
                })
                .ToListAsync(cancellationToken);

            var response = APIResponse<IEnumerable<SysAuditLogDto>>.Ok(items);
            response.Pagination = new PaginationMetadata(pageNumber, pageSize, total);
            return Ok(response);
        }

        /// <summary>
        /// Resolve a physical table name or a logical/entity class name to the physical
        /// table name. Falls back to the input when nothing matches.
        /// </summary>
        private string ResolveTableName(string name)
        {
            var entityTypes = _db.Model.GetEntityTypes();

            // Already a physical table name (e.g. opened from the Audit Log list).
            foreach (var et in entityTypes)
            {
                var table = et.GetTableName();
                if (!string.IsNullOrEmpty(table) && string.Equals(table, name, StringComparison.OrdinalIgnoreCase))
                    return table!;
            }

            // Logical/entity class name (e.g. "ARSalesOrder" -> table "ARSalesOrders").
            foreach (var et in entityTypes)
            {
                if (string.Equals(et.ClrType.Name, name, StringComparison.OrdinalIgnoreCase))
                    return et.GetTableName() ?? name;
            }

            return name;
        }
    }
}
