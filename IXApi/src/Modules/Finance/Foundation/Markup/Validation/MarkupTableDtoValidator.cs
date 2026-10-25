using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Foundation.Markup;

public sealed class MarkupTableDtoValidator : BaseValidator<MarkupTableDto>
{
    public MarkupTableDtoValidator()
    {
        RuleFor(x => x.MarkupCode).NotEmpty().MaximumLength(FieldLengths.MarkupCode);
        RuleFor(x => x.Txt).NotEmpty().MaximumLength(FieldLengths.Txt);
    }
}
