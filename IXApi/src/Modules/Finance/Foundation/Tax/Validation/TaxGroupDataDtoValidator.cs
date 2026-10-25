using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax.Validation
{
    public class TaxGroupDataDtoValidator : BaseValidator<TaxGroupDataDto>
    {
        public TaxGroupDataDtoValidator()
        {
            RuleFor(x => x.TaxCode).NotEmpty().MaximumLength(FieldLengths.TaxCode);
        }
    }
}
