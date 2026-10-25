using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax.Validation
{
    public class TaxExemptCodeDtoValidator : BaseValidator<TaxExemptCodeDto>
    {
        public TaxExemptCodeDtoValidator()
        {
            RuleFor(x => x.ExemptCode).NotEmpty();
        }
    }
}
