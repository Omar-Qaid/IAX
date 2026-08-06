using IAX.IXApi.Shared.Application.Validation;
using FluentValidation;


namespace IAX.IXApi.Modules.Workflow.Priorities
{
    public class WfPriorityDtoValidator : BaseValidator<WfPriorityDto>
    {
        public WfPriorityDtoValidator()
        {
             RuleFor(x => x.NameAR).NotEmpty().WithMessage("Arabic Name is required");
             RuleFor(x => x.Name).NotEmpty().WithMessage("English Name is required");
        }
    }
}


