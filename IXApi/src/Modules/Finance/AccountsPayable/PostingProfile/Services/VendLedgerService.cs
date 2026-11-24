using FluentValidation;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public sealed class VendLedgerService : BaseService<VendLedger>, IVendLedgerService
{
    public VendLedgerService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        : base(unitOfWork, currentUser) { }

    public override async Task RemoveAsync(VendLedger entity, CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.Context.Set<VendLedgerAccounts>().AnyAsync(x => x.DataAreaId == entity.DataAreaId
            && x.PostingProfile == entity.PostingProfile, cancellationToken))
            throw new ValidationException("Remove the posting profile's setup rules before deleting the profile.");
        if (await _unitOfWork.Context.Set<VendParameters>().AnyAsync(x => x.DataAreaId == entity.DataAreaId
            && x.VendPostingProfile == entity.PostingProfile, cancellationToken))
            throw new ValidationException("Change the default vendor posting profile before deleting this profile.");
        await base.RemoveAsync(entity, cancellationToken);
    }
}
