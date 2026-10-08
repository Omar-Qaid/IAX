using IAX.IXApi.Shared.Application.Validation;
using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Foundation.Currency
{
    public class ExchangeRateTypeDtoValidator : BaseValidator<ExchangeRateTypeDto>
    {
        public ExchangeRateTypeDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(FieldLengths.Name);
            RuleFor(x => x.Description).NotEmpty().MaximumLength(FieldLengths.Description);
        }
    }
}
