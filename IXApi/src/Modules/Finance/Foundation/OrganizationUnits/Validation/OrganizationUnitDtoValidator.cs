using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Foundation.OrganizationUnits;

public sealed class OrganizationUnitDtoValidator : BaseValidator<OrganizationUnitDto>
{
    public OrganizationUnitDtoValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(60);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
    }
}
