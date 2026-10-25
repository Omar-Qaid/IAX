using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax.Validation
{
    public class TaxAuthorityAddressDtoValidator : BaseValidator<TaxAuthorityAddressDto>
    {
        public TaxAuthorityAddressDtoValidator()
        {
            RuleFor(x => x.TaxAuthority).NotEmpty();
        }
    }
}
