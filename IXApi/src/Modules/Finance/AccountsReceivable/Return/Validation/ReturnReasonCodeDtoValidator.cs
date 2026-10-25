using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public sealed class ReturnReasonCodeDtoValidator : BaseValidator<ReturnReasonCodeDto>
{
    public ReturnReasonCodeDtoValidator()
    {
        RuleFor(x => x.ReasonCodeId).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Description).MaximumLength(250);
    }
}
