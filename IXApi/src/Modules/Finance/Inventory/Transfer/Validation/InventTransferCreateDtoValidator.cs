using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory.Transfer.Validation;

/// <summary>
/// Shape of the request only. Warehouse types, item release, stock availability and the
/// shipment/receipt sequence are decided by the transfer order service.
/// </summary>
public sealed class InventTransferCreateDtoValidator : BaseValidator<InventTransferCreateDto>
{
    public InventTransferCreateDtoValidator()
    {
        RuleFor(x => x.TransferId).NotEmpty().WithMessage("A transfer identifier is required.");
        RuleFor(x => x.InventSiteId).NotEmpty().WithMessage("A site is required.");
        RuleFor(x => x.InventLocationIdFrom).NotEmpty().WithMessage("A source warehouse is required.");
        RuleFor(x => x.InventLocationIdTransit).NotEmpty().WithMessage("A transit warehouse is required.");
        RuleFor(x => x.InventLocationIdTo).NotEmpty().WithMessage("A destination warehouse is required.");
        RuleFor(x => x.Lines).NotNull().Must(x => x.Count > 0).WithMessage("A transfer order needs at least one line.");
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(x => x.ItemId).NotEmpty().WithMessage("An item is required on every line.");
            line.RuleFor(x => x.UnitId).NotEmpty().WithMessage("An inventory unit is required on every line.");
            line.RuleFor(x => x.QtyTransfer).GreaterThan(0m).WithMessage("A positive transfer quantity is required.");
        });
    }
}
