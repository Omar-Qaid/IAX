using IAX.IXApi.Modules.Finance.AccountsReceivable;

namespace IAX.IXApi.Modules.Finance.Foundation.Markup;

public interface IMarkupChargeCalculator
{
    Task<decimal> CalculateAmount(MarkupTrans charge, CancellationToken ct);
}
