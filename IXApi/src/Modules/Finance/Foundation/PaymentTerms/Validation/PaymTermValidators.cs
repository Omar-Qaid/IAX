using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Shared.Features;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class PaymTermDtoValidator : AbstractValidator<PaymTermDto>
    {
        public PaymTermDtoValidator()
        {
            RuleFor(x => x.PaymTermId).NotEmpty().MaximumLength(FieldLengths.PaymTermId);
            RuleFor(x => x.Description).NotEmpty().MaximumLength(FieldLengths.Description);
        }
    }
}

