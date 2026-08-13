using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Workflow.Performers
{
    public class WfPerformerTypeDtoValidator : BaseValidator<WfPerformerTypeDto>
    {
        public WfPerformerTypeDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("English Name is required");
        }
    }
}
