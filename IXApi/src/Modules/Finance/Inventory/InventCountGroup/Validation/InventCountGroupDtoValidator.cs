using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory.Validation;

public sealed class InventCountGroupDtoValidator : BaseValidator<InventCountGroupDto>
{
    public InventCountGroupDtoValidator()
    {
    }
}
