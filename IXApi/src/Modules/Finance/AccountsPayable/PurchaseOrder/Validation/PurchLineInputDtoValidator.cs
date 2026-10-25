using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public sealed class PurchLineInputDtoValidator : BaseValidator<PurchLineInputDto>
{
    public PurchLineInputDtoValidator()
    {
        RuleFor(x => x.PurchQty).GreaterThan(0)
            .WithMessage("The purchase quantity must be greater than zero.");
        RuleFor(x => x.PurchPrice).GreaterThanOrEqualTo(0)
            .WithMessage("The purchase price cannot be negative.");
    }
}
