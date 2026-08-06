using IAX.IXApi.Shared.Application.Validation;
using FluentValidation;


namespace IAX.IXApi.Modules.Workflow.Activities
{
    public class WfActivityDtoValidator : BaseValidator<WfActivityDto>
    {
        public WfActivityDtoValidator()
        {
             RuleFor(x => x.NameAR).NotEmpty().WithMessage("Arabic Name is required");
             RuleFor(x => x.Name).NotEmpty().WithMessage("English Name is required");
             RuleFor(x => x.ActivityTypeId).GreaterThan((byte)0).WithMessage("Activity Type is required");
             RuleFor(x => x.StepId).GreaterThan(0).WithMessage("Step is required");
        }
    }
}


