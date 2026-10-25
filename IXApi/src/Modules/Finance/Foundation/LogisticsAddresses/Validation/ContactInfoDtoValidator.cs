using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Foundation.LogisticsAddresses
{
    public class ContactInfoDtoValidator : BaseValidator<ContactInfoDto>
    {
        public ContactInfoDtoValidator()
        {
            RuleFor(x => x.Number).NotEmpty().MaximumLength(50);
        }
    }
}
