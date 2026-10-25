using IAX.IXApi.Shared.Application.Validation;
using FluentValidation;

namespace IAX.IXApi.Modules.Finance.Foundation.Occupations
{
    public class HcmOccupationDtoValidator : BaseValidator<HcmOccupationDto>
    {
        public HcmOccupationDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
        }
    }
}
