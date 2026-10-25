using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventSiteDtoValidator : BaseValidator<InventSiteDto>
{
    public InventSiteDtoValidator()
    {
        RuleFor(x => x.SiteId)
            .NotEmpty().WithMessage("Site ID is required.")
            .MaximumLength(FieldLengths.InventSiteId).WithMessage($"Site ID cannot exceed {FieldLengths.InventSiteId} characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(FieldLengths.Name).WithMessage($"Name cannot exceed {FieldLengths.Name} characters.");
    }
}
