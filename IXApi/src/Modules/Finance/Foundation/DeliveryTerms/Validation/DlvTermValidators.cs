using IAX.IXApi.Shared.Application.Validation;
using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Foundation.DeliveryTerms
{
    public class DlvTermDtoValidator : BaseValidator<DlvTermDto>
    {
        public DlvTermDtoValidator()
        {
            RuleFor(x => x.Code).NotEmpty().MaximumLength(FieldLengths.Code);
            RuleFor(x => x.Txt).NotEmpty().MaximumLength(FieldLengths.Txt);
        }
    }
}
