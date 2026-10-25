using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Foundation.Currency
{
    public class BulkExchangeRatePairDtoValidator : BaseValidator<BulkExchangeRatePairDto>
    {
        public BulkExchangeRatePairDtoValidator()
        {
            RuleFor(x => x.FromCurrencyCode).NotEmpty().MaximumLength(3);
            RuleFor(x => x.ToCurrencyCode).NotEmpty().MaximumLength(3);
        }
    }
}
