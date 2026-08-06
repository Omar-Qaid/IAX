using IAX.IXApi.Shared.Application.Validation;
using FluentValidation;

namespace IAX.IXApi.Modules.Workflow.Performers
{
    public class WfPerformerDtoValidator : BaseValidator<WfPerformerDto>
    {
        public WfPerformerDtoValidator()
        {
            RuleFor(x => x.NameAR).NotEmpty().WithMessage("Arabic Name is required");
            RuleFor(x => x.Name).NotEmpty().WithMessage("English Name is required");
        }
    }
}
