using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.LedgerAccount.Validation;

public sealed class LedgerChartOfAccountsDtoValidator : BaseValidator<LedgerChartOfAccountsDto>
{
    public LedgerChartOfAccountsDtoValidator()
    {
    }
}
