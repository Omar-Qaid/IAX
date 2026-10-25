using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory.Validation;

public sealed class InventCountGroupRecordDtoValidator : BaseValidator<InventCountGroupRecordDto>
{
    public InventCountGroupRecordDtoValidator()
    {
        RuleFor(x => x.CountGroupId).NotEmpty().MaximumLength(FieldLengths.CountGroupId);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(FieldLengths.Name);
    }
}
