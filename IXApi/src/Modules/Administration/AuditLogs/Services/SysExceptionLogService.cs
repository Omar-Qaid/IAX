using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Interfaces;

namespace IAX.IXApi.Modules.Administration.AuditLogs.Services
{
    public class SysExceptionLogService : BaseService<SysExceptionLog>, ISysExceptionLogService
    {
        public SysExceptionLogService(IUnitOfWork unitOfWork, ICurrentUserService currentUser) : base(unitOfWork, currentUser)
        {
        }
    }
}
