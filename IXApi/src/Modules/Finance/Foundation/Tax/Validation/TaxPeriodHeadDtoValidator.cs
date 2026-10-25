using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax.Validation
{
    public class TaxPeriodHeadDtoValidator : BaseValidator<TaxPeriodHeadDto>
    {
        public TaxPeriodHeadDtoValidator()
        {
            RuleFor(x => x.TaxPeriod).NotEmpty().MaximumLength(FieldLengths.TaxPeriod);
            RuleFor(x => x.Name).MaximumLength(FieldLengths.Name);
        }
    }
}
