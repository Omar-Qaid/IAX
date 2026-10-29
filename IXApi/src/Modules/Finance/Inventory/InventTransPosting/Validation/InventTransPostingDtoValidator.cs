using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory.InventTransPosting.Validation;

/// <summary>
/// Inventory posting records are written by the posting engine, never entered. This validator
/// only guards the shape of what a read returns.
/// </summary>
public sealed class InventTransPostingDtoValidator : BaseValidator<InventTransPostingDto>
{
    public InventTransPostingDtoValidator()
    {
        RuleFor(x => x.InventTransOrigin).GreaterThan(0).WithMessage("An inventory origin reference is required.");
        RuleFor(x => x.ItemId).NotEmpty().WithMessage("An item is required.");
    }
}
