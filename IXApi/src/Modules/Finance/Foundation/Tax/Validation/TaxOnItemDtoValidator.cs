using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax.Validation
{
    public class TaxOnItemDtoValidator : BaseValidator<TaxOnItemDto>
    {
        public TaxOnItemDtoValidator()
        {
            RuleFor(x => x.TaxCode).NotEmpty().MaximumLength(FieldLengths.TaxCode);
        }
    }
}
