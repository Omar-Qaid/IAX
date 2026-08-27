using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Organization.Features.HcmWorkerGroup;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using IAX.IXApi.Modules.Organization.DocumentManagement.Entities;

namespace IAX.IXApi.Modules.Organization.Persistence;

public interface IOrganizationDataContext
{
    DatabaseFacade Database { get; }
    DbSet<HcmWorkerGroup> HcmWorkerGroups { get; }
    DbSet<HcmWorkerGroupDetail> HcmWorkerGroupDetails { get; }
    DbSet<AspNetUser> Users { get; }
    DbSet<DocuType> DocuTypes { get; }
    DbSet<DocuValue> DocuValues { get; }
    DbSet<DocuRef> DocuRefs { get; }

    Task<long> CreateWorkerPartyAsync(
        string name,
        string nameAlias,
        string partyNumber,
        string createdBy,
        string ownerAccountId,
        CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
