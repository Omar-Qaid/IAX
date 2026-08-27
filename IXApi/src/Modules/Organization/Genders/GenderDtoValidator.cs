using IAX.IXApi.Shared.Application.Validation;
using FluentValidation;

namespace IAX.IXApi.Modules.Organization.Genders
{
    public class GenderDtoValidator : BaseValidator<GenderDto>
    {
        public GenderDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("English Name is required");
        }
    }
}
