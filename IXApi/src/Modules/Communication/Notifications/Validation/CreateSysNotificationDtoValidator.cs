using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Communication.Notifications.Validation
{
    public class CreateSysNotificationDtoValidator : BaseValidator<CreateSysNotificationDto>
    {
        public CreateSysNotificationDtoValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(500);
            RuleFor(x => x.Message).NotEmpty();
        }
    }
}
