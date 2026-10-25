using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.Ledger.Validation;

public sealed class LedgerDtoValidator : BaseValidator<LedgerDto>
{
    public LedgerDtoValidator()
    {
    }
}
