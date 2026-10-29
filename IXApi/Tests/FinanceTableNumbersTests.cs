using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Common;
using Xunit;

namespace IXApi.Tests;

public sealed class FinanceTableNumbersTests
{
    [Fact]
    public void SalesDocumentNumbersMatchTypedLookup()
    {
        Assert.Equal(0, FinanceTableNumbers.Unspecified);
        Assert.Equal(2001, FinanceTableNumbers.For<CustTable>());
        Assert.Equal(2002, FinanceTableNumbers.For<SalesTable>());
        Assert.Equal(2003, FinanceTableNumbers.For<SalesLine>());
        Assert.Equal(3001, FinanceTableNumbers.For<global::IAX.IXApi.Modules.Finance.Entities.VendTable>());
        Assert.Equal(1326239103, FinanceTableNumbers.For<global::IAX.IXApi.Modules.Finance.AccountsPayable.VendInvoiceTrans>());
        Assert.Equal(4001, FinanceTableNumbers.For<global::IAX.IXApi.Modules.Finance.Foundation.HcmWorkers.HcmWorker>());
        Assert.Throws<ArgumentException>(() => FinanceTableNumbers.For<FinanceTableNumbersTests>());
    }
}
