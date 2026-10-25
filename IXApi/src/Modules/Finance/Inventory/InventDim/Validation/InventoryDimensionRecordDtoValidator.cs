using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventoryDimensionRecordDtoValidator : BaseValidator<InventoryDimensionRecordDto>
{
    public InventoryDimensionRecordDtoValidator()
    {
        RuleFor(x => x.InventDimId)
            .NotEmpty().WithMessage("Inventory dimension ID is required.")
            .MaximumLength(FieldLengths.InventDimId);
    }
}
