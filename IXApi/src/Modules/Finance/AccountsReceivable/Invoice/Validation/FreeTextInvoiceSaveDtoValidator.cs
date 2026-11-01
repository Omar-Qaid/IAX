using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Invoice;

public sealed class FreeTextInvoiceSaveDtoValidator : BaseValidator<FreeTextInvoiceSaveDto>
{
    public FreeTextInvoiceSaveDtoValidator()
    {
        RuleFor(x => x.CustomerAccount).NotEmpty();
        RuleFor(x => x.Lines).NotEmpty();
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(x => x.Description).NotEmpty();
            line.RuleFor(x => x.Quantity).GreaterThan(0).PrecisionScale(18, 4, false);
            line.RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0).PrecisionScale(18, 4, false);
            line.RuleFor(x => x.LedgerDimension).GreaterThan(0);
        });
    }
}
