using IAX.IXApi.Modules.Identity.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using IAX.IXApi.Modules.Organization.DocumentManagement.Entities;


namespace IAX.IXApi.Modules.Organization.Persistence;

public interface IOrganizationDataContext
{
    DatabaseFacade Database { get; }
    Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker ChangeTracker { get; }
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
