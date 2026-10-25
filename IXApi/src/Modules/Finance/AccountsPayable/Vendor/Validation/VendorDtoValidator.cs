using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public sealed class VendorDtoValidator : BaseValidator<VendorDto>
{
    public VendorDtoValidator()
    {
        RuleFor(x => x.AccountNum).NotEmpty().MaximumLength(20);
        RuleFor(x => x.VendGroup).NotEmpty().MaximumLength(10);
    }
}
