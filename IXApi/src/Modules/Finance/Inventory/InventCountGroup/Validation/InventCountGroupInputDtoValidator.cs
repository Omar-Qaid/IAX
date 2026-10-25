using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory.Validation;

public sealed class InventCountGroupInputDtoValidator : BaseValidator<InventCountGroupInputDto>
{
    public InventCountGroupInputDtoValidator()
    {
        RuleFor(x => x.CountGroupId).NotEmpty().MaximumLength(FieldLengths.CountGroupId);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(FieldLengths.Name);
        RuleFor(x => x)
            .Must(x => x.CountCode != InventCountCode.Period || x.CountPeriod > 0)
            .WithMessage("Period counting requires a positive interval in days.");
    }
}
