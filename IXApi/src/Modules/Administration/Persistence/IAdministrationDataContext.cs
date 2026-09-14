using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace IAX.IXApi.Modules.Administration.Persistence;

public interface IAdministrationDataContext
{
    IModel Model { get; }
    Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade Database { get; }
    DbSet<BatchSettings> BatchSettings { get; }
    DbSet<SysAuditLog> SysAuditLogs { get; }
    DbSet<SysBackgroundJob> SysBackgroundJobs { get; }
    DbSet<SysBackgroundJobExecution> SysBackgroundJobExecutions { get; }
    DbSet<SysBackgroundJobTask> SysBackgroundJobTasks { get; }
    DbSet<SysBackgroundJobTaskExecution> SysBackgroundJobTaskExecutions { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
