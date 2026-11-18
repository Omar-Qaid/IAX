using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Tax.Validation
{
    public class TaxAuthorityAddressDtoValidator : BaseValidator<TaxAuthorityAddressDto>
    {
        public TaxAuthorityAddressDtoValidator()
        {
            RuleFor(x => x.TaxAuthority).NotEmpty().MaximumLength(10);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
            RuleFor(x => x.TaxAuthorityId).MaximumLength(50);
            RuleFor(x => x.AccountNum).MaximumLength(20);
            RuleFor(x => x.Phone).MaximumLength(20);
            RuleFor(x => x.Mobile).MaximumLength(20);
            RuleFor(x => x.Fax).MaximumLength(20);
            RuleFor(x => x.Sms).MaximumLength(20);
            RuleFor(x => x.Telex).MaximumLength(20);
            RuleFor(x => x.Pager).MaximumLength(20);
            RuleFor(x => x.Extension).MaximumLength(10);
            RuleFor(x => x.Email).MaximumLength(100);
            RuleFor(x => x.Url).MaximumLength(255);
            RuleFor(x => x.Address).MaximumLength(500);
        }
    }
}
