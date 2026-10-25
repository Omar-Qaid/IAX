using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public sealed class PurchOrderCreateDtoValidator : BaseValidator<PurchOrderCreateDto>
{
    public PurchOrderCreateDtoValidator()
    {
        RuleFor(x => x).Must(x => !string.IsNullOrWhiteSpace(x.InventSiteId)
                && !string.IsNullOrWhiteSpace(x.InventLocationId))
            .WithMessage("Site and warehouse are required.");
    }
}
