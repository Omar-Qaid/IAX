using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Workflow.Requests;

public sealed class WfRequestControlsOptionDtoValidator : BaseValidator<WfRequestControlsOptionDto>
{
    public WfRequestControlsOptionDtoValidator()
    {
        RuleFor(option => option.RequestControlId)
            .GreaterThan(0).WithMessage("Request Control ID is required");
        RuleFor(option => option.Value)
            .NotEmpty().WithMessage("Option value is required")
            .MaximumLength(1000);
        RuleFor(option => option.Name)
            .NotEmpty().WithMessage("Option name is required")
            .MaximumLength(500);
        RuleFor(option => option.NameAlias)
            .MaximumLength(255);
        RuleFor(option => option.SortOrder)
            .GreaterThanOrEqualTo(0);
    }
}
