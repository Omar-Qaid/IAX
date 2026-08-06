using IAX.IXApi.Shared.Application.Validation;
using FluentValidation;

namespace IAX.IXApi.Modules.Workflow.Activities
{
    public class WfActivityControlsValidationDtoValidator : BaseValidator<WfActivityControlsValidationDto>
    {
        public WfActivityControlsValidationDtoValidator()
        {
            RuleFor(x => x.ActivityControlId).GreaterThan(0).WithMessage("Activity Control ID is required");
            RuleFor(x => x.ValidationType).NotEmpty().WithMessage("Validation Type is required");
            RuleFor(x => x.ErrorMessageAr).NotEmpty().WithMessage("Arabic error message is required");
            RuleFor(x => x.ErrorMessageEn).NotEmpty().WithMessage("English error message is required");
            RuleFor(x => x.Severity).NotEmpty().WithMessage("Severity is required");
        }
    }
}
