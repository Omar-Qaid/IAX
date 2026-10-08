using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed record InventSiteUpdateValidation(string ExistingSiteId, SiteInputDto Input);

public sealed class InventSiteUpdateValidator : BaseValidator<InventSiteUpdateValidation>
{
    public InventSiteUpdateValidator()
    {
        RuleFor(request => request.Input.SiteId)
            .Must((request, siteId) => string.Equals(request.ExistingSiteId, siteId.Trim(), StringComparison.OrdinalIgnoreCase))
            .WithMessage("Site ID cannot be changed after creation.");
    }
}
