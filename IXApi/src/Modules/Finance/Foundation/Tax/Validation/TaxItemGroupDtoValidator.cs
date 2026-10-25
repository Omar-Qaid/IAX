using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax.Validation
{
    public class TaxItemGroupDtoValidator : BaseValidator<TaxItemGroupDto>
    {
        public TaxItemGroupDtoValidator()
        {
            RuleFor(x => x.TaxItemGroup).NotEmpty().MaximumLength(FieldLengths.TaxItemGroup);
            RuleFor(x => x.Name).MaximumLength(FieldLengths.TaxGroupName);
        }
    }
}
