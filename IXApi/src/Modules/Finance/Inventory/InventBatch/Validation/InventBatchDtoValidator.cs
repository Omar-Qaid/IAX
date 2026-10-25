using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory.Validation;

public sealed class InventBatchDtoValidator : BaseValidator<InventBatchDto>
{
    public InventBatchDtoValidator()
    {
        RuleFor(x => x.ItemId).NotEmpty().MaximumLength(FieldLengths.ItemId);
        RuleFor(x => x.InventBatchId).NotEmpty().MaximumLength(FieldLengths.InventBatchId);
    }
}
