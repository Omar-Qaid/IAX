using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventItemLocationDtoValidator : BaseValidator<InventItemLocationDto>
{
    public InventItemLocationDtoValidator()
    {
    }
}
