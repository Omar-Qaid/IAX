using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Communication.Notifications.Validation
{
    public class SysNotificationDtoValidator : BaseValidator<SysNotificationDto>
    {
        public SysNotificationDtoValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(500);
            RuleFor(x => x.Message).NotEmpty();
        }
    }
}
