using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Administration.AuditLogs.Services
{
    public interface ISysAuditLogService : IBaseService<SysAuditLog>
    {
        Task<(IReadOnlyList<SysAuditLogDto> Items, int TotalCount)> GetByRecordAsync(
            string tableName, string recordId, int pageNumber, int pageSize,
            CancellationToken cancellationToken = default);
    }
}

