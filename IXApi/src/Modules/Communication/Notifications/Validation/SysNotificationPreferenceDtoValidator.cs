using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Communication.Notifications.Validation
{
    public class SysNotificationPreferenceDtoValidator : BaseValidator<SysNotificationPreferenceDto>
    {
        public SysNotificationPreferenceDtoValidator()
        {
            RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
        }
    }
}
