using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.PostingProfile;

public sealed class CustLedgerService : BaseService<CustLedger>, ICustLedgerService
{
    public CustLedgerService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        : base(unitOfWork, currentUser) { }

    public override async Task RemoveAsync(CustLedger entity, CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.Context.Set<CustLedgerAccounts>().AnyAsync(x =>
            x.PostingProfile == entity.PostingProfile, cancellationToken))
            throw new ValidationException("Remove the posting profile's setup rules before deleting the profile.");
        if (await _unitOfWork.Context.Set<CustTrans>().AnyAsync(x => x.DataAreaId == entity.DataAreaId
            && x.PostingProfile == entity.PostingProfile, cancellationToken))
            throw new ValidationException("A posting profile used by customer transactions cannot be deleted.");
        if (await _unitOfWork.Context.Set<CustParameters>().AnyAsync(x => x.DataAreaId == entity.DataAreaId
            && (x.CustPostingProfile == entity.PostingProfile || x.PrepaymentPostingProfile == entity.PostingProfile), cancellationToken))
            throw new ValidationException("Change the default customer posting profile before deleting this profile.");
        await base.RemoveAsync(entity, cancellationToken);
    }
}
