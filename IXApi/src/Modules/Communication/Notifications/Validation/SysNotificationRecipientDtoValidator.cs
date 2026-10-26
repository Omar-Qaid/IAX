using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Communication.Notifications.Validation
{
    public class SysNotificationRecipientDtoValidator : BaseValidator<SysNotificationRecipientDto>
    {
        public SysNotificationRecipientDtoValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().MaximumLength(256);
        }
    }
}
