using IAX.IXApi.Shared.Application.Validation;
using FluentValidation;

namespace IAX.IXApi.Modules.Finance.Foundation.Nationalities
{
    public class HcmNationalityDtoValidator : BaseValidator<HcmNationalityDto>
    {
        public HcmNationalityDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("English Name is required");
        }
    }
}
