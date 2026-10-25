using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax.Validation
{
    public class TaxLedgerAccountGroupDtoValidator : BaseValidator<TaxLedgerAccountGroupDto>
    {
        public TaxLedgerAccountGroupDtoValidator()
        {
            RuleFor(x => x.TaxAccountGroup).NotEmpty().MaximumLength(FieldLengths.TaxAccountGroup);
            RuleFor(x => x.Name).MaximumLength(FieldLengths.TaxGroupName);
        }
    }
}
