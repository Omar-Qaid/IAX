using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Foundation.Currency
{
    public class ExchangeRateDtoValidator : BaseValidator<ExchangeRateDto>
    {
        public ExchangeRateDtoValidator()
        {
            RuleFor(x => x.ExchangeRateValue).GreaterThan(0);
        }
    }
}
