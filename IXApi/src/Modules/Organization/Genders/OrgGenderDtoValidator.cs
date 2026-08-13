using IAX.IXApi.Shared.Application.Validation;
using FluentValidation;

namespace IAX.IXApi.Modules.Organization.Genders
{
    public class OrgGenderDtoValidator : BaseValidator<OrgGenderDto>
    {
        public OrgGenderDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("English Name is required");
        }
    }
}
