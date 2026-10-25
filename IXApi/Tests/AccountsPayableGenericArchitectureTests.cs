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
        foreach (var controller in new[] { typeof(VendorController), typeof(VendorGroupController), typeof(VendLedgerAccountsController), typeof(PurchTableController) })
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
    public void Accounts_payable_validators_use_existing_base_validator()
    {
        var validators = new[]
        {
            typeof(VendorDtoValidator),
            typeof(VendorGroupDtoValidator),
            typeof(VendLedgerAccountsDtoValidator),
            typeof(VendTransQueryDtoValidator),
            typeof(PurchTableDtoValidator),
            typeof(PurchOrderCreateDtoValidator),
            typeof(PurchLineInputDtoValidator),
            typeof(ProductReceiptPostDtoValidator),
            typeof(VendorInvoicePostDtoValidator),
            typeof(PurchReturnCreateDtoValidator),
            typeof(PurchReturnShipDtoValidator),
            typeof(VendCreditNoteDtoValidator)
        };

        foreach (var validator in validators)
        {
            Assert.Equal(typeof(IAX.IXApi.Shared.Application.Validation.BaseValidator<>), validator.BaseType?.GetGenericTypeDefinition());
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
        Assert.True(typeof(BaseController<PurchTable, PurchTableDto>).IsAssignableFrom(typeof(PurchTableController)));
        Assert.Equal(typeof(Microsoft.AspNetCore.Mvc.ControllerBase), typeof(PurchReturnController).BaseType);
    }
}
