using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Administration.NumberSequences
{
    public class SysNumberSequenceDtoValidator : BaseValidator<SysNumberSequenceDto>
    {
        public SysNumberSequenceDtoValidator()
        {
           
            RuleFor(x => x.EntityName).NotEmpty().WithMessage("EntityName is required").MaximumLength(150);
            RuleFor(x => x.FormatPattern).NotEmpty().WithMessage("FormatPattern is required");
            RuleFor(x => x.SmallestValue).GreaterThanOrEqualTo(0);
            RuleFor(x => x.LargestValue).GreaterThan(x => x.SmallestValue)
                .WithMessage("LargestValue must be greater than SmallestValue");
            RuleFor(x => x.Step).GreaterThan(0);
            RuleFor(x => x.PaddingLength).InclusiveBetween(0, 20);
        }
    }
}
