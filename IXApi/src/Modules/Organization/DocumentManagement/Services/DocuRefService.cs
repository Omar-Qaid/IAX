using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Organization.DocumentManagement.Entities;
using IAX.IXApi.Modules.Organization.DocumentManagement.Interfaces;

namespace IAX.IXApi.Modules.Organization.DocumentManagement.Services;

public class DocuRefService : BaseService<DocuRef>, IDocuRefService
{
    public DocuRefService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        : base(unitOfWork, currentUser)
    {
    }
}
