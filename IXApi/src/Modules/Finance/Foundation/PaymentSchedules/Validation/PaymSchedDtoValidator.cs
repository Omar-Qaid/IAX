using IAX.IXApi.Shared.Application.Validation;
using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Foundation.PaymentSchedules
{
    public class PaymSchedDtoValidator : BaseValidator<PaymSchedDto>
    {
        public PaymSchedDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(FieldLengths.PaymSched);
            RuleFor(x => x.Description).MaximumLength(FieldLengths.Description);
        }
    }
}
