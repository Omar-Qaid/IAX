using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Administration.AuditLogs.Dtos;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Interfaces;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Administration.AuditLogs.Services
{
    public class SysAuditLogService : BaseService<SysAuditLog>, ISysAuditLogService
    {
        public SysAuditLogService(IUnitOfWork unitOfWork, ICurrentUserService currentUser) : base(unitOfWork, currentUser)
        {
        }

        public async Task<(IReadOnlyList<SysAuditLogDto> Items, int TotalCount)> GetByRecordAsync(
            string tableName, string recordId, int pageNumber, int pageSize,
            CancellationToken cancellationToken = default)
        {
            var resolved = ResolveTableName(tableName);
            var query = _unitOfWork.Context.Set<SysAuditLog>().AsNoTracking()
                .Where(audit => audit.TableName == resolved && audit.RecordId == recordId);
            var total = await query.CountAsync(cancellationToken);
            var records = await query.OrderByDescending(audit => audit.ChangedOn)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
            return (records.Adapt<List<SysAuditLogDto>>(), total);
        }

        private string ResolveTableName(string name)
        {
            var entityTypes = _unitOfWork.Context.Model.GetEntityTypes();
            foreach (var entityType in entityTypes)
            {
                var table = entityType.GetTableName();
                if (!string.IsNullOrEmpty(table) && string.Equals(table, name, StringComparison.OrdinalIgnoreCase))
                    return table;
            }
            foreach (var entityType in entityTypes)
            {
                if (string.Equals(entityType.ClrType.Name, name, StringComparison.OrdinalIgnoreCase))
                    return entityType.GetTableName() ?? name;
            }
            return name;
        }
    }
}
