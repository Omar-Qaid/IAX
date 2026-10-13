using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Common;
using Xunit;

namespace IXApi.Tests;

public sealed class FinanceTableNumbersTests
{
    [Fact]
    public void SalesDocumentNumbersMatchTypedLookup()
    {
        Assert.Equal(2002, FinanceTableNumbers.For<SalesTable>());
        Assert.Equal(2003, FinanceTableNumbers.For<SalesLine>());
        Assert.Throws<ArgumentException>(() => FinanceTableNumbers.For<FinanceTableNumbersTests>());
    }
}
