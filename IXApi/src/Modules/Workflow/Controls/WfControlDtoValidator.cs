using IAX.IXApi.Shared.Application.Validation;
using FluentValidation;


namespace IAX.IXApi.Modules.Workflow.Controls
{
    public class WfControlDtoValidator : BaseValidator<WfControlDto>
    {
        public WfControlDtoValidator()
        {
             RuleFor(x => x.NameAR).NotEmpty().WithMessage("Arabic Name is required");
             RuleFor(x => x.Name).NotEmpty().WithMessage("English Name is required");
             RuleFor(x => x.ControlType).NotEmpty().WithMessage("Control Type is required");
        }
    }
}


