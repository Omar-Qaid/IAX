using FluentValidation;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Dtos;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Validation
{
    public class SysBackgroundJobTaskDtoValidator : BaseValidator<SysBackgroundJobTaskDto>
    {
        public SysBackgroundJobTaskDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required").MaximumLength(200);
            RuleFor(x => x.JobKey).NotEmpty().WithMessage("JobKey is required").MaximumLength(200);
            RuleFor(x => x.ExecutionOrder).GreaterThanOrEqualTo(1);
            RuleFor(x => x.MaxRetryCount).InclusiveBetween(0, 10);
            RuleFor(x => x.RetryDelaySeconds).InclusiveBetween(1, 3600);
        }
    }
}
