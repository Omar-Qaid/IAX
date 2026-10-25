using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public sealed class ReturnDispositionCodeDtoValidator : BaseValidator<ReturnDispositionCodeDto>
{
    public ReturnDispositionCodeDtoValidator()
    {
        RuleFor(x => x.DispositionCodeId).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Description).MaximumLength(250);
    }
}
