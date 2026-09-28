using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Persistence;
using IAX.IXApi.Modules.Finance.Foundation.WorkerShowroomAssignments;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Foundation.HcmShowrooms;

public class HcmShowroomService : BaseService<HcmShowroom>, IHcmShowroomService
{
    private readonly ISysNumberSequenceService _numberSequenceService;
    private readonly IFinanceDataContext _dbContext;

    public HcmShowroomService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        ISysNumberSequenceService numberSequenceService,
        IFinanceDataContext dbContext) : base(unitOfWork, currentUser)
    {
        _numberSequenceService = numberSequenceService;
        _dbContext = dbContext;
    }

    public async Task<HcmShowroom> AddShowroomAsync(
        HcmShowroom entity,
        string name,
        string? nameAlias,
        CancellationToken cancellationToken = default)
    {
        async Task<HcmShowroom> AddAsync()
        {
            var partyNumberResult = await _numberSequenceService.NextAsync(
                "DirPartyTable",
                cancellationToken: cancellationToken);
            var partyNumber = partyNumberResult.Code ?? Guid.NewGuid().ToString("N")[..20];
            var partyName = name.Trim();
            var partyAlias = string.IsNullOrWhiteSpace(nameAlias) ? partyName : nameAlias.Trim();

            if (string.IsNullOrWhiteSpace(entity.PersonnelNumber))
            {
                var showroomNumberResult = await _numberSequenceService.NextAsync(
                    "HcmShowroom",
                    cancellationToken: cancellationToken);
                entity.PersonnelNumber = showroomNumberResult.Code ?? Guid.NewGuid().ToString("N")[..20];
            }

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

            entity.Party = party.RecId;
            var showroom = await base.AddAsync(entity, cancellationToken);
            showroom.PartyTable = party;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return showroom;
        }

        if (_dbContext.Database.CurrentTransaction is not null)
            return await AddAsync();

        var strategy = _dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var showroom = await AddAsync();
                await transaction.CommitAsync(cancellationToken);
                return showroom;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    public async Task<HcmShowroom> UpdateShowroomAsync(
        HcmShowroom entity,
        string name,
        string? nameAlias,
        CancellationToken cancellationToken = default)
    {
        var party = await _dbContext.Set<DirPartyTable>()
            .SingleAsync(candidate => candidate.RecId == entity.Party, cancellationToken);
        party.Name = name.Trim();
        party.NameAlias = string.IsNullOrWhiteSpace(nameAlias)
            ? party.Name
            : nameAlias.Trim();
        entity.PartyTable = party;
        return await base.UpdateAsync(entity, cancellationToken);
    }

    public async Task<IReadOnlyList<HcmShowroomWorkerAssignmentDto>> GetWorkerAssignmentsAsync(
        long showroomId,
        CancellationToken cancellationToken = default) =>
        await _dbContext.HcmWorkerShowroomAssignments
            .AsNoTracking()
            .Where(assignment => assignment.HcmShowroomId == showroomId)
            .OrderByDescending(assignment => assignment.IsPrimary)
            .ThenByDescending(assignment => assignment.ValidFrom)
            .ThenByDescending(assignment => assignment.RecId)
            .Select(assignment => new HcmShowroomWorkerAssignmentDto(
                assignment.RecId,
                assignment.HcmWorkerId,
                assignment.HcmWorker.PersonnelNumber,
                assignment.HcmWorker.Party.Name,
                assignment.HcmWorker.Party.NameAlias,
                assignment.ValidFrom,
                assignment.ValidTo,
                assignment.IsPrimary,
                assignment.IsActive))
            .ToListAsync(cancellationToken);

    public async Task SaveWorkerAssignmentAsync(
        long showroomId,
        long? assignmentId,
        SaveHcmShowroomWorkerAssignmentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.ValidFrom == default)
            throw new InvalidOperationException("Valid from is required.");
        if (request.ValidTo is not null && request.ValidTo <= request.ValidFrom)
            throw new InvalidOperationException("Valid to must be later than valid from.");

        var showroom = await _dbContext.HcmShowrooms.AsNoTracking()
            .SingleOrDefaultAsync(item => item.RecId == showroomId && item.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException("Showroom not found.");
        _ = await _dbContext.HcmWorkers.AsNoTracking()
            .SingleOrDefaultAsync(item => item.RecId == request.HcmWorkerId && item.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException("Worker not found.");

        HcmWorkerShowroomAssignment assignment;
        if (assignmentId is long id)
        {
            assignment = await _dbContext.HcmWorkerShowroomAssignments
                .SingleOrDefaultAsync(item => item.RecId == id && item.HcmShowroomId == showroomId, cancellationToken)
                ?? throw new KeyNotFoundException("Showroom assignment not found.");
        }
        else
        {
            assignment = new HcmWorkerShowroomAssignment
            {
                HcmShowroomId = showroomId,
                HcmWorkerId = request.HcmWorkerId,
                DataAreaId = showroom.DataAreaId
            };
            _dbContext.HcmWorkerShowroomAssignments.Add(assignment);
        }

        assignment.HcmWorkerId = request.HcmWorkerId;
        assignment.ValidFrom = request.ValidFrom;
        assignment.ValidTo = request.ValidTo;
        assignment.IsPrimary = true;
        assignment.IsActive = request.IsActive;

        var previousPrimary = await _dbContext.HcmWorkerShowroomAssignments
            .Where(item => item.HcmWorkerId == request.HcmWorkerId && item.RecId != assignment.RecId && item.IsPrimary)
            .ToListAsync(cancellationToken);
        foreach (var previous in previousPrimary)
        {
            previous.IsPrimary = false;
            if (assignmentId is null)
            {
                previous.IsActive = false;
                if (request.ValidFrom > previous.ValidFrom) previous.ValidTo = request.ValidFrom;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
