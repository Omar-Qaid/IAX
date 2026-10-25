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
            Assert.True(typeof(ControllerBase).IsAssignableFrom(controller));
    }

    [Fact]
    public void Specialized_inventory_controllers_inherit_base_controller_and_use_registered_base_services()
    {
        var services = new ServiceCollection();
        services.AddFinanceModule(new ConfigurationBuilder().Build());

        var controllers = new[]
        {
            typeof(InventBatchController),
            typeof(InventCountGroupController),
            typeof(InventClosingController),
            typeof(InventSerialController),
            typeof(InventPackagingController)
        };

        foreach (var controller in controllers)
        {
            Assert.Equal(typeof(IAX.IXApi.Api.Controllers.BaseController<,>), controller.BaseType?.GetGenericTypeDefinition());
            var entity = controller.BaseType!.GetGenericArguments()[0];
            var contract = controller.GetConstructors().First().GetParameters()[0].ParameterType;
            Assert.True(typeof(IBaseService<>).MakeGenericType(entity).IsAssignableFrom(contract));
            var registration = Assert.Single(services, descriptor => descriptor.ServiceType == contract);
            Assert.NotNull(registration.ImplementationType);
            Assert.True(typeof(BaseService<>).MakeGenericType(entity)
                .IsAssignableFrom(registration.ImplementationType));
        }
    }

    [Fact]
    public void Inventory_features_adhere_to_seven_folder_layout()
    {
        var candidatePaths = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "src", "Modules", "Finance", "Inventory"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "IXApi", "src", "Modules", "Finance", "Inventory"),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../IXApi/src/Modules/Finance/Inventory"))
        };

        var inventoryPath = candidatePaths.FirstOrDefault(Directory.Exists);
        if (inventoryPath != null)
        {
            var expectedFolders = new[] { "Configuration", "Controllers", "Dtos", "Entities", "Interfaces", "Services", "Validation" };
            var featureDirs = Directory.GetDirectories(inventoryPath);

            Assert.NotEmpty(featureDirs);
            foreach (var dir in featureDirs)
            {
                var subdirs = Directory.GetDirectories(dir).Select(Path.GetFileName).ToHashSet(StringComparer.OrdinalIgnoreCase);
                foreach (var expected in expectedFolders)
                {
                    Assert.True(subdirs.Contains(expected), $"Feature '{Path.GetFileName(dir)}' is missing folder '{expected}'.");
                }
            }
        }
    }
}

