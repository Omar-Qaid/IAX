using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax.Validation
{
    public class TaxTableDtoValidator : BaseValidator<TaxTableDto>
    {
        public TaxTableDtoValidator()
        {
            RuleFor(x => x.TaxCode).NotEmpty().MaximumLength(FieldLengths.TaxCode);
            RuleFor(x => x.TaxName).NotEmpty().MaximumLength(FieldLengths.TaxName);
        }
    }
}
