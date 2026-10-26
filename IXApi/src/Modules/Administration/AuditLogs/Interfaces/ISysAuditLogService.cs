using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Administration.AuditLogs.Dtos;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;

namespace IAX.IXApi.Modules.Administration.AuditLogs.Interfaces
{
    public interface ISysAuditLogService : IBaseService<SysAuditLog>
    {
        Task<(IReadOnlyList<SysAuditLogDto> Items, int TotalCount)> GetByRecordAsync(
            string tableName, string recordId, int pageNumber, int pageSize,
            CancellationToken cancellationToken = default);
    }
}
