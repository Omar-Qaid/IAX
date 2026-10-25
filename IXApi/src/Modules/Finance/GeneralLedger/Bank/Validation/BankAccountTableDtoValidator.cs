using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.Bank.Validation;

public sealed class BankAccountTableDtoValidator : BaseValidator<BankAccountTableDto>
{
    public BankAccountTableDtoValidator()
    {
    }
}
