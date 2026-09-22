using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Persistence;
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
}
