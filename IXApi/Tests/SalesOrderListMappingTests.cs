using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Mapster;
using Xunit;

namespace IAX.IXApi.Tests;

public sealed class SalesOrderListMappingTests
{
    [Fact]
    public void Maps_sales_order_list_fields_and_existing_date_defaults()
    {
        new SalesOrderListMapping().Register(TypeAdapterConfig.GlobalSettings);
        var createdAt = new DateTime(2025, 4, 3, 14, 30, 0, DateTimeKind.Utc);
        var order = new SalesTable
        {
            RecId = 42,
            SalesId = "SO-42",
            CustAccount = "C-1",
            SalesName = "Fallback",
            SalesStatus = SalesStatus.Backorder,
            CreatedAt = createdAt,
            DeliveryPostalAddress = 87,
            OneTimeCustomer = NoYes.Yes,
            CurrencyCode = "USD"
        };

        var dto = new SalesOrderListMappingSource(order, "Customer name", "Delivery address")
            .Adapt<SalesOrderListDto>();

        Assert.Equal(42, dto.RecId);
        Assert.Equal("SO-42", dto.SalesId);
        Assert.Equal("Customer name", dto.CustomerName);
        Assert.Equal("Delivery address", dto.DeliveryAddress);
        Assert.Equal("87", dto.DeliveryPostalAddress);
        Assert.Equal(createdAt.Date, dto.OrderDate);
        Assert.Equal("Backorder", dto.SalesStatus);
        Assert.True(dto.OneTimeCustomer);
        Assert.Null(dto.Deadline);
        Assert.Equal("USD", dto.CurrencyCode);
    }

    [Fact]
    public void Quick_create_mapping_keeps_its_existing_response_fields()
    {
        new SalesOrderQuickCreateMapping().Register(TypeAdapterConfig.GlobalSettings);
        var order = new SalesTable
        {
            RecId = 58,
            SalesId = "SO-58",
            CustAccount = "C-2",
            SalesName = "Fallback",
            SalesStatus = SalesStatus.Backorder,
            CurrencyCode = "EUR",
            OrderDate = new DateTime(2025, 4, 4),
            OneTimeCustomer = NoYes.No
        };

        var dto = new SalesOrderQuickCreateMappingSource(order, "Quick customer")
            .Adapt<SalesOrderListDto>();

        Assert.Equal(58, dto.RecId);
        Assert.Equal("SO-58", dto.SalesId);
        Assert.Equal("Quick customer", dto.CustomerName);
        Assert.Equal("Backorder", dto.SalesStatus);
        Assert.Equal("EUR", dto.CurrencyCode);
        Assert.Equal(order.OrderDate, dto.OrderDate);
        Assert.False(dto.OneTimeCustomer);
        Assert.Null(dto.Deadline);
    }
}
