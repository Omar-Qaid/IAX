using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory.Validation;

public sealed class InventBatchInputDtoValidator : BaseValidator<InventBatchInputDto>
{
    public InventBatchInputDtoValidator()
    {
        RuleFor(x => x.ItemId).NotEmpty().MaximumLength(FieldLengths.ItemId);
        RuleFor(x => x.InventBatchId).NotEmpty().MaximumLength(FieldLengths.InventBatchId);
        RuleFor(x => x)
            .Must(x => x.ProdDate == null || x.ExpDate == null || x.ExpDate.Value.Date >= x.ProdDate.Value.Date)
            .WithMessage("Expiration date cannot precede production date.");
    }
}
