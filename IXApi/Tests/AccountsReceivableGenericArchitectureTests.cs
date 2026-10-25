using FluentValidation;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Customer;
using IAX.IXApi.Shared.Application.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IAX.IXApi.Tests;

public sealed class AccountsReceivableGenericArchitectureTests
{
    [Fact]
    public void Crud_controllers_use_registered_generic_services()
    {
        var services = new ServiceCollection();
        services.AddFinanceModule(new ConfigurationBuilder().Build());
        var controllers = typeof(CustomerController).Assembly.GetTypes()
            .Where(type => type.IsClass && type.Namespace?.Contains(".AccountsReceivable") == true
                && type.Name.EndsWith("Controller", StringComparison.Ordinal)
                && type.BaseType is { IsGenericType: true }
                && type.BaseType.GetGenericTypeDefinition() == typeof(BaseController<,>))
            .ToList();

        var expectedControllers = new[]
        {
            "CustomerController", "CustomerGroupController", "SalesPoolController",
            "CustParametersController", "CustPaymModeController", "CustLedgerController",
            "CustLedgerAccountsController", "PriceDiscTableController", "PriceDiscGroupController",
            "PriceDiscAdmTableController", "PriceDiscAdmTransController", "PriceDiscAdmNameController",
            "SalesTableController", "SalesOrderConfirmationController",
            "PackingSlipJournalController", "SalesInvoiceController", "CustTransController"
        };
        Assert.Empty(expectedControllers.Except(controllers.Select(type => type.Name)));
        foreach (var controller in controllers)
        {
            var entity = controller.BaseType!.GetGenericArguments()[0];
            var serviceContract = typeof(IBaseService<>).MakeGenericType(entity);
            var serviceParameter = controller.GetConstructors().Single().GetParameters()[0].ParameterType;
            Assert.True(serviceContract.IsAssignableFrom(serviceParameter), controller.FullName);
            var registration = Assert.Single(services, descriptor => descriptor.ServiceType == serviceParameter);
            Assert.NotNull(registration.ImplementationType);
            Assert.True(serviceParameter.IsAssignableFrom(registration.ImplementationType), controller.FullName);
            Assert.True(typeof(BaseService<>).MakeGenericType(entity)
                .IsAssignableFrom(registration.ImplementationType), controller.FullName);
        }
    }

    [Fact]
    public void Accounts_receivable_validators_use_existing_base_validator()
    {
        var validators = typeof(CustomerController).Assembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract
                && type.Namespace?.Contains(".AccountsReceivable") == true
                && type.Name.EndsWith("Validator", StringComparison.Ordinal)
                && type.GetInterfaces().Any(contract => contract.IsGenericType
                    && contract.GetGenericTypeDefinition() == typeof(IValidator<>)))
            .ToList();

        var expectedValidators = new[]
        {
            "CustomerDtoValidator", "CustGroupDtoValidator",
            "CustTransDtoValidator", "CustTransOpenDtoValidator",
            "CustInvoiceJourDtoValidator", "CustInvoiceTableDtoValidator", "CustInvoiceTransDtoValidator",
            "CustSettlementDtoValidator",
            "SalesQuotationDtoValidator", "SalesOrderCopyValidator", "SalesOrderConfirmationValidator",
            "SalesTableDtoValidator", "SalesLineDtoValidator",
            "CustConfirmJourDtoValidator", "CustConfirmTransDtoValidator",
            "CustPackingSlipJourDtoValidator", "CustPackingSlipTransDtoValidator",
            "CustQuotationJourDtoValidator", "SpecTransDtoValidator",
            "CustPaymentPostDtoValidator",
            "CustReturnCreateDtoValidator", "CustReturnReceiptDtoValidator", "CustCreditNoteDtoValidator",
            "SalesPoolDtoValidator", "CustPaymModeDtoValidator",
            "CustParametersDtoValidator", "CustLedgerDtoValidator",
            "CustLedgerAccountsDtoValidator", "PriceDiscTableDtoValidator",
            "PriceDiscGroupDtoValidator", "PriceDiscAdmTableDtoValidator",
            "PriceDiscAdmTransDtoValidator", "PriceDiscAdmNameDtoValidator"
        };
        Assert.Empty(expectedValidators.Except(validators.Select(type => type.Name)));
        foreach (var validator in validators)
            Assert.Equal(typeof(BaseValidator<>), validator.BaseType?.GetGenericTypeDefinition());
    }

    [Fact]
    public void Customer_transaction_controller_disables_generic_mutations()
    {
        foreach (var action in new[] { "Create", "Update", "Delete" })
            Assert.True(typeof(CustTransController).GetMethod(action)!
                .IsDefined(typeof(NonActionAttribute), inherit: false));
    }
}
