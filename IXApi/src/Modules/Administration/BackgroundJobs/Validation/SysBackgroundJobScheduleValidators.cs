using FluentValidation;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Dtos;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Validation
{
    public class CreateSysBackgroundJobDtoValidator : BaseValidator<CreateSysBackgroundJobDto>
    {
        public CreateSysBackgroundJobDtoValidator()
        {
            RuleFor(x => x.Caption).NotEmpty().WithMessage("Caption is required").MaximumLength(250);
            RuleFor(x => x.JobKey).NotEmpty().WithMessage("JobKey is required").MaximumLength(200);
        }
    }

    public class UpdateSysBackgroundJobScheduleDtoValidator : BaseValidator<UpdateSysBackgroundJobScheduleDto>
    {
        public UpdateSysBackgroundJobScheduleDtoValidator()
        {
            RuleFor(x => x.ScheduleType).IsInEnum().WithMessage("Valid ScheduleType is required");
        }
    }
}
