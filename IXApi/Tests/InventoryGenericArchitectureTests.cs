using FluentValidation;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance;
using IAX.IXApi.Modules.Finance.Inventory;
using IAX.IXApi.Shared.Application.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IAX.IXApi.Tests;

public sealed class InventoryGenericArchitectureTests
{
    [Fact]
    public void Generic_inventory_setup_services_are_registered_with_base_service()
    {
        var services = new ServiceCollection();
        services.AddFinanceModule(new ConfigurationBuilder().Build());
        var contracts = new[]
        {
            typeof(IEcoResProductService), typeof(IInventItemGroupService),
            typeof(IInventModelGroupService), typeof(IInventLocationService),
            typeof(IInventSiteService), typeof(IInventPostingService),
            typeof(IStorageDimensionGroupService), typeof(ITrackingDimensionGroupService)
        };

        foreach (var contract in contracts)
        {
            var entity = Assert.Single(contract.GetInterfaces(), type =>
                type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IBaseService<>))
                .GetGenericArguments()[0];
            var registration = Assert.Single(services, descriptor => descriptor.ServiceType == contract);
            Assert.NotNull(registration.ImplementationType);
            Assert.True(contract.IsAssignableFrom(registration.ImplementationType), contract.Name);
            Assert.True(typeof(BaseService<>).MakeGenericType(entity)
                .IsAssignableFrom(registration.ImplementationType), contract.Name);
        }
    }

    [Fact]
    public void Inventory_validators_use_existing_base_validator()
    {
        var validators = typeof(EcoResProductController).Assembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract
                && type.Namespace?.Contains(".Inventory", StringComparison.Ordinal) == true
                && type.GetInterfaces().Any(contract => contract.IsGenericType
                    && contract.GetGenericTypeDefinition() == typeof(IValidator<>)))
            .ToList();

        Assert.NotEmpty(validators);
        foreach (var validator in validators)
            Assert.Equal(typeof(BaseValidator<>), validator.BaseType?.GetGenericTypeDefinition());
    }

    [Fact]
    public void Specialized_inventory_controllers_keep_their_existing_routes()
    {
        foreach (var controller in new[]
        {
            typeof(EcoResProductController), typeof(InventBatchController),
            typeof(InventJournalController), typeof(InventCloseController),
            typeof(InventoryReservationController)
        })
            Assert.Equal(typeof(ControllerBase), controller.BaseType);
    }
}
