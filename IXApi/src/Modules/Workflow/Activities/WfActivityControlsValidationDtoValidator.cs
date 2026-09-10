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
            RuleFor(x => x.ErrorMessage).NotEmpty().WithMessage("Error message is required");
            RuleFor(x => x.Severity).NotEmpty().WithMessage("Severity is required");
            RuleFor(x => x.Value).Must(value => IAX.IXApi.Modules.Workflow.Requests.DateBoundRule.Resolve(value, DateOnly.FromDateTime(DateTime.UtcNow)).HasValue)
                .When(x => string.Equals(x.ValidationType, "minDate", StringComparison.OrdinalIgnoreCase) || string.Equals(x.ValidationType, "maxDate", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Use yyyy-MM-dd, today, or today followed by an offset such as +1y, +6m or -30d (UTC).");
        }
    }
}
