using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Administration.Settings
{
    public class SysUserSettingsDtoValidator : BaseValidator<SysUserSettingsDto>
    {
        public SysUserSettingsDtoValidator()
        {
            RuleFor(x => x.Theme).NotEmpty().WithMessage("Theme is required").MaximumLength(20);
            RuleFor(x => x.Language).NotEmpty().WithMessage("Language is required").MaximumLength(10);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100");
            RuleFor(x => x.DashboardLayout).MaximumLength(2000);
        }
    }
}
