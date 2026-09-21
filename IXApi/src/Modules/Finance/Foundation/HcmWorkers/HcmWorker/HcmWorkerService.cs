using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Persistence;
using IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;

public class HcmWorkerService : BaseService<HcmWorker>, IHcmWorkerService
{
    private readonly ISysNumberSequenceService _numberSequenceService;
    private readonly IFinanceDataContext _dbContext;

    public HcmWorkerService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        ISysNumberSequenceService numberSequenceService,
        IFinanceDataContext dbContext) : base(unitOfWork, currentUser)
    {
        _numberSequenceService = numberSequenceService;
        _dbContext = dbContext;
    }

    public async Task<HcmWorker> AddWorkerAsync(
        HcmWorker entity,
        string name,
        string? nameAlias,
        long? initialPositionId,
        CancellationToken cancellationToken = default)
    {
        async Task<HcmWorker> AddWorkerAsync()
        {
            var partyNumberResult = await _numberSequenceService.NextAsync(
                "DirPartyTable",
                cancellationToken: cancellationToken);
            var partyNumber = partyNumberResult.Code ?? Guid.NewGuid().ToString("N")[..20];
            var partyName = name.Trim();
            var partyAlias = string.IsNullOrWhiteSpace(nameAlias) ? partyName : nameAlias.Trim();

            var party = new DirPartyTable
            {
                Name = partyName,
                NameAlias = partyAlias,
                PartyNumber = partyNumber,
                LanguageId = "en-us",
                AddressBookNames = string.Empty,
                CreatedBy = _currentUser.GetCurrentUserId() ?? "sys",
                OwnerAccountId = _currentUser.GetOwnerAccountId() ?? "sys",
                IsActive = NoYes.Yes
            };
            _dbContext.Set<DirPartyTable>().Add(party);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var workerNumberResult = await _numberSequenceService.NextAsync(
                "HcmWorker",
                cancellationToken: cancellationToken);
            entity.PersonnelNumber = workerNumberResult.Code ?? Guid.NewGuid().ToString("N")[..20];
            entity.Person = party.RecId;

            var worker = await base.AddAsync(entity, cancellationToken);
            worker.Party = party;
            party.HcmWorker = worker.RecId;
            if (initialPositionId is long positionId && positionId > 0)
            {
                var position = await _dbContext.HcmPositions.SingleOrDefaultAsync(x => x.RecId == positionId && x.IsActive, cancellationToken)
                    ?? throw new KeyNotFoundException("Initial position not found.");
                var validFrom = DateOnly.FromDateTime(entity.HireDate ?? DateTime.UtcNow);
                if (position.ValidFrom > validFrom || (position.ValidTo is not null && validFrom >= position.ValidTo))
                    throw new InvalidOperationException("Initial assignment date must be within the selected position period.");
                _dbContext.HcmWorkerOrganizationAssignments.Add(new HcmWorkerOrganizationAssignment { DataAreaId = entity.DataAreaId, HcmWorkerId = worker.RecId, PositionId = position.RecId, OrganizationUnitId = position.OrganizationUnitId, OrganizationRoleId = position.RoleId, AssignmentRole = 1, IsPrimary = true, IsActive = true, ValidFrom = validFrom });
            }
            await _dbContext.SaveChangesAsync(cancellationToken);
            return worker;
        }

        // Reuse an ambient transaction when one exists; otherwise keep party, worker, and initial
        // assignment creation atomic for direct service and controller callers.
        if (_dbContext.Database.CurrentTransaction is not null)
            return await AddWorkerAsync();

        var strategy = _dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var worker = await AddWorkerAsync();
                await transaction.CommitAsync(cancellationToken);
                return worker;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    public async Task<HcmWorker> UpdateWorkerAsync(
        HcmWorker entity,
        string name,
        string? nameAlias,
        CancellationToken cancellationToken = default)
    {
        var party = await _dbContext.Set<DirPartyTable>()
            .SingleAsync(candidate => candidate.RecId == entity.Person, cancellationToken);
        party.Name = name.Trim();
        party.NameAlias = string.IsNullOrWhiteSpace(nameAlias)
            ? party.Name
            : nameAlias.Trim();
        party.HcmWorker = entity.RecId;
        entity.Party = party;
        return await base.UpdateAsync(entity, cancellationToken);
    }

    public override async Task RemoveAsync(
        HcmWorker entity,
        CancellationToken cancellationToken = default)
    {
        await CleanupManagerLinksAsync([entity.RecId], cancellationToken);
        await base.RemoveAsync(entity, cancellationToken);
    }

    public override async Task RemoveRangeAsync(
        IEnumerable<HcmWorker> entities,
        CancellationToken cancellationToken = default)
    {
        await CleanupManagerLinksAsync(entities.Select(entity => entity.RecId), cancellationToken);
        await base.RemoveRangeAsync(entities, cancellationToken);
    }

    private static Task CleanupManagerLinksAsync(
        IEnumerable<long> employeeIds,
        CancellationToken cancellationToken)
    {
        _ = employeeIds;
        _ = cancellationToken;
        return Task.CompletedTask;
    }
}
