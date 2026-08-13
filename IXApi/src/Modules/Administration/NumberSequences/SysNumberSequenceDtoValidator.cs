using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Administration.NumberSequences
{
    public class SysNumberSequenceDtoValidator : BaseValidator<SysNumberSequenceDto>
    {
        public SysNumberSequenceDtoValidator()
        {
            RuleFor(x => x.NumberSequence).NotEmpty().WithMessage("NumberSequence is required").MaximumLength(22);
            RuleFor(x => x.Txt).NotEmpty().WithMessage("Txt is required").MaximumLength(100);
            RuleFor(x => x.Format).NotEmpty().WithMessage("Format is required").MaximumLength(20);
            RuleFor(x => x.AnnotatedFormat).NotEmpty().WithMessage("AnnotatedFormat is required").MaximumLength(100);
            
            RuleFor(x => x.Lowest).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Highest).GreaterThan(x => x.Lowest)
                .WithMessage("Highest must be greater than Lowest");
        }
    }
}
