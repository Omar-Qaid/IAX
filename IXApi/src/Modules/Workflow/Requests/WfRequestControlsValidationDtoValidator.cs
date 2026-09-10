using IAX.IXApi.Shared.Application.Validation;
using FluentValidation;

namespace IAX.IXApi.Modules.Workflow.Requests
{
    public class WfRequestControlsValidationDtoValidator : BaseValidator<WfRequestControlsValidationDto>
    {
        public WfRequestControlsValidationDtoValidator()
        {
            RuleFor(x => x.RequestControlId).GreaterThan(0).WithMessage("Request Control ID is required");
            RuleFor(x => x.ValidationType).NotEmpty().WithMessage("Validation Type is required");
            RuleFor(x => x.ErrorMessage).NotEmpty().WithMessage("Error message is required");
            RuleFor(x => x.Severity).NotEmpty().WithMessage("Severity is required");
            RuleFor(x => x.Value).Must(value => DateBoundRule.Resolve(value, DateOnly.FromDateTime(DateTime.UtcNow)).HasValue)
                .When(x => string.Equals(x.ValidationType, "minDate", StringComparison.OrdinalIgnoreCase) || string.Equals(x.ValidationType, "maxDate", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Use yyyy-MM-dd, today, or today followed by an offset such as +1y, +6m or -30d (UTC).");
        }
    }
}
