using FluentValidation;
using IAX.IXApi.Modules.ERP.Common;
using IAX.IXApi.Modules.ERP.Shared.Features;

namespace IAX.IXApi.Modules.ERP.Shared.Features
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
