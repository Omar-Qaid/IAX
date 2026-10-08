using IAX.IXApi.Shared.Application.Validation;
using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Foundation.Currency
{
    public class ExchangeRateCurrencyPairDtoValidator : BaseValidator<ExchangeRateCurrencyPairDto>
    {
        public ExchangeRateCurrencyPairDtoValidator()
        {
            RuleFor(x => x.FromCurrencyCode).NotEmpty().MaximumLength(FieldLengths.FromCurrencyCode);
            RuleFor(x => x.ToCurrencyCode).NotEmpty().MaximumLength(FieldLengths.ToCurrencyCode);
        }
    }
}
