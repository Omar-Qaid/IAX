using IAX.IXApi.Shared.Application.Validation;
using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Foundation.DeliveryModes
{
    public class DlvModeDtoValidator : BaseValidator<DlvModeDto>
    {
        public DlvModeDtoValidator()
        {
            RuleFor(x => x.Code).NotEmpty().MaximumLength(FieldLengths.Code);
            RuleFor(x => x.Txt).NotEmpty().MaximumLength(FieldLengths.Txt);
            RuleFor(x => x.MarkupGroup).MaximumLength(FieldLengths.MarkupGroup);
            RuleFor(x => x.McrExpedite).MaximumLength(FieldLengths.McrExpedite);
        }
    }
}
