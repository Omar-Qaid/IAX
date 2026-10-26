using FluentValidation;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Dtos;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Validation
{
    public class BatchJobActivePeriodDtoValidator : BaseValidator<BatchJobActivePeriodDto>
    {
        public BatchJobActivePeriodDtoValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required").MaximumLength(10);
            RuleFor(x => x.Name).MaximumLength(150);
            RuleFor(x => x.FromTimeUtc).InclusiveBetween(0, 86399);
            RuleFor(x => x.ToTimeUtc).InclusiveBetween(0, 86399);
            RuleFor(x => x.FromTimeLocal).InclusiveBetween(0, 86399);
            RuleFor(x => x.ToTimeLocal).InclusiveBetween(0, 86399);
        }
    }

    public class BatchSettingsDtoValidator : BaseValidator<BatchSettingsDto>
    {
        public BatchSettingsDtoValidator()
        {
            RuleFor(x => x.PollIntervalSeconds).InclusiveBetween(1, 3600);
        }
    }
}
