using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Shared.Features;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class DlvModeDtoValidator : AbstractValidator<DlvModeDto>
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

