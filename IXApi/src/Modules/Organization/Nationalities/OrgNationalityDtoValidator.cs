using IAX.IXApi.Shared.Application.Validation;
using FluentValidation;

namespace IAX.IXApi.Modules.Organization.Nationalities
{
    public class OrgNationalityDtoValidator : BaseValidator<OrgNationalityDto>
    {
        public OrgNationalityDtoValidator()
        {
            RuleFor(x => x.NameAR).NotEmpty().WithMessage("Arabic Name is required");
            RuleFor(x => x.Name).NotEmpty().WithMessage("English Name is required");
        }
    }
}
