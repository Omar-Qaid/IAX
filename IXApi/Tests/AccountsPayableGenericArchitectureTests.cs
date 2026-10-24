using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IAX.IXApi.Tests;

public sealed class AccountsPayableGenericArchitectureTests
{
    [Fact]
    public void Vendor_crud_controllers_use_registered_base_services()
    {
        var services = new ServiceCollection();
        services.AddFinanceModule(new ConfigurationBuilder().Build());
        foreach (var controller in new[] { typeof(VendorController), typeof(VendorGroupController) })
        {
            Assert.Equal(typeof(BaseController<,>), controller.BaseType?.GetGenericTypeDefinition());
            var entity = controller.BaseType!.GetGenericArguments()[0];
            var contract = controller.GetConstructors().Single().GetParameters()[0].ParameterType;
            Assert.True(typeof(IBaseService<>).MakeGenericType(entity).IsAssignableFrom(contract));
            var registration = Assert.Single(services, descriptor => descriptor.ServiceType == contract);
            Assert.NotNull(registration.ImplementationType);
            Assert.True(typeof(BaseService<>).MakeGenericType(entity)
                .IsAssignableFrom(registration.ImplementationType));
        }
    }

    [Fact]
    public void Purchase_posting_services_remain_specialized_and_registered()
    {
        var services = new ServiceCollection();
        services.AddFinanceModule(new ConfigurationBuilder().Build());
        var contracts = new[]
        {
            typeof(IPurchOrderService), typeof(IPurchInventorySupplyService),
            typeof(IPurchConfirmationService), typeof(IProductReceiptService),
            typeof(IVendorInvoiceService), typeof(IPurchReturnService)
        };
        foreach (var contract in contracts)
        {
            var registration = Assert.Single(services, descriptor => descriptor.ServiceType == contract);
            Assert.NotNull(registration.ImplementationType);
            Assert.True(contract.IsAssignableFrom(registration.ImplementationType));
        }
        Assert.Equal(typeof(Microsoft.AspNetCore.Mvc.ControllerBase), typeof(PurchTableController).BaseType);
        Assert.Equal(typeof(Microsoft.AspNetCore.Mvc.ControllerBase), typeof(PurchReturnController).BaseType);
    }
}
