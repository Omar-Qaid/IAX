using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.Bank.Validation;

public sealed class BankAccountStatementDtoValidator : BaseValidator<BankAccountStatementDto>
{
    public BankAccountStatementDtoValidator()
    {
    }
}
