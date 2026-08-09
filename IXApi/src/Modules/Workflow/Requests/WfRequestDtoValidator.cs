using IAX.IXApi.Shared.Application.Validation;
using FluentValidation;


namespace IAX.IXApi.Modules.Workflow.Requests
{
    public class WfRequestDtoValidator : BaseValidator<WfRequestDto>
    {
        public WfRequestDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Request Name is required");
            RuleFor(x => x.NameAR).NotEmpty().WithMessage("Request Name is required");
            RuleFor(x => x.ProcessId).GreaterThan(0).WithMessage("Process ID is required");
            RuleFor(x => x.RequestDate).NotEmpty();
        }
    }
}


