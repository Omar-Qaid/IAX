using IAX.IXApi.Shared.Application.Validation;
using FluentValidation;


namespace IAX.IXApi.Modules.Workflow.Activities
{
    public class WfActivityTypeDtoValidator : BaseValidator<WfActivityTypeDto>
    {
        public WfActivityTypeDtoValidator()
        {
             RuleFor(x => x.Name).NotEmpty().WithMessage("English Name is required");
        }
    }
}


