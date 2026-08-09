using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Shared.Features;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class DlvTermDtoValidator : AbstractValidator<DlvTermDto>
    {
        public DlvTermDtoValidator()
        {
            RuleFor(x => x.Code).NotEmpty().MaximumLength(FieldLengths.Code);
            RuleFor(x => x.Txt).NotEmpty().MaximumLength(FieldLengths.Txt);
        }
    }
}

