using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Workflow.ProcessTypes
{
    public class WfProcessTypeDtoValidator : BaseValidator<WfProcessTypeDto>
    {
        public WfProcessTypeDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("English Name is required");
        }
    }
}
