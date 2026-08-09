using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Modules.Organization.EmployeeManagers;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using Microsoft.EntityFrameworkCore;

using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Organization.Persistence;

namespace IAX.IXApi.Modules.Organization.Employees
{
    public class HcmWorkerService : BaseService<HcmWorker>, IHcmWorkerService
    {
        private readonly ISysNumberSequenceService _numberSequenceService;
        private readonly IOrganizationDataContext _dbContext;

        public HcmWorkerService(
            IUnitOfWork unitOfWork, 
            ICurrentUserService currentUser,
            ISysNumberSequenceService numberSequenceService,
            IOrganizationDataContext dbContext) : base(unitOfWork, currentUser)
        {
            _numberSequenceService = numberSequenceService;
            _dbContext = dbContext;
        }

        public override async Task<HcmWorker> AddAsync(HcmWorker entity, CancellationToken cancellationToken = default)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // 1. Save DirPartyTable
                    var partyNsResult = await _numberSequenceService.NextAsync("DirPartyTable", cancellationToken: cancellationToken);
                    string partyNumber = partyNsResult.Code ?? Guid.NewGuid().ToString("N").Substring(0, 20);

                    var partyId = await _dbContext.CreateWorkerPartyAsync(
                        "New Worker",
                        "New Worker",
                        partyNumber,
                        _currentUser.GetCurrentUserId() ?? "sys",
                        _currentUser.GetOwnerAccountId() ?? "sys",
                        cancellationToken);

                    // 2. Setup Worker
                    var workerNsResult = await _numberSequenceService.NextAsync("HcmWorker", cancellationToken: cancellationToken);
                    string personnelNumber = workerNsResult.Code ?? Guid.NewGuid().ToString("N").Substring(0, 20);

                    entity.PersonnelNumber = personnelNumber;
                    entity.Person = partyId;
                    
                    var addedWorker = await base.AddAsync(entity, cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    return addedWorker;
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            });
        }

        // When an employee is removed, also drop their management links in BOTH directions â€” rows
        // where they are the employee AND rows where they are someone's manager â€” so no dangling
        // manager chains are left pointing at a deleted person. Cleanup is tracked on the same unit
        // of work, so it persists atomically with the employee removal.
        public override async Task RemoveAsync(HcmWorker entity, CancellationToken cancellationToken = default)
        {
            await CleanupManagerLinksAsync(new[] { entity.RecId }, cancellationToken);
            await base.RemoveAsync(entity, cancellationToken);
        }

        public override async Task RemoveRangeAsync(IEnumerable<HcmWorker> entities, CancellationToken cancellationToken = default)
        {
            await CleanupManagerLinksAsync(entities.Select(e => e.RecId), cancellationToken);
            await base.RemoveRangeAsync(entities, cancellationToken);
        }

        private async Task CleanupManagerLinksAsync(IEnumerable<long> employeeIds, CancellationToken cancellationToken)
        {
            var ids = employeeIds.ToList();
            if (ids.Count == 0)
                return;

            var linkRepo = _unitOfWork.Repository<OrgEmployeeManager>();
            var links = await linkRepo.GetQueryable()
                .Where(x => ids.Contains(x.EmployeeId) || ids.Contains(x.ManagerId))
                .ToListAsync(cancellationToken);

            if (links.Count > 0)
                await linkRepo.RemoveRangeAsync(links);
        }
    }
}




