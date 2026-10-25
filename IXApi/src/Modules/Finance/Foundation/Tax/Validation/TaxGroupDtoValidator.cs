using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax.Validation
{
    public class TaxGroupDtoValidator : BaseValidator<TaxGroupDto>
    {
        public TaxGroupDtoValidator()
        {
            RuleFor(x => x.TaxGroup).NotEmpty().MaximumLength(FieldLengths.TaxGroup);
            RuleFor(x => x.TaxGroupName).MaximumLength(FieldLengths.TaxGroupName);
        }
    }
}
