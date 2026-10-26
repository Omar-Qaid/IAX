using FluentValidation;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Dtos;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Validation
{
    public class SysBackgroundJobDtoValidator : BaseValidator<SysBackgroundJobDto>
    {
        public SysBackgroundJobDtoValidator()
        {
            RuleFor(x => x.Caption).NotEmpty().WithMessage("Caption is required").MaximumLength(250);
            RuleFor(x => x.JobKey).NotEmpty().WithMessage("JobKey is required").MaximumLength(200);
        }
    }
}
