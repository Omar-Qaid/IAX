using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Organization.DocumentManagement.Entities;
using IAX.IXApi.Modules.Organization.DocumentManagement.Interfaces;

namespace IAX.IXApi.Modules.Organization.DocumentManagement.Services;

public class DocuTypeService : BaseService<DocuType>, IDocuTypeService
{
    public DocuTypeService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        : base(unitOfWork, currentUser)
    {
    }
}
