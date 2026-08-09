using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class ExchangeRateCurrencyPairDtoValidator : AbstractValidator<ExchangeRateCurrencyPairDto>
    {
        public ExchangeRateCurrencyPairDtoValidator()
        {
            RuleFor(x => x.FromCurrencyCode).NotEmpty().MaximumLength(FieldLengths.FromCurrencyCode);
            RuleFor(x => x.ToCurrencyCode).NotEmpty().MaximumLength(FieldLengths.ToCurrencyCode);
        }
    }
}