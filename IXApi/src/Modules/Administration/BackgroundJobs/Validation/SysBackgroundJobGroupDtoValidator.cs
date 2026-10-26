using FluentValidation;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Dtos;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Validation
{
    public class SysBackgroundJobGroupDtoValidator : BaseValidator<SysBackgroundJobGroupDto>
    {
        public SysBackgroundJobGroupDtoValidator()
        {
            RuleFor(x => x.GroupCode).NotEmpty().WithMessage("GroupCode is required").MaximumLength(10);
            RuleFor(x => x.Description).MaximumLength(60);
            RuleFor(x => x.SchedulingPriority).InclusiveBetween(0, 2);
            RuleFor(x => x.MaxConcurrency).InclusiveBetween(1, 100);
        }
    }
}
