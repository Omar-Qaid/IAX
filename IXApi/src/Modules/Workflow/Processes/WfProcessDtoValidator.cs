using IAX.IXApi.Shared.Application.Validation;
using FluentValidation;


namespace IAX.IXApi.Modules.Workflow.Processes
{
    public class WfProcessDtoValidator : BaseValidator<WfProcessDto>
    {
        public WfProcessDtoValidator()
        {
             RuleFor(x => x.Name).NotEmpty().WithMessage("English Name is required");
        }
    }
}


