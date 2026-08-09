using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class ExchangeRateTypeDtoValidator : AbstractValidator<ExchangeRateTypeDto>
    {
        public ExchangeRateTypeDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(FieldLengths.Name);
            RuleFor(x => x.Description).NotEmpty().MaximumLength(FieldLengths.Description);
        }
    }
}