using IAX.IXApi.Shared.Application.Validation;
using FluentValidation;


namespace IAX.IXApi.Modules.Workflow.Steps
{
    public class WfStepDtoValidator : BaseValidator<WfStepDto>
    {
        public WfStepDtoValidator()
        {
             //RuleFor(x => x.NameEn).NotEmpty().WithMessage("English Name is required");
        }
    }
}


