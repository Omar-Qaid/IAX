using IAX.IXApi.Shared.Application.Validation;
using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Foundation.Currency
{
    public class CurrencyDtoValidator : BaseValidator<CurrencyDto>
    {
        public CurrencyDtoValidator()
        {
            RuleFor(x => x.CurrencyCode).NotEmpty().MaximumLength(FieldLengths.CurrencyCode);
            RuleFor(x => x.CurrencyCodeIso).NotEmpty().MaximumLength(FieldLengths.CurrencyCodeIso);
            RuleFor(x => x.Txt).MaximumLength(FieldLengths.Txt);
            RuleFor(x => x.Symbol).MaximumLength(FieldLengths.Symbol);
        }
    }
}
