using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Infrastructure.Identity;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class PaymSchedService : BaseService<PaymSched>, IPaymSchedService
    {
        public PaymSchedService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
            : base(unitOfWork, currentUser)
        {
        }
    }
}


