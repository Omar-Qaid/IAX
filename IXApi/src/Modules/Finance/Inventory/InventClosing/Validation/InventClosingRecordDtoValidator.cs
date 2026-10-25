using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory.Validation;

public sealed class InventClosingRecordDtoValidator : BaseValidator<InventClosingRecordDto>
{
    public InventClosingRecordDtoValidator()
    {
        RuleFor(x => x.Voucher).MaximumLength(50);
        RuleFor(x => x.PeriodCode).MaximumLength(20);
    }
}
