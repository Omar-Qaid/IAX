using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Foundation.LogisticsAddresses
{
    public class ContactPersonDtoValidator : BaseValidator<ContactPersonDto>
    {
        public ContactPersonDtoValidator()
        {
            RuleFor(x => x.ContactPersonId).NotEmpty().MaximumLength(20);
        }
    }
}
