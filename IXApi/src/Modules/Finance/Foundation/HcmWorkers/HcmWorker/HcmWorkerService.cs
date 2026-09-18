using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Persistence;
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

    public override async Task<HcmWorker> AddAsync(
        HcmWorker entity,
        CancellationToken cancellationToken = default)
    {
        async Task<HcmWorker> AddWorkerAsync()
        {
            var partyNumberResult = await _numberSequenceService.NextAsync(
                "DirPartyTable",
                cancellationToken: cancellationToken);
            var partyNumber = partyNumberResult.Code ?? Guid.NewGuid().ToString("N")[..20];
            var partyName = entity.Name.Trim();
            var partyAlias = string.IsNullOrWhiteSpace(entity.NameAlias)
                ? partyName
                : entity.NameAlias.Trim();

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
            party.HcmWorker = worker.RecId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return worker;
        }

        // BaseController already supplies an execution-strategy transaction. Keep that transaction
        // instead of nesting another one; direct service callers still receive atomic persistence.
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

    public override async Task<HcmWorker> UpdateAsync(
        HcmWorker entity,
        CancellationToken cancellationToken = default)
    {
        var party = await _dbContext.Set<DirPartyTable>()
            .SingleAsync(candidate => candidate.RecId == entity.Person, cancellationToken);
        party.Name = entity.Name.Trim();
        party.NameAlias = string.IsNullOrWhiteSpace(entity.NameAlias)
            ? party.Name
            : entity.NameAlias.Trim();
        party.HcmWorker = entity.RecId;
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