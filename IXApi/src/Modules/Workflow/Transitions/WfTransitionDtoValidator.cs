using IAX.IXApi.Shared.Application.Validation;
using FluentValidation;


namespace IAX.IXApi.Modules.Workflow.Transitions
{
    public class WfTransitionDtoValidator : BaseValidator<WfTransitionDto>
    {
        public WfTransitionDtoValidator()
        {
             RuleFor(x => x.ProcessId).GreaterThan(0).WithMessage("Process ID is required");
             RuleFor(x => x.VariableId).GreaterThan(0).WithMessage("Variable ID is required");
             RuleFor(x => x.OperatorId).GreaterThan((byte)0).WithMessage("Operator ID is required");
             RuleFor(x => x.StepId).GreaterThan(0).WithMessage("Step ID is required");
             // Operators such as IsEmpty intentionally persist an empty comparison value.
             RuleFor(x => x.Value).MaximumLength(255).WithMessage("Value cannot exceed 255 characters");
        }
    }
}


