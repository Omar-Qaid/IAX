using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.PaymentTerms;
using Xunit;
namespace IXApi.Tests;
public class PurchasePaymentDueDateTests
{
    [Theory]
    [InlineData(2026, 1, 31, 1, 0, 2026, 2, 28)]
    [InlineData(2028, 1, 31, 1, 0, 2028, 2, 29)]
    [InlineData(2026, 10, 27, 0, 30, 2026, 11, 26)]
    public void Net_terms_apply_months_then_days(int y,int m,int d,int months,int days,int ey,int em,int ed)
    {
        Assert.Equal(new DateTime(ey,em,ed), PaymentDueDateCalculator.Calculate(new DateTime(y,m,d),
            new PaymTerm { PaymMethod=PaymMethod.Net, NumOfMonths=months, NumOfDays=days }));
    }
    [Fact]
    public void Unsupported_calendar_is_rejected_instead_of_silently_using_invoice_date()
    {
        Assert.Throws<InvalidOperationException>(() => PaymentDueDateCalculator.Calculate(DateTime.Today,
            new PaymTerm { PaymDayId="CUSTOM" }));
    }
    [Fact]
    public void Cash_on_delivery_uses_invoice_date()
    {
        var date=new DateTime(2026,10,27);
        Assert.Equal(date,PaymentDueDateCalculator.Calculate(date,new PaymTerm {PaymMethod=PaymMethod.COD}));
    }
}
