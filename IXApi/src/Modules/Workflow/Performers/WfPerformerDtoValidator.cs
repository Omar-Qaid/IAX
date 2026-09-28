using IAX.IXApi.Shared.Application.Validation;
using FluentValidation;

namespace IAX.IXApi.Modules.Workflow.Performers
{
    public class WfPerformerDtoValidator : BaseValidator<WfPerformerDto>
    {
        public WfPerformerDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("English Name is required");
            RuleFor(x => x.PerformerTypeId)
                .InclusiveBetween((short)1, (short)5)
                .WithMessage("A supported performer type is required.");

            When(x => x.PerformerTypeId == 1, () =>
            {
                RuleFor(x => x)
                    .Must(x => x.IsApplicant || x.IsEmployee || x.IsManager1 ||
                               x.IsManager2 || x.IsManager3 || x.IsManager4)
                    .WithMessage("Select an applicant, employee, or manager role for an organizational performer.");
                RuleFor(x => x)
                    .Must(x => new[] { x.IsManager1, x.IsManager2, x.IsManager3, x.IsManager4 }
                        .Count(selected => selected) <= 1)
                    .WithMessage("Select only one manager level for an organizational performer.");
            });

            When(x => x.PerformerTypeId is 2 or 3, () =>
            {
                RuleFor(x => x.RelatedField)
                    .NotNull()
                    .GreaterThan(0)
                    .WithMessage("A related form control is required for this performer type.");
            });

            When(x => x.PerformerTypeId == 4, () =>
            {
                RuleFor(x => x.UserIds)
                    .NotEmpty()
                    .WithMessage("Select at least one worker for a user performer.");
                RuleFor(x => x.UserIds)
                    .Must(ids => ids.Count == ids.Distinct().Count())
                    .WithMessage("The same worker cannot be selected more than once.");
            });

            When(x => x.PerformerTypeId == 5, () =>
            {
                RuleFor(x => x.SqlTable).NotEmpty().WithMessage("SQL table is required for a database query performer.");
                RuleFor(x => x.SqlField).NotEmpty().WithMessage("SQL field is required for a database query performer.");
            });
        }
    }
}
