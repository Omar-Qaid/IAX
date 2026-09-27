using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Persistence;
using IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments;
using IAX.IXApi.Modules.Finance.Foundation.WorkerShowroomAssignments;
using IAX.IXApi.Modules.Finance.Foundation.Occupations;
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

    public override async Task<IEnumerable<HcmWorker>> GetAllAsync(
        string[] includes,
        CancellationToken cancellationToken = default)
    {
        IQueryable<HcmWorker> query = _dbContext.HcmWorkers.AsNoTracking();
        foreach (var include in includes.Where(value => !string.IsNullOrWhiteSpace(value)))
            query = query.Include(include);
        return await query.AsSplitQuery().ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<HcmWorkerDto>> GetWorkerListAsync(
        CancellationToken cancellationToken = default)
    {
        var workers = await _dbContext.HcmWorkers
            .AsNoTracking()
            .Select(worker => new HcmWorkerDto
            {
                RecId = worker.RecId,
                PersonnelNumber = worker.PersonnelNumber,
                Person = worker.Person,
                Name = worker.Party.Name,
                NameAlias = worker.Party.NameAlias,
                OccupationId = worker.OccupationId,
                OccupationName = worker.Occupation.Name,
                GenderId = worker.GenderId,
                GenderName = worker.Gender.Name,
                NationalityId = worker.NationalityId,
                NationalityName = worker.Nationality.Name,
                HireDate = worker.HireDate,
                BirthDate = worker.BirthDate,
                UserId = worker.UserId,
                IsActive = worker.IsActive,
                DataAreaId = worker.DataAreaId
            })
            .ToListAsync(cancellationToken);

        var organizationAssignments = await _dbContext.HcmWorkerOrganizationAssignmentsV1
            .AsNoTracking()
            .Where(assignment => assignment.IsPrimary)
            .OrderByDescending(assignment => assignment.ValidFrom)
            .ThenByDescending(assignment => assignment.RecId)
            .Select(assignment => new
            {
                assignment.HcmWorkerId,
                assignment.HcmManagerWorkerId,
                assignment.DepartmentId
            })
            .ToListAsync(cancellationToken);
        var organizationByWorker = organizationAssignments
            .GroupBy(assignment => assignment.HcmWorkerId)
            .ToDictionary(group => group.Key, group => group.First());

        var showroomAssignments = await _dbContext.HcmWorkerShowroomAssignments
            .AsNoTracking()
            .Where(assignment => assignment.IsPrimary)
            .OrderByDescending(assignment => assignment.ValidFrom)
            .ThenByDescending(assignment => assignment.RecId)
            .Select(assignment => new { assignment.HcmWorkerId, assignment.HcmShowroomId })
            .ToListAsync(cancellationToken);
        var showroomByWorker = showroomAssignments
            .GroupBy(assignment => assignment.HcmWorkerId)
            .ToDictionary(group => group.Key, group => group.First());

        foreach (var worker in workers)
        {
            if (organizationByWorker.TryGetValue(worker.RecId, out var organization))
            {
                worker.ManagerWorkerId = organization.HcmManagerWorkerId;
                worker.DepartmentId = organization.DepartmentId;
            }
            if (showroomByWorker.TryGetValue(worker.RecId, out var showroom))
                worker.ShowroomId = showroom.HcmShowroomId;
        }

        return workers;
    }

    public async Task<HcmWorkerLookupPageDto> GetWorkerLookupAsync(
        int pageNumber,
        int pageSize,
        string? search,
        long? selectedId,
        CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 10, 100);
        var query = _dbContext.HcmWorkers.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(worker =>
                worker.PersonnelNumber.Contains(term) ||
                worker.Party.Name.Contains(term) ||
                (worker.Party.NameAlias != null && worker.Party.NameAlias.Contains(term)));
        }

        var totalRecords = await query.CountAsync(cancellationToken);
        var page = await query
            .OrderByDescending(worker => selectedId.HasValue && worker.RecId == selectedId.Value)
            .ThenBy(worker => worker.PersonnelNumber)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(worker => new HcmWorkerLookupDto(
                worker.RecId,
                worker.PersonnelNumber,
                worker.Party.Name,
                worker.Party.NameAlias))
            .ToListAsync(cancellationToken);
        return new HcmWorkerLookupPageDto(
            page,
            pageNumber,
            Math.Max(1, (int)Math.Ceiling(totalRecords / (double)pageSize)),
            totalRecords);
    }

    public async Task<IReadOnlyList<HcmWorkerAssignmentChainNodeDto>> GetAssignmentChainAsync(
        long workerId,
        CancellationToken cancellationToken = default)
    {
        var chain = new List<HcmWorkerAssignmentChainNodeDto>();
        var visited = new HashSet<long>();
        var currentWorkerId = workerId;

        while (visited.Add(currentWorkerId) && chain.Count < 20)
        {
            var worker = await _dbContext.HcmWorkers
                .AsNoTracking()
                .Where(item => item.RecId == currentWorkerId)
                .Select(item => new HcmWorkerAssignmentChainNodeDto(
                    currentWorkerId == workerId ? "worker" : "manager",
                    item.RecId,
                    item.PersonnelNumber,
                    item.Party.Name,
                    item.Party.NameAlias,
                    item.Occupation.Name,
                    item.Occupation.NameAlias))
                .SingleOrDefaultAsync(cancellationToken);
            if (worker is null) break;
            chain.Add(worker);

            var managerId = await _dbContext.HcmWorkerOrganizationAssignmentsV1
                .AsNoTracking()
                .Where(assignment => assignment.HcmWorkerId == currentWorkerId && assignment.IsPrimary)
                .OrderByDescending(assignment => assignment.ValidFrom)
                .ThenByDescending(assignment => assignment.RecId)
                .Select(assignment => (long?)assignment.HcmManagerWorkerId)
                .FirstOrDefaultAsync(cancellationToken);
            if (managerId is null) break;
            currentWorkerId = managerId.Value;
        }

        chain.Reverse();
        var showroom = await _dbContext.HcmWorkerShowroomAssignments
            .AsNoTracking()
            .Where(assignment => assignment.HcmWorkerId == workerId && assignment.IsPrimary)
            .OrderByDescending(assignment => assignment.ValidFrom)
            .ThenByDescending(assignment => assignment.RecId)
            .Select(assignment => new HcmWorkerAssignmentChainNodeDto(
                "showroom",
                assignment.HcmShowroomId,
                assignment.HcmShowroomId.ToString(),
                assignment.HcmShowroom.PartyTable.Name,
                assignment.HcmShowroom.PartyTable.NameAlias,
                null,
                null))
            .FirstOrDefaultAsync(cancellationToken);
        if (showroom is not null) chain.Add(showroom);
        return chain;
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

    public async Task<IReadOnlyList<HcmWorkerOrganizationAssignmentV1Dto>> GetOrganizationAssignmentsV1Async(
        long workerId,
        CancellationToken cancellationToken = default) =>
        await _dbContext.HcmWorkerOrganizationAssignmentsV1
            .AsNoTracking()
            .Where(assignment => assignment.HcmWorkerId == workerId)
            .OrderByDescending(assignment => assignment.IsPrimary)
            .ThenByDescending(assignment => assignment.ValidFrom)
            .ThenByDescending(assignment => assignment.RecId)
            .Select(assignment => new HcmWorkerOrganizationAssignmentV1Dto(
                assignment.RecId,
                assignment.HcmManagerWorkerId,
                assignment.HcmManager.PersonnelNumber,
                assignment.HcmManager.Party.Name,
                assignment.HcmManager.Party.NameAlias,
                assignment.DepartmentId,
                assignment.Department != null ? assignment.Department.Name : null,
                assignment.Department != null ? assignment.Department.NameAlias : null,
                assignment.OccupationId,
                assignment.Occupation != null ? assignment.Occupation.Name : null,
                assignment.Occupation != null ? assignment.Occupation.NameAlias : null,
                assignment.ValidFrom,
                assignment.ValidTo,
                assignment.IsPrimary,
                assignment.IsActive))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<HcmWorkerShowroomAssignmentDto>> GetShowroomAssignmentsAsync(
        long workerId,
        CancellationToken cancellationToken = default) =>
        await _dbContext.HcmWorkerShowroomAssignments
            .AsNoTracking()
            .Where(assignment => assignment.HcmWorkerId == workerId)
            .OrderByDescending(assignment => assignment.IsPrimary)
            .ThenByDescending(assignment => assignment.ValidFrom)
            .ThenByDescending(assignment => assignment.RecId)
            .Select(assignment => new HcmWorkerShowroomAssignmentDto(
                assignment.RecId,
                assignment.HcmShowroomId,
                assignment.HcmShowroom.PartyTable.Name,
                assignment.HcmShowroom.PartyTable.NameAlias,
                assignment.ValidFrom,
                assignment.ValidTo,
                assignment.IsPrimary,
                assignment.IsActive))
            .ToListAsync(cancellationToken);

    public async Task SaveOrganizationAssignmentV1Async(
        long workerId,
        long? assignmentId,
        SaveHcmWorkerOrganizationAssignmentV1Request request,
        CancellationToken cancellationToken = default)
    {
        ValidateDates(request.ValidFrom, request.ValidTo);
        if (workerId == request.HcmManagerWorkerId)
            throw new InvalidOperationException("A worker cannot be their own manager.");

        var worker = await _dbContext.HcmWorkers.AsNoTracking()
            .SingleOrDefaultAsync(worker => worker.RecId == workerId && worker.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException("Worker not found.");
        _ = await _dbContext.HcmWorkers.AsNoTracking()
            .SingleOrDefaultAsync(worker => worker.RecId == request.HcmManagerWorkerId && worker.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException("Manager worker not found.");

        if (request.DepartmentId is short departmentId)
            _ = await _dbContext.HcmDepartments.AsNoTracking()
                .SingleOrDefaultAsync(department => department.RecId == departmentId && department.IsActive, cancellationToken)
                ?? throw new KeyNotFoundException("Department not found.");
        if (request.OccupationId is short occupationId)
            _ = await _dbContext.Set<HcmOccupation>().AsNoTracking()
                .SingleOrDefaultAsync(occupation => occupation.RecId == occupationId && occupation.IsActive, cancellationToken)
                ?? throw new KeyNotFoundException("Occupation not found.");

        HcmWorkerOrganizationAssignmentV1 assignment;
        if (assignmentId is long id)
        {
            assignment = await _dbContext.HcmWorkerOrganizationAssignmentsV1
                .SingleOrDefaultAsync(item => item.RecId == id && item.HcmWorkerId == workerId, cancellationToken)
                ?? throw new KeyNotFoundException("Organization assignment not found.");
        }
        else
        {
            assignment = new HcmWorkerOrganizationAssignmentV1
            {
                HcmWorkerId = workerId,
                DataAreaId = worker.DataAreaId
            };
            _dbContext.HcmWorkerOrganizationAssignmentsV1.Add(assignment);
        }

        assignment.HcmManagerWorkerId = request.HcmManagerWorkerId;
        assignment.DepartmentId = request.DepartmentId;
        assignment.OccupationId = request.OccupationId;
        assignment.ValidFrom = request.ValidFrom;
        assignment.ValidTo = request.ValidTo;
        assignment.IsPrimary = request.IsPrimary;
        assignment.IsActive = request.IsActive;
        if (request.IsPrimary)
        {
            var otherPrimaryAssignments = await _dbContext.HcmWorkerOrganizationAssignmentsV1
                .Where(item => item.HcmWorkerId == workerId && item.RecId != assignment.RecId && item.IsPrimary)
                .ToListAsync(cancellationToken);
            foreach (var other in otherPrimaryAssignments)
            {
                other.IsPrimary = false;
                if (assignmentId is null)
                {
                    other.IsActive = false;
                    if (request.ValidFrom > other.ValidFrom) other.ValidTo = request.ValidFrom;
                }
            }
        }
        else if (!await _dbContext.HcmWorkerOrganizationAssignmentsV1
                     .AnyAsync(item => item.HcmWorkerId == workerId && item.RecId != assignment.RecId && item.IsPrimary,
                         cancellationToken))
        {
            assignment.IsPrimary = true;
        }
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveShowroomAssignmentAsync(
        long workerId,
        long? assignmentId,
        SaveHcmWorkerShowroomAssignmentRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateDates(request.ValidFrom, request.ValidTo);
        var worker = await _dbContext.HcmWorkers.AsNoTracking()
            .SingleOrDefaultAsync(item => item.RecId == workerId && item.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException("Worker not found.");
        _ = await _dbContext.HcmShowrooms.AsNoTracking()
            .SingleOrDefaultAsync(showroom => showroom.RecId == request.HcmShowroomId && showroom.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException("Showroom not found.");

        HcmWorkerShowroomAssignment assignment;
        if (assignmentId is long id)
        {
            assignment = await _dbContext.HcmWorkerShowroomAssignments
                .SingleOrDefaultAsync(item => item.RecId == id && item.HcmWorkerId == workerId, cancellationToken)
                ?? throw new KeyNotFoundException("Showroom assignment not found.");
        }
        else
        {
            assignment = new HcmWorkerShowroomAssignment
            {
                HcmWorkerId = workerId,
                DataAreaId = worker.DataAreaId
            };
            _dbContext.HcmWorkerShowroomAssignments.Add(assignment);
        }

        assignment.HcmShowroomId = request.HcmShowroomId;
        assignment.ValidFrom = request.ValidFrom;
        assignment.ValidTo = request.ValidTo;
        assignment.IsPrimary = request.IsPrimary;
        assignment.IsActive = request.IsActive;
        if (request.IsPrimary)
        {
            var otherPrimaryAssignments = await _dbContext.HcmWorkerShowroomAssignments
                .Where(item => item.HcmWorkerId == workerId && item.RecId != assignment.RecId && item.IsPrimary)
                .ToListAsync(cancellationToken);
            foreach (var other in otherPrimaryAssignments)
            {
                other.IsPrimary = false;
                if (assignmentId is null)
                {
                    other.IsActive = false;
                    if (request.ValidFrom > other.ValidFrom) other.ValidTo = request.ValidFrom;
                }
            }
        }
        else if (!await _dbContext.HcmWorkerShowroomAssignments
                     .AnyAsync(item => item.HcmWorkerId == workerId && item.RecId != assignment.RecId && item.IsPrimary,
                         cancellationToken))
        {
            assignment.IsPrimary = true;
        }
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static void ValidateDates(DateOnly validFrom, DateOnly? validTo)
    {
        if (validFrom == default) throw new InvalidOperationException("Valid from is required.");
        if (validTo is not null && validTo <= validFrom)
            throw new InvalidOperationException("Valid to must be later than valid from.");
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
