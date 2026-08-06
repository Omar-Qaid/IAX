using IAX.IXApi.Shared.Application.Validation;
using FluentValidation;


namespace IAX.IXApi.Modules.Workflow.Variables
{
    public class WfVariableDtoValidator : BaseValidator<WfVariableDto>
    {
        public WfVariableDtoValidator()
        {
             //RuleFor(x => x.NameAR).NotEmpty().WithMessage("Arabic Name is required");
             //RuleFor(x => x.NameEn).NotEmpty().WithMessage("English Name is required");
        }
    }
}


