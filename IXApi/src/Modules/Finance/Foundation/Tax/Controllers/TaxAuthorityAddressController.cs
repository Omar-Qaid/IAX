using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax.Controllers;

[ApiController]
[Route("api/v1/TaxAuthorityAddress")]
[Route("api/TaxAuthorityAddress")]
[DomainPermission("Tax", "TaxAuthorities")]
public sealed class TaxAuthorityAddressController
    : BaseController<TaxAuthorityAddress, TaxAuthorityAddressDto>
{
    public TaxAuthorityAddressController(
        IBaseService<TaxAuthorityAddress> service,
        ILogger<TaxAuthorityAddressController> logger)
        : base(service, logger)
    {
    }

    protected override Task OnBeforeCreateAsync(TaxAuthorityAddress entity)
    {
        Normalize(entity);
        return Task.CompletedTask;
    }

    protected override Task OnBeforeUpdateAsync(TaxAuthorityAddress entity)
    {
        Normalize(entity);
        return Task.CompletedTask;
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
