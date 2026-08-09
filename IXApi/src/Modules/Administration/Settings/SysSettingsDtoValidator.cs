using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Administration.Settings
{
    public class SysSettingsDtoValidator : BaseValidator<SysSettingsDto>
    {
        public SysSettingsDtoValidator()
        {
            RuleFor(x => x.AppName).NotEmpty().WithMessage("AppName is required").MaximumLength(256);
            RuleFor(x => x.DefaultLanguage).NotEmpty().WithMessage("DefaultLanguage is required").MaximumLength(10);
            RuleFor(x => x.TimeZone).NotEmpty().WithMessage("TimeZone is required").MaximumLength(50);
            RuleFor(x => x.Currency).NotEmpty().WithMessage("Currency is required").MaximumLength(10);
            RuleFor(x => x.DateFormat).NotEmpty().WithMessage("DateFormat is required").MaximumLength(50);
            RuleFor(x => x.MaxUploadSize).GreaterThan(0).WithMessage("MaxUploadSize must be greater than 0");
            RuleFor(x => x.PaginationSize).InclusiveBetween(1, 100).WithMessage("PaginationSize must be between 1 and 100");
        }
    }
}
