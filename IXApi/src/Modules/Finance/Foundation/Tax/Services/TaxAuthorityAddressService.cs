using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax;

public sealed class TaxAuthorityAddressService : BaseService<TaxAuthorityAddress>, ITaxAuthorityAddressService
{
    public TaxAuthorityAddressService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        : base(unitOfWork, currentUser) { }

    public override Task<TaxAuthorityAddress> AddAsync(TaxAuthorityAddress entity, CancellationToken cancellationToken = default)
    {
        Normalize(entity);
        return base.AddAsync(entity, cancellationToken);
    }

    public override Task<TaxAuthorityAddress> UpdateAsync(TaxAuthorityAddress entity, CancellationToken cancellationToken = default)
    {
        Normalize(entity);
        return base.UpdateAsync(entity, cancellationToken);
    }

    private static void Normalize(TaxAuthorityAddress entity)
    {
        entity.TaxAuthority = entity.TaxAuthority.Trim().ToUpperInvariant();
        entity.Name = entity.Name.Trim();
        entity.TaxAuthorityId = entity.TaxAuthorityId.Trim().ToUpperInvariant();
        entity.AccountNum = entity.AccountNum.Trim();
        entity.Phone = entity.Phone.Trim();
        entity.Mobile = entity.Mobile.Trim();
        entity.Fax = entity.Fax.Trim();
        entity.Sms = entity.Sms.Trim();
        entity.Telex = entity.Telex.Trim();
        entity.Extension = entity.Extension.Trim();
        entity.Pager = entity.Pager.Trim();
        entity.Email = entity.Email.Trim();
        entity.Url = entity.Url.Trim();
        entity.Address = entity.Address.Trim();
    }
}
