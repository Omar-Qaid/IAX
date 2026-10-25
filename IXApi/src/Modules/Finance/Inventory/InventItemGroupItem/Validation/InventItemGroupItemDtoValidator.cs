using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventItemGroupItemDtoValidator : BaseValidator<InventItemGroupItemDto>
{
    public InventItemGroupItemDtoValidator()
    {
    }
}
