using IAX.IXApi.Modules.Communication.Notifications.Entities;
using IAX.IXApi.Modules.Identity.Roles;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Organization.Features.HcmWorkerGroup;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace IAX.IXApi.Modules.Communication.Persistence;

public interface ICommunicationDataContext
{
    DatabaseFacade Database { get; }
    DbSet<SysNotificationAuditLog> SysNotificationAuditLogs { get; }
    DbSet<AspNetUserRole> UserRoles { get; }
    DbSet<AspNetRole> Roles { get; }
    DbSet<HcmWorker> HcmWorkers { get; }
    DbSet<AspNetUser> AspNetUser { get; }
    DbSet<HcmWorkerGroupDetail> HcmWorkerGroupDetails { get; }
    DbSet<TEntity> Set<TEntity>() where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
