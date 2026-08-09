using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Shared.Application.Attributes;

namespace IAX.IXApi.Modules.Workflow.Variables
{
    public class WfDataTypeService : BaseService<WfDataType>, IWfDataTypeService
    {
        public WfDataTypeService(IUnitOfWork unitOfWork, ICurrentUserService currentUser) : base(unitOfWork, currentUser)
        {
        }
    }
}

