using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Foundation.Dimensions.AccountStructure.Validation;

/// <summary>
/// Shape of the request only. Whether the dimensions exist, whether a main account segment is
/// present and whether the structure overlaps another is decided by the service.
/// </summary>
public sealed class AccountStructureCreateDtoValidator : BaseValidator<AccountStructureCreateDto>
{
    public AccountStructureCreateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("An account structure name is required.")
            .MaximumLength(FieldLengths.Name);
        RuleFor(x => x.Description)
            .MaximumLength(FieldLengths.Description);
        RuleFor(x => x.Segments)
            .NotNull().WithMessage("An account structure needs at least one segment.")
            .Must(x => x.Count > 0).WithMessage("An account structure needs at least one segment.");
        RuleForEach(x => x.Segments).ChildRules(segment =>
        {
            segment.RuleFor(x => x.Dimension)
                .NotEmpty().WithMessage("A segment must name the dimension it constrains.")
                .MaximumLength(FieldLengths.Name);
            segment.RuleFor(x => x.AllowedValues)
                .NotEmpty().WithMessage("A segment must state the values it allows.");
        });
    }
}
