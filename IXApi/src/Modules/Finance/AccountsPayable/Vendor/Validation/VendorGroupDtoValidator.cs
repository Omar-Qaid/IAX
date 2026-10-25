using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public sealed class VendorGroupDtoValidator : BaseValidator<VendorGroupDto>
{
    public VendorGroupDtoValidator()
    {
        RuleFor(x => x.VendGroup).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(60);
    }
}
