using IAX.IXApi.Shared.Application.Validation;
using FluentValidation;

namespace IAX.IXApi.Modules.Organization.Companies
{
    public class OrgCompanyDtoValidator : BaseValidator<OrgCompanyDto>
    {
        public OrgCompanyDtoValidator()
        {
            RuleFor(x => x.NameAR).NotEmpty().WithMessage("Arabic Name is required");
            RuleFor(x => x.Name).NotEmpty().WithMessage("English Name is required");
            RuleFor(x => x.PrimaryEmail).EmailAddress().When(x => !string.IsNullOrEmpty(x.PrimaryEmail))
                .WithMessage("Primary Email must be a valid email address");
            RuleFor(x => x.NotificationEmail).EmailAddress().When(x => !string.IsNullOrEmpty(x.NotificationEmail))
                .WithMessage("Notification Email must be a valid email address");
        }
    }
}
