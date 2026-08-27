using IAX.IXApi.Shared.Application.Validation;
using FluentValidation;

namespace IAX.IXApi.Modules.Organization.Occupations
{
    public class OccupationDtoValidator : BaseValidator<OccupationDto>
    {
        public OccupationDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("English Name is required");
        }
    }
}
