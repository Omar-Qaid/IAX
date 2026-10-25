using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Foundation.LogisticsAddresses
{
    public class AddressInfoDtoValidator : BaseValidator<AddressInfoDto>
    {
        public AddressInfoDtoValidator()
        {
            RuleFor(x => x.CountryRegionId).NotEmpty().MaximumLength(10);
        }
    }
}
