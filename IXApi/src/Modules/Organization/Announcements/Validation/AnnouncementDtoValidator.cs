using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Organization.Announcements
{
    public class AnnouncementDtoValidator : BaseValidator<AnnouncementDto>
    {
        public AnnouncementDtoValidator()
        {
            RuleFor(x => x.Title).NotEmpty().WithMessage("Title is required.");
        }
    }
}
