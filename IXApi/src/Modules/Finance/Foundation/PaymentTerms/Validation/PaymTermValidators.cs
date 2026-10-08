using IAX.IXApi.Shared.Application.Validation;
using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Foundation.PaymentTerms
{
    public class PaymTermDtoValidator : BaseValidator<PaymTermDto>
    {
        public PaymTermDtoValidator()
        {
            RuleFor(x => x.PaymTermId).NotEmpty().MaximumLength(FieldLengths.PaymTermId);
            RuleFor(x => x.Description).NotEmpty().MaximumLength(FieldLengths.Description);
        }
    }
}
