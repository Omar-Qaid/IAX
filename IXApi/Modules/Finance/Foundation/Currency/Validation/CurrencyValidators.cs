using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class CurrencyDtoValidator : AbstractValidator<CurrencyDto>
    {
        public CurrencyDtoValidator()
        {
            RuleFor(x => x.CurrencyCode).NotEmpty().MaximumLength(FieldLengths.CurrencyCode);
            RuleFor(x => x.CurrencyCodeIso).NotEmpty().MaximumLength(FieldLengths.CurrencyCodeIso);
            RuleFor(x => x.Txt).MaximumLength(FieldLengths.Txt);
            RuleFor(x => x.Symbol).MaximumLength(FieldLengths.Symbol);
        }
    }

    public class ExchangeRateCurrencyPairDtoValidator : AbstractValidator<ExchangeRateCurrencyPairDto>
    {
        public ExchangeRateCurrencyPairDtoValidator()
        {
            RuleFor(x => x.FromCurrencyCode).NotEmpty().MaximumLength(FieldLengths.FromCurrencyCode);
            RuleFor(x => x.ToCurrencyCode).NotEmpty().MaximumLength(FieldLengths.ToCurrencyCode);
        }
    }

    public class ExchangeRateTypeDtoValidator : AbstractValidator<ExchangeRateTypeDto>
    {
        public ExchangeRateTypeDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(FieldLengths.Name);
            RuleFor(x => x.Description).NotEmpty().MaximumLength(FieldLengths.Description);
        }
    }
}

