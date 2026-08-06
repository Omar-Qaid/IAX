using FluentValidation;
using IAX.IXApi.Modules.ERP.Common;
using IAX.IXApi.Modules.ERP.Shared.Features;

namespace IAX.IXApi.Modules.ERP.Shared.Features
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
