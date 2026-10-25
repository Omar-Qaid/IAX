using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory.Validation;

public sealed class CloseRequestValidator : BaseValidator<InventCloseController.CloseRequest>
{
    public CloseRequestValidator()
    {
        RuleFor(x => x.Cutoff).NotEmpty();
    }
}
