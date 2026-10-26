using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Communication.Notifications.Validation
{
    public class SysNotificationTemplateDtoValidator : BaseValidator<SysNotificationTemplateDto>
    {
        public SysNotificationTemplateDtoValidator()
        {
            RuleFor(x => x.Code).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
            RuleFor(x => x.Subject).NotEmpty().MaximumLength(500);
            RuleFor(x => x.Body).NotEmpty();
        }
    }
}
